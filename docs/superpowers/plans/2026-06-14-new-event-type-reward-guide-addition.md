# 새로운 이벤트 타입 및 보상 타입 추가 가이드 보강 계획

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 새로운 이벤트 조건 및 보상 타입 추가 시 런타임 비즈니스 로직 및 이벤트 관리자(어드민) UI 수정 방법을 다루는 안내서 보강 및 README.md 업데이트
**Architecture:** OCP 기반의 리플렉션 팩토리 패턴을 설명하고, EventAdminScene UI와 연동할 수 있도록 드롭다운 리스트 및 상호 매핑 헬퍼(GetEnglish/GetKorean)의 수정 단계를 문서화함.
**Tech Stack:** Markdown, C# (Unity Event System)

---

### Task 1: docs/event_addition_guide.md 파일 업데이트

**Files:**
- Modify: [event_addition_guide.md](file:///Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/docs/event_addition_guide.md)

- [ ] **Step 1: 팩토리 switch-case 설명을 리플렉션 및 Attribute 방식으로 수정**
  - 기존에 팩토리 코드 수정이 필요하다고 안내되어 있던 3.1.3) 및 3.2.3) 단락을 리플렉션 기반 자동 매핑(`[EventCondition]` 및 `[EventReward]` Attribute 적용) 방식으로 변경하고, 팩토리 코드 수정이 불필요하다는 장점을 기술합니다.

- [ ] **Step 2: 이벤트 관리자 UI(어드민) 연동 방법 가이드 추가**
  - 새로운 이벤트 조건 및 보상 타입이 이벤트 관리자 씬의 UI 드롭다운과 매핑 헬퍼에서 연동되도록 하기 위해 [EventAdminView.cs](file:///Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/Assets/_Game/Scripts/EventSystem/Views/EventAdminView.cs)와 [EventAdminRewardRowView.cs](file:///Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/Assets/_Game/Scripts/EventSystem/Views/EventAdminRewardRowView.cs)를 수정하는 절차를 추가합니다.

---

### Task 2: docs/extension_guide.md 파일 업데이트

**Files:**
- Modify: [extension_guide.md](file:///Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/docs/extension_guide.md)

- [ ] **Step 1: 이벤트 관리자(어드민) UI 수정 안내 추가**
  - 새로운 이벤트 조건 추가 시 [EventAdminView.cs](file:///Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/Assets/_Game/Scripts/EventSystem/Views/EventAdminView.cs)의 `Bind` 및 한글-영문 매핑 헬퍼 변경 사항을 설명하는 2.4 섹션을 추가합니다.
  - 새로운 보상 타입 추가 시 [EventAdminRewardRowView.cs](file:///Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/Assets/_Game/Scripts/EventSystem/Views/EventAdminRewardRowView.cs)의 `Bind` 및 한글-영문 매핑 헬퍼 변경 사항을 설명하는 3.3 섹션을 추가합니다.

---

### Task 3: README.md 파일 업데이트

**Files:**
- Modify: [README.md](file:///Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/README.md)

- [ ] **Step 1: 확장 가이드 요약본에 어드민 UI 수정 지침을 추가하고 상세 안내서 링크를 보완**
  - 3.1 및 3.2 섹션을 최신 리플렉션 기반 생성 방식으로 명시하고, 새로운 타입을 반영할 때 이벤트 관리자 UI 단에서 수정이 필요한 코드가 있음을 환기시킵니다.
  - 상세한 확장 가이드 문서([event_addition_guide.md](file:///Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/docs/event_addition_guide.md), [extension_guide.md](file:///Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/docs/extension_guide.md))의 링크를 걸어 기획자와 개발자 모두가 안내서를 찾아볼 수 있도록 정돈합니다.
