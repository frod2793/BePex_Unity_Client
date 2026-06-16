# 이벤트 시스템 리팩토링 구현 계획서 (Event System Refactoring Plan)

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 이벤트 시스템 모듈 내 조건/보상 클래스의 중복을 추상화 구조로 소거하고, 누락된 XML 주석 헤더 및 Allman 괄호 개행, Unity 안정성 수칙(Fake Null 체크 및 루프 가비지 누적 제거)을 완벽히 수리 및 전수 적용합니다.

**Architecture:** `BaseEventCondition` 추상 클래스를 파생 조건들의 부모로 삼아 데이터 연산을 공통화하고, `BaseEventReward` 추상 클래스로 포인트, 경험치, 티켓 보상의 중복을 흡수합니다. 인라인 람다식을 func_ 접두사가 붙은 UI 이벤트 콜백 메서드로 추출하고, XML 주석 규격을 100% 완성합니다.

**Tech Stack:** Unity 6 (Awaitable), NUnit Framework, Pure C#

---

### Task 1: BaseEventCondition 및 BaseEventReward 추상 기반 클래스 설계

**Files:**
- Create: `Assets/_Game/Scripts/EventSystem/Conditions/BaseEventCondition.cs`
- Create: `Assets/_Game/Scripts/EventSystem/Rewards/BaseEventReward.cs`
- Modify: `Assets/_Game/Scripts/EventSystem/Interfaces/IEventCondition.cs`
- Modify: `Assets/_Game/Scripts/EventSystem/Interfaces/IEventReward.cs`

