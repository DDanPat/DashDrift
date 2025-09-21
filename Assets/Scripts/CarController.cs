using System;
using Unity.VisualScripting;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody carRB;
    [SerializeField] private Transform[] rayPoints; // 자동차 바퀴 위치
    [SerializeField] private LayerMask groundLayer; // 지면 레이어
    [SerializeField] private Transform accelerationPoint; // 가속력 적용 지점
    [SerializeField] private GameObject[] tires = new GameObject[4]; // 바퀴 오브젝트
    [SerializeField] private GameObject[] frontTireParents = new GameObject[2]; // 앞바퀴 좌우 회전 오브젝트

    [Header("서스펜션 설정")]
    [SerializeField] private float springStiffness; // 스프링 강성
    [SerializeField] private float damperStiffness; // 댐퍼 강성
    [SerializeField] private float restLength; // 스프링 정지 길이
    [SerializeField] private float springTravel; // 스프링 이동 거리
    [SerializeField] private float wheelRadius; // 바퀴 반지름

    [Header("입력")]
    private float moveInput = 0;
    private float steerInput = 0;
    private bool isBraking = false;
    private bool isDrifting = false;

    [Header("자동차 설정")]
    [SerializeField] private float acceleration = 15f; // 가속도
    [SerializeField] private float brakeForce = 10f; // 제동력
    [SerializeField] private float maxSpeed = 180f; // 최대 속도
    [SerializeField] private float deceleration = 5f; // 감속도
    [SerializeField] private float steelStrength = 15f; // 조향 강도(바퀴 좌우 회전 각도)
    [SerializeField] private AnimationCurve turningCurve; // 속도에 따른 조향 곡선
    [SerializeField] private float dragCoefficient = 0.8f; // 공기 저항 계수

    private Vector3 currentCarLocalVelocity = Vector3.zero;
    private float carVelocityRatio = 0f;    

    private int[] wheelsIsGrounded = new int[4]; // 바퀴 접지 여부
    private bool isGrounded = false; // 자동차 접지 여부

    [Header("드리프트 설정")]
    [SerializeField] private float driftDragReduction = 0.5f; // 드리프트 시 측면 저항 감소율 (0~1)
    [SerializeField] private float driftTorque = 15f; // 드리프트 시 추가 회전력
    [SerializeField] private float driftSteerLerpSpeed = 2f; // 드리프트 시 조향 전환 속도
    private float currentSteerAngle = 0f;


    [Header("비쥬얼")]
    [SerializeField] private float tireRotSpeed = 3000f; // 바퀴 회전 속도
    [SerializeField] private float maxSteerAngle = 30f; // 최대 조향 각도

    private void Start()
    {
        carRB = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Suspension();
        GroundCheck();
        CalulateCarVelocity();
        Movement();
        Visuals();
    }

    private void Update()
    {
        GetPlayerInput();
    }

    #region Movement

    private void Movement()
    {
        if (isGrounded)
        {
            Acceleration();
            Decelration();
            Trun();
            SidewaysDrag();
        }
        if (isBraking) { Brake(); }
    }

    private void Acceleration()
    {
        carRB.AddForceAtPosition(acceleration * moveInput * transform.forward,
            accelerationPoint.position,
            ForceMode.Acceleration);
    }

    private void Brake()
    {
        if (carRB.linearVelocity.magnitude > 0.1f)
        {
            carRB.AddForceAtPosition(brakeForce * -transform.forward,
                accelerationPoint.position,
                ForceMode.Acceleration);
        }
    }

    private void Decelration()
    {
        carRB.AddForceAtPosition(deceleration * moveInput * - transform.forward,
            accelerationPoint.position,
            ForceMode.Acceleration);
    }

    private void Trun()
    {
        // 드리프트 중에는 조향값을 점진적으로 변경
        if (isDrifting)
        {
            currentSteerAngle = Mathf.Lerp(currentSteerAngle, steerInput, Time.deltaTime * driftSteerLerpSpeed);
        }
        else
        {
            // 드리프트가 아닐 때는 즉시 반응하도록
            currentSteerAngle = steerInput;
        }

        // 전진/후진 방향에 따라 핸들 조작 방향을 일치시키기 위해
        // steerInput에 carVelocityRatio의 부호를 곱한다.
        float effectiveSteerInput = currentSteerAngle * Mathf.Sign(carVelocityRatio);

        // turningCurve.Evaluate에는 속도의 절댓값을 전달하여 회전력의 크기를 조절한다.
        // 예를 들어, 속도가 낮을 때 더 잘 꺾이게 할 수 있다.
        float turningForce = steelStrength * effectiveSteerInput * turningCurve.Evaluate(Mathf.Abs(carVelocityRatio));

        // 드리프트 중에는 추가적인 회전력 부여
        if (isDrifting)
        {
            turningForce += driftTorque * effectiveSteerInput;
        }

        // 토크를 적용한다.
        carRB.AddTorque(turningForce * transform.up, ForceMode.Acceleration);
    }

    private void SidewaysDrag()
    {
        float currentsidewaySpeed = currentCarLocalVelocity.x;

        // 드리프트 중에는 측면 저항을 줄임
        float currentDragCoefficient = dragCoefficient;
        if (isDrifting)
        {
            currentDragCoefficient *= driftDragReduction;
        }

        float dragMagnitude = -currentsidewaySpeed * dragCoefficient;
        Vector3 dragForce = transform.right * dragMagnitude;

        carRB.AddForceAtPosition(dragForce, accelerationPoint.position, ForceMode.Acceleration);
    }

    #endregion

    #region Suspension System

    private void Suspension()
    {
        for (int i = 0; i < rayPoints.Length; i++)
        {
            RaycastHit hit;
            float maxDistance = restLength;// + springTravel;

            if (Physics.Raycast(rayPoints[i].position, -rayPoints[i].up, out hit, maxDistance + wheelRadius, groundLayer))
            {
                wheelsIsGrounded[i] = 1;

                float currentSpringLength = hit.distance - wheelRadius;
                float springCompression = restLength - currentSpringLength / springTravel;

                float springVelocity = Vector3.Dot(carRB.GetPointVelocity(rayPoints[i].position), rayPoints[i].up);
                float dampForce = damperStiffness * springVelocity;

                float springForce = springCompression * springStiffness;

                float netForce = springForce - dampForce;

                carRB.AddForceAtPosition(netForce * rayPoints[i].up, rayPoints[i].position);

                // Visuals (바퀴 회전)
                SetTirePosition(tires[i], hit.point + rayPoints[i].up * wheelRadius);

                Debug.DrawLine(rayPoints[i].position, hit.point, Color.red);
            }
            else
            {
                wheelsIsGrounded[i] = 0;

                // Visuals (바퀴 회전)
                SetTirePosition(tires[i], rayPoints[i].position - rayPoints[i].up * maxDistance);

                Debug.DrawLine(rayPoints[i].position,
                    rayPoints[i].position + (wheelRadius + maxDistance) * -rayPoints[i].up, Color.green);
            }
        }
    }

    #endregion

    #region Car Status Check

    private void GroundCheck()
    {
        int tempGroundedSheels = 0;

        for (int i = 0; i < wheelsIsGrounded.Length; i++)
        {
            tempGroundedSheels += wheelsIsGrounded[i];
        }

        if (tempGroundedSheels > 1)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void CalulateCarVelocity()
    {
        currentCarLocalVelocity = transform.InverseTransformDirection(carRB.linearVelocity);
        carVelocityRatio = currentCarLocalVelocity.z / maxSpeed;
    }

    #endregion

    #region Input Management

    // TODO : Input System으로 변경
    private void GetPlayerInput()
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
        isBraking = Input.GetKey(KeyCode.Space);
        isDrifting = Input.GetKey(KeyCode.LeftShift);
    }

    #endregion

    #region Visuals

    private void Visuals()
    {
        TireVisuals();
    }

    private void TireVisuals()
    {
        float steeringAngle = maxSteerAngle * steerInput;

        for (int i = 0; i < tires.Length; i++)
        {
            tires[i].transform.Rotate(Vector3.right,
                    tireRotSpeed * carVelocityRatio * Time.deltaTime, Space.Self);

            if (i < 2)
            {
                frontTireParents[i].transform.localEulerAngles = new Vector3(
                    frontTireParents[i].transform.localEulerAngles.x,
                    steeringAngle,
                    frontTireParents[i].transform.localEulerAngles.z);
            }
        }
    }


    private void SetTirePosition(GameObject tire, Vector3 targetPosition)
    {
        tire.transform.position = targetPosition;
    }

    #endregion

}
