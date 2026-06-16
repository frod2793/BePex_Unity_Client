# BePex Unity Client — 이벤트 시스템 확장성 방법 안내서

> **작성자**: 윤승종  
> **작성일**: 2026-06-16  
> **대상**: 개발자 및 기획자  

---

## 1. 확장성 설계 개요

본 이벤트 시스템은 새로운 기능 요구사항이 발생했을 때 기존 코드의 직접적인 수정을 우회하고 결합도를 낮추는 **개방-폐쇄 원칙(OCP, Open-Closed Principle)**을 엄격하게 준수합니다.

이전의 취약한 소스 코드 문자열 치환(Enum) 방식에서 벗어나, 이제 **Type Object 패턴**을 적용하여 유니티 에셋 데이터와 문자열 키 기반으로 동작합니다.
시스템은 다음과 같은 핵심 디자인 패턴의 유기적인 결합을 통해 무한한 확장을 지원합니다:
1. **Strategy (전략) 패턴**: 개별 조건 판정(`IQuestCondition`)과 보상 지급(`IQuestReward`)을 각각 독자적인 알고리즘 클래스로 캡슐화합니다.
2. **Reflection-based Factory (리플렉션 기반 팩토리)**: 하드코딩된 분기문(`switch-case`)이나 이넘 매핑 테이블 없이, 어셈블리 내 어트리뷰트(`Attribute`) 장식을 스캔하여 조건/보상 매핑 사전을 런타임 시작 시 1회 자동 빌드합니다.

이로 인해 새로운 조건이나 보상이 추가되어도 **기존 팩토리 클래스(`QuestConditionFactory`, `QuestRewardFactory`)의 소스 코드는 전혀 수정할 필요가 없습니다.**

---

## 1.5 에디터 자동화 도구를 활용한 빠른 확장 (권장)

수작업으로 인한 보일러플레이트 코드 작성 및 직렬화 에셋 생성의 번거로움을 줄이기 위해, 유니티 에디터 상에서 자동으로 코드를 추가하고 에셋을 생성 및 레지스트리에 자동 등록해주는 **이벤트 시스템 확장 도구**를 제공합니다.

### 🛠️ 도구 실행 방법
1. 유니티 에디터 상단 메뉴 바에서 **Tools > BePex > 이벤트 시스템 확장 도구**를 선택합니다.
2. 실행된 창에서 아래 항목들을 입력합니다:
   - **확장 대상**: `이벤트 타입` (조건 확장 시) 또는 `보상 타입` (보상 확장 시) 선택.
   - **식별자 영문명**: 추가할 타입의 영문명 입력 (예: `GuildEvent`, `SeasonPoint`).
   - **표시명 한글명**: 에디터 인스펙터나 어드민 UI에 출력될 한글 표시명 입력 (예: `길드 이벤트`, `시즌 포인트`).
3. **확장 파일 생성 및 등록** 버튼을 클릭합니다.

### ⚡ 자동화 동작 과정
- **직렬화 에셋 자동 생성 및 등록**:
  - `ConditionTypeSO` 또는 `RewardTypeSO` 에셋 파일(각각 `Assets/_Game/Data/ConditionTypes/{TypeName}.asset` 및 `Assets/_Game/Data/RewardTypes/{TypeName}.asset`)을 자동 생성합니다.
  - 생성된 타입 에셋은 해당하는 레지스트리 에셋(`ConditionTypeRegistry.asset` 또는 `RewardTypeRegistry.asset`)의 활성 목록에 즉시 자동 등록되어 실시간으로 반영됩니다.
- **클래스 템플릿 자동 생성**:
  - **이벤트 타입(조건)**: `Assets/_Game/Scripts/EventSystem/Conditions/` 경로에 `{영문명}QuestCondition.cs`가 자동 생성되며, `BaseQuestCondition` 상속 및 적절한 `[QuestCondition("{영문명}")]` 문자열 어트리뷰트 장식이 완성됩니다.
  - **보상 타입**: `Assets/_Game/Scripts/EventSystem/Rewards/` 경로에 `{영문명}QuestReward.cs`가 자동 생성되며, `BaseQuestReward` 상속 및 보상 지급을 수행하는 `Grant` 구현부 형태의 템플릿 코드가 생성됩니다.
