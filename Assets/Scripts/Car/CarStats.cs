using UnityEngine;

public class CarStats : MonoBehaviour
{
    [Header("서스펜션 설정")]
    [SerializeField] private float _springStiffness = 30000f; // 스프링 강성 
    [SerializeField] private float _damperStiffness = 3000f;  // 댐퍼 강성 
    [SerializeField] private float _restLength = 0.3f;        // 스프링 정지 길이 
    [SerializeField] private float _springTravel = 0.25f;     // 스프링 이동 거리 
    [SerializeField] private float _wheelRadius = 0.37f;      // 바퀴 반지름 

    [Header("자동차 설정")]
    [SerializeField] private float _acceleration = 20f;     // 가속도
    [SerializeField] private float _brakeForce = 15f;       // 제동력
    [SerializeField] private float _maxSpeed = 120f;        // 최대 속도
    [SerializeField] private float _deceleration = 5f;      // 감속도
    [SerializeField] private float _steelStrength = 20f;    // 조향 강도
    [SerializeField] private AnimationCurve _turningCurve;   // 속도에 따른 조향 곡선
    [SerializeField] private float _dragCoefficient = 1f;  // 공기 저항 계수

    [Header("드리프트 설정")]
    [SerializeField] private float _driftDragReduction = 0.5f; // 드리프트 시 측면 저항 감소율
    [SerializeField] private float _driftTorque = 15f;         // 드리프트 시 추가 회전력
    [SerializeField] private float _driftSteerLerpSpeed = 2f;  // 드리프트 시 조향 전환 속도

    [Header("비쥬얼/VFX 설정")]
    [SerializeField] private float _tireRotSpeed = 3000f;      // 바퀴 회전 속도
    [SerializeField] private float _maxSteerAngle = 30f;       // 최대 조향 각도
    [SerializeField] private float _minSideSkidVelocity = 10f; // 드리프트 시작 최소 측면 속도

    // 서스펜션
    public float SpringStiffness => _springStiffness;
    public float DamperStiffness => _damperStiffness;
    public float RestLength => _restLength;
    public float SpringTravel => _springTravel;
    public float WheelRadius => _wheelRadius;

    // 자동차
    public float Acceleration => _acceleration;
    public float BrakeForce => _brakeForce;
    public float MaxSpeed => _maxSpeed;
    public float Deceleration => _deceleration;
    public float SteelStrength => _steelStrength;
    public AnimationCurve TurningCurve => _turningCurve;
    public float DragCoefficient => _dragCoefficient;

    // 드리프트
    public float DriftDragReduction => _driftDragReduction;
    public float DriftTorque => _driftTorque;
    public float DriftSteerLerpSpeed => _driftSteerLerpSpeed;

    // 비쥬얼/VFX
    public float TireRotSpeed => _tireRotSpeed;
    public float MaxSteerAngle => _maxSteerAngle;
    public float MinSideSkidVelocity => _minSideSkidVelocity;
}