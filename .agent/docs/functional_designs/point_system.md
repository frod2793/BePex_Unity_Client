# 이벤트 포인트 시스템 설계서 (Event Point System)

> **작성자**: 윤승종  
> **작성일**: 2026-06-16  

---

## 1. 개요
조건 달성 시 정해진 인게임 아이템을 일방적으로 수령하는 직관적인 수령 모델을 넘어, 특정 보상(이벤트 포인트, 시즌 포인트, 크레딧 등)을 지급하여 사용자가 이 재화를 적립한 뒤, 원하는 가치의 상점 상품이나 다른 보상 세트로 직접 교환할 수 있도록 제공하는 게임 내 화폐 경제 시스템입니다.

---

## 2. 클래스 구조 및 책임 (Class Diagram)

재화는 `PlayerRewardModel`에서 통합 관리되며, 보상 객체(`GeneralQuestReward`)에 의해 가산됩니다. 재화 변동 시 UI는 뷰모델(`CurrencyHUDViewModel`)을 통해 단방향으로 자동 갱신됩니다.

```mermaid
classDiagram
    class IQuestReward {
        <<interface>>
        +RewardId string
        +ClaimRewardAsync(PlayerRewardModel, ISaveSystem) Awaitable
    }

    class BaseQuestReward {
        <<abstract>>
        #m_rewardId string
        #m_rewardType string
        #m_amount int
        +ClaimRewardAsync(PlayerRewardModel, ISaveSystem) Awaitable
    }

    class GeneralQuestReward {
        +ClaimRewardAsync(PlayerRewardModel, ISaveSystem) Awaitable
    }

    class PlayerRewardModel {
        -Dictionary~string, int~ m_balances
        +AddCurrency(string, int)
        +TrySpendCurrency(string, int) bool
        +GetBalance(string) int
    }

    class EventPointManager {
        +POINT_KEY string
        -PlayerRewardModel m_playerReward
        +AddPoint(int)
        +TrySpendPoint(int) bool
        +GetPointBalance() int
    }

    class CurrencyHUDViewModel {
        -PlayerRewardModel m_rewardModel
        +OnCurrencyChanged Action~string, int~
        +NotifyCurrencyChanged(string, int)
    }

    class CurrencyHUDView {
        -CurrencyHUDViewModel m_viewModel
        +Bind(CurrencyHUDViewModel)
    }

    IQuestReward <|.. BaseQuestReward
    BaseQuestReward <|-- GeneralQuestReward
    GeneralQuestReward ..> PlayerRewardModel : Modifies
    EventPointManager --> PlayerRewardModel : Delegates & Wraps
    CurrencyHUDViewModel --> PlayerRewardModel : Refers
    CurrencyHUDView --> CurrencyHUDViewModel : Observes
```

### 2.1. 주요 클래스 정의
*   **`PlayerRewardModel`**
    *   유저가 보유한 모든 재화(Point, SeasonPoint, Credit 등)의 잔액을 캡슐화 관리하는 순수 C# 도메인 모델(POCO)입니다.
    *   데이터의 증가(`AddCurrency`) 및 차감(`TrySpendCurrency`)의 무결성 검증을 책임집니다.
*   **`EventPointManager`**
    *   `PlayerRewardModel`을 감싸서 외부 시스템(StageManager 등)에 포인트("Point") 적립, 소비, 조회 기능을 하드코딩 문자열 노출 없이 제공하는 비즈니스 레이어 헬퍼입니다.
*   **`GeneralQuestReward`**
    *   보상 수령 명령이 실행될 때 `PlayerRewardModel`에 접근하여, 지정된 재화 코드와 수량만큼 화폐를 더해주는 보상 전략 구현체입니다.
*   **`CurrencyHUDViewModel`**
    *   보유 화폐 데이터가 변경되었음을 브로드캐스팅하는 이벤트 핸들러(`NotifyCurrencyChanged`)를 제공하며, 상단 재화 HUD UI 갱신을 주도합니다.