- **컴파일 자동 트리거**: 파일 생성 직후 `AssetDatabase.Refresh()`가 호출되어 변경 사항이 프로젝트 컴파일에 즉시 반영됩니다.

개발자는 도구 실행 완료 후 생성된 클래스 내부에서 고유한 판정/지급 로직만 작성해주면 즉시 확장 작업이 완료됩니다.

---

## 2. 새로운 이벤트(조건) 타입 추가하기 (수동)

새로운 이벤트 달성 조건(예: 길드 이벤트, 시즌 이벤트, 월간 이벤트, 랭킹 이벤트)을 추가해야 할 경우, 개발자는 다음 3단계 프로세스만 수행하면 됩니다.

### 2.1 1단계: 조건 형식 식별자 에셋 추가 및 레지스트리 등록
1. 유니티 에디터 인스펙터 또는 프로젝트 뷰의 `Assets/_Game/Data/ConditionTypes/` 경로 내 빈 공간을 우클릭하고 `Create > BePex > Condition Type`을 선택하여 `{영문명}.asset`을 생성합니다.
2. 생성된 에셋의 인스펙터에서 `Type Name`(영문 식별 키, 예: `GuildEvent`) 및 `Display Name`(한글 표시명, 예: `길드 이벤트`)을 설정합니다.
3. `Assets/_Game/Data/ConditionTypeRegistry.asset` 에셋의 Conditions 목록 슬롯에 생성한 에셋을 끌어다 추가합니다.

### 2.2 2단계: 신규 조건 전략(Strategy) 클래스 구현
`IQuestCondition` 인터페이스를 구현하는 순수 C# 클래스를 생성하고, `[QuestCondition("식별키")]` 어트리뷰트로 장식하여 팩토리에 매핑을 통지합니다.

