using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AICarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private CarController carController; // 움직임을 제어할 기존 CarController
    [SerializeField]
    private CarStats carStats; // CarController에서 사용되는 CarStats (속도 제어에 활용)

    [Header("Waypoint System")]
    [SerializeField]
    private List<Transform> waypoints = new List<Transform>(); // AI가 따라갈 웨이포인트 목록
    private int currentWaypointIndex = 0; // 현재 목표 웨이포인트 인덱스

    // ✨ 드리프트 레이어 필터 추가
    [Header("Drifting Settings")]
    [SerializeField]
    [Tooltip("드리프트 진입/유지 시 필요한 최소 속도 (km/h)")]
    private float minSpeedForDrift = 50f;
    [SerializeField]
    [Tooltip("드리프트 코너로 인식할 웨이포인트의 레이어")]
    private LayerMask driftingWaypointLayer;

    [Header("AI Parameters")]
    [SerializeField]
    [Tooltip("다음 웨이포인트로 넘어갈 최소 거리")]
    private float waypointReachDistance = 5f;
    [SerializeField]
    [Tooltip("웨이포인트에 도달했을 때의 목표 속도 (km/h)")]
    private float targetSpeedAtWaypoint = 30f;
    [SerializeField]
    [Tooltip("현재 속도와 목표 속도 간의 허용 오차 (속도 제어 안정화)")]
    private float speedTolerance = 5f;
    [SerializeField]
    [Range(0.01f, 1f)]
    [Tooltip("조향 입력의 부드러움을 결정")]
    private float steerSmoothness = 0.5f;

    // AI에 의해 계산된 입력 값
    private float moveInput = 0f;
    private float steerInput = 0f;
    private bool isBraking = false;
    private bool isDrifting = false; // ✨ 드리프트 상태 변수

    private void Start()
    {
        // CarController 참조 확보
        if (carController == null)
        {
            carController = GetComponent<CarController>();
        }
        if (carController == null)
        {
            Debug.LogError("AICarController requires a CarController component!");
            enabled = false; // 스크립트 비활성화
            return;
        }

        // CarStats 참조 확보 (CarController가 이미 가지고 있다고 가정)
        if (carStats == null)
        {
            carStats = GetComponent<CarStats>();
        }
        if (carStats == null)
        {
            Debug.LogError("AICarController requires a CarStats component!");
            enabled = false;
            return;
        }

        // 웨이포인트가 없으면 경고
        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogWarning("Waypoint list is empty. AI will not move.");
        }
    }

    private void FixedUpdate()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        // 1. 목표 웨이포인트 확인 및 업데이트
        UpdateWaypoint();

        // 2. 입력 계산
        CalculateSteerInput();
        CalculateMoveInput(); // ✨ 드리프트 시 가속/감속 로직 포함

        // 3. CarController에 입력 전달
        carController.GetPlayerInput(moveInput, steerInput, isBraking, isDrifting);
    }

    /// <summary>
    /// 목표 웨이포인트에 도달했는지 확인하고 다음 웨이포인트로 인덱스를 업데이트합니다.
    /// ✨ (웨이포인트 레이어를 확인하여 드리프트 상태를 제어합니다.)
    /// </summary>
    private void UpdateWaypoint()
    {
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        float distanceToWaypoint = Vector3.Distance(transform.position, targetWaypoint.position);

        // 목표 웨이포인트에 충분히 가까워지면 다음 웨이포인트로 전환
        if (distanceToWaypoint < waypointReachDistance)
        {
            // 다음 웨이포인트로 전환
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
            targetWaypoint = waypoints[currentWaypointIndex]; // 새로 설정된 목표 웨이포인트

            // ✨ 다음 목표 웨이포인트의 레이어를 확인하여 드리프트 상태 설정
            // 웨이포인트 GameObject의 Layer를 확인합니다.
            // (1 << targetWaypoint.gameObject.layer) & driftingWaypointLayer는 
            // 현재 웨이포인트 레이어가 driftingWaypointLayer에 포함되는지 확인합니다.
            if (((1 << targetWaypoint.gameObject.layer) & driftingWaypointLayer) != 0)
            {
                // 드리프트 코너입니다.
                isDrifting = true;
            }
            else
            {
                // 일반 코너 또는 직선 구간입니다.
                isDrifting = false;
            }
        }
    }

    /// <summary>
    /// 현재 위치와 목표 웨이포인트를 기반으로 조향 입력을 계산합니다.
    /// </summary>
    private void CalculateSteerInput()
    {
        if (waypoints.Count == 0) return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        //Vector3 directionToTarget = (targetWaypoint.position - transform.position).normalized; // 사용하지 않음

        // 1. 목표 방향을 자동차의 로컬 좌표계로 변환
        Vector3 localTarget = transform.InverseTransformPoint(targetWaypoint.position);

        // 2. 로컬 z축(앞/뒤) 대비 x축(좌/우)의 상대적 위치를 기반으로 조향을 결정
        float targetSteer = localTarget.x;
        targetSteer = Mathf.Clamp(targetSteer, -1f, 1f);

        // ✨ 드리프트 중에는 조향을 더 급격하게(Lerp 없이) 또는 다른 Lerp 속도로 조절할 수 있습니다.
        if (isDrifting)
        {
            // 드리프트 중에는 강한 조향을 유지하여 원심력을 이끌어냄
            // CarController에서 currentSteerAngle을 Lerp하므로, 여기서는 목표 값을 바로 전달합니다.
            steerInput = targetSteer;
        }
        else
        {
            // 일반적인 주행 시 부드러운 조향
            steerInput = Mathf.Lerp(steerInput, targetSteer, Time.deltaTime * steerSmoothness);
        }
    }

    /// <summary>
    /// 목표 웨이포인트까지의 거리에 따라 가속 및 제동 입력을 계산합니다.
    /// ✨ (드리프트 상태에 따라 움직임을 변경합니다.)
    /// </summary>
    private void CalculateMoveInput()
    {
        float currentSpeed = carController.GetCurrentSpeed(); // km/h

        // 웨이포인트까지의 거리
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        float distanceToWaypoint = Vector3.Distance(transform.position, targetWaypoint.position);

        // --- 목표 속도 계산 (기존과 동일) ---
        float targetSpeedCurve = Mathf.Clamp01(distanceToWaypoint / (waypointReachDistance * 2f));
        float baseTargetSpeed = carStats.MaxSpeed * 0.8f;
        float desiredSpeed = Mathf.Lerp(targetSpeedAtWaypoint, baseTargetSpeed, targetSpeedCurve);

        float absSteerInput = Mathf.Abs(steerInput);
        float corneringFactor = 1f - absSteerInput * 0.5f;
        desiredSpeed *= corneringFactor;
        desiredSpeed = Mathf.Min(desiredSpeed, carStats.MaxSpeed);
        // ------------------------------------

        // ===================================================
        // ✨ 1. 드리프트 중일 때의 움직임 로직
        // ===================================================
        if (isDrifting)
        {
            // 드리프트 코너를 통과하는 동안 속도 제어
            if (currentSpeed < minSpeedForDrift)
            {
                // 속도가 너무 느리면 가속하여 드리프트를 유지하거나 코너를 탈출합니다.
                moveInput = 1f;
                isBraking = false;
            }
            else
            {
                // 충분히 빠르면 (관성 유지) 약하게 가속하거나 가속을 중지합니다.
                moveInput = 0.5f; // 드리프트 시 차량이 너무 미끄러지지 않도록 가속을 줄임
                isBraking = false;
            }

            // 드리프트 진입을 위해 목표 웨이포인트에 도달한 직후 잠시 제동할 수 있습니다.
            // 여기서는 UpdateWaypoint에서 isDrifting을 켰으므로,
            // 코너 초입부에서 isBraking = true, moveInput = 0f를 잠시 유지하는 로직을 추가할 수 있습니다.
            // 하지만 CarController의 SidewaysDrag와 TurningCurve가 드리프트를 유도한다고 가정하고,
            // 일단 속도 제어에만 집중합니다.

            return; // 드리프트 중에는 일반 주행 로직을 건너끕니다.
        }

        // ===================================================
        // 2. 일반적인 가속/제동 로직 (Normal Acceleration/Brake)
        // ===================================================

        // 가속
        if (currentSpeed < desiredSpeed - speedTolerance)
        {
            moveInput = 1f; // 전진 가속
            isBraking = false;
        }
        // 제동
        else if (currentSpeed > desiredSpeed + speedTolerance)
        {
            // 급감속이 필요하면 제동 (Braking)
            if (currentSpeed > desiredSpeed * 1.5f || distanceToWaypoint < waypointReachDistance * 1.5f)
            {
                moveInput = 0f;
                isBraking = true;
            }
            // 일반 감속이면 가속 입력 해제 (Deceleration)
            else
            {
                moveInput = 0f;
                isBraking = false;
            }
        }
        // 속도 유지 (Deadzone)
        else
        {
            moveInput = 0f;
            isBraking = false;
        }
    }

    // 디버깅을 위한 시각화 (생략)
    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null) continue;

            // 웨이포인트 지점 시각화
            Gizmos.DrawWireSphere(waypoints[i].position, 0.5f);

            // 다음 웨이포인트까지의 경로
            int nextIndex = (i + 1) % waypoints.Count;
            if (waypoints[nextIndex] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[nextIndex].position);
            }
        }

        // 현재 목표 웨이포인트 시각화
        if (currentWaypointIndex >= 0 && currentWaypointIndex < waypoints.Count && waypoints[currentWaypointIndex] != null)
        {
            // 드리프트 코너인 경우 노란색으로 표시
            if (((1 << waypoints[currentWaypointIndex].gameObject.layer) & driftingWaypointLayer) != 0)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.yellow;
            }

            Gizmos.DrawWireSphere(waypoints[currentWaypointIndex].position, waypointReachDistance); // 도달 반경
            Gizmos.DrawLine(transform.position, waypoints[currentWaypointIndex].position); // 현재 위치에서 목표까지 선
        }
    }
}