---

## 3. 동작 흐름 (Data Flow)

### 3.1. 퀘스트 보상 수령을 통한 포인트 획득 흐름
보상 수령 명령이 호출되어 범용 보상 객체가 `PlayerRewardModel`에 직접 재화를 적립하고 저장하는 흐름입니다.

```mermaid
sequenceDiagram
    autonumber
    actor Player as 플레이어 UI 클릭
    participant VM as EventDetailViewModel
    participant Reward as GeneralQuestReward
    participant Model as PlayerRewardModel
    participant Save as ISaveSystem (Decorator)
    participant HudVM as CurrencyHUDViewModel
    participant HudView as CurrencyHUDView

    Player->>VM: func_OnClaimRewardClick()
    VM->>Reward: ClaimRewardAsync(Model, Save)
    
    Reward->>Model: AddCurrency("Point", 50)
    Note over Model: balances["Point"] 수치 50 가산
    
    Reward->>Save: SaveRewardStateAsync(Model)
    Save-->>Reward: 비동기 세이브 완료 회신
    
    VM->>HudVM: NotifyCurrencyChanged("Point", newBalance)
    HudVM->>HudView: OnCurrencyChanged Action 브로드캐스트
    HudView->>HudView: 상단 UI의 텍스트 수치 갱신 (예: 50 -> 100)
```

### 3.2. 인게임 시스템에서의 직접 포인트 적립 흐름 (EventPointManager 경유)
인게임 콘텐츠(StageManager 등)에서 마술 문자열 `"Point"` 노출 없이 안전하게 포인트를 획득 및 세이브에 반영하는 흐름입니다.

```mermaid
sequenceDiagram
    autonumber
    participant SM as StageManager (인게임)
    participant EPM as EventPointManager
    participant Model as PlayerRewardModel
    participant Save as ISaveSystem (Decorator)
    participant HudVM as CurrencyHUDViewModel
    participant HudView as CurrencyHUDView

    SM->>EPM: AddPoint(15)
    activate EPM
    EPM->>Model: AddCurrency("Point", 15)
    activate Model
    Note over Model: balances["Point"] 수치 15 가산
    deactivate Model
    deactivate EPM
    
    Note over SM: (포인트 변동 시 세이브 데코레이터를 통해 저장 수행)
    SM->>Save: SaveRewardStateAsync(Model)
    Save-->>SM: 비동기 세이브 완료 회신
    
    SM->>HudVM: NotifyCurrencyChanged("Point", newBalance)
    HudVM->>HudView: OnCurrencyChanged Action 브로드캐스트
    HudView->>HudView: 상단 UI의 텍스트 수치 갱신
```

---

## 4. 확장성 및 OCP

*   **새로운 화폐 타입 추가 시 (예: 길드 코인, 다이아 등)**:
    *   데이터 테이블(JSON)의 보상 정적 정의 파트에서 화폐 식별 문자열(예: `"GuildCoin"`)로 지정하기만 하면 됩니다.
    *   `PlayerRewardModel`은 내부적으로 `string`-`int` 딕셔너리로 잔액을 관리하므로 C# 코드 추가나 스키마 수정 없이도 새로운 포인트 체계를 즉시 누적 및 차감하여 활용할 수 있습니다.
    *   UI 상단 HUD에 추가된 화폐를 바인딩하고 싶을 경우, `CurrencyHUDView`에서 갱신을 원하는 화폐 식별자를 바인딩하고 `NotifyCurrencyChanged`에 체이닝해주는 것으로 코드 변화를 최소화할 수 있습니다.
*   **하드코딩 및 데이터 결합성 방지 (EventPointManager)**:
    *   기본적으로 딕셔너리에 문자열 키로 저장하지만, 인게임에서 가장 자주 쓰이는 핵심 화폐인 `"Point"`에 대해서는 `EventPointManager` 래퍼 클래스를 사용하여 오타 및 직접적인 데이터 수정을 방지하는 비즈니스 가드를 구현하였습니다.
