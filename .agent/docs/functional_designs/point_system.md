# 이벤트 포인트 시스템 설계서 (Event Point System)

> **작성자**: 윤승종  
> **작성일**: 2026-06-16  

## 1. 개요
단일 조건 달성 시 보상을 즉시 수령하는 구조를 넘어서, 게임 내 특정 행동 시 이벤트 포인트(Point), 시즌 포인트(SeasonPoint), 크레딧(Credit) 등의 재화를 지급하고 이 포인트를 모아 상점이나 보상 팝업에서 원하는 가치의 상품과 교환하는 경제 구조 시스템입니다.

## 2. 주요 클래스 및 책임
*   **`PlayerRewardModel` (Model)**
    *   유저가 현재 보유하고 있는 여러 형태의 화폐(Point, SeasonPoint, Credit 등) 잔액을 데이터로 관리하고 캡슐화 처리하는 순수 C# 클래스(POCO)입니다.
    *   `AddCurrency()`와 `TrySpendCurrency()` API를 제공하여 데이터 가산 및 차감을 안전하게 처리합니다.
*   **`PointQuestReward` (IQuestReward)**
    *   보상 타입 중 하나로, 지급 시 인게임 아이템이 아니라 `PlayerRewardModel`의 보유 포인트를 일정 수치만큼 가산하는 구현체입니다.
*   **`CurrencyHUDViewModel`**
    *   현재 보유하고 있는 다양한 화폐 잔액의 변화를 감지하고, 화면 상단 재화 HUD UI의 수치 갱신 이벤트(`NotifyCurrencyChanged`)를 View로 전파합니다.

## 3. 동작 흐름 (Data Flow)
1.  **포인트 적립**: 스테이지 클리어 등 이벤트 조건 달성 시 보상(Reward)이 실행될 때, 해당 보상이 `PointQuestReward`인 경우 포인트가 적립됩니다.
2.  **데이터 바인딩**: `PlayerRewardModel`의 화폐 수치 변경이 발생하여 데이터 저장 장치에 세이브되면, `CurrencyHUDViewModel`이 이를 감지하고 View 상단의 재화 UI 그룹 수치를 갱신합니다.
3.  **포인트 소모 (교환)**: 사용자가 상점 교환이나 포인트 차감 행동을 요구하면 뷰모델을 통해 `TrySpendCurrency(화폐타입, 금액)`가 호출됩니다.
4.  **결과 반영**: 잔액 부족 시 차감이 거부되며, 잔액이 충분할 시 포인트를 차감하고 실제 보상(아이템 등)을 지급하는 `QuestRewardFactory` 로직을 트리거합니다.

## 4. 확장성 및 OCP
*   다양한 형태의 포인트(예: 길드 코인, 우정 포인트)를 추가할 시 `PlayerRewardModel` 내의 `RewardType` 이넘 필드 확장 및 딕셔너리 내부 잔액 매핑 구조를 통해, 핵심 로직 및 뷰모델 수정 없이 손쉽게 포인트를 통합 관리 및 추가할 수 있도록 설계되었습니다.
