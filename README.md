# 1. 프로젝트 소개

## 1.2 개요

기간 : 2025.09~2025.10(약 한달)

인원 : 1인

## 1.2 프로젝트 소개

- 장르 : 3D 레이싱
- 목표 : 커스터마이징 가능한 레이싱 게임용 차량 물리 시뮬레이션 및 경쟁 AI 시스템 구현
- 핵심 기능 : 차량 서스펜션 시스템, 속도/조향 곡선 기반의 현실적인 주행 제어, 드리프트 매커니즘, 코너 인지 및 드리프트 AI

## 1.3 기술 스텍

- 엔진 : Unity6

## 1.4 시연 영상

https://drive.google.com/file/d/18icFjWpcsJK-4ciV6VsAh9gAEGi3apTG/view?usp=sharing

https://github.com/DDanPat/DashDrift/tree/main

---

# 2. 클라이언트 개발

## 2.1 커스텀 차량 물리 엔진

Unity에서 제공하는 `WheelCollider`  대신 Rigidbody와 Raycast를 활용하여 차량의 움직임을 직접 제어하는 커스텀 물리 엔진을 구현했습니다. 이를 통해 원하는 만큼 파라미터를 조정하여 아케이드 또는 시뮬레이션 같이 주행감을 설계할 수 있습니다.

### 2.1.1 Raycast기반 커스텀 서스펜션 시스템

```csharp
private void Suspension()
{
    for (int i = 0; i < rayPoints.Length; i++)
    {
        RaycastHit hit;
        float maxDistance = carStats.RestLength;// + springTravel;

        if (Physics.Raycast(rayPoints[i].position, -rayPoints[i].up, out hit, maxDistance + carStats.WheelRadius, groundLayer))
        {
            wheelsIsGrounded[i] = 1;

            float currentSpringLength = hit.distance - carStats.WheelRadius;
            float springCompression = carStats.RestLength - currentSpringLength / carStats.SpringTravel;

            float springVelocity = Vector3.Dot(carRB.GetPointVelocity(rayPoints[i].position), rayPoints[i].up);
            float dampForce = carStats.DamperStiffness * springVelocity;

            float springForce = springCompression * carStats.SpringStiffness;

            float netForce = springForce - dampForce;

            carRB.AddForceAtPosition(netForce * rayPoints[i].up, rayPoints[i].position);
        }
        else
        {
            wheelsIsGrounded[i] = 0;
            Debug.DrawLine(rayPoints[i].position,
                rayPoints[i].position + (carStats.WheelRadius + maxDistance) * -rayPoints[i].up, Color.green);
        }
    }
}
```

- **원리:** 각 바퀴 위치(`rayPoints`)에서 지면(`groundLayer`)으로 Raycast를 발사하여 지면과의 거리를 측정합니다.
- **물리 계산:** Raycast 히트 거리를 기반으로 스프링 압축 정도를 계산하고, 바퀴의 수직 속도(`springVelocity`)를 계산하여 **스프링 힘**(`springForce`)과 **댐퍼 힘**(`dampForce`)을 분리하여 적용합니다.
    - `springCompression * carStats.SpringStiffness` (스프링 강성)
    - `carStats.DamperStiffness * springVelocity` (댐퍼 강성)
- **효과:** 차량의 질량 중심(`carRB`)이 아닌 바퀴 위치(`rayPoints`)에 직접 힘(`netForce * rayPoints[i].up`)을 적용하여, 차량의 무게중심 이동과 안정적인 지면 접지감을 시뮬레이션했습니다.

### 2.1.2 주행 제어 메커니즘 (CarController.cs)

CarController.cs : https://github.com/DDanPat/DashDrift/blob/main/Assets/Scripts/Car/CarController.cs

| **기능** | **구현 내용** | **관련 코드** |
| --- | --- | --- |
| **가속 및 감속** | `carStats.Acceleration`을 사용하여 최대 속도(`carStats.MaxSpeed`)까지 힘을 적용하며, 최고 속도 초과 시 `linearVelocity`를 강제로 Clamp하여 속도를 제한합니다. | `Acceleration()`  |
| **제동** | `carStats.BrakeForce`를 사용하여 현재 진행 방향의 반대 방향으로 힘을 적용합니다. 전진/후진 여부를 판단하여 올바른 방향으로 제동력을 적용하는 로직을 구현했습니다. | `Brake()` |
| **조향 제어** | `carStats.TurningCurve`를 활용하여 속도에 따른 회전력 크기를 동적으로 조절했습니다. `carVelocityRatio`의 부호로 조향 방향을 조정하여 전진/후진 시에도 조작감이 일관되도록 처리했습니다. | `Turn()` |
| **측면 저항** | 차량의 로컬 측면 속도(`currentCarLocalVelocity.x`)에 비례하여 횡방향 저항(`SidewaysDrag()`)을 적용하여 슬립을 제어합니다. | `SidewaysDrag()` |

