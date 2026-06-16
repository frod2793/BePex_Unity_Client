# Event System and Pure DI Rules Setup Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 외부 아키텍처 라이브러리 사용 금지 조건에 부합하도록 VContainer 규칙 파일을 폐기하고, 수동 의존성 주입(Pure DI) 및 확장성 있는 이벤트 시스템 설계를 위한 에이전트 전용 규칙서들을 수정/신설합니다.

**Architecture:** VContainer 룰을 삭제하고 `architecture-rules.md`에 생성자 주입 원칙을 기재하며, 전략(Strategy) 및 팩토리(Factory) 패턴과 ScriptableObject를 이용한 데이터 구동 설계를 다루는 `event-system-rules.md`를 신규 생성합니다.

**Tech Stack:** Markdown (문서 관리)

---

### Task 1: VContainer Rules 파일 폐기

**Files:**
- Delete: `/.agent/rules/vcontainer-rules.md`

- [ ] **Step 1: Delete vcontainer-rules.md using git rm**

  Run:
  ```bash
  git rm .agent/rules/vcontainer-rules.md
  ```

- [ ] **Step 2: Verify file deletion**

  Verify deletion:
  `test ! -f /Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/.agent/rules/vcontainer-rules.md`
  Expected: 파일이 존재하지 않아야 합니다.

- [ ] **Step 3: Commit deletion**

  Run:
  ```bash
  git commit -m "refactor: remove VContainer rules file due to external DI library restriction"
  ```


### Task 2: Architecture Rules 수정

**Files:**
- Modify: `/.agent/rules/architecture-rules.md`

- [ ] **Step 1: Replace file content of architecture-rules.md**

  아래 내용으로 `/.agent/rules/architecture-rules.md` 전체 내용을 덮어씁니다.

  ```markdown
  # Unity Client Architecture Rules
  
  ## 1. Clean Code & POCO
  - **Decoupling**: 인게임 비즈니스 로직, 계산, 상태 관리는 `MonoBehaviour`를 상속받지 않는 일반 C# 클래스(POCO)로 작성합니다.
  - **No Singleton**: 싱글톤(Singleton) 패턴은 절대 사용하지 마십시오.
  - **ISP & DIP**: 구체적인 클래스 구현보다 인터페이스(예: `IMovable`)에 의존하도록 설계하십시오.

  ## 2. 수동 의존성 주입 (Pure DI)
  - **생성자 주입 (Constructor Injection)**: 의존성을 필요로 하는 모든 순수 C# 클래스는 필드 주입이나 프로퍼티 주입 대신, 생성자를 통해 필요한 객체나 인터페이스를 명시적으로 전달받아야 합니다.
  - **컴포지션 루트 (Composition Root)**: 씬이 시작될 때 단 하나의 MonoBehaviour 스크립트(예: `SceneInitializer` 혹은 `ProjectContext`)가 모든 매니저, 뷰모델, 컨트롤러의 인스턴스를 올바른 라이프사이클 순서에 맞게 생성하고 조립하여 의존성을 해소해 줍니다. 씬 내부의 다른 컴포넌트들이 `new`를 남발하거나 서로를 직접 찾지 않도록 제약합니다.

  ## 3. UI Architecture (MVVM)
  - **Model**: View나 Unity 엔진에 대해 전혀 모르는 순수 데이터 클래스로 작성합니다. 비즈니스 도메인 데이터와 규칙만을 담으며, UI 표현 방식에 대한 어떠한 정보도 포함하지 않습니다.
  - **ViewModel**: Model의 데이터를 View가 사용하기 좋은 형태로 가공하여 제공하며, View가 구독할 상태(State/Action)와 명령(Command)을 가집니다. ViewModel은 Model을 직접 참조하여 데이터를 읽고 가공할 수 있으나, View에 대해서는 전혀 알지 못합니다. View에서 발생한 사용자 입력은 ViewModel의 Command(public 메서드)를 호출하여 처리합니다.
  - **View**: `MonoBehaviour`를 상속하며, 오직 데이터 바인딩(시각화)과 입력 전달만 수행해야 합니다.
  - **[절대 금지] View → Model 직접 참조**: View는 어떠한 경우에도 Model 클래스를 직접 참조(import, 필드 선언, 캐스팅 등)해서는 안 됩니다. 모든 데이터와 명령은 오직 ViewModel을 통해서만 교환해야 합니다. View는 ViewModel의 상태 변경 이벤트(`Action`)를 구독하여 UI를 갱신합니다. View에서 사용자 입력이 발생하면 직접 로직을 실행하지 않고 ViewModel의 Command 메서드를 호출합니다.
  - **데이터 흐름 (단방향)**:
    `[사용자 입력] → View → ViewModel.Command() → Model 갱신 → UI 갱신 ← View ← ViewModel.StateChanged`
    이 단방향 흐름을 반드시 준수하며, View가 Model을 직접 읽거나 수정하는 양방향 흐름은 엄격히 금지합니다.
  ```