- [ ] **Step 1: IEventCondition 인터페이스 검증 및 BaseEventCondition 작성**
  - 아래의 코드를 [BaseEventCondition.cs](file:///Assets/_Game/Scripts/EventSystem/Conditions/BaseEventCondition.cs) 경로로 생성합니다.
  ```csharp
  /// <summary>
  /// [기능]: 공통적인 이벤트 조건의 진행도 로드 및 달성 상태 비교 로직을 제공하는 추상 기반 클래스.
  /// [작성자]: 윤승종
  /// </summary>
  using UnityEngine;
  using BePex.EventSystem.Interfaces;

  namespace BePex.EventSystem.Conditions
  {
      public abstract class BaseEventCondition : IEventCondition
      {
          #region 내부 필드
          protected readonly int m_targetValue;
          protected readonly ISaveSystem m_saveSystem;
          protected readonly string m_eventId;
          #endregion

          #region 초기화
          /// <summary>
          /// [기능]: 기반 조건 인스턴스에 필요한 의존성을 주입받아 초기화합니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 정의
          /// </summary>
          protected BaseEventCondition(int targetValue, ISaveSystem saveSystem, string eventId)
          {
              m_targetValue = targetValue;
              m_saveSystem = saveSystem;
              m_eventId = eventId;
          }
          #endregion

          #region 공개 메서드
          /// <summary>
          /// [기능]: 해당 이벤트의 누적 진행도를 세이브 데이터로부터 비동기 로드합니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 정의
          /// </summary>
          public virtual async Awaitable<int> GetCurrentProgressAsync()
          {
              var progress = await m_saveSystem.LoadProgressAsync(m_eventId);
              return progress.currentProgress;
          }

          /// <summary>
          /// [기능]: 설정된 목표 수치를 반환합니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 정의
          /// </summary>
          public int GetTargetValue()
          {
              return m_targetValue;
          }

          /// <summary>
          /// [기능]: 목표 진행 수치에 도달하여 완료되었는지 비동기로 판정합니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 정의
          /// </summary>
          public virtual async Awaitable<bool> IsCompletedAsync()
          {
              int currentProgress = await GetCurrentProgressAsync();
              return currentProgress >= m_targetValue;
          }
          #endregion
      }
  }
  ```

- [ ] **Step 2: BaseEventReward 작성**
  - 아래의 코드를 [BaseEventReward.cs](file:///Assets/_Game/Scripts/EventSystem/Rewards/BaseEventReward.cs) 경로로 생성합니다.
  ```csharp
  /// <summary>
  /// [기능]: 공통적인 보상 정보 관리 및 조회를 제공하는 추상 기반 클래스.
  /// [작성자]: 윤승종
  /// </summary>
  using BePex.EventSystem.Interfaces;
  using BePex.EventSystem.Models;

  namespace BePex.EventSystem.Rewards
  {
      public abstract class BaseEventReward : IEventReward
      {
          #region 내부 필드
          protected readonly int m_amount;
          protected readonly string m_displayName;
          #endregion

          #region 초기화
          /// <summary>
          /// [기능]: 지급할 보상 수량 및 표시 이름을 주입받는 생성자.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 정의
          /// </summary>
          protected BaseEventReward(int amount, string displayName)
          {
              m_amount = amount;
              m_displayName = displayName;
          }
          #endregion

          #region 공개 메서드
          /// <summary>
          /// [기능]: 보상의 노출 이름을 반환합니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 정의
          /// </summary>
          public string GetRewardName()
          {
              return m_displayName;
          }

          /// <summary>
          /// [기능]: 보상 수량을 반환합니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 정의
          /// </summary>
          public int GetRewardAmount()
          {
              return m_amount;
          }

          /// <summary>
          /// [기능]: 플레이어에게 보상을 수령 및 적립하도록 파생 클래스에 위임하는 가상 비즈니스 추상 메서드.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 정의
          /// </summary>
          public abstract void Grant(PlayerRewardModel playerReward);
          #endregion
      }
  }
  ```

- [ ] **Step 3: Commit**
  ```bash
  git add Assets/_Game/Scripts/EventSystem/Conditions/BaseEventCondition.cs Assets/_Game/Scripts/EventSystem/Rewards/BaseEventReward.cs
  git commit -m "refactor: add BaseEventCondition and BaseEventReward abstract classes"
  ```

---

### Task 2: StageClearCondition, AttendanceCondition, KillCountCondition 리팩토링

**Files:**
- Modify: `Assets/_Game/Scripts/EventSystem/Conditions/StageClearCondition.cs`
- Modify: `Assets/_Game/Scripts/EventSystem/Conditions/AttendanceCondition.cs`
- Modify: `Assets/_Game/Scripts/EventSystem/Conditions/KillCountCondition.cs`

- [ ] **Step 1: StageClearCondition 수정**
  - 부모 클래스 `BaseEventCondition`을 상속받도록 수정하고 중복 멤버와 메서드를 제거합니다.
  ```csharp
  using UnityEngine;
  using BePex.EventSystem.Interfaces;
  using BePex.EventSystem.Data;

  namespace BePex.EventSystem.Conditions
  {
      /// <summary>
      /// [기능]: 스테이지 클리어 횟수를 이벤트 완료 조건으로 달성하였는지 판정하는 Strategy 클래스.
      /// [작성자]: 윤승종
      /// </summary>
      [EventCondition(ConditionDefinitionSO.ConditionType.StageClear)]
      public class StageClearCondition : BaseEventCondition
      {
          #region 초기화
          /// <summary>
          /// [기능]: 부모 생성자를 경유해 목표 스테이지 클리어 수치, 세이브장치 및 이벤트 ID를 주입받습니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: BaseEventCondition 상속을 활용해 단순화
          /// </summary>
          public StageClearCondition(int targetValue, ISaveSystem saveSystem, string eventId)
              : base(targetValue, saveSystem, eventId)
          {
          }
          #endregion
      }
  }
  ```

- [ ] **Step 2: AttendanceCondition 수정**
  - 부모 클래스 `BaseEventCondition`을 상속받도록 수정하고 중복 멤버와 메서드를 제거합니다.
  ```csharp
  using UnityEngine;
  using BePex.EventSystem.Interfaces;
  using BePex.EventSystem.Data;

  namespace BePex.EventSystem.Conditions
  {
      /// <summary>
      /// [기능]: 출석 체크 일수를 이벤트 완료 조건으로 달성하였는지 판정하는 Strategy 클래스.
      /// [작성자]: 윤승종
      /// </summary>
      [EventCondition(ConditionDefinitionSO.ConditionType.Attendance)]
      public class AttendanceCondition : BaseEventCondition
      {
          #region 초기화
          /// <summary>
          /// [기능]: 부모 생성자를 경유해 목표 출석 체크 일수, 세이브장치 및 이벤트 ID를 주입받습니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: BaseEventCondition 상속을 활용해 단순화
          /// </summary>
          public AttendanceCondition(int targetValue, ISaveSystem saveSystem, string eventId)
              : base(targetValue, saveSystem, eventId)
          {
          }
          #endregion
      }
  }
  ```

- [ ] **Step 3: KillCountCondition 수정**
  - 부모 클래스 `BaseEventCondition`을 상속받도록 수정하고 중복 멤버와 메서드를 제거합니다.
  ```csharp
  using UnityEngine;
  using BePex.EventSystem.Interfaces;
  using BePex.EventSystem.Data;

  namespace BePex.EventSystem.Conditions
  {
      /// <summary>
      /// [기능]: 적 처치 수를 이벤트 완료 조건으로 달성하였는지 판정하는 Strategy 클래스.
      /// [작성자]: 윤승종
      /// </summary>
      [EventCondition(ConditionDefinitionSO.ConditionType.KillCount)]
      public class KillCountCondition : BaseEventCondition
      {
          #region 초기화
          /// <summary>
          /// [기능]: 부모 생성자를 경유해 목표 처치 횟수, 세이브장치 및 해당 이벤트 ID를 매핑받습니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: BaseEventCondition 상속을 활용해 단순화
          /// </summary>
          public KillCountCondition(int targetValue, ISaveSystem saveSystem, string eventId)
              : base(targetValue, saveSystem, eventId)
          {
          }
          #endregion
      }
  }
  ```

- [ ] **Step 4: Commit**
  ```bash
  git add Assets/_Game/Scripts/EventSystem/Conditions/StageClearCondition.cs Assets/_Game/Scripts/EventSystem/Conditions/AttendanceCondition.cs Assets/_Game/Scripts/EventSystem/Conditions/KillCountCondition.cs
  git commit -m "refactor: simplify conditions using BaseEventCondition"
  ```

---

### Task 3: ExpReward, PointReward, TicketReward 리팩토링

**Files:**
- Modify: `Assets/_Game/Scripts/EventSystem/Rewards/ExpReward.cs`
- Modify: `Assets/_Game/Scripts/EventSystem/Rewards/PointReward.cs`
- Modify: `Assets/_Game/Scripts/EventSystem/Rewards/TicketReward.cs`

- [ ] **Step 1: ExpReward 수정**
  - `BaseEventReward`를 상속받도록 수정하고 중복 필드 및 메서드를 제거합니다.
  ```csharp
  using BePex.EventSystem.Models;
  using BePex.EventSystem.Data;

  namespace BePex.EventSystem.Rewards
  {
      /// <summary>
      /// [기능]: 플레이어 자산에 이벤트 완료 보상으로 경험치를 부여해 주는 Strategy 클래스.
      /// [작성자]: 윤승종
      /// </summary>
      [EventReward(RewardDefinitionSO.RewardType.Exp)]
      public class ExpReward : BaseEventReward
      {
          #region 초기화
          /// <summary>
          /// [기능]: 부모 생성자를 경유해 지급할 경험치 수량 및 표시 이름을 주입받습니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: BaseEventReward 상속을 활용해 단순화
          /// </summary>
          public ExpReward(int amount, string displayName)
              : base(amount, displayName)
          {
          }
          #endregion

          #region 공개 메서드
          /// <summary>
          /// [기능]: 플레이어의 누적 경험치에 보상 수량을 더해 지급합니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 작성
          /// </summary>
          public override void Grant(PlayerRewardModel playerReward)
          {
              if (playerReward != null)
              {
                  playerReward.totalExp += m_amount;
              }
          }
          #endregion
      }
  }
  ```

- [ ] **Step 2: PointReward 수정**
  - `BaseEventReward`를 상속받도록 수정하고 중복 필드 및 메서드를 제거합니다.
  ```csharp
  using BePex.EventSystem.Models;
  using BePex.EventSystem.Data;

  namespace BePex.EventSystem.Rewards
  {
      /// <summary>
      /// [기능]: 플레이어 자산에 이벤트 상점 등에서 쓸 포인트 보상을 적립해 주는 Strategy 클래스.
      /// [작성자]: 윤승종
      /// </summary>
      [EventReward(RewardDefinitionSO.RewardType.Point)]
      public class PointReward : BaseEventReward
      {
          #region 초기화
          /// <summary>
          /// [기능]: 부모 생성자를 경유해 지급할 포인트 수량 및 표시 이름을 주입받습니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: BaseEventReward 상속을 활용해 단순화
          /// </summary>
          public PointReward(int amount, string displayName)
              : base(amount, displayName)
          {
          }
          #endregion

          #region 공개 메서드
          /// <summary>
          /// [기능]: 플레이어의 누적 포인트에 보상 수량을 더해 지급합니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 작성
          /// </summary>
          public override void Grant(PlayerRewardModel playerReward)
          {
              if (playerReward != null)
              {
                  playerReward.totalPoints += m_amount;
              }
          }
          #endregion
      }
  }
  ```

- [ ] **Step 3: TicketReward 수정**
  - `BaseEventReward`를 상속받도록 수정하고 중복 필드 및 메서드를 제거합니다.
  ```csharp
  using BePex.EventSystem.Models;
  using BePex.EventSystem.Data;

  namespace BePex.EventSystem.Rewards
  {
      /// <summary>
      /// [기능]: 플레이어 자산에 이벤트 응모용 티켓을 보상으로 가산해 주는 Strategy 클래스.
      /// [작성자]: 윤승종
      /// </summary>
      [EventReward(RewardDefinitionSO.RewardType.Ticket)]
      public class TicketReward : BaseEventReward
      {
          #region 초기화
          /// <summary>
          /// [기능]: 부모 생성자를 경유해 지급할 티켓 수량 및 표시 이름을 주입받습니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: BaseEventReward 상속을 활용해 단순화
          /// </summary>
          public TicketReward(int amount, string displayName)
              : base(amount, displayName)
          {
          }
          #endregion

          #region 공개 메서드
          /// <summary>
          /// [기능]: 플레이어의 누적 티켓 수에 보상 수량을 더해 지급합니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 작성
          /// </summary>
          public override void Grant(PlayerRewardModel playerReward)
          {
              if (playerReward != null)
              {
                  playerReward.totalTickets += m_amount;
              }
          }
          #endregion
      }
  }
  ```

- [ ] **Step 4: Commit**
  ```bash
  git add Assets/_Game/Scripts/EventSystem/Rewards/ExpReward.cs Assets/_Game/Scripts/EventSystem/Rewards/PointReward.cs Assets/_Game/Scripts/EventSystem/Rewards/TicketReward.cs
  git commit -m "refactor: simplify rewards using BaseEventReward"
  ```

---

### Task 4: XML 주석 누락 보강 & SeasonPassModel 리스트 생성자 이관

**Files:**
- Modify: `Assets/_Game/Scripts/EventSystem/ViewModels/EventListViewModel.cs`
- Modify: `Assets/_Game/Scripts/EventSystem/ViewModels/EventDetailViewModel.cs`
- Modify: `Assets/_Game/Scripts/EventSystem/Views/EventDetailView.cs`
- Modify: `Assets/_Game/Scripts/EventSystem/Views/RewardPopupView.cs`
- Modify: `Assets/_Game/Scripts/EventSystem/Views/EventListView.cs`
- Modify: `Assets/_Game/Scripts/EventSystem/Factories/ConditionFactory.cs`
- Modify: `Assets/_Game/Scripts/EventSystem/Factories/RewardFactory.cs`
- Modify: `Assets/_Game/Scripts/EventSystem/Models/SeasonPassModel.cs`

- [ ] **Step 1: ViewModel 내 누락된 XML 주석 작성**
  - `EventListViewModel.cs`의 `HandleEventProgressChanged` 와 `HandleEventRewardClaimed` 메서드 위에 XML 주석을 작성합니다.
  ```csharp
  /// <summary>
  /// [기능]: 이벤트 모델의 진행도 가산 상태 변경 알림을 수신해 목록 갱신 이벤트를 전송합니다.
  /// [작성자]: 윤승종
  /// [수정 날짜]: 2026-06-14
  /// [마지막 수정 작성자]: 윤승종
  /// [수정 내용]: XML 주석 보완
  /// </summary>
  private void HandleEventProgressChanged(string eventId)
  ```
  - `EventDetailViewModel.cs`의 `HandleProgressChanged` 와 `HandleRewardClaimed` 메서드 위에 XML 주석을 작성합니다.
  ```csharp
  /// <summary>
  /// [기능]: 모델 진행 변경에 반응해 상세 UI 다시 그리기 통지를 전송합니다.
  /// [작성자]: 윤승종
  /// [수정 날짜]: 2026-06-14
  /// [마지막 수정 작성자]: 윤승종
  /// [수정 내용]: XML 주석 보완
  /// </summary>
  private void HandleProgressChanged(string eventId)
  ```

- [ ] **Step 2: View 내 누락된 XML 주석 작성**
  - `EventDetailView.cs`의 `OnDestroy`, `func_OnDetailUpdatedWrapper`에 주석 추가.
  - `RewardPopupView.cs`의 `OnDestroy`에 주석 추가.
  - `EventListView.cs`의 `OnDestroy`에 주석 추가.

- [ ] **Step 3: Factory 내 누락된 XML 주석 작성**
  - `ConditionFactory.cs`와 `RewardFactory.cs`의 `BuildRegistry`에 주석 추가.
  ```csharp
  /// <summary>
  /// [기능]: 리플렉션을 통해 어셈블리 내 해당 어트리뷰트 타입 전략 클래스들을 레지스트리에 자동 등록합니다.
  /// [작성자]: 윤승종
  /// [수정 날짜]: 2026-06-14
  /// [마지막 수정 작성자]: 윤승종
  /// [수정 내용]: XML 주석 보완
  /// </summary>
  private void BuildRegistry()
  ```

- [ ] **Step 4: SeasonPassModel 생성자 및 리스트 초기화 이관**
  - `SeasonPassModel.cs`의 클래스 주석을 보강하고, 리스트 멤버 변수를 기본 생성자 내부에서 정의하도록 수정합니다.
  ```csharp
  using System.Collections.Generic;

  namespace BePex.EventSystem.Models
  {
      /// <summary>
      /// [기능]: 사용자의 시즌패스 진행 레벨, 획득한 EXP, 수령 완료한 보상 리스트 상태를 저장하는 순수 데이터 모델 (DTO).
      /// [작성자]: 윤승종
      /// </summary>
      [System.Serializable]
      public class SeasonPassModel
      {
          #region 데이터 멤버
          public string passId;
          public int currentExp;
          public int currentLevel;
          public bool isPremiumActive;

          public List<int> claimedFreeLevels;
          public List<int> claimedPremiumLevels;
          #endregion

          #region 초기화
          /// <summary>
          /// [기능]: 기본 생성자를 호출해 무료/프리미엄 보상 클레임 레벨 리스트를 안전하게 인스턴싱합니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 필드 이니셜라이저를 생성자 초기화 방식으로 이관
          /// </summary>
          public SeasonPassModel()
          {
              claimedFreeLevels = new List<int>();
              claimedPremiumLevels = new List<int>();
              passId = string.Empty;
              currentExp = 0;
              currentLevel = 0;
              isPremiumActive = false;
          }
          #endregion
      }
  }
  ```

- [ ] **Step 5: Commit**
  ```bash
  git add Assets/_Game/Scripts/EventSystem/ViewModels/EventListViewModel.cs Assets/_Game/Scripts/EventSystem/ViewModels/EventDetailViewModel.cs Assets/_Game/Scripts/EventSystem/Views/EventDetailView.cs Assets/_Game/Scripts/EventSystem/Views/RewardPopupView.cs Assets/_Game/Scripts/EventSystem/Views/EventListView.cs Assets/_Game/Scripts/EventSystem/Factories/ConditionFactory.cs Assets/_Game/Scripts/EventSystem/Factories/RewardFactory.cs Assets/_Game/Scripts/EventSystem/Models/SeasonPassModel.cs
  git commit -m "docs: complete missing XML comments and SeasonPassModel constructor"
  ```

---

### Task 5: EventItemCell 인라인 람다를 func_ 메서드로 분리

**Files:**
- Modify: `Assets/_Game/Scripts/EventSystem/Views/EventItemCell.cs`

- [ ] **Step 1: EventItemCell 수정**
  - 83번 라인 부근의 `m_selectButton` 클릭 리스너의 익명 람다식을 별도의 `func_` 메서드로 분리합니다.
  - `LoadSpriteAsync` 메서드에 XML 주석을 추가합니다.
  ```csharp
  using System;
  using UnityEngine;
  using UnityEngine.UI;
  using BePex.EventSystem.DTOs;
  using BePex.EventSystem.ViewModels;
  using TMPro;
  using UnityEngine.AddressableAssets;

  namespace BePex.EventSystem.Views
  {
      /// <summary>
      /// [기능]: 이벤트 리스트 뷰 내부의 개별 항목 셀을 DTO 데이터를 활용해 렌더링하고 사용자 입력을 뷰모델에 전달하는 View 클래스.
      /// [작성자]: 윤승종
      /// </summary>
      public class EventItemCell : MonoBehaviour
      {
          #region UI 참조 (Inspector)
          [SerializeField] private TextMeshProUGUI m_titleText;
          [SerializeField] private Image m_iconImage;
          [SerializeField] private Button m_selectButton;
          #endregion

          #region 내부 필드
          private EventDefinitionDTO m_definition;
          private EventListViewModel m_viewModel;
          private Action<string> m_onSelectAction;
          #endregion

          #region 공개 메서드
          /// <summary>
          /// [기능]: 이벤트 DTO 데이터 및 뷰모델 인스턴스를 주입받아 UI 텍스트를 구성하고, 어드레서블 주소로 스프라이트를 비동기 로드 및 바인딩합니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: ScriptableObject 의존성을 EventDefinitionDTO로 교체 및 어드레서블 스프라이트 지연 로딩 추가
          /// </summary>
          public void Setup(EventDefinitionDTO definition, EventListViewModel viewModel)
          {
              m_definition = definition;
              m_viewModel = viewModel;
              m_onSelectAction = null;

              if (m_titleText != null)
              {
                  m_titleText.text = m_definition.eventTitle;
              }

              if (m_iconImage != null && !string.IsNullOrEmpty(m_definition.eventIconAddress))
              {
                  LoadSpriteAsync(m_definition.eventIconAddress, m_iconImage);
              }

              if (m_selectButton != null)
              {
                  m_selectButton.onClick.RemoveAllListeners();
                  m_selectButton.onClick.AddListener(func_OnSelectCell);
              }
          }

          /// <summary>
          /// [기능]: 기획 데이터 및 선택 액션 콜백을 주입받아 셀을 초기화합니다. (어드민 등 재사용 목적)
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 액션 기반의 Setup 오버로드 추가
          /// </summary>
          public void Setup(EventDefinitionDTO definition, Action<string> onSelect)
          {
              m_definition = definition;
              m_viewModel = null;
              m_onSelectAction = onSelect;

              if (m_titleText != null)
              {
                  m_titleText.text = m_definition.eventTitle;
              }

              if (m_iconImage != null && !string.IsNullOrEmpty(m_definition.eventIconAddress))
              {
                  LoadSpriteAsync(m_definition.eventIconAddress, m_iconImage);
              }

              if (m_selectButton != null)
              {
                  m_selectButton.onClick.RemoveAllListeners();
                  m_selectButton.onClick.AddListener(func_OnActionSelectTriggered);
              }
          }

          /// <summary>
          /// [기능]: 사용자가 셀 버튼을 클릭하였을 때 실행되는 UI Callback 메서드. 뷰모델에 해당 이벤트 선택 사실을 통지합니다. func_ 접두사 준수.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 작성
          /// </summary>
          public void func_OnSelectCell()
          {
              if (m_viewModel != null && m_definition != null)
              {
                  m_viewModel.SelectEvent(m_definition.eventId);
              }
          }

          /// <summary>
          /// [기능]: 액션 콜백 기반 리스너 트리거 시 호출되어 안전하게 이벤트를 발송하는 UI Callback 메서드. func_ 접두사 준수.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 람다 분리 및 정식 메서드 추출
          /// </summary>
          public void func_OnActionSelectTriggered()
          {
              if (m_onSelectAction != null && m_definition != null)
              {
                  m_onSelectAction.Invoke(m_definition.eventId);
              }
          }
          #endregion

          #region 내부 메서드
          /// <summary>
          /// [기능]: 지정된 어드레서블 주소값으로 스프라이트 애셋을 로드하여 이미지 컴포넌트에 할당합니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: XML 주석 보완
          /// </summary>
          private async void LoadSpriteAsync(string address, Image targetImage)
          {
              var handle = Addressables.LoadAssetAsync<Sprite>(address);
              Sprite sprite = await handle.Task;
              if (targetImage != null && sprite != null)
              {
                  targetImage.sprite = sprite;
              }
          }
          #endregion
      }
  }
  ```

- [ ] **Step 2: Commit**
  ```bash
  git add Assets/_Game/Scripts/EventSystem/Views/EventItemCell.cs
  git commit -m "refactor: extract inline lambda to formal action callback method in EventItemCell"
  ```

---

### Task 6: Unity Object 널 조건부 연산자(?. / ??) 제거 전수 검사 및 수정

**Files:**
- Modify: `Assets/_Game/Scripts/EventSystem/Views/EventDetailView.cs`
- Modify: `Assets/_Game/Scripts/EventSystem/Views/RewardPopupView.cs`
- Modify: `Assets/_Game/Scripts/EventSystem/Views/EventListView.cs`
- Modify: `Assets/_Game/Scripts/EventSystem/Views/EventAdminView.cs`

- [ ] **Step 1: View 전수 조사 및 Fake Null 방지 구현 검사**
  - Unity Object(`UnityEngine.Object`를 상속하는 컴포넌트, GameObject 등)에 널 조건부 연산자가 사용되었는지 재검사하여, `if (obj != null)` 형태로 전환합니다. (이미 `EventAdminView.cs` 등에는 올바른 명시적 널 체크가 들어가 있으나, 다른 세부 코드에서 실수가 없는지 체크하고 일관성을 보강합니다.)

- [ ] **Step 2: Commit**
  ```bash
  git commit -m "refactor: verify zero fake null checks in all views"
  ```

---

### Task 7: 테스트 구동 및 최종 동작 검증

**Files:**
- Test: `Assets/_Game/Tests/Editor/EventSystemTests.cs`

- [ ] **Step 1: NUnit 단위 테스트 컴파일 및 실행**
  - 기존 단위 테스트 13개를 전체 구동하여 모든 테스트가 통과하는지 확인합니다.
  - 실행: 유니티 에디터 Test Runner Window에서 `EventSystemTests` 구동
  - 기대: 13/13 PASS 및 한글 입력/출력값 디버그 로그 정상 수집

- [ ] **Step 2: 최종 커밋 및 마무리**
  ```bash
  git status
  ```