### 2.1.3 드리프트 물리 구현

드리프트는 물리 계산에 직접 영향을 주어 레이싱의 핵심인 드리프트 주행을 구현했습니다.

- 측면 저항 감소 : `SidewaysDrag()`에서 드리프트 시 `carStats.DriftDragReduction`만큼 저항 계수(`currentDragCoefficient`)를 감소시켜 차량이 쉽게 미끄러지도록 했습니다.
- 보너스 가속/회전력 : `Acceleration()` 및 `Turn()` 로직에 `carStats.DriftTorque`를 활용한 추가 가속력과 회전력을 부여하여, 드리프트 시 속도 유지와 코너링 성능을 높였습니다.
- 조향 반응 속도 : `Turn()`에서 `carStats.DriftSteerLerpSpeed`를 사용하여 드리프트 중에는 조향 입력에 대한 차량의 반응 속도를 늦춰 조작 난이도와 주행감을 조절했습니다.

---

## 2.2 레이싱 AI 및 경로 탐색 시스템

AI 차량은 FSM을 기반으로 현재 코스 상황을 인지하고 주행 전략을 동적으로 변경합니다.

### 2.2.1 AI 상태 머신 (AICarController.cs)

AI의 동작을 5가지 상태로 정의 하고, 각 상태에서 최적의 값을 계산하여 `CarController` 에 전달합니다.

AICarController.cs : https://github.com/DDanPat/DashDrift/blob/main/Assets/Scripts/Car/AI/AICarController.cs

| **상태** | **설명** | **핵심 로직** |
| --- | --- | --- |
| **Driving** | 풀 가속 상태. 코너 각도에 따라 `SlowingDown` 또는 `Drifting`으로 전환을 시도합니다. | `ExecuteDrivingState()` |
| **SlowingDown** | 급격한 코너 진입 시 감속합니다. 코너 각도(`angleToTarget`)에 따라 `moveInput`을 Lerp하여 속도를 제어합니다. | `ExecuteSlowingDownState()` |
| **Drifting** | 코너 진입 전용 드리프트 로직을 실행합니다. `isDrifting = true`를 전달하여 물리 엔진의 드리프트 모드를 활성화합니다. | `ExecuteDriftingState()` |
| **RaceReady** | 레이스 시작 대기 상태. | `GetPlayerInput(0f, 0f, true, false)` (정지 유지) |
| **RaceFinished** | 레이스가 끝났을 때 강제 정지 상태를 유지합니다. | `ExecuteRaceFinishedState()` |

### 2.2.2 코너 인지 및 감속 전략

AI는 다음 웨이포인트까지의 방향 백터와 차량의 전방방향 사이의 각도를 기준으로 코너의 위험도를 측정하고 전략을 전환합니다

```csharp
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
```

- **감속 기준 :** `angleToTarget`이 `maxCornerAngle`(`30f`) 초과 시 `SlowingDown` 상태로 전환합니다.
- **제동 기준** : `angleToTarget`이 `hardBrakeAngle`(`75f`) 초과 시, 가속을 중지하고 강제 제동(`isBraking = true`)을 시작합니다.
- **감속 비율** : `angleRatio`에 따라 가속도(`moveInput`)를 `cornerSlowDownFactor`(`0.5f`)까지 선형 보간(`Mathf.Lerp`)하여 차량 속도를 능동적으로 제어합니다.

### 2.2.3 AI 드리프트

웨이포인트 시스템과 연동하여 특정 지점에서 드리프트를 수행합니다.

```csharp
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
```

- **드리프트 웨이포인트** : `WaypointManager`의 `driftingWaypointLayer`를 사용하여 특정 웨이포인트가 드리프트가 필요한 코너임을 명시적으로 지정합니다.
- **진입 조건** :
    1. 현재 목표 웨이포인트가 드리프트 웨이포인트 레이어에 속해야 함.
    2. 차량과 웨이포인트 사이의 거리(`distance`)가 `driftStartDistance`(`20f`) 이내여야 함.
    3. 코너 각도(`angleTOTarget` )가 `minDriftAngle`(`20f`) 이상이어야 함.
- 효과 : 조건이 만족되면 `Drifting`상태로 전환하여 코너를 빠르게 통과하고, 물리 엔진으로부터 보너스 가속/회전력을 얻습니다.

---

# 3. 트러블 슈팅

## 3.1 문제

- 문제 발생 : 속도계 UI가 최고 속도에 도달하면 1의 자리 숫자가 계속해서 올라갔다 내려가는 문제 발생
- 원인 : 최고 속도에 도달하면 작용하는 힘이 멈추고 최고 속도에서 떨어지면 다시 힘을 주어 최고 속도에 도달하게 하여 발생
- 해결 방법 : 최고 속도 도달 시 클램프를 걸어 최고 속도를 유지

## 3.2 문제

- 문제 발생 :
