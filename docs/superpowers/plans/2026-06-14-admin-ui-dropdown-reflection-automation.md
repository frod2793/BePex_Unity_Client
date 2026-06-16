# 어드민 UI 드롭다운 리플렉션 자동화 개선 구현 계획

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 50개 이상의 이벤트 종류 확장 시 switch-case 코딩을 완전히 배제하고 Enum 디스플레이 어트리뷰트 및 리플렉션 캐싱을 통해 어드민 UI 드롭다운 및 한영 매핑 자동화
**Architecture:** Enum Display Attribute + Generic Reflection Utility (O(1) Caching)
**Tech Stack:** C# (Unity Event System UI, Reflection)

---

### Task 1: 커스텀 어트리뷰트 및 리플렉션 유틸리티 신설

**Files:**
- Create: [EventDisplayNameAttribute.cs](file:///Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/Assets/_Game/Scripts/EventSystem/Attributes/EventDisplayNameAttribute.cs)
- Create: [EnumDisplayHelper.cs](file:///Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/Assets/_Game/Scripts/EventSystem/Utils/EnumDisplayHelper.cs)

- [ ] **Step 1: EventDisplayNameAttribute.cs 생성 및 구현**
  - `AttributeUsage`를 `AttributeTargets.Field`로 제한하고, 생성자에서 한글 표시명을 수집하는 속성 클래스 작성.

- [ ] **Step 2: EnumDisplayHelper.cs 생성 및 구현**
  - 제네릭 인자 `T`가 `Enum`인 정적 메서드 `RegisterEnum<T>()`, `GetDisplayName<T>()`, `GetEnumValue<T>()`, `GetDisplayNames<T>()` 작성.
  - 양방향 매핑용 `Dictionary<Enum, string>` 및 `Dictionary<string, Enum>` 캐시를 내장하여 불필요한 가비지 컬렉션(GC) 및 반복적인 리플렉션 오버헤드를 최적화.

---

### Task 2: 데이터 스크립트 Enum에 속성 장식

**Files:**
- Modify: [ConditionDefinitionSO.cs](file:///Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/Assets/_Game/Scripts/EventSystem/Data/ConditionDefinitionSO.cs)
- Modify: [RewardDefinitionSO.cs](file:///Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/Assets/_Game/Scripts/EventSystem/Data/RewardDefinitionSO.cs)

- [ ] **Step 1: ConditionType 이넘에 EventDisplayName 어트리뷰트 부여**
  - `KillCount` -> `[EventDisplayName("적 처치 수")]`
  - `StageClear` -> `[EventDisplayName("스테이지 클리어")]`
  - `Attendance` -> `[EventDisplayName("누적 출석")]`

- [ ] **Step 2: RewardType 이넘에 EventDisplayName 어트리뷰트 부여**
  - `Exp` -> `[EventDisplayName("경험치")]`
  - `Ticket` -> `[EventDisplayName("아이템 티켓")]`
  - `Point` -> `[EventDisplayName("이벤트 포인트")]`

---

### Task 3: 어드민 뷰 클래스 리팩토링 및 하드코딩 제거

**Files:**
- Modify: [EventAdminView.cs](file:///Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/Assets/_Game/Scripts/EventSystem/Views/EventAdminView.cs:93-98)
- Modify: [EventAdminView.cs](file:///Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/Assets/_Game/Scripts/EventSystem/Views/EventAdminView.cs:634-671)
- Modify: [EventAdminRewardRowView.cs](file:///Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/Assets/_Game/Scripts/EventSystem/Views/EventAdminRewardRowView.cs:49-60)
- Modify: [EventAdminRewardRowView.cs](file:///Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/Assets/_Game/Scripts/EventSystem/Views/EventAdminRewardRowView.cs:228-265)

- [ ] **Step 1: EventAdminView.cs의 조건 드롭다운 로딩 및 한/영 매핑을 EnumDisplayHelper로 대체**
  - `Bind` 메서드 내 `m_condTypeDropdown.AddOptions`에 `EnumDisplayHelper.GetDisplayNames<ConditionDefinitionSO.ConditionType>()` 주입.
  - `GetEnglishConditionType` 및 `GetKoreanConditionType` 메서드의 switch-case 제거 후 `EnumDisplayHelper`를 이용하도록 리팩토링.

- [ ] **Step 2: EventAdminRewardRowView.cs의 보상 드롭다운 로딩 및 한/영 매핑을 EnumDisplayHelper로 대체**
  - `Bind` 메서드 내 `m_typeDropdown.AddOptions`에 `EnumDisplayHelper.GetDisplayNames<RewardDefinitionSO.RewardType>()` 주입.
  - `GetEnglishRewardType` 및 `GetKoreanRewardType` 메서드의 switch-case 제거 후 `EnumDisplayHelper`를 이용하도록 리팩토링.

---

### Task 4: 가이드라인 문서 최신화

**Files:**
- Modify: [event_addition_guide.md](file:///Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/docs/event_addition_guide.md)
- Modify: [extension_guide.md](file:///Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/docs/extension_guide.md)
- Modify: [README.md](file:///Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/README.md)

- [ ] **Step 1: event_addition_guide.md 가이드 갱신**
  - 어드민 UI 연동 지침에서 switch-case 방식 설명을 제거하고, Enum 필드 장식용 `[EventDisplayName]` 설명으로 교체.

- [ ] **Step 2: extension_guide.md 가이드 갱신**
  - 2.4 및 3.3 섹션의 영-한 헬퍼 수정 지침을 `EnumDisplayHelper` 및 어트리뷰트 장식 방식으로 수정.

- [ ] **Step 3: README.md 확장 가이드 갱신**
  - 3.1 및 3.2 섹션의 어드민 UI 연동 요약을 어트리뷰트 장식 방식으로 조정.
