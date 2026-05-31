# TheGameBlur

**2.5D 사이드뷰 액션 게임**입니다.  
3D 오브젝트와 3D 물리를 사용하되, 플레이어와 전투 흐름은 고정된 Z 평면 위에서 진행되도록 구성했습니다. 플레이어는 이동, 점프, 대시, 기본 공격, 스킬, 주사위 기반 궁극기 시스템을 활용해 적과 전투합니다.

## 게임 개요

| 항목 | 내용 |
| --- | --- |
| 장르 | 2.5D 사이드뷰 액션 |
| 핵심 조작 | 이동, 달리기, 점프, 대시, 기본 공격, 스킬, 주사위 스킬, 회복, 상호작용 |
| 전투 방식 | 마우스 방향 기반 근접 공격, 공중 공격, 스킬, 주사위 게이지 기반 궁극기 |
| 핵심 특징 | 3D 물리 기반 사이드뷰 이동, Z축 고정, 마우스 방향 전투, 주사위 확률/게이지 시스템, 전투 상태 기반 UI/능력 관리 |
| 개발 환경 | Unity 6, C# |
| 주요 구현 범위 | 플레이어 구조, 이동/대시/점프, 전투 판정, 스킬 구조, 주사위 시스템, 플레이어 스탯, 카메라, 입력 관리, 적 AI 연동 |

## 핵심 기획

TheGameBlur는 단순한 2D 스프라이트 액션이 아니라, **3D 공간을 활용하면서 플레이 감각은 사이드뷰 액션에 가깝게 제한한 2.5D 액션 게임**입니다.

플레이어는 3D `Rigidbody`를 사용하지만, Z축 위치를 고정하고 X/Y 축 중심으로 이동합니다. 이를 통해 3D 모델, 이펙트, 카메라 연출을 활용하면서도 조작과 전투는 사이드스크롤 액션처럼 읽히도록 설계했습니다.

전투는 마우스 방향을 기준으로 공격 방향을 결정하고, 기본 공격과 공중 공격, 스킬, 주사위 게이지 기반 궁극기로 확장됩니다. 특히 전투 상태에 진입하면 주사위가 굴러가며, 주사위 결과가 회피 확률, 치명타 확률, 스킬 크기 등에 영향을 주는 구조를 갖습니다.

## 플레이 루프

```text
필드 이동
  ↓
적 감지 / 전투 상태 진입
  ↓
이동, 점프, 대시로 위치 조정
  ↓
마우스 방향 기반 기본 공격
  ↓
스킬 사용 / MP 소비
  ↓
전투 중 주사위 굴림 및 게이지 축적
  ↓
궁극기 조건 충족
  ↓
궁극기 사용
  ↓
적 처치 / 다음 구간 진행
```

## 주요 시스템

### 1. 플레이어 통합 구조

플레이어는 `Player`를 중심으로 이동, 전투, 스탯, UI 모듈을 분리해 참조하는 구조입니다.

주요 파일:

```text
Assets/_Project/Runtime/Features/Player/Scripts/Controller/Player.cs
Assets/_Project/Runtime/Features/Player/Scripts/Controller/PlayerMovement.cs
Assets/_Project/Runtime/Features/Player/Scripts/Controller/PlayerCombat.cs
Assets/_Project/Runtime/Features/Player/Scripts/Controller/PlayerStats.cs
```

`Player`는 플레이어의 중심 허브 역할을 합니다.

- `PlayerMovement`: 이동, 점프, 대시, 방향 전환
- `PlayerCombat`: 기본 공격, 공중 공격, 스킬, 궁극기
- `PlayerStats`: HP, MP, 주사위, 전투 상태, 무적 상태
- `PlayerUI`: 플레이어 능력 상태와 회복 UI 연동

이 구조를 통해 입력, 이동, 전투, 스탯 처리를 하나의 거대한 컨트롤러에 몰아넣지 않고 기능 단위로 분리했습니다.

### 2. 2.5D 이동 시스템

플레이어는 3D `Rigidbody`를 사용하지만, Z축을 고정해 사이드뷰 액션처럼 움직입니다.

주요 특징:

- Z축 위치 고정
- X축 이동
- Y축 점프/낙하
- 지상 가속과 공중 가속 분리
- 코요테 타임 적용
- 점프 버튼 유지 시간에 따른 점프 높이 변화
- 지상 대시와 공중 대시 처리
- 대시 중 무적 처리
- 이동 중 발소리와 먼지 파티클 연동

주요 파일:

```text
Assets/_Project/Runtime/Features/Player/Scripts/Controller/PlayerMovement.cs
Assets/_Project/Runtime/Features/Player/Scripts/Controller/PlayerSettings.cs
```

