# 🌆 2D Platformer - Boss Fight

사이버펑크 톤의 2D 액션 플랫포머 프로토타입입니다. 상태머신 기반 보스전과 여러 종류의 적 AI, 사다리/앉아쏘기 등 캐릭터 컨트롤러를 구현했습니다.

## 🎮 Gameplay
스크린샷 추가 예정

## 📌 Features
- 사다리 등반, 앉아쏘기, 넉백/무적 프레임을 포함한 캐릭터 컨트롤러
- 잡몹 3종(드론/슬라임/터렛) — 각기 다른 이동 패턴
- 상태머신 기반 보스전 (순찰 → 예비동작 → 연속 사격 → 쿨다운 → 사망)
- 체력바 UI, 파티클 이펙트, 패럴랙스 배경

## 🛠 Tech Stack
- Unity
- C#
- Unity Input System

## ⚙️ Key Systems

### Boss State Machine
- `Entering / Patrolling / Attacking / Cooldown / Dying` 상태를 enum + switch로 관리
- 공격 전 몸 색을 바꿔 예비동작(텔레그래프)을 보여준 뒤 연속 사격 — 플레이어에게 반응할 시간을 주는 공정한 전투 설계

### Enemy 타입 분기
- 드론/슬라임/터렛을 하나의 `Enemy` 클래스에서 `EnemyType` enum으로 분기 처리
- 드론은 방향 전환 시 스프라이트를 옆모습→대각선→정면 순으로 교체해 회전 연출

### Player Controller
- 사다리 등반, 앉아쏘기, 낙사 시 리스폰/사망 분기
- 피격 시 넉백 + 무적 프레임 처리

## 🛠 Troubleshooting

### 죽은 코드 정리
**문제**: `Boss.cs`에 `StopCoroutine("Patrol")` 호출이 있었으나, `Patrol()`은 코루틴이 아닌 일반 함수라 실질적으로 아무 효과가 없는 코드였음.
**해결**: 상태 전환(switch) 자체가 이미 `Patrol()` 호출을 막아주므로, 불필요한 호출을 제거.

### 빌드 실패 위험 요소 제거
**문제**: `Player.cs`에 `UnityEditor.Callbacks`, `UnityEditor.ShaderGraph.Internal` 등 에디터 전용 네임스페이스가 참조되어 있었음. 이 네임스페이스는 실제 빌드(플레이어 빌드)에는 포함되지 않아 빌드 시 컴파일 에러를 유발할 수 있음.
**해결**: 실제로 사용되지 않는 참조였으므로 제거.

## 📝 Known Issues / Future Improvements
- 2스테이지는 1스테이지와 유사한 구조로 기획했으나, 스코프 관리 차원에서 보류
- 오브젝트 풀링 미적용 — 총알/이펙트가 자주 생성/파괴되는 구간에서 개선 여지 있음
- `Enemy` 타입 분기를 현재 enum + switch 구조에서 상속(다형성) 구조로 리팩토링 예정

## 🎨 Assets
무료 에셋 사용 (에셋 팩 출처는 추후 명시 예정)
