# Event System Rules

## 1. 데이터 구동 설계 (Data-Driven)
- **인스펙터 중심 조합**: 이벤트의 세부 매개변수(예: 처치해야 할 적의 수, 보상으로 지급할 아이템 및 수량 등)는 코드에 하드코딩하지 않습니다.
- **ScriptableObject 활용**: 모든 이벤트의 기본 템플릿과 설정 스펙은 `ScriptableObject`로 구현하여 기획자(운영자)가 인스펙터상에서 드래그 앤 드롭 및 값 수정만으로 이벤트를 설계하고 추가할 수 있도록 구현합니다.

## 2. 관심사 분리 및 Strategy 패턴
- **조건(Condition)과 보상(Reward)의 분리**: 이벤트의 진행도를 판단하는 로직과 보상을 지급하는 로직은 완전히 분리되어야 합니다.
- **IEventCondition**: 이벤트 달성 조건을 추상화합니다.
  - 예시: `KillCountCondition`, `StageClearCondition`, `AttendanceCondition`
- **IEventReward**: 지급 보상을 추상화합니다.
  - 예시: `ExpReward`, `TicketReward`, `PointReward`
- 이벤트 객체(`EventInstance`)는 `IEventCondition`과 `IEventReward`를 전략(Strategy)으로 가지고 조립되어 실행됩니다.

## 3. 확장성 확보 및 Factory Method 패턴
- **개방-폐쇄 원칙 (OCP)**: 새로운 이벤트 조건 타입이나 새로운 보상 타입이 추가되어도 기존의 `EventManager`나 핵심 로직 코드는 수정되지 않아야 합니다.
- **팩토리 위임**: 새로운 타입이 추가되면 구체 클래스(`IEventCondition` 또는 `IEventReward` 상속)만 생성하고, ScriptableObject 데이터 모델을 실제 객체로 바인딩하는 `ConditionFactory` 및 `RewardFactory`의 분기문만 추가하도록 작성합니다.
