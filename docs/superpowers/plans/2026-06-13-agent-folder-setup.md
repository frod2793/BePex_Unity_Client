# Agent Folder and Rules Setup Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** BePex Unity Client 프로젝트에 독립된 에이전트 전용 규칙 베이스 및 문서 폴더(`/.agent/`)를 설정하고 핵심 규칙서와 가이드 문서를 생성합니다.

**Architecture:** 숨겨진 디렉토리 구조인 `/.agent/` 최상위 아래 `rules/` 및 `docs/` 폴더를 생성하고 도메인별 마크다운 파일로 관심사를 명확히 분리하여 규칙을 정의합니다.

**Tech Stack:** Markdown (문서 관리)

---

### Task 1: Architecture Rules 생성

**Files:**
- Create: `/.agent/rules/architecture-rules.md`

- [ ] **Step 1: Write code content to architecture-rules.md**

  아래 텍스트 내용을 `/.agent/rules/architecture-rules.md`에 작성합니다.

  ```markdown
  # Unity Client Architecture Rules
  
  ## 1. Clean Code & POCO
  - **Decoupling**: 인게임 비즈니스 로직, 계산, 상태 관리는 `MonoBehaviour`를 상속받지 않는 일반 C# 클래스(POCO)로 작성합니다.
  - **No Singleton**: 싱글톤(Singleton) 패턴은 절대 사용하지 마십시오.
  - **Dependency Injection**: 로직 클래스의 의존성은 '프로퍼티 주입(Property Injection)'을 기본으로 합니다. (VContainer 라이브러리 사용을 가정하거나 인터페이스를 통해 주입)
  - **ISP & DIP**: 구체적인 클래스 구현보다 인터페이스(예: `IMovable`)에 의존하도록 설계하십시오.

  ## 2. UI Architecture (MVVM)
  - **Model**: View나 Unity 엔진에 대해 전혀 모르는 순수 데이터 클래스로 작성합니다. 비즈니스 도메인 데이터와 규칙만을 담으며, UI 표현 방식에 대한 어떠한 정보도 포함하지 않습니다.
  - **ViewModel**: Model의 데이터를 View가 사용하기 좋은 형태로 가공하여 제공하며, View가 구독할 상태(State/Action)와 명령(Command)을 가집니다. ViewModel은 Model을 직접 참조하여 데이터를 읽고 가공할 수 있으나, View에 대해서는 전혀 알지 못합니다. View에서 발생한 사용자 입력은 ViewModel의 Command(public 메서드)를 호출하여 처리합니다.
  - **View**: `MonoBehaviour`를 상속하며, 오직 데이터 바인딩(시각화)과 입력 전달만 수행해야 합니다.
  - **[절대 금지] View → Model 직접 참조**: View는 어떠한 경우에도 Model 클래스를 직접 참조(import, 필드 선언, 캐스팅 등)해서는 안 됩니다. 모든 데이터와 명령은 오직 ViewModel을 통해서만 교환해야 합니다. View는 ViewModel의 상태 변경 이벤트(`Action`)를 구독하여 UI를 갱신합니다. View에서 사용자 입력이 발생하면 직접 로직을 실행하지 않고 ViewModel의 Command 메서드를 호출합니다.
  - **데이터 흐름 (단방향)**:
    `[사용자 입력] → View → ViewModel.Command() → Model 갱신 → UI 갱신 ← View ← ViewModel.StateChanged`
    이 단방향 흐름을 반드시 준수하며, View가 Model을 직접 읽거나 수정하는 양방향 흐름은 엄격히 금지합니다.
  ```

- [ ] **Step 2: Verify file existence**

  Verify existence:
  `test -f /Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/.agent/rules/architecture-rules.md`
  Expected: 파일이 존재하며 에러 없이 완료되어야 합니다.

- [ ] **Step 3: Commit**

  Run:
  ```bash
  git add .agent/rules/architecture-rules.md
  git commit -m "feat: add architecture-rules for MVVM and Clean Code"
  ```


### Task 2: VContainer Rules 생성

**Files:**
- Create: `/.agent/rules/vcontainer-rules.md`

