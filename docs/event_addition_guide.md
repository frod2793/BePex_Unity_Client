# 이벤트 추가 가이드 (Event Addition Guide)

> **작성자**: 윤승종  
> **작성일**: 2026-06-14  
> **대상 프로젝트**: BePex Unity Client  

본 문서는 데이터 구동형(Data-Driven) 이벤트 시스템에서 운영자/기획자가 에디터 상에서 데이터 에셋(ScriptableObject) 조립만으로 새로운 이벤트를 추가(배포)하거나, 개발자가 신규 조건/보상 전략을 설계하여 시스템을 확장하기 위한 구체적인 기술 명세 및 가이드라인입니다.

---

## 1. 개요 및 구조 관계

본 시스템은 게임 비즈니스 로직과 UI 시각화 영역의 결합도를 철저히 차단하는 **Pure DI** 및 **Strategy 패턴**을 바탕으로 구현되었습니다. 기획자가 조립한 데이터 정의서(ScriptableObject)는 런타임에 해당하는 조건 및 보상 전략 클래스로 Factory를 통해 인스턴스화됩니다.

```
[EventTableSO] (활성화된 전체 이벤트 테이블 에셋)
      └── [EventDefinitionSO] (단일 이벤트 정의 에셋)
                ├── [ConditionDefinitionSO] (이벤트 완료 조건 에셋)
                └── [RewardDefinitionSO] (보상 정보 에셋 - 다중 설정 가능)
```

---

## 2. 에디터 상에서 새로운 이벤트 추가하기 (기획자/운영자용 - Code-Free)

운영자나 기획자는 **코드를 전혀 수정하지 않고(Code-Free)** 기존에 구동되어 컴파일 완료된 조건 타입(`ConditionType`)과 보상 타입(`RewardType`)을 조합하고 수치 데이터만 변경하는 것으로 새로운 이벤트를 출시하고 에디터 상에서 배포할 수 있습니다.

### 2.1 1단계: 조건(Condition) 에셋 생성
기존에 구현 완료된 조건(처치 수, 스테이지 클리어, 출석 등) 중 원하는 조건을 설정합니다.
1. Project 뷰에서 에셋을 생성할 폴더(예: `Assets/_Game/Data/Conditions/`)로 이동합니다.
2. 우클릭 후 **Create > BePex > Event > Condition**을 클릭하여 새 `ConditionDefinitionSO` 에셋을 생성합니다. (예: `Cond_StageClear_10`)
3. 인스펙터 창에서 다음 속성을 설정합니다.
   * **Condition Type**: `StageClear` (기존 구현된 스테이지 클리어 조건)
   * **Target Value**: `10` (목표 수치: 10단계 클리어)

### 2.2 2단계: 보상(Reward) 에셋 생성
기존에 구현 완료된 보상(경험치, 가차 티켓, 이벤트 포인트 등) 중 원하는 보상을 설정합니다.
1. Project 뷰에서 보상 폴더(예: `Assets/_Game/Data/Rewards/`)로 이동합니다.
2. 우클릭 후 **Create > BePex > Event > Reward**를 클릭하여 새 `RewardDefinitionSO` 에셋을 생성합니다. (예: `Rew_Point_500`)
3. 인스펙터 창에서 보상 정보를 설정합니다.
   * **Reward Type**: `Point` (기존 구현된 이벤트 포인트 보상)
   * **Amount**: `500` (지급 수량: 500P)
   * **Display Name**: `이벤트 포인트 500P`
   * **Icon**: 포인트 UI 아이콘 이미지 드래그 할당

