# BePex Unity Client — 이벤트 시스템 확장성 방법 안내서

> **작성자**: 윤승종  
> **작성일**: 2026-06-14  
> **대상**: 개발자 및 기획자  

---

## 1. 확장성 설계 개요

본 이벤트 시스템은 새로운 기능 요구사항이 발생했을 때 기존 코드의 직접적인 수정을 우회하고 결합도를 낮추는 **개방-폐쇄 원칙(OCP, Open-Closed Principle)**을 엄격하게 준수합니다.

시스템은 다음과 같은 핵심 디자인 패턴의 유기적인 결합을 통해 무한한 확장을 지원합니다:
1. **Strategy (전략) 패턴**: 개별 조건 판정(`IEventCondition`)과 보상 지급(`IEventReward`)을 각각 독자적인 알고리즘 클래스로 캡슐화합니다.
2. **Reflection-based Factory (리플렉션 기반 팩토리)**: 하드코딩된 분기문(`switch-case`)을 제거하고, 어셈블리 내 어트리뷰트(`Attribute`) 장식을 스캔하여 조건/보상 매핑 사전을 런타임 시작 시 1회 자동 빌드합니다.

이로 인해 새로운 조건이나 보상이 추가되어도 **기존 팩토리 클래스(`ConditionFactory`, `RewardFactory`)의 소스 코드는 전혀 수정할 필요가 없습니다.**

---

## 2. 새로운 이벤트(조건) 타입 추가하기

새로운 이벤트 달성 조건(예: 길드 이벤트, 시즌 이벤트, 월간 이벤트, 랭킹 이벤트)을 추가해야 할 경우, 개발자는 다음 3단계 프로세스만 수행하면 됩니다.

