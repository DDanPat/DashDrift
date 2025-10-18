using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CarController))]
public class AICarController : MonoBehaviour
{
    [Header("AI 설정")]
    [SerializeField] private float maxCornerAngle = 30f;         // 급격한 코너 판단 기준 각도
    [SerializeField] private float cornerSlowDownFactor = 0.5f;   // 코너에서 가속도를 줄이는 비율
    [SerializeField] private float driftStartDistance = 20f;     // 드리프트를 시작할 목표 웨이포인트까지의 거리

    [Header("코너 감속 기준")]
    [SerializeField] private float cornerRatioBaseAngle = 90f;   // (기존 90f) 코너 비율 계산의 기준 각도
    [SerializeField] private float hardBrakeAngle = 75f;         // (기존 75f) 이 각도 초과 시 강제 제동 시작

    private CarController _carController;
    private WaypointManager _waypointManager; // WaypointManager 참조 추가
    private List<Transform> _waypoints;
    private int _currentWaypointIndex = 0;
    private Transform _targetWaypoint;

    // 드리프트 상태 변수
    private bool _isDrifting = false;

    private void Start()
    {
        _carController = GetComponent<CarController>();
        _waypointManager = WaypointManager.Instance; // WaypointManager 인스턴스 저장

        if (_waypointManager != null)
        {
            _waypoints = _waypointManager.GetWaypoints();
        }

        if (_waypoints == null || _waypoints.Count == 0)
        {
            Debug.LogError("WaypointManager에서 유효한 웨이포인트를 찾을 수 없습니다.");
            enabled = false;
            return;
        }

        _targetWaypoint = _waypoints[_currentWaypointIndex];
    }

    private void FixedUpdate()
    {
        CheckWaypointProgress();

        if (_targetWaypoint == null) return;

        // AI 로직 실행 (입력값 계산)
        (float moveInput, float steerInput, bool isBraking, bool isDrifting) = CalculateAIInput();

        // CarController에 입력 전달
        _carController.GetPlayerInput(moveInput, steerInput, isBraking, isDrifting);
    }

    //
    // 경로 추적 및 웨이포인트 전환
    //
    private void CheckWaypointProgress()
    {
        float distanceToWaypoint = Vector3.Distance(transform.position, _targetWaypoint.position);
        float waypointSwitchDistance = _waypointManager.waypointSwitchDistance;

        if (distanceToWaypoint < waypointSwitchDistance)
        {
            // 웨이포인트 전환 시 드리프트 상태 초기화
            _isDrifting = false;

            _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Count;
            _targetWaypoint = _waypoints[_currentWaypointIndex];
        }
    }

    //
    // AI 자동차의 움직임 및 조향 입력값 계산
    //
    private (float moveInput, float steerInput, bool isBraking, bool isDrifting) CalculateAIInput()
    {
        float moveInput = 1f;
        float steerInput = 0f;
        bool isBraking = false;

        Vector3 directionToTarget = _targetWaypoint.position - transform.position;
        directionToTarget.y = 0;
        directionToTarget.Normalize();

        // 1. 조향 계산 (Steering)
        Vector3 localTarget = transform.InverseTransformDirection(directionToTarget);
        steerInput = localTarget.x / localTarget.magnitude;
        steerInput = Mathf.Clamp(steerInput * 2f, -1f, 1f);

        // 2. 속도/제동 계산 (Acceleration/Braking)
        float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

        if (angleToTarget > maxCornerAngle)
        {
            float angleRatio = angleToTarget / cornerRatioBaseAngle;

            if (angleToTarget > hardBrakeAngle)
            {
                isBraking = true;
                moveInput = 0f;
            }
            else
            {
                moveInput = Mathf.Lerp(1f, cornerSlowDownFactor, angleRatio);
            }
        }
        else
        {
            moveInput = 1f;
        }

        // 3. 드리프트 로직 (Drifting)
        _isDrifting = ShouldDrift(directionToTarget);

        // 드리프트 중일 때는 제동을 비활성화하고 가속을 유지하도록 오버라이드
        if (_isDrifting)
        {
            isBraking = false;
            moveInput = 1f; // 드리프트 중에는 풀 가속을 시도하여 드리프트 유지
        }

        return (moveInput, steerInput, isBraking, _isDrifting);
    }

    /// <summary>
    /// 현재 목표 웨이포인트에서 드리프트가 필요한지 판단하는 로직
    /// </summary>
    private bool ShouldDrift(Vector3 directionToTarget)
    {
        // 1. 현재 목표 웨이포인트의 레이어가 '드리프트 웨이포인트 레이어'인지 확인
        bool isDriftingWaypoint = ((1 << _targetWaypoint.gameObject.layer) & _waypointManager.driftingWaypointLayer) != 0;

        if (!isDriftingWaypoint)
        {
            return false;
        }

        // 2. 드리프트 웨이포인트라면, 코너에 충분히 접근했는지 확인
        float distance = directionToTarget.magnitude;

        // 거리가 driftStartDistance 이내일 때 드리프트 시작
        if (distance < driftStartDistance)
        {
            // 코너 진입 각도가 충분히 클 때만 드리프트 시작 (너무 얕은 코너는 드리프트 불필요)
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
            if (angleToTarget > 20f) // 20도 이상 꺾인 코너에서만 드리프트
            {
                return true;
            }
        }

        return false;
    }
}