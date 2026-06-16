# Recreate Event System Rules Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 로딩 오류가 나는 `event-system-rules.md` 파일을 영구 삭제하고, 클린 텍스트 규격으로 다시 깨끗하게 재작성합니다.

**Architecture:** 파일 시스템 캐시 및 인코딩 이슈를 정화하기 위해 `git rm`으로 파일을 완전히 지우고, 다시 순수 마크다운 파일로 작성한 후 복구 검증 및 커밋을 진행합니다.

**Tech Stack:** Git, Markdown (문서 관리)

---

### Task 1: 파일 삭제 및 클린 재생성

**Files:**
- Modify: `/.agent/rules/event-system-rules.md` (삭제 후 재생성)

- [ ] **Step 1: Execute git rm command to delete old file**

  문제가 된 기존 파일을 Git 명령으로 제거합니다.
  Run:
  ```bash
  git rm .agent/rules/event-system-rules.md
  ```

- [ ] **Step 2: Recreate event-system-rules.md with clean content**

  아래 텍스트 내용을 `/.agent/rules/event-system-rules.md`에 새로 작성합니다.

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

- [ ] **Step 3: Verify folder path and file existence**

  파일이 올바르게 생성되었으며 파일 스트림에서 읽을 수 있는지 확인합니다.
  Verify:
  `test -f /Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/.agent/rules/event-system-rules.md`
  Expected: 성공(존재함)

- [ ] **Step 4: Commit recreate changes**

  재생성 완료 이력을 커밋합니다.
  Run:
  ```bash
  git add .agent/rules/event-system-rules.md
  git commit -m "refactor: recreate event-system-rules file with clean encoding"
  ```
