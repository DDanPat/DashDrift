using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CarController))]
public class AICarController : MonoBehaviour
{
    // AI의 현재 동작 상태를 정의하는 Enum
    private enum AICarState
    {
        RaceReady,
        Driving,
        SlowingDown,
        Drifting,
        RaceFinished // 레이스가 끝났을 때 정지 상태
    }

    [Header("AI 설정")]
    [SerializeField] private float maxCornerAngle = 30f;            // 급격한 코너 판단 기준 각도 (Driving -> SlowingDown/Braking 전환 기준)
    [SerializeField] private float cornerSlowDownFactor = 0.5f;    // 코너에서 가속도를 줄이는 비율
    [SerializeField] private float driftStartDistance = 20f;        // 드리프트를 시작할 목표 웨이포인트까지의 거리
    [SerializeField] private float minDriftAngle = 20f;             // 드리프트를 시작하기 위한 최소 코너 각도 (20도 이상)
    [SerializeField] private float minDrivingAngle = 10f;           // SlowingDown -> Driving 전환 기준 각도

    [Header("코너 감속 기준")]
    [SerializeField] private float cornerRatioBaseAngle = 90f;    // 코너 비율 계산의 기준 각도
    [SerializeField] private float hardBrakeAngle = 75f;            // 이 각도 초과 시 강제 제동 시작

    private CarController _carController;
    private WaypointManager _waypointManager;
    private List<Transform> _waypoints;
    private int _currentWaypointIndex = 0;
    private Transform _targetWaypoint;

    // 상태 변수
    private AICarState _currentState = AICarState.Driving;
    private bool _isDrifting = false;

    private void Start()
    {
        _carController = GetComponent<CarController>();
        // WaypointManager가 static Instance를 가지고 있다고 가정합니다.
        _waypointManager = WaypointManager.Instance;

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

        // 초기 웨이포인트 설정
        _targetWaypoint = _waypoints[_currentWaypointIndex];
        // 초기 상태 설정
        TransitionToState(AICarState.RaceReady);
    }

    private void FixedUpdate()
    {
        // 레이스 종료 상태일 때는 웨이포인트 체크 및 추가 로직 수행을 건너뜁니다.
        if (_currentState != AICarState.RaceFinished)
        {
            CheckWaypointProgress();
        }

        if (_targetWaypoint == null && _currentState != AICarState.RaceFinished) return;

        // **(추가)** RaceReady 상태에서는 입력 처리를 건너뜁니다.
        if (_currentState == AICarState.RaceReady)
        {
            // 준비 상태에서는 0 입력만 반환합니다.
            _carController.GetPlayerInput(0f, 0f, true, false); // 브레이크를 잡아 정지 상태 유지
            return;
        }

        // 현재 상태에 따른 AI 로직 실행
        (float moveInput, float steerInput, bool isBraking, bool isDrifting) = HandleState();

        // CarController에 최종 입력 전달
        _carController.GetPlayerInput(moveInput, steerInput, isBraking, isDrifting);
    }

    //
    // 상태 전환 및 관리
    //
    private void TransitionToState(AICarState newState)
    {
        // Debug.Log($"AI Transition: {_currentState} -> {newState}"); // 디버깅용
        _currentState = newState;
        // 상태 전환 시 드리프트 플래그 초기화 (Drifting 상태에서만 true)
        if (newState != AICarState.Drifting)
        {
            _isDrifting = false;
        }
    }
    public void RaceStart()
    {
        if (_currentState == AICarState.RaceReady)
        {
            TransitionToState(AICarState.Driving);
        }
    }