#### 예시 A: 길드 이벤트 조건 (`GuildQuestCondition.cs`)
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
    [QuestCondition("GuildEvent")]
    public class GuildQuestCondition : BaseQuestCondition
    {
        public GuildQuestCondition(int targetValue, ISaveSystem saveSystem, ITimeProvider timeProvider, string eventId, string questId)
            : base(targetValue, saveSystem, timeProvider, eventId, questId)
        {
        }

        // BaseQuestCondition이 제공하는 기본 메서드(GetCurrentProgressAsync, IsCompletedAsync)가
        // 로컬 진행도 판정을 완벽하게 지원하므로, 일반적인 누적/판정 로직은 추가 구현이 필요 없습니다!
    }
}
```

### 2.3 3단계: 기획 데이터 조립 (에디터 영역)
개발자가 코드를 작성하여 컴파일하면, 팩토리는 자동으로 신규 클래스를 감지합니다. 기획자는 유니티 에디터 인스펙터에서 다음과 같이 사용합니다.
1. `ConditionDefinitionSO` 에셋을 생성하고, `Condition Type` 슬롯에 새롭게 추가한 `GuildEvent.asset` 에셋을 할당합니다.
2. 기획 수치를 입력하고 이를 `EventDefinitionSO`에 조립합니다.

### 2.4 4단계: 이벤트 관리자(어드민) UI 자동 연동
본 시스템은 에셋 레지스트리를 자동으로 참조하여 드롭다운을 그리고 OCP 매핑을 수행하므로, 어드민 UI의 소스 코드를 수정할 필요가 없습니다. 
- 재생(Play) 시 `EventAdminView` 컴포넌트는 `EventAdminViewModel.GetAvailableConditionTypes()`를 통해 레지스트리 정보를 그대로 구독하여, 동적으로 조건 드롭다운에 "길드 이벤트" 항목을 바인딩하고 값을 에디터 JSON 파일에 직렬화 매핑합니다.

---

## 3. 새로운 보상(Reward) 타입 추가하기 (수동)

새로운 보상 수령 타입(예: 경험치, 뽑기 티켓, 시즌 포인트 등)을 설계하고 유기적으로 바인딩하는 방법도 동일하게 적용됩니다.

### 3.1 1단계: 보상 형식 식별자 에셋 추가 및 레지스트리 등록
1. 프로젝트 뷰의 `Assets/_Game/Data/RewardTypes/` 경로 내 빈 공간을 우클릭하고 `Create > BePex > Reward Type`을 선택하여 `{영문명}.asset`을 생성합니다.
2. 생성된 에셋의 인스펙터에서 `Type Name`(영문 식별 키, 예: `SeasonPoint`) 및 `Display Name`(한글 표시명, 예: `시즌 포인트`)을 설정합니다.
3. `Assets/_Game/Data/RewardTypeRegistry.asset` 에셋의 Rewards 목록 슬롯에 생성한 에셋을 추가합니다.

### 3.2 2단계: 신규 보상 전략(Strategy) 클래스 구현
`IQuestReward` 인터페이스를 구현하는 클래스를 만들고, `[QuestReward("식별키")]` 어트리뷰트로 관계를 매핑합니다.

#### 예시 B: 시즌 포인트 보상 (`SeasonPointQuestReward.cs` 구현 예시)
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
    [QuestReward("SeasonPoint")]
    public class SeasonPointQuestReward : BaseQuestReward
    {
        public SeasonPointQuestReward(int amount, string displayName)
            : base(amount, displayName)
        {
        }

        public override void Grant(PlayerRewardModel playerReward)
        {
            if (playerReward != null)
            {
                // 플레이어 데이터 내 시즌패스 경험치/포인트 가산 (문자열 식별 키 활용)
                playerReward.AddCurrency("SeasonPoint", m_amount);
                Debug.Log($"[SeasonPointQuestReward] '{m_displayName}'이(가) 정상 지급되었습니다. (시즌 포인트: +{m_amount}P)");
            }
        }
    }
}
```

### 3.3 3단계: 이벤트 관리자(어드민) UI 자동 연동
보상 타입 역시 자동화 매핑을 지원하므로 UI 뷰의 스위치 분기 수정 없이 1단계만 완료하면 어드민 패널에 등록되고 편집할 수 있습니다.
- `EventAdminView`와 `EventAdminRewardRowView`는 `RewardTypeRegistry.asset`에 기반한 보상 타입 리스트를 뷰모델을 통해 받아와 드롭다운을 자동으로 채웁니다. 
- 사용자가 보상을 생성하고 입력값을 저장하면, 에디터 및 런타임 저장소(JSON) 상의 `rewardType` 필드에 설정한 `"SeasonPoint"` 문자열이 직접 기입되어 연동됩니다.

---

## 4. 확장 작업 시 핵심 체크리스트

1. **순수 C# POCO 준수**:
   새롭게 작성하는 조건 및 보상 클래스는 `BaseQuestCondition` 및 `BaseQuestReward` 추상 클래스를 상속받는 순수 C# 도메인 클래스여야 합니다. `MonoBehaviour`를 절대 상속받지 마십시오.
2. **어트리뷰트 데코레이터 선언**:
   클래스 정의 바로 위에 반드시 `[QuestCondition("<영문 식별키>")]` 또는 `[QuestReward("<영문 식별키>")]`를 문자열 상수로 정확하게 장식해야 팩토리 레지스트리 자동화 기작이 누락되지 않고 등록됩니다.
3. **생성자 구조 준수**:
   리플렉션 팩토리의 `Activator.CreateInstance` 바인딩 규격 상, 클래스의 생성자 파라미터 시그니처가 다음과 같이 통일되어 있어야 정상 작동합니다:
   - **Condition**: `(int targetValue, ISaveSystem saveSystem, ITimeProvider timeProvider, string eventId, string questId)`
   - **Reward**: `(int amount, string displayName)`