### 2.1 1단계: 조건 형식 식별자 추가
[ConditionDefinitionSO.cs](file:///Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/Assets/_Game/Scripts/EventSystem/Data/ConditionDefinitionSO.cs) 내의 `ConditionType` 열거형(Enum)에 새로운 조건 타입을 정의합니다.

```csharp
namespace BePex.EventSystem.Data
{
    public class ConditionDefinitionSO : ScriptableObject
    {
        public enum ConditionType
        {
            KillCount,      // 적 처치 수
            StageClear,     // 스테이지 클리어
            Attendance,     // 누적 출석
            
            // --- [신규 추가 조건 타입 예시] ---
            GuildEvent,     // 길드 이벤트 (예: 길드원 협동 퀘스트 달성)
            SeasonEvent,    // 시즌 이벤트 (예: 시즌 테마 누적 포인트)
            MonthlyEvent,   // 월간 이벤트 (예: 월간 임무 클리어)
            RankingEvent    // 랭킹 이벤트 (예: 특정 순위 달성 또는 랭킹 점수 획득)
        }
        
        // ... 생략
    }
}
```

### 2.2 2단계: 신규 조건 전략(Strategy) 클래스 구현
`IEventCondition` 인터페이스를 구현하는 순수 C# 클래스를 생성하고, `[EventCondition]` 어트리뷰트로 장식하여 팩토리에 매핑을 통지합니다.

#### 예시 A: 길드 이벤트 조건 (`GuildEventCondition.cs`)
```csharp
using BePex.EventSystem.Interfaces;
using BePex.EventSystem.Conditions;
using BePex.EventSystem.Data;
using UnityEngine;

namespace BePex.EventSystem.Conditions
{
    /// <summary>
    /// [기능]: 길드 기부 또는 길드 레이드 참여 횟수를 누적해 판정하는 조건 전략 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    [EventCondition(ConditionDefinitionSO.ConditionType.GuildEvent)]
    public class GuildEventCondition : BaseEventCondition
    {
        public GuildEventCondition(int targetValue, ISaveSystem saveSystem, string eventId)
            : base(targetValue, saveSystem, eventId)
        {
        }

        // BaseEventCondition이 제공하는 기본 메서드(GetCurrentProgressAsync, IsCompletedAsync)가
        // 로컬 진행도 판정을 완벽하게 지원하므로, 일반적인 누적/판정 로직은 추가 구현이 필요 없습니다!
    }
}
```

#### 예시 B: 랭킹 이벤트 조건 (`RankingEventCondition.cs`)
```csharp
using BePex.EventSystem.Interfaces;
using BePex.EventSystem.Conditions;
using BePex.EventSystem.Data;
using UnityEngine;

namespace BePex.EventSystem.Conditions
{
    /// <summary>
    /// [기능]: 랭킹 보드 점수 달성 또는 특정 등수 안으로 진입했는지 판정하는 조건 전략 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    [EventCondition(ConditionDefinitionSO.ConditionType.RankingEvent)]
    public class RankingEventCondition : BaseEventCondition
    {
        public RankingEventCondition(int targetValue, ISaveSystem saveSystem, string eventId)
            : base(targetValue, saveSystem, eventId)
        {
        }

        // 랭킹은 로컬 저장이 아닌 실시간 랭킹 시스템 API를 호출해야 하므로 virtual 메서드를 오버라이드합니다.
        public override async Awaitable<int> GetCurrentProgressAsync()
        {
            // (예시) 실시간 랭킹 시스템 API 호출
            if (m_saveSystem != null)
            {
                var progress = await m_saveSystem.LoadProgressAsync(m_eventId);
                return progress.currentProgress;
            }
            return 0;
        }

        public override async Awaitable<bool> IsCompletedAsync()
        {
            int currentRank = await GetCurrentProgressAsync();
            // 등수 조건인 경우 숫자가 낮을수록(1등에 가까울수록) 완료로 판정하도록 역방향 로직 구현
            return currentRank > 0 && currentRank <= m_targetValue;
        }
    }
}
```

### 2.3 3단계: 기획 데이터 조립 (에디터 영역)
개발자가 코드를 작성하여 컴파일하면, 팩토리는 자동으로 신규 클래스를 감지합니다. 기획자는 유니티 에디터 인스펙터에서 다음과 같이 사용합니다.
1. `ConditionDefinitionSO` 에셋을 생성하고, `Condition Type` 드롭다운에서 새롭게 추가된 `GuildEvent` 또는 `RankingEvent` 등을 선택합니다.
2. 기획 수치를 입력하고 이를 `EventDefinitionSO`에 조립합니다.

### 2.4 4단계: 이벤트 관리자(어드민) UI 자동 연동
본 시스템은 리플렉션 자동 매핑을 사용하므로, 어드민 UI 드롭다운 및 양방향 한영 변환 처리를 위해 소스 코드를 수정할 필요가 없습니다. 단축된 1단계 프로세스만 적용하면 완료됩니다.

1. **[ConditionDefinitionSO.cs](file:///Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/Assets/_Game/Scripts/EventSystem/Data/ConditionDefinitionSO.cs)** 소스 코드를 엽니다.
2. `ConditionType` 이넘 필드에 원하는 한글 표시명 어트리뷰트(`[EventDisplayName]`)를 부여합니다.
   ```csharp
   public enum ConditionType
   {
       // ...
       [EventDisplayName("길드 이벤트")]
       GuildEvent,
       [EventDisplayName("랭킹 이벤트")]
       RankingEvent
   }
   ```
3. 컴파일이 완료되면 `EnumDisplayHelper`에 의해 `EventAdminView`의 조건 드롭다운에 자동으로 "길드 이벤트", "랭킹 이벤트" 항목이 추가되고 입출력 시 영한 변환 처리가 수행됩니다.

---

## 3. 새로운 보상(Reward) 타입 추가하기

새로운 보상 수령 타입(예: 경험치, 뽑기 티켓, 시즌 포인트 등)을 설계하고 유기적으로 바인딩하는 방법도 동일하게 적용됩니다.

### 3.1 1단계: 보상 형식 식별자 추가
[RewardDefinitionSO.cs](file:///Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/Assets/_Game/Scripts/EventSystem/Data/RewardDefinitionSO.cs) 내의 `RewardType` 열거형(Enum)에 새로운 타입을 정의합니다.

```csharp
namespace BePex.EventSystem.Data
{
    public class RewardDefinitionSO : ScriptableObject
    {
        public enum RewardType
        {
            Exp,            // 경험치
            Ticket,         // 뽑기 티켓
            Point,          // 일반 이벤트 포인트
            
            // --- [신규 추가 보상 타입 예시] ---
            SeasonPoint     // 시즌 포인트
        }
        
        // ... 생략
    }
}
```

### 3.2 2단계: 신규 보상 전략(Strategy) 클래스 구현
`IEventReward` 인터페이스를 구현하는 클래스를 만들고, `[EventReward]` 어트리뷰트로 열거형과 관계를 매핑합니다.

#### 예시 C: 뽑기 티켓 보상 (`TicketReward.cs` 구현 예시)
```csharp
using BePex.EventSystem.Interfaces;
using BePex.EventSystem.Rewards;
using BePex.EventSystem.Data;
using BePex.EventSystem.Models;
using UnityEngine;

namespace BePex.EventSystem.Rewards
{
    /// <summary>
    /// [기능]: 수량에 맞춰 뽑기 티켓 재화를 플레이어 정보에 지급하는 보상 전략 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    [EventReward(RewardDefinitionSO.RewardType.Ticket)]
    public class TicketReward : BaseEventReward
    {
        public TicketReward(int amount, string displayName)
            : base(amount, displayName)
        {
        }

        // BaseEventReward의 GetRewardName()과 GetRewardAmount()는 이미 구현되어 있으므로,
        // 실제 보상 지급을 처리하는 Grant 추상 메서드만 오버라이드하면 됩니다.
        public override void Grant(PlayerRewardModel playerReward)
        {
            if (playerReward != null)
            {
                // 인게임 플레이어 인벤토리 모델의 티켓 항목 가산
                playerReward.totalTickets += m_amount;
                Debug.Log($"[TicketReward] '{m_displayName}'이(가) 정상 지급되었습니다. (수량: +{m_amount}개)");
            }
        }
    }
}
```

#### 예시 D: 시즌 포인트 보상 (`SeasonPointReward.cs` 구현 예시)
```csharp
using BePex.EventSystem.Interfaces;
using BePex.EventSystem.Rewards;
using BePex.EventSystem.Data;
using BePex.EventSystem.Models;
using UnityEngine;

namespace BePex.EventSystem.Rewards
{
    /// <summary>
    /// [기능]: 시즌패스 진행에 기여하는 전용 시즌 포인트를 지급하는 보상 전략 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    [EventReward(RewardDefinitionSO.RewardType.SeasonPoint)]
    public class SeasonPointReward : BaseEventReward
    {
        public SeasonPointReward(int amount, string displayName)
            : base(amount, displayName)
        {
        }

        public override void Grant(PlayerRewardModel playerReward)
        {
            if (playerReward != null)
            {
                // 플레이어 데이터 내 시즌패스 경험치/포인트 가산
                playerReward.totalSeasonPoints += m_amount;
                Debug.Log($"[SeasonPointReward] '{m_displayName}'이(가) 정상 지급되었습니다. (시즌 포인트: +{m_amount}P)");
            }
        }
    }
}
```

### 3.3 3단계: 이벤트 관리자(어드민) UI 자동 연동
보상 타입 역시 자동화 매핑을 지원하므로 UI 뷰의 스위치 분기 수정 없이 다음 단계를 수행하면 어드민 패널에 등록되고 입력할 수 있습니다.

1. **[RewardDefinitionSO.cs](file:///Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/Assets/_Game/Scripts/EventSystem/Data/RewardDefinitionSO.cs)** 소스 코드를 엽니다.
2. `RewardType` 이넘 필드에 원하는 한글 표시명 어트리뷰트(`[EventDisplayName]`)를 부여합니다.
   ```csharp
   public enum RewardType
   {
       // ...
       [EventDisplayName("시즌 포인트")]
       SeasonPoint
   }
   ```
3. 컴파일이 완료되면 `EnumDisplayHelper`에 의해 `EventAdminView`의 신규 보상 종류 드롭다운과 `EventAdminRewardRowView`의 보상 목록 뷰 드롭다운 모두에 자동으로 "시즌 포인트" 항목이 추가되고 입출력 시 영한 변환 처리가 수행됩니다.
4. 씬 하이어라키의 `NewReward_InputGroup` 하위에 신규 보상 입력 필드군(종류, 수량, 표시명, 아이콘)이 이미 배치 완료되었고 `EventAdminView`에 사전 바인딩되어 있으므로, 새로운 데이터 형식 추가 시 별도의 UI 조작 없이 컴파일 완료 즉시 정상 연동됩니다.

---

## 4. 확장 작업 시 핵심 체크리스트

1. **순수 C# POCO 준수**:
   새롭게 작성하는 조건 및 보상 클래스는 `BaseEventCondition` 및 `BaseEventReward` 추상 클래스를 상속받는 순수 C# 도메인 클래스여야 합니다. `MonoBehaviour`를 절대 상속받지 마십시오.
2. **어트리뷰트 데코레이터 선언**:
   클래스 정의 바로 위에 반드시 `[EventCondition(ConditionDefinitionSO.ConditionType.<타입>)]` 또는 `[EventReward(RewardDefinitionSO.RewardType.<타입>)]`를 장식해야 팩토리 레지스트리 자동화 기작이 누락되지 않고 등록됩니다.
3. **생성자 구조 준수**:
   리플렉션 팩토리의 `Activator.CreateInstance` 바인딩 규격 상, 클래스의 생성자 파라미터 시그니처가 다음과 같이 통일되어 있어야 정상 작동합니다:
   * **Condition**: `(int targetValue, ISaveSystem saveSystem, string eventId)`
   * **Reward**: `(int amount, string displayName)`
