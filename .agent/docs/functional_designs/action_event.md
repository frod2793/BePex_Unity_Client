# 누적 행동 이벤트 시스템 설계서 (Action Accumulation Event System)

> **작성자**: 윤승종  
> **작성일**: 2026-06-16  

## 1. 개요
플레이어가 인게임에서 수행하는 특정 행동(적 처치, 스테이지 클리어, 광고 시청 등)의 횟수를 누적하여, 사전에 정의된 목표 횟수에 도달하면 보상을 지급하는 이벤트 모델입니다.

## 2. 주요 클래스 및 책임
*   **`KillCountQuestCondition`, `StageClearQuestCondition` (IQuestCondition)**
    *   누적 행동에 대한 구체적인 타겟팅 및 횟수 증가를 처리합니다.
    *   예시: 적을 처치했을 때 발생하는 메시지나 콜백을 수신하여 `QuestProgressModel`의 `currentProgress`를 상승시킵니다.
*   **`BaseQuestCondition` (추상 클래스)**
    *   누적 진행도를 반환하고 완료 여부를 판별하는 기본 로직(`IsCompletedAsync()`, `GetCurrentProgressAsync()`)을 구현하여 중복 코드를 최소화합니다.

## 3. 동작 흐름 (Data Flow)
1.  **행동 발생**: 인게임에서 적 처치 또는 스테이지 클리어 이벤트 트리거가 작동합니다.
2.  **이벤트 전달**: 해당 행동과 매핑된 `IQuestCondition` 구현체의 진척도를 가산하는 메서드가 호출됩니다. (예: `Debug_AddProgressNoSave` 또는 인게임 세이브 가산 호출)
3.  **목표 검사**: 증가된 현재 진행도(`currentProgress`)가 목표치(`targetValue`)에 도달했는지 `IsCompletedAsync()`로 검증합니다.
4.  **UI 알림**: 진행도가 업데이트되면 ViewModel의 속성 변경 알림을 통해 View의 프로그레스 바(게이지)가 실시간으로 갱신됩니다.

## 4. 확장성 및 OCP
*   새로운 행동 누적 이벤트(예: 골드 소모 횟수, 로그인 횟수 등)를 추가하려면, `BaseQuestCondition`을 상속받는 `GoldSpentQuestCondition` 클래스를 생성하고 `[QuestCondition]` 어트리뷰트를 선언하기만 하면 됩니다. 팩토리나 핵심 로직에 대한 수정은 발생하지 않습니다.