- [ ] **Step 2: Verify modification**

  Verify existence:
  `test -f /Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/.agent/rules/architecture-rules.md`
  Expected: 파일이 존재해야 합니다.

- [ ] **Step 3: Commit**

  Run:
  ```bash
  git add .agent/rules/architecture-rules.md
  git commit -m "feat: replace VContainer rules with Pure DI standard in architecture rules"
  ```


### Task 3: Event System Rules 생성

**Files:**
- Create: `/.agent/rules/event-system-rules.md`

- [ ] **Step 1: Write code content to event-system-rules.md**

  아래 텍스트 내용을 `/.agent/rules/event-system-rules.md`에 작성합니다.

  ```markdown
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
  ```

- [ ] **Step 2: Verify file existence**

  Verify existence:
  `test -f /Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/.agent/rules/event-system-rules.md`
  Expected: 파일이 존재해야 합니다.

- [ ] **Step 3: Commit**

  Run:
  ```bash
  git add .agent/rules/event-system-rules.md
  git commit -m "feat: add event system rules for Strategy and Factory patterns"
  ```


### Task 4: Architecture Guide 수정

**Files:**
- Modify: `/.agent/docs/architecture-guide.md`

- [ ] **Step 1: Replace file content of architecture-guide.md**

  아래 내용으로 `/.agent/docs/architecture-guide.md` 전체 내용을 덮어씁니다.

  ```markdown
  # BePex Unity Client Architecture Guide
  
  ## 1. Directory Structure (디렉토리 구조)
  - `Assets/` 아래 핵심 구조는 관심사 분리(SoC)를 보장하기 위해 다음과 같이 계층을 이룹니다:
    - `/Models/`: 순수 C# 데이터 정의 및 비즈니스 규칙 도메인
    - `/ViewModels/`: 상태와 상태 변화 Action, View의 상호작용 명령 바인딩 레이어
    - `/Views/`: MonoBehaviour 기반 컴포넌트, UI 및 오브젝트 드로우, Input 전달만 처리
    - `/Initializers/`: 씬 진입점(Composition Root)으로 수동으로 모든 의존성을 조립하는 MonoBehaviour 파일들
    - `/DTOs/`: 씬 간 상태 이전용 Data Transfer Object 모음

  ## 2. Pure DI (수동 의존성 주입) 아키텍처 흐름
  - 외부 DI 컨테이너 라이브러리 사용이 불가능하므로, 아래와 같이 씬의 `SceneInitializer`에서 일괄 생성 및 생성자 주입을 해소합니다:
    ```
    [Unity Awake / Start]
              ↓
    [SceneInitializer.Initialize()]
              ↓
    Create Model instances (e.g. SaveSystem, EventModel)
              ↓
    Create ViewModel instances (e.g. EventListViewModel(EventModel, SaveSystem))
              ↓
    Inject ViewModels to Views in Hierarchy (Property Assignment / Method)
    ```

  ## 3. 씬 전환 및 데이터 전송 (DTO)
  - 씬 전환 시 유지해야 할 데이터는 전역 의존성(싱글톤 등)을 배제하고, 반드시 순수 데이터 클래스인 DTO(Data Transfer Object)로 캡슐화합니다.
  - DTO는 씬 로더나 컨텍스트 매니저를 통해 다음 씬의 초기화(Initializer) 클래스로 직접 주입되어야 합니다.
  - DTO 클래스의 네이밍은 `~DTO` 접미사를 필수로 붙여야 합니다. (예: `LobbyStateDTO`, `InGameConfigDTO`)
  ```

- [ ] **Step 2: Verify modification**

  Verify existence:
  `test -f /Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/.agent/docs/architecture-guide.md`
  Expected: 파일이 존재해야 합니다.

- [ ] **Step 3: Commit**

  Run:
  ```bash
  git add .agent/docs/architecture-guide.md
  git commit -m "feat: replace VContainer layout with Pure DI layout in architecture guide"
  ```