- [ ] **Step 1: Write code content to vcontainer-rules.md**

  아래 텍스트 내용을 `/.agent/rules/vcontainer-rules.md`에 작성합니다.

  ```markdown
  # VContainer Rules
  
  ## 1. LifetimeScope
  - 씬 당 하나의 LifetimeScope를 배치하고, 기능 영역별로 `private` 메서드(`ConfigureXxx`)를 분리하여 `Configure` 내에서 호출합니다.
  
  ## 2. 뷰(View) 및 컴포넌트 등록 방식
  - **하이어라키 뷰 탐색**: `RegisterComponentInHierarchy<T>()`를 사용하여 씬 하이어라키에서 자동 탐색 및 주입합니다.
  - **런타임 동적 생성**: 런타임 동적 생성이 필요한 뷰는 `RegisterComponentOnNewGameObject<T>(Lifetime, string)`을 사용합니다.
  - **[금지 사항]**: LifetimeScope에서 뷰 컴포넌트를 `[SerializeField]`로 직접 참조하여 `RegisterInstance`로 등록하는 방식을 금지합니다.
  
  ## 3. ViewModel/Service 및 EntryPoint 등록
  - 순수 C# 클래스는 `Register<T>(Lifetime)`을 사용하며, 인터페이스 바인딩 시 `.AsImplementedInterfaces()` 또는 `.As<IInterface>()`를 체이닝합니다.
  - `IStartable`, `ITickable` 등 VContainer 라이프사이클 인터페이스를 구현하는 클래스는 `RegisterEntryPoint<T>()`로 등록합니다.
  ```

- [ ] **Step 2: Verify file existence**

  Verify existence:
  `test -f /Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/.agent/rules/vcontainer-rules.md`
  Expected: 파일이 존재하며 에러 없이 완료되어야 합니다.

- [ ] **Step 3: Commit**

  Run:
  ```bash
  git add .agent/rules/vcontainer-rules.md
  git commit -m "feat: add VContainer rules"
  ```


### Task 3: Coding Standards & Safety Rules 생성

**Files:**
- Create: `/.agent/rules/safety-rules.md`