    private (float moveInput, float steerInput, bool isBraking, bool isDrifting) HandleState()
    {
        switch (_currentState)
        {
            case AICarState.Driving:
                return ExecuteDrivingState();

            case AICarState.SlowingDown:
                return ExecuteSlowingDownState();

            case AICarState.Drifting:
                return ExecuteDriftingState();

            case AICarState.RaceFinished:
                return ExecuteRaceFinishedState(); // 레이스 종료 상태 처리

            default:
                // 예상치 못한 상태일 경우 기본 Driving 상태로 처리
                TransitionToState(AICarState.Driving);
                return ExecuteDrivingState();
        }
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
            // 웨이포인트 전환 시 드리프트 상태 및 현재 상태 초기화
            _isDrifting = false;
            TransitionToState(AICarState.Driving);

            _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Count;
            _targetWaypoint = _waypoints[_currentWaypointIndex];
        }
    }


    //
    // AI 상태별 실행 로직
    //

    // ----------------------------------------------------
    // Driving State: 풀 가속 및 경로 추적
    // ----------------------------------------------------
    private (float moveInput, float steerInput, bool isBraking, bool isDrifting) ExecuteDrivingState()
    {
        Vector3 directionToTarget = GetTargetDirection();
        float angleToTarget = GetTargetAngle(directionToTarget);

        // 1. 상태 전환 조건 확인
        if (ShouldDrift(directionToTarget, angleToTarget))
        {
            TransitionToState(AICarState.Drifting);
            return ExecuteDriftingState(); // 즉시 드리프트 로직 실행
        }
        else if (angleToTarget > maxCornerAngle)
        {
            TransitionToState(AICarState.SlowingDown);
            return ExecuteSlowingDownState(); // 즉시 감속 로직 실행
        }

        // 2. Driving 실행 (풀 가속)
        float moveInput = 1f;
        float steerInput = CalculateSteerInput(directionToTarget);
        bool isBraking = false;

        return (moveInput, steerInput, isBraking, false);
    }

    // ----------------------------------------------------
    // SlowingDown State: 코너 진입 시 감속 또는 제동
    // ----------------------------------------------------
    private (float moveInput, float steerInput, bool isBraking, bool isDrifting) ExecuteSlowingDownState()
    {
        Vector3 directionToTarget = GetTargetDirection();
        float angleToTarget = GetTargetAngle(directionToTarget);

        // 1. 상태 전환 조건 확인 (코너를 충분히 돌았을 경우 Driving으로 전환)
        if (angleToTarget < minDrivingAngle)
        {
            TransitionToState(AICarState.Driving);
            return ExecuteDrivingState(); // 즉시 Driving 로직 실행
        }

        // 2. SlowingDown 실행 (감속/제동)
        float moveInput = 1f;
        bool isBraking = false;

        // 각도에 따른 감속/제동 계산
        float angleRatio = angleToTarget / cornerRatioBaseAngle;

        if (angleToTarget > hardBrakeAngle)
        {
            isBraking = true;
            moveInput = 0f; // 제동 시 가속 중지
        }
        else
        {
            // 각도 비율에 따라 가속도를 Lerp하여 감속
            moveInput = Mathf.Lerp(1f, cornerSlowDownFactor, angleRatio);
        }

        float steerInput = CalculateSteerInput(directionToTarget);

        return (moveInput, steerInput, isBraking, false);
    }

    // ----------------------------------------------------
    // Drifting State: 드리프트 실행
    // ----------------------------------------------------
    private (float moveInput, float steerInput, bool isBraking, bool isDrifting) ExecuteDriftingState()
    {
        Vector3 directionToTarget = GetTargetDirection();

        // 드리프트 조건이 더 이상 충족되지 않으면 Driving으로 복귀
        if (!ShouldDrift(directionToTarget, GetTargetAngle(directionToTarget)))
        {
            TransitionToState(AICarState.Driving);
            return ExecuteDrivingState(); // 즉시 Driving 로직 실행
        }

        // 1. Drifting 실행 (풀 가속, 드리프트 플래그 ON)
        float moveInput = 1f;
        float steerInput = CalculateSteerInput(directionToTarget);
        bool isBraking = false;
        _isDrifting = true; // 드리프트 플래그 설정

        return (moveInput, steerInput, isBraking, _isDrifting);
    }

    // ----------------------------------------------------
    // RaceFinished State: 레이스가 끝났을 때 정지
    // ----------------------------------------------------
    private (float moveInput, float steerInput, bool isBraking, bool isDrifting) ExecuteRaceFinishedState()
    {
        // 정지 상태 유지 (강제 제동)
        return (0f, 0f, true, false);
    }


    //
    // 공통 헬퍼 메서드
    //

    /// <summary>
    /// 현재 위치에서 목표 웨이포인트까지의 방향 벡터를 계산합니다.
    /// </summary>
    private Vector3 GetTargetDirection()
    {
        Vector3 directionToTarget = _targetWaypoint.position - transform.position;
        directionToTarget.y = 0;
        return directionToTarget.normalized;
    }

    /// <summary>
    /// 자동차의 전방 방향과 목표 방향 사이의 각도를 계산합니다.
    /// </summary>
    private float GetTargetAngle(Vector3 directionToTarget)
    {
        return Vector3.Angle(transform.forward, directionToTarget);
    }

    /// <summary>
    /// 목표 방향으로 조향 입력을 계산합니다.
    /// </summary>
    private float CalculateSteerInput(Vector3 directionToTarget)
    {
        // 로컬 좌표계에서 타겟 방향을 계산
        Vector3 localTarget = transform.InverseTransformDirection(directionToTarget);

        // 조향 입력은 로컬 x축 위치(좌우 편차)에 비례
        float steerInput = localTarget.x / localTarget.magnitude;

        // 조향 입력값 클램프 및 민감도 조절 (2f)
        return Mathf.Clamp(steerInput * 2f, -1f, 1f);
    }

    /// <summary>
    /// 현재 목표 웨이포인트에서 드리프트가 필요한지 판단하는 로직
    /// </summary>
    private bool ShouldDrift(Vector3 directionToTarget, float angleToTarget)
    {
        // 1. 현재 목표 웨이포인트의 레이어가 '드리프트 웨이포인트 레이어'인지 확인
        bool isDriftingWaypoint = ((1 << _targetWaypoint.gameObject.layer) & _waypointManager.driftingWaypointLayer) != 0;

        if (!isDriftingWaypoint)
        {
            return false;
        }

        // 2. 드리프트 웨이포인트라면, 코너에 충분히 접근했는지 확인
        // directionToTarget은 normalized 벡터이므로, 재계산이 필요합니다.
        float distance = Vector3.Distance(transform.position, _targetWaypoint.position);

        // 거리가 driftStartDistance 이내일 때 and 코너 각도가 충분히 클 때 드리프트 시작
        if (distance < driftStartDistance)
        {
            if (angleToTarget > minDriftAngle)
            {
                return true;
            }
        }

        return false;
    }

    //
    // 레이스가 끝났을 때 AI 자동차를 멈추게 하는 메서드
    //
    public void StopAICar()
    {
        // 외부에서 이 메서드가 호출되면 RaceFinished 상태로 전환합니다.
        // 실제 정지 입력은 FixedUpdate의 ExecuteRaceFinishedState에서 처리됩니다.
        if (_currentState != AICarState.RaceFinished)
        {
            TransitionToState(AICarState.RaceFinished);
        }
        // FixedUpdate는 계속 실행되지만, RaceFinished 상태가 정지 입력을 지속적으로 보냅니다.
    }
}