### 2.3 3단계: 단일 이벤트 정의(EventDefinition) 조립
앞서 생성한 조건과 보상 조각들을 하나의 이벤트 정의로 패키징합니다.
1. Project 뷰에서 이벤트 정의 폴더(예: `Assets/_Game/Data/Events/`)로 이동합니다.
2. 우클릭 후 **Create > BePex > Event > EventDefinition**을 클릭하여 새 `EventDefinitionSO` 에셋을 생성합니다. (예: `Event_Clear_10_Stage`)
3. 인스펙터에서 아래의 상세 필드를 드래그 앤 드롭으로 완성합니다.
   * **Event Id**: `stage_clear_10` (세이브 및 런타임 추적용 고유 ID)
   * **Event Title**: `초보 모험가의 도전`
   * **Event Description**: `스테이지를 10단계까지 클리어하고 이벤트 포인트 500P를 받으세요!`
   * **Start Date / End Date**: `2026-06-01` / `2026-06-30` (포맷: YYYY-MM-DD)
   * **Condition**: 1단계에서 생성한 **Cond_StageClear_10** 에셋을 드래그하여 할당합니다.
   * **Rewards**: 2단계에서 생성한 **Rew_Point_500** 에셋을 드래그하여 리스트 슬롯에 추가합니다.

### 2.4 4단계: 활성 기획 테이블(EventTable)에 최종 등록
1. 프로젝트 내부의 활성화된 테이블 에셋(기본 위치: `Assets/_Game/Data/Mock/Mock_EventTable.asset`)을 엽니다.
2. 인스펙터의 `Events` 리스트 항목 하단에 3단계에서 완성한 **Event_Clear_10_Stage** 에셋을 드래그하여 드롭합니다.
3. 게임을 구동하면 해당 이벤트가 런타임에 동적으로 컴포지션 루트에 의해 주입되어 화면에 즉각적으로 노출됩니다.

---

## 3. 신규 조건 및 보상 타입 확장하기 (개발자용)

기획이나 이벤트 정책에 의해 새로운 기믹 조건(예: 길드 퀘스트 달성 등)이나 새로운 자산 보상(예: 골드 지급 등)을 추가할 때 코드를 설계하고 구조적으로 매핑하는 방법입니다.

### 3.1 신규 조건(Condition) 타입 추가 방법

#### 1) Condition 구현체 클래스 작성
`IEventCondition` 인터페이스를 구현하는 순수 C# POCO 클래스를 작성합니다.
```csharp
namespace BePex.EventSystem.Conditions
{
    using BePex.EventSystem.Interfaces;

    /// <summary>
    /// [기능]: 길드 퀘스트 수행 횟수를 조건 진척도로 계산하고 달성 상태를 판정하는 Strategy 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    public class GuildQuestCondition : IEventCondition
    {
        #region 내부 필드
        private readonly int m_targetValue;
        private readonly ISaveSystem m_saveSystem;
        private readonly string m_eventId;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 목표 값, 세이브 장치 및 추적할 이벤트 ID를 바인딩하는 생성자.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public GuildQuestCondition(int targetValue, ISaveSystem saveSystem, string eventId)
        {
            m_targetValue = targetValue;
            m_saveSystem = saveSystem;
            m_eventId = eventId;
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 세이브 시스템으로부터 현재 진행 횟수를 쿼리하여 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public int GetCurrentProgress()
        {
            if (m_saveSystem != null)
            {
                var progress = m_saveSystem.LoadProgress(m_eventId);
                return progress.currentProgress;
            }
            return 0;
        }

        public int GetTargetValue()
        {
            return m_targetValue;
        }

        public bool IsCompleted()
        {
            return GetCurrentProgress() >= m_targetValue;
        }
        #endregion
    }
}
```

#### 2) 기획 데이터 Enum 형식 정의 추가
`ConditionDefinitionSO.cs` 내의 `ConditionType` 열거형에 식별자를 추가합니다.
```csharp
public enum ConditionType
{
    KillCount,
    StageClear,
    Attendance,
    GuildQuest // 신규 식별자 추가
}
```

#### 3) Factory 클래스 바인딩 매핑
`ConditionFactory.cs` 내의 `Create` 메서드에 전략 생성 스위치를 바인딩합니다.
```csharp
case ConditionDefinitionSO.ConditionType.GuildQuest:
    return new GuildQuestCondition(definition.TargetValue, m_saveSystem, eventId);
```

---

### 3.2 신규 보상(Reward) 타입 추가 방법