이동 시스템은 단순히 좌우 이동만 처리하는 것이 아니라, 전투 액션과 충돌하지 않도록 설계되어 있습니다. 예를 들어 스킬 또는 궁극기 사용 중에는 이동 물리를 제한하고, 대시 시작 시 기본 공격 상태를 취소하며, 공중 공격 성공 시 공중 대시 사용 가능 상태를 회복합니다.

### 3. 마우스 방향 기반 전투

기본 공격은 플레이어의 위치와 마우스 월드 좌표를 기준으로 방향을 계산합니다.  
지상 공격은 마우스 방향으로 공격 중심점을 잡고, 해당 범위 안의 `IDamageable` 대상에게 피해를 줍니다.

공중 공격은 플레이어 주변 범위를 판정하며, 명중 시 다음 행동으로 이어질 수 있도록 공중 공격 사용 상태를 초기화하고 포고 바운스를 적용합니다.

주요 파일:

```text
Assets/_Project/Runtime/Features/Player/Scripts/Controller/PlayerCombat.cs
```

공격 판정의 주요 흐름:

```text
공격 입력
  ↓
마우스 월드 좌표 계산
  ↓
공격 방향 계산
  ↓
OverlapSphereNonAlloc으로 타격 대상 검색
  ↓
IDamageable 대상 필터링
  ↓
피해 적용
  ↓
치명타 여부에 따라 이펙트/사운드 분기
```

### 4. 스킬 시스템

스킬은 `ScriptableObject` 기반의 추상 클래스 `PlayerSkill`을 상속해 구현합니다.  
스킬마다 아이콘, MP 소모량, 락 지속 시간, 쿨타임을 가질 수 있으며, 실제 동작은 `Execute`에서 정의합니다.

주요 파일:

```text
Assets/_Project/Runtime/Features/Player/Scripts/PlayerSkill/PlayerSkill.cs
Assets/_Project/Runtime/Features/Player/Scripts/PlayerSkill/Skill/FireballPlayerSkill.cs
Assets/_Project/Runtime/Features/Player/Scripts/PlayerSkill/Skill/FireballProjectile.cs
```

예시 스킬인 Fireball은 마우스 방향으로 투사체를 생성합니다.  
주사위 값에 따라 스킬 크기가 달라지고, 치명타 확률 판정을 통해 최종 피해량이 달라질 수 있습니다.

### 5. 주사위 / 궁극기 시스템

TheGameBlur의 전투는 주사위 시스템과 연결됩니다.  
전투 상태에 진입하면 일정 시간마다 주사위가 굴러가며, 주사위 결과는 전투 확률과 스킬 성능에 영향을 줍니다.

주요 요소:

| 요소 | 설명 |
| --- | --- |
| Dice Value | 두 주사위 값의 합 |
| Dice Gauge | 전투 중 주사위가 굴러갈 때 누적되는 궁극기 게이지 |
| Critical Chance | 주사위 값에 따라 공격 치명타 확률에 영향 |
| Dodge Chance | 주사위 값에 따라 피격 회피 확률에 영향 |
| Skill Size | 주사위 값에 따라 일부 스킬 크기에 영향 |
| Ultimate | 게이지가 가득 찼을 때 사용 가능한 주사위 기반 능력 |

주요 파일:

```text
Assets/_Project/Runtime/Features/Player/Scripts/Controller/PlayerStats.cs
Assets/_Project/Runtime/Features/Player/Scripts/PlayerSkill/PlayerUltimate.cs
Assets/_Project/Runtime/Features/Player/Scripts/PlayerSkill/Ultimate/BasicPlayerUltimate.cs
```

궁극기는 `PlayerUltimate`을 상속하는 구조이며, 게이지 최대치와 주사위 프리팹, 고정 주사위 사용 여부 등을 정의할 수 있습니다.

### 6. 전투 상태 감지

플레이어는 주변에 전투 대상이 있거나, 플레이어가 공격/스킬/궁극기 같은 전투 행동을 수행 중이면 전투 상태로 진입합니다.

전투 상태는 다음 기능과 연결됩니다.

- 주사위 굴림 시작
- 주사위 게이지 축적
- 전투 UI 표시 조건
- 마우스 방향 기준 바라보기 처리
- 능력 사용 가능 상태 관리

주요 파일:

```text
Assets/_Project/Runtime/Features/Player/Scripts/Controller/Player.cs
Assets/_Project/Runtime/Features/Player/Scripts/Controller/PlayerStats.cs
```

### 7. 사이드뷰 카메라

카메라는 플레이어를 따라가되, 사이드뷰 액션에 맞게 바라보는 방향과 상황에 따라 오프셋을 조정합니다.

주요 기능:

