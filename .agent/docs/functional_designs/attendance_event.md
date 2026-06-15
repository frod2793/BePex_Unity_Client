# 출석 이벤트 시스템 설계서 (Attendance Event System)

> **작성자**: 윤승종  
> **작성일**: 2026-06-16  

## 1. 개요
출석 이벤트 시스템은 사용자가 매일 접속할 때마다 보상을 제공하고, 일정 누적 일수를 채웠을 때 추가 보상을 제공하는 기능입니다.

## 2. 주요 클래스 및 책임
*   **`AttendanceQuestCondition` (IQuestCondition)**
    *   사용자의 마지막 출석 시간을 추적하고, 하루 단위로 초기화되는 출석 체크 비즈니스 로직을 담당합니다.
    *   `ITimeProvider`를 주입받아 현재 시간을 검증하며, 이는 테스트 및 가상 디버그 환경에서도 용이하게 사용할 수 있습니다.
*   **`QuestProgressModel`**
    *   `currentProgress`: 현재 연속/누적 출석 일수 데이터.
    *   `targetValue`: 목표 출석 일수.
    *   `lastUpdatedTicks`: 마지막으로 출석 체크가 진행된 시점의 Ticks 값.

## 3. 동작 흐름 (Data Flow)
1.  **로그인 및 초기화**: 시스템이 로드되면 저장 장치(`ISaveSystem`)로부터 저장된 `EventProgressModel`의 하위 퀘스트 진행 데이터(`QuestProgressModel`)를 불러옵니다.
2.  **출석 체크 처리**: `AttendanceQuestCondition.CanAddProgress()`가 호출되어 현재 시간(`m_timeProvider.GetCurrentTime()`)이 이전 출석일(`lastUpdatedTicks`) 기준 익일(다음날)을 넘었는지 검사합니다 (`currentTime.Date > lastTime.Date`).
3.  **상태 갱신**: 조건 만족 시 `currentProgress`를 +1 증가시키고 상태를 저장(`ISaveSystem.SaveProgressAsync()`)합니다.
4.  **UI 렌더링**: 값이 갱신되면 `EventDetailViewModel`을 통해 View로 변경 사항이 전파되고 UI 게이지 및 완료 연출이 갱신됩니다.

## 4. 확장성 및 OCP
*   새로운 출석 방식(예: 시간 단위 출석, 월간 출석)이 필요할 경우, `AttendanceQuestCondition`을 수정하는 대신 새로운 `IQuestCondition` 구현체(`MonthQuestCondition` 등)를 생성하고 `[QuestCondition]` 어트리뷰트를 붙여 자동으로 팩토리에 등록합니다.
