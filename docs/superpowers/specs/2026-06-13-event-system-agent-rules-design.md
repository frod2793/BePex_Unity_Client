# Spec: Event System & Pure DI Agent Rules Setup

## 1. 개요
이 문서는 BePex Unity Client 프로그래머 사전 과제의 제약 조건(외부 아키텍처 및 DI 라이브러리 사용 금지, 확장성 확보, 데이터-로직 분리)에 부합하도록 기존 에이전트 규칙 베이스(`/.agent/`)를 수정 및 신설하기 위한 기획 사양서입니다.

## 2. 요구사항 및 목표
- 외부 DI 라이브러리인 VContainer 관련 규칙 파일(`vcontainer-rules.md`)을 완전히 삭제합니다.
- `architecture-rules.md`를 수정하여 수동 의존성 주입(Pure DI) 표준을 수립합니다.
- `event-system-rules.md`를 신설하여 이벤트 시스템 개발을 위한 디자인 패턴(Strategy, Factory Method) 및 ScriptableObject 데이터 구동 설계 표준을 명문화합니다.
- `architecture-guide.md` 문서를 보완하여 Pure DI 레이아웃과 데이터 흐름 다이어그램을 기술합니다.

## 3. 디렉토리 구조 변경 명세
```
/.agent/
├── rules/
│   ├── architecture-rules.md  # [수정] Pure DI 수동 생성자 주입 원칙 명세
│   ├── event-system-rules.md  # [NEW] 조건/보상 분리, SO 데이터 구동 및 패턴 적용 명세
│   └── safety-rules.md        # [유지] 코딩 스타일, Unity Safety 표준
└── docs/
    └── architecture-guide.md  # [수정] 수동 DI 씬 진입 구조(Composition Root) 명세
```

## 4. 상세 파일 설계

### 4.1. [삭제] rules/vcontainer-rules.md
- 외부 DI 프레임워크 사용 금지 조항에 따라 해당 규칙 파일을 완전히 폐기합니다.

### 4.2. [수정] rules/architecture-rules.md
- **Pure DI (수동 생성자 주입)**: 
  - 싱글톤 패턴은 금지하며, 의존성을 가진 순수 C# 객체는 오직 생성자 주입(Constructor Injection)을 통해서만 필요한 인터페이스나 객체를 주입받아야 합니다.
  - 씬 진입 시 단 하나의 MonoBehaviour 초기화 클래스(Composition Root, 예: `SceneInitializer` 혹은 `ProjectContext`)가 모든 매니저와 뷰모델, 컨트롤러의 인스턴스를 순서대로 생성하고 조립하도록 강제합니다.

### 4.3. [NEW] rules/event-system-rules.md
- **데이터 구동 설계 (Data-Driven)**: 
  - 이벤트 조건 사양과 보상 정보는 코드에 하드코딩하지 않고, `ScriptableObject` 상속 객체로 만들어 인스펙터 상에서 조합할 수 있도록 설계합니다.
- **관심사 분리 (Strategy 패턴)**:
  - 이벤트 진행 검증 조건은 `IEventCondition` 인터페이스를 구현하는 개별 클래스로 쪼갭니다 (예: `KillCountCondition`, `StageClearCondition`).
  - 보상 지급 처리는 `IEventReward` 인터페이스를 구현하는 개별 클래스로 분리합니다 (예: `ExpReward`, `TicketReward`).
- **확장성 확보 (Factory Method 패턴)**:
  - 새로운 이벤트 타입이나 보상 타입이 생기면 기존 매니저나 이벤트 제어 클래스를 수정하는 대신, 새로운 `IEventCondition` 혹은 `IEventReward` 구현체만 추가하고 이를 인스턴스화하는 팩토리 클래스(`ConditionFactory`, `RewardFactory`)에 분기만 추가하도록 합니다.

### 4.4. [수정] docs/architecture-guide.md
- **Pure DI 씬 진입부 구조 설명**: 
  - 외부 프레임워크가 없을 때 씬 진입 시 의존성 해소를 위해 컴포지션 루트 클래스가 인스턴스들을 연결하는 순서와 흐름을 텍스트 다이어그램으로 명시합니다.
  - 저장 시스템(PlayerPrefs/JSON)과 런타임 도메인 간의 의존 구조를 명시합니다.

## 5. 검증 계획
- `/.agent/rules/vcontainer-rules.md` 파일이 존재하지 않는지 검사합니다.
- `/.agent/rules/event-system-rules.md` 파일이 정상 생성되었는지 확인합니다.
- 수정된 `architecture-rules.md` 및 `architecture-guide.md`가 Pure DI 사양을 완전하게 기술하는지 확인합니다.