- 플레이어 방향 기준 Look Ahead
- 아래 방향 입력 유지 시 Look Down
- 절벽 감지 시 하단 시야 확보
- 카메라 경계 클램프
- 카메라 락
- 화면 흔들림
- DOTween 기반 부드러운 오프셋 전환

주요 파일:

```text
Assets/_Project/Runtime/Features/Player/Scripts/Camara/SideScrollerCameraController.cs
```

### 8. 입력 관리

입력은 Unity Input System 기반의 `InputManager`에서 통합적으로 관리합니다.

지원 입력:

- 이동
- 달리기
- 점프
- 대시
- 기본 공격
- 스킬
- 주사위 스킬
- 회복
- 상호작용
- 맵
- ESC

주요 파일:

```text
Assets/_Project/Runtime/Systems/InputSystem/InputManager.cs
```

### 9. 적 AI 및 피해 인터페이스

적은 `IDamageable` 인터페이스를 통해 플레이어의 공격과 스킬 피해를 받을 수 있습니다.  
일반 적은 대기, 이동, 추적, 공격 상태를 가지며, 감지 대상이 생기면 추적 후 공격 범위 안에서 공격을 수행합니다.

주요 파일:

```text
Assets/_Project/Runtime/Features/Enemy/Scripts/Normal/EnemyScript.cs
```

적 AI 흐름:

```text
대기 / 순찰
  ↓
플레이어 감지
  ↓
어그로 획득
  ↓
추적
  ↓
공격 범위 진입
  ↓
공격 실행
  ↓
피해 누적
  ↓
사망 처리
```

## 프로젝트 구조

```text
Assets/
├─ _Project/
│  ├─ Runtime/
│  │  ├─ Core/
│  │  ├─ Features/
│  │  │  ├─ Player/
│  │  │  ├─ Enemy/
│  │  │  ├─ Cinematic/
│  │  │  ├─ ElectricChair/
│  │  │  ├─ Prolog/
│  │  │  └─ Tutorial/
│  │  ├─ Systems/
│  │  │  ├─ AudioSystem/
│  │  │  └─ InputSystem/
│  │  └─ UI/
│  └─ Scenes/
├─ Plugins/
├─ MagicaCloth2/
├─ ShaderLab/
└─ ProjectSettings/
```

## 기술 스택

| 분류 | 사용 기술 |
| --- | --- |
| Engine | Unity 6 |
| Language | C# |
| Physics | Unity 3D Physics |
| Input | Unity Input System |
| Camera | Orthographic Side Scroller Camera |
| VFX | Unity Visual Effect / Particle System |
| Tweening | DOTween Pro |
| Rendering | HDRP/ShaderLab 기반 프로젝트 리소스 포함 |
| Tools | Odin Inspector, Magica Cloth 2, DOTween Pro |

## 구현 의도

TheGameBlur의 플레이어 구조는 **3D 기반 프로젝트에서 사이드뷰 액션의 조작성과 전투 가독성을 유지하는 것**을 목표로 설계했습니다.

일반적인 3D 이동처럼 자유로운 Z축 이동을 허용하면 사이드뷰 전투의 판정과 카메라 구성이 복잡해지기 때문에, 플레이어 위치를 `planeZ` 기준으로 고정하고 X/Y 축 중심으로만 제어했습니다. 대신 3D 모델과 VFX, 카메라 연출은 유지하여 2D 액션보다 입체적인 화면 구성이 가능하도록 했습니다.

전투는 마우스 방향을 기준으로 공격 방향을 결정해, 단순히 바라보는 방향으로만 공격하는 방식보다 더 유연하게 만들었습니다. 또한 공중 공격 명중 시 포고 바운스와 공중 대시 회복을 연결해, 공격 성공이 곧 이동 리듬으로 이어지도록 구성했습니다.

주사위 시스템은 전투의 변수를 만드는 장치입니다. 단순히 데미지를 올리는 능력치가 아니라, 치명타, 회피, 스킬 크기, 궁극기 게이지와 연결되어 전투 중 계속 변화하는 리듬을 만들도록 설계했습니다.

## 주요 구현 포인트

- 3D 물리 기반 2.5D 사이드뷰 플레이어 제어
- Z축 고정과 `planeZ` 기준 마우스 월드 좌표 계산
- 지상/공중 이동 로직 분리
- 대시 중 무적 및 공중 대시 제한
- 마우스 방향 기반 근접 공격
- 공중 공격 명중 시 포고 바운스와 공중 대시 회복
- `ScriptableObject` 기반 스킬/궁극기 구조
- 주사위 값 기반 치명타, 회피, 스킬 크기, 궁극기 게이지 시스템
- 전투 상태 감지와 주사위 시스템 연동
- 사이드뷰 카메라 Look Ahead, Look Down, 절벽 감지, 화면 흔들림
- `IDamageable` 기반 플레이어 공격과 적 피해 처리