#### 1) Reward 구현체 클래스 작성
`IEventReward` 인터페이스를 상속하는 순수 C# 클래스를 설계합니다.
```csharp
namespace BePex.EventSystem.Rewards
{
    using BePex.EventSystem.Interfaces;
    using BePex.EventSystem.Models;

    /// <summary>
    /// [기능]: 플레이어의 재화 정보에 골드를 지급해 주는 보상 Strategy 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    public class GoldReward : IEventReward
    {
        #region 내부 필드
        private readonly int m_amount;
        private readonly string m_displayName;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 골드 지급 수량과 명칭을 설정하는 생성자.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public GoldReward(int amount, string displayName)
        {
            m_amount = amount;
            m_displayName = displayName;
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 플레이어의 보상 수령 전용 모델에 골드를 가산합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public void Grant(PlayerRewardModel playerReward)
        {
            if (playerReward != null)
            {
                playerReward.totalGold += m_amount;
                UnityEngine.Debug.Log($"[GoldReward] 보상이 정상 지급되었습니다: {m_displayName} (+{m_amount} 골드)");
            }
        }
        #endregion
    }
}
```

#### 2) 기획 데이터 Enum 형식 정의 추가
`RewardDefinitionSO.cs` 내의 `RewardType` 열거형에 식별자를 추가합니다.
```csharp
public enum RewardType
{
    Exp,
    Ticket,
    Point,
    Gold // 신규 보상 타입 추가
}
```

#### 3) Factory 클래스 바인딩 매핑
`RewardFactory.cs` 내의 `Create` 메서드에 해당하는 생성 분기를 맵핑합니다.
```csharp
case RewardDefinitionSO.RewardType.Gold:
    return new GoldReward(definition.Amount, definition.DisplayName);
```

---

## 4. 이벤트 포인트 시스템 및 연동 확장 가이드

기획 상에서 획득한 이벤트 포인트(`Point`)는 플레이어의 교환 상점 등과 밀접하게 작용합니다. 시스템 흐름을 확장하여 특정 행동(예: 광고 시청)을 통해 포인트를 지급하는 구조는 다음과 같은 흐름으로 연동할 수 있습니다.

1. **포인트 증가 명령 처리**:
   플레이어가 포인트 보상을 획득하여 `PlayerRewardModel.totalPoints`가 가산되면, UI 상에 바인딩된 상태 변수 변경 이벤트(`OnPointsChanged`)가 작동하여 화면에 보유 포인트가 즉시 반영됩니다.
2. **포인트 교환/소모 전략**:
   추후 상점 시스템 구축 시, 아래의 캡슐화된 획득/차감 구조를 호출하도록 설계할 것을 권장합니다.
   ```csharp
   // EventPointManager 또는 ShopModel 내부에 구현 예시
   public bool TryConsumePoints(PlayerRewardModel model, int amount)
   {
       if (model != null && model.totalPoints >= amount)
       {
           model.totalPoints -= amount;
           return true;
       }
       return false;
   }
   ```

---

## 5. 검증 시뮬레이션 방법

신규 이벤트를 추가한 후 씬 상에서 정상 동작하는지 테스트하는 절차입니다.

1. **테스트 모드 활성화**:
   씬 내부에 위치한 컴포지션 루트 `EventSceneInitializer` 인스펙터 상에서 `m_useDebugMode` 필드를 `True`로 활성화하고, 비활성 상태인 `EventDebugView` 오브젝트를 필드에 드래그하여 연결합니다.
2. **이벤트 진행도 상승 테스트**:
   게임을 실행한 후 화면 하단의 디버그 패널 인풋 필드에 등록한 이벤트 ID(예: `stage_clear_10`)와 진행도 증가량(예: `1`)을 입력한 뒤 **[진행도 가산]** 버튼을 누릅니다.
   * `EventListView` 상의 해당 이벤트 진행도 게이지가 실시간으로 가산되어 오르는지 확인합니다.
3. **보상 수령 검증**:
   게이지가 가득 차면(목표 수치 `10` 도달), 이벤트 우측의 [보상 받기] 버튼이 활성화됩니다.
   * 버튼을 터치하여 `RewardPopupView`에 설정된 `Gold` 또는 `Point`가 올바르게 합산 표출되는지, 콘솔 창에 정상 지급 로그가 찍히는지 최종적으로 확인합니다.
