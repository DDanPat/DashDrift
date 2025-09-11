using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody carRB;
    [SerializeField] private Transform[] rayPoints; // 자동차 바퀴 위치
    [SerializeField] private LayerMask groundLayer; // 지면 레이어
    [SerializeField] private Transform accelerationPoint; // 가속력 적용 지점

    [Header("서스펜션 설정")]
    [SerializeField] private float springStiffness; // 스프링 강성
    [SerializeField] private float damperStiffness; // 댐퍼 강성
    [SerializeField] private float restLength; // 스프링 정지 길이
    [SerializeField] private float springTravel; // 스프링 이동 거리
    [SerializeField] private float wheelRadius; // 바퀴 반지름

    private int[] wheelsIsGrounded = new int[4]; // 바퀴 접지 여부
    private bool isGrounded = false; // 자동차 접지 여부


    [Header("입력")]
    private float moveInput = 0;
    private float steerInput = 0;

    [Header("자동차 설정")]
    [SerializeField] private float acceleration = 25f; // 가속도
    [SerializeField] private float maxSpeed = 100f; // 최대 속도
    [SerializeField] private float deceleration = 10f; // 감속도

    private Vector3 currentCarLocalVelocity = Vector3.zero;
    private float carVelocityRatio = 0f;    

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
        }
    }

    private void Acceleration()
    {
        carRB.AddForceAtPosition(acceleration * moveInput * transform.forward,
            accelerationPoint.position,
            ForceMode.Acceleration);
    }

    private void Decelration()
    {
        carRB.AddForceAtPosition(deceleration * moveInput * - transform.forward,
            accelerationPoint.position,
            ForceMode.Acceleration);
    }

    #endregion

    #region Suspension System

    private void Suspension()
    {
        for (int i = 0; i < rayPoints.Length; i++)
        {
            RaycastHit hit;
            float maxLength = restLength + springTravel;

            if (Physics.Raycast(rayPoints[i].position, -rayPoints[i].up, out hit, maxLength + wheelRadius, groundLayer))
            {
                wheelsIsGrounded[i] = 1;

                float currentSpringLength = hit.distance - wheelRadius;
                float springCompression = restLength - currentSpringLength / springTravel;

                float springVelocity = Vector3.Dot(carRB.GetPointVelocity(rayPoints[i].position), rayPoints[i].up);
                float dampForce = damperStiffness * springVelocity;

                float springForce = springCompression * springStiffness;

                float netForce = springForce - dampForce;

                carRB.AddForceAtPosition(netForce * rayPoints[i].up, rayPoints[i].position);

                Debug.DrawLine(rayPoints[i].position, hit.point, Color.red);
            }
            else
            {
                wheelsIsGrounded[i] = 0;

                Debug.DrawLine(rayPoints[i].position,
                    rayPoints[i].position + (wheelRadius + maxLength) * -rayPoints[i].up, Color.green);
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

    #region Input Handling
    
    private void GetPlayerInput()
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
    }

    #endregion
}