- [ ] **Step 1: Write code content to safety-rules.md**

  아래 텍스트 내용을 `/.agent/rules/safety-rules.md`에 작성합니다.

  ```markdown
  # Coding Standards & Safety Rules
  
  ## 1. Naming & Formatting
  - **Private Field**: `m_` 접두사 + camelCase (예: `m_playerData`)
  - **Public Property/Method**: PascalCase (예: `PlayerLevel`)
  - **Interface**: `I` 접두사 (예: `IViewModel`)
  - **DTO Class**: `~DTO` 접미사 (예: `PlayerStatsDTO`)
  - **UI Event Callbacks**: UI 버튼의 OnClick 이벤트 등에 연결되는 public 메서드는 반드시 `func_` 접두사를 사용합니다. (예: `public void func_OnStartButtonClick()`)
  - **Brace & Indent**: 중괄호는 항상 줄 바꿈 후 작성하는 Allman Style을 엄격히 준수하며, 들여쓰기는 4개의 공백(Space)을 사용하십시오.
  - **중괄호 생략 금지**: 모든 `if`, `else if`, `else`, `for`, `while` 문에는 로직이 단 한 줄이더라도 예외 없이 중괄호를 사용해야 합니다.

  ## 2. Unity Safety (Strict)
  - **Fake Null 방지**: `UnityEngine.Object` 파생 타입에는 널 조건부 연산자(`?.`, `??`)를 절대 사용하지 마십시오. 널 체크는 반드시 `if (obj != null)` 구문을 명시적으로 사용하십시오.
  - 직렬화 필드명 변경 시에는 데이터 유실을 막기 위해 `[FormerlySerializedAs("기존이름")]`을 추가하십시오.

  ## 3. Using Directives (using 지시문)
  - 파일 최상단에 `using` 지시문을 명시하고, 본문 코드 내에서는 클래스 이름 등을 최대한 간결하게 호출하십시오.
  - 자주 사용되는 정적 클래스는 `using static`을 활용하여 호출을 단축하십시오. (예: `using static UnityEngine.Mathf;`)
  - 긴 제네릭 타입이나 중복되는 타입명은 `using` 별칭을 사용하여 가독성을 높이십시오.
  
  ## 4. 주석 및 리전 (#region)
  - 모든 스크립트 최상단, 클래스 정의 바로 위에 작성자와 기능을 명시하는 XML 주석을 작성합니다.
    ```csharp
    /// <summary>
    /// [기능]: 이 클래스가 수행하는 주요 역할에 대해 간략히 기술합니다.
    /// [작성자]: 윤승종
    /// </summary>
    ```
  - 모든 메서드에 대해 동작 설명과 작성/수정 이력을 포함하는 XML 주석을 작성합니다.
    ```csharp
    /// <summary>
    /// [기능]: 메서드의 동작을 상세히 설명합니다.
    /// [작성자]: 윤승종
    /// [수정 날짜]: YYYY-MM-DD
    /// [마지막 수정 작성자]: 윤승종
    /// [수정 내용]: 구체적인 수정 사항 요약
    /// </summary>
    ```
  - `#region`을 적극적으로 사용하여 코드 섹션을 명확히 구분하고 구조화하십시오. 리전 이름은 한글로 작성합니다.
  - 기존 코드에 이미 존재하는 주석이나 `#region` 블록은 절대 삭제하거나 훼손하지 말고 그대로 유지하십시오.

  ## 5. 로깅 및 성능 최적화
  - **한글 로그 강제**: `Debug.Log` 등 모든 로그 메시지는 `[클래스명]`을 포함하여 한글로 작성하십시오.
  - **루프 성능 최적화**: 빈번하게 호출되는 로직(`Update` 등)에서는 `foreach` 대신 반드시 `for` 루프를 사용하십시오.
  - **Zero Allocation**: `Update` 루프 내에서 `new` 키워드, Boxing, LINQ 사용을 엄격히 금지합니다.
  - **비동기 및 트윈**: 코루틴 대신 `UniTask`를 사용하고 (`async UniTaskVoid` 및 `CancellationToken` 필수 적용), 트윈 처리는 `DOTween`을 활용합니다.
  ```

- [ ] **Step 2: Verify file existence**

  Verify existence:
  `test -f /Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/.agent/rules/safety-rules.md`
  Expected: 파일이 존재하며 에러 없이 완료되어야 합니다.

- [ ] **Step 3: Commit**

  Run:
  ```bash
  git add .agent/rules/safety-rules.md
  git commit -m "feat: add coding standards and safety rules"
  ```


### Task 4: Architecture Guide 문서 생성

**Files:**
- Create: `/.agent/docs/architecture-guide.md`

- [ ] **Step 1: Write code content to architecture-guide.md**

  아래 텍스트 내용을 `/.agent/docs/architecture-guide.md`에 작성합니다.

  ```markdown
  # BePex Unity Client Architecture Guide
  
  ## 1. Directory Structure (디렉토리 구조)
  - `Assets/` 아래 핵심 구조는 관심사 분리(SoC)를 보장하기 위해 다음과 같이 계층을 이룹니다:
    - `/Models/`: 순수 C# 데이터 정의 및 비즈니스 규칙 도메인
    - `/ViewModels/`: 상태와 상태 변화 Action, View의 상호작용 명령 바인딩 레이어
    - `/Views/`: MonoBehaviour 기반 컴포넌트, UI 및 오브젝트 드로우, Input 전달만 처리
    - `/DI/`: VContainer LifetimeScope 및 의존성 설정 파일들
    - `/DTOs/`: 씬 간 상태 이전용 Data Transfer Object 모음

  ## 2. 씬 전환 및 데이터 전송 (DTO)
  - 씬 전환 시 유지해야 할 데이터는 전역 의존성(싱글톤 등)을 배제하고, 반드시 순수 데이터 클래스인 DTO(Data Transfer Object)로 캡슐화합니다.
  - DTO는 씬 로더나 컨텍스트 매니저를 통해 다음 씬의 초기화(Initializer) 클래스로 직접 주입되어야 합니다.
  - DTO 클래스의 네이밍은 `~DTO` 접미사를 필수로 붙여야 합니다. (예: `LobbyStateDTO`, `InGameConfigDTO`)
  ```

- [ ] **Step 2: Verify file existence**

  Verify existence:
  `test -f /Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/.agent/docs/architecture-guide.md`
  Expected: 파일이 존재하며 에러 없이 완료되어야 합니다.

- [ ] **Step 3: Commit**

  Run:
  ```bash
  git add .agent/docs/architecture-guide.md
  git commit -m "feat: add architecture-guide document for DTO and scene transition"
  ```
