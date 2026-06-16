# Event System Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Unity Clean Code 규칙 및 수동 의존성 주입(Pure DI), MVVM 아키텍처, Strategy/Factory 디자인 패턴을 적용하여 확장 가능하고 데이터 구동식인 이벤트 센터 시스템을 구축합니다.

**Architecture:** 
- 비즈니스 로직(Model)과 상태/명령(ViewModel)은 일반 C# 클래스(POCO)로 작성하여 Unity 엔진 및 UI와 완전히 격리합니다.
- View는 MonoBehaviour를 상속하며 ViewModel의 상태 변화를 Action으로 구독하여 갱신하고, 조작 시 ViewModel의 Command 메서드를 호출하는 단방향 데이터 흐름을 준수합니다.
- 씬 진입점인 `EventSceneInitializer`(Composition Root)에서 수동으로 모든 객체의 라이프사이클 관리와 생성자 주입을 해소합니다.

**Tech Stack:** Unity 6.3.16f1, C# (.NET 8.0/10.0), JSON 기반 Save/Load, NUnit (Unity Test Framework), DOTween (트윈 애니메이션), UniTask (비동기 처리)

---

## Proposed File Structure

```
Assets/
└── _Game/
    ├── Scripts/
    │   └── EventSystem/
    │       ├── Models/
    │       │   ├── EventModel.cs
    │       │   ├── EventProgressModel.cs
    │       │   └── PlayerRewardModel.cs
    │       ├── ViewModels/
    │       │   ├── EventListViewModel.cs
    │       │   ├── EventDetailViewModel.cs
    │       │   └── RewardPopupViewModel.cs
    │       ├── Views/
    │       │   ├── EventListView.cs
    │       │   ├── EventDetailView.cs
    │       │   ├── EventItemCell.cs
    │       │   └── RewardPopupView.cs
    │       ├── Interfaces/
    │       │   ├── IEventCondition.cs
    │       │   ├── IEventReward.cs
    │       │   └── ISaveSystem.cs
    │       ├── Conditions/
    │       │   ├── KillCountCondition.cs
    │       │   ├── StageClearCondition.cs
    │       │   └── AttendanceCondition.cs
    │       ├── Rewards/
    │       │   ├── ExpReward.cs
    │       │   ├── TicketReward.cs
    │       │   └── PointReward.cs
    │       ├── Factories/
    │       │   ├── ConditionFactory.cs
    │       │   └── RewardFactory.cs
    │       ├── Data/
    │       │   ├── EventDefinitionSO.cs
    │       │   ├── ConditionDefinitionSO.cs
    │       │   ├── RewardDefinitionSO.cs
    │       │   └── EventTableSO.cs
    │       ├── Infrastructure/
    │       │   ├── JsonSaveSystem.cs
    │       │   ├── EventSceneInitializer.cs
    │       │   ├── InMemorySaveSystem.cs
    │       │   └── EventTestInitializer.cs
    │       ├── ViewModelsDebug/
    │       │   └── EventDebugViewModel.cs
    │       ├── ViewsDebug/
    │       │   └── EventDebugView.cs
    │       └── DTOs/
    │           └── EventSceneDTO.cs
    └── Tests/
        └── Editor/
            └── EventSystemTests.cs
```

---

### Task 1: 핵심 인터페이스 및 기초 데이터 모델 구현

**Files:**
- Create: `Assets/_Game/Scripts/EventSystem/Interfaces/ISaveSystem.cs`
- Create: `Assets/_Game/Scripts/EventSystem/Interfaces/IEventCondition.cs`
- Create: `Assets/_Game/Scripts/EventSystem/Interfaces/IEventReward.cs`
- Create: `Assets/_Game/Scripts/EventSystem/Models/EventProgressModel.cs`
- Create: `Assets/_Game/Scripts/EventSystem/Models/PlayerRewardModel.cs`
- Create: `Assets/_Game/Tests/Editor/EventSystemTests.cs`

- [ ] **Step 1: 유닛 테스트 기본 구조 생성**
  `Assets/_Game/Tests/Editor/EventSystemTests.cs` 파일을 생성하고 테스트 기본 골격을 작성합니다.
  ```csharp
  using NUnit.Framework;

  namespace BePex.EventSystem.Tests
  {
      public class EventSystemTests
      {
          [Test]
          public void Test_Placeholder()
          {
              Assert.Pass();
          }
      }
  }
  ```

- [ ] **Step 2: 테스트 동작 검증**
  - Unity 에디터 상단 메뉴 `Window > General > Test Runner` 창을 엽니다.
  - `EditMode` 탭에서 `Test_Placeholder`를 실행하여 통과(GREEN)함을 확인합니다.

- [ ] **Step 3: 핵심 인터페이스 구현**
  - `Assets/_Game/Scripts/EventSystem/Interfaces/IEventCondition.cs` 작성:
    ```csharp
    /// <summary>
    /// [기능]: 이벤트 달성 조건을 정의하는 인터페이스.
    /// [작성자]: 윤승종
    /// </summary>
    public interface IEventCondition
    {
        int GetCurrentProgress();
        int GetTargetValue();
        bool IsCompleted();
    }
    ```
  - `Assets/_Game/Scripts/EventSystem/Interfaces/IEventReward.cs` 작성:
    ```csharp
    using BePex.EventSystem.Models;

    /// <summary>
    /// [기능]: 보상 지급 규칙을 정의하는 인터페이스.
    /// [작성자]: 윤승종
    /// </summary>
    public interface IEventReward
    {
        string GetRewardName();
        int GetRewardAmount();
        void Grant(PlayerRewardModel playerReward);
    }
    ```
  - `Assets/_Game/Scripts/EventSystem/Interfaces/ISaveSystem.cs` 작성:
    ```csharp
    using BePex.EventSystem.Models;

    /// <summary>
    /// [기능]: 데이터 저장 및 로드를 처리하는 인터페이스.
    /// [작성자]: 윤승종
    /// </summary>
    public interface ISaveSystem
    {
        EventProgressModel LoadProgress(string eventId);
        void SaveProgress(string eventId, EventProgressModel progress);
        PlayerRewardModel LoadRewardState();
        void SaveRewardState(PlayerRewardModel rewardState);
        void ClearAll();
    }
    ```

- [ ] **Step 4: 데이터 모델(POCO) 구현**
  - `Assets/_Game/Scripts/EventSystem/Models/EventProgressModel.cs` 작성:
    ```csharp
    using System;

    namespace BePex.EventSystem.Models
    {
        /// <summary>
        /// [기능]: 이벤트 진행 정보를 나타내는 데이터 클래스.
        /// [작성자]: 윤승종
        /// </summary>
        [Serializable]
        public class EventProgressModel
        {
            public string eventId;
            public int currentProgress;
            public bool isCompleted;
            public bool isRewardClaimed;

            public EventProgressModel()
            {
                eventId = string.Empty;
                currentProgress = 0;
                isCompleted = false;
                isRewardClaimed = false;
            }
        }
    }
    ```
  - `Assets/_Game/Scripts/EventSystem/Models/PlayerRewardModel.cs` 작성:
    ```csharp
    using System;
    using System.Collections.Generic;

    namespace BePex.EventSystem.Models
    {
        /// <summary>
        /// [기능]: 플레이어의 누적 보상 데이터 및 수령 이력을 보유하는 클래스.
        /// [작성자]: 윤승종
        /// </summary>
        [Serializable]
        public class PlayerRewardModel
        {
            public List<string> claimedEventIds;
            public int totalExp;
            public int totalTickets;
            public int totalPoints;

            public PlayerRewardModel()
            {
                claimedEventIds = new List<string>();
                totalExp = 0;
                totalTickets = 0;
                totalPoints = 0;
            }
        }
    }
    ```

- [ ] **Step 5: 변경 사항 커밋**
  ```bash
  git add Assets/_Game/Scripts/EventSystem/Interfaces/ Assets/_Game/Scripts/EventSystem/Models/ Assets/_Game/Tests/Editor/
  git commit -m "feat: add event system core interfaces and models"
  ```

---

### Task 2: JSON 기반 저장 시스템 및 Condition/Reward 전략 클래스 구현

**Files:**
- Create: `Assets/_Game/Scripts/EventSystem/Infrastructure/JsonSaveSystem.cs`
- Create: `Assets/_Game/Scripts/EventSystem/Conditions/KillCountCondition.cs`
- Create: `Assets/_Game/Scripts/EventSystem/Conditions/StageClearCondition.cs`
- Create: `Assets/_Game/Scripts/EventSystem/Conditions/AttendanceCondition.cs`
- Create: `Assets/_Game/Scripts/EventSystem/Rewards/ExpReward.cs`
- Create: `Assets/_Game/Scripts/EventSystem/Rewards/TicketReward.cs`
- Create: `Assets/_Game/Scripts/EventSystem/Rewards/PointReward.cs`
- Modify: `Assets/_Game/Tests/Editor/EventSystemTests.cs`

- [ ] **Step 1: JsonSaveSystem 작성 및 파일 I/O 테스트**
  - `Assets/_Game/Scripts/EventSystem/Infrastructure/JsonSaveSystem.cs` 작성:
    ```csharp
    using System.IO;
    using UnityEngine;
    using BePex.EventSystem.Interfaces;
    using BePex.EventSystem.Models;

    namespace BePex.EventSystem.Infrastructure
    {
        /// <summary>
        /// [기능]: JSON 파일 I/O 기반 세이브 데이터 처리기.
        /// [작성자]: 윤승종
        /// </summary>
        public class JsonSaveSystem : ISaveSystem
        {
            private readonly string m_saveDir;

            public JsonSaveSystem()
            {
                m_saveDir = Path.Combine(Application.persistentDataPath, "save");
                if (Directory.Exists(m_saveDir) == false)
                {
                    Directory.CreateDirectory(m_saveDir);
                }
            }

            public EventProgressModel LoadProgress(string eventId)
            {
                string path = Path.Combine(m_saveDir, $"event_progress_{eventId}.json");
                if (File.Exists(path) == false)
                {
                    var progress = new EventProgressModel { eventId = eventId };
                    return progress;
                }
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<EventProgressModel>(json);
            }

            public void SaveProgress(string eventId, EventProgressModel progress)
            {
                string path = Path.Combine(m_saveDir, $"event_progress_{eventId}.json");
                string json = JsonUtility.ToJson(progress, true);
                File.WriteAllText(path, json);
            }

            public PlayerRewardModel LoadRewardState()
            {
                string path = Path.Combine(m_saveDir, "player_rewards.json");
                if (File.Exists(path) == false)
                {
                    return new PlayerRewardModel();
                }
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<PlayerRewardModel>(json);
            }

            public void SaveRewardState(PlayerRewardModel rewardState)
            {
                string path = Path.Combine(m_saveDir, "player_rewards.json");
                string json = JsonUtility.ToJson(rewardState, true);
                File.WriteAllText(path, json);
            }

            public void ClearAll()
            {
                if (Directory.Exists(m_saveDir))
                {
                    string[] files = Directory.GetFiles(m_saveDir);
                    for (int i = 0; i < files.Length; i++)
                    {
                        File.Delete(files[i]);
                    }
                }
            }
        }
    }
    ```
  - `Assets/_Game/Tests/Editor/EventSystemTests.cs` 에 테스트 추가:
    ```csharp
    [Test]
    public void Test_JsonSaveSystem_SaveAndLoad()
    {
        var saveSystem = new BePex.EventSystem.Infrastructure.JsonSaveSystem();
        saveSystem.ClearAll();

        var progress = new BePex.EventSystem.Models.EventProgressModel
        {
            eventId = "test_ev_01",
            currentProgress = 10,
            isCompleted = true,
            isRewardClaimed = false
        };
        saveSystem.SaveProgress("test_ev_01", progress);

        var loaded = saveSystem.LoadProgress("test_ev_01");
        Assert.AreEqual(progress.eventId, loaded.eventId);
        Assert.AreEqual(progress.currentProgress, loaded.currentProgress);
        Assert.AreEqual(progress.isCompleted, loaded.isCompleted);
    }
    ```
  - Unity Test Runner에서 테스트를 구동하여 통과(GREEN)를 확인합니다.

- [ ] **Step 2: Condition 전략 클래스 구현**
  - `Assets/_Game/Scripts/EventSystem/Conditions/KillCountCondition.cs` 작성:
    ```csharp
    using BePex.EventSystem.Interfaces;

    namespace BePex.EventSystem.Conditions
    {
        /// <summary>
        /// [기능]: 적 처치 조건을 판정하는 클래스.
        /// [작성자]: 윤승종
        /// </summary>
        public class KillCountCondition : IEventCondition
        {
            private readonly int m_targetValue;
            private readonly ISaveSystem m_saveSystem;
            private readonly string m_eventId;

            public KillCountCondition(int targetValue, ISaveSystem saveSystem, string eventId)
            {
                m_targetValue = targetValue;
                m_saveSystem = saveSystem;
                m_eventId = eventId;
            }

            public int GetCurrentProgress()
            {
                var progress = m_saveSystem.LoadProgress(m_eventId);
                return progress.currentProgress;
            }

            public int GetTargetValue() => m_targetValue;
            public bool IsCompleted() => GetCurrentProgress() >= m_targetValue;
        }
    }
    ```
  - `Assets/_Game/Scripts/EventSystem/Conditions/StageClearCondition.cs` 작성:
    ```csharp
    using BePex.EventSystem.Interfaces;

    namespace BePex.EventSystem.Conditions
    {
        /// <summary>
        /// [기능]: 스테이지 클리어 조건을 판정하는 클래스.
        /// [작성자]: 윤승종
        /// </summary>
        public class StageClearCondition : IEventCondition
        {
            private readonly int m_targetValue;
            private readonly ISaveSystem m_saveSystem;
            private readonly string m_eventId;

            public StageClearCondition(int targetValue, ISaveSystem saveSystem, string eventId)
            {
                m_targetValue = targetValue;
                m_saveSystem = saveSystem;
                m_eventId = eventId;
            }

            public int GetCurrentProgress()
            {
                var progress = m_saveSystem.LoadProgress(m_eventId);
                return progress.currentProgress;
            }

            public int GetTargetValue() => m_targetValue;
            public bool IsCompleted() => GetCurrentProgress() >= m_targetValue;
        }
    }
    ```
  - `Assets/_Game/Scripts/EventSystem/Conditions/AttendanceCondition.cs` 작성:
    ```csharp
    using BePex.EventSystem.Interfaces;

    namespace BePex.EventSystem.Conditions
    {
        /// <summary>
        /// [기능]: 출석 일수 조건을 판정하는 클래스.
        /// [작성자]: 윤승종
        /// </summary>
        public class AttendanceCondition : IEventCondition
        {
            private readonly int m_targetValue;
            private readonly ISaveSystem m_saveSystem;
            private readonly string m_eventId;

            public AttendanceCondition(int targetValue, ISaveSystem saveSystem, string eventId)
            {
                m_targetValue = targetValue;
                m_saveSystem = saveSystem;
                m_eventId = eventId;
            }

            public int GetCurrentProgress()
            {
                var progress = m_saveSystem.LoadProgress(m_eventId);
                return progress.currentProgress;
            }

            public int GetTargetValue() => m_targetValue;
            public bool IsCompleted() => GetCurrentProgress() >= m_targetValue;
        }
    }
    ```

- [ ] **Step 3: Reward 전략 클래스 구현**
  - `Assets/_Game/Scripts/EventSystem/Rewards/ExpReward.cs` 작성:
    ```csharp
    using BePex.EventSystem.Interfaces;
    using BePex.EventSystem.Models;

    namespace BePex.EventSystem.Rewards
    {
        /// <summary>
        /// [기능]: 경험치 보상 지급 클래스.
        /// [작성자]: 윤승종
        /// </summary>
        public class ExpReward : IEventReward
        {
            private readonly int m_amount;
            private readonly string m_displayName;

            public ExpReward(int amount, string displayName)
            {
                m_amount = amount;
                m_displayName = displayName;
            }

            public string GetRewardName() => m_displayName;
            public int GetRewardAmount() => m_amount;

            public void Grant(PlayerRewardModel playerReward)
            {
                if (playerReward != null)
                {
                    playerReward.totalExp += m_amount;
                }
            }
        }
    }
    ```
  - `Assets/_Game/Scripts/EventSystem/Rewards/TicketReward.cs` 작성:
    ```csharp
    using BePex.EventSystem.Interfaces;
    using BePex.EventSystem.Models;

    namespace BePex.EventSystem.Rewards
    {
        /// <summary>
        /// [기능]: 티켓 보상 지급 클래스.
        /// [작성자]: 윤승종
        /// </summary>
        public class TicketReward : IEventReward
        {
            private readonly int m_amount;
            private readonly string m_displayName;

            public TicketReward(int amount, string displayName)
            {
                m_amount = amount;
                m_displayName = displayName;
            }

            public string GetRewardName() => m_displayName;
            public int GetRewardAmount() => m_amount;

            public void Grant(PlayerRewardModel playerReward)
            {
                if (playerReward != null)
                {
                    playerReward.totalTickets += m_amount;
                }
            }
        }
    }
    ```
  - `Assets/_Game/Scripts/EventSystem/Rewards/PointReward.cs` 작성:
    ```csharp
    using BePex.EventSystem.Interfaces;
    using BePex.EventSystem.Models;

    namespace BePex.EventSystem.Rewards
    {
        /// <summary>
        /// [기능]: 포인트 보상 지급 클래스.
        /// [작성자]: 윤승종
        /// </summary>
        public class PointReward : IEventReward
        {
            private readonly int m_amount;
            private readonly string m_displayName;

            public PointReward(int amount, string displayName)
            {
                m_amount = amount;
                m_displayName = displayName;
            }

            public string GetRewardName() => m_displayName;
            public int GetRewardAmount() => m_amount;

            public void Grant(PlayerRewardModel playerReward)
            {
                if (playerReward != null)
                {
                    playerReward.totalPoints += m_amount;
                }
            }
        }
    }
    ```

- [ ] **Step 4: 전략 및 상태 모델의 유닛 테스트 작성 및 검증**
  - `Assets/_Game/Tests/Editor/EventSystemTests.cs` 에 아래 테스트 추가:
    ```csharp
    [Test]
    public void Test_ConditionsAndRewards_Behavior()
    {
        var saveSystem = new BePex.EventSystem.Infrastructure.JsonSaveSystem();
        saveSystem.ClearAll();

        // 1. Condition 검증
        var killCondition = new BePex.EventSystem.Conditions.KillCountCondition(5, saveSystem, "ev_kill_01");
        Assert.IsFalse(killCondition.IsCompleted());

        var progress = saveSystem.LoadProgress("ev_kill_01");
        progress.currentProgress = 5;
        saveSystem.SaveProgress("ev_kill_01", progress);
        Assert.IsTrue(killCondition.IsCompleted());

        // 2. Reward 검증
        var playerReward = new BePex.EventSystem.Models.PlayerRewardModel();
        var expReward = new BePex.EventSystem.Rewards.ExpReward(100, "경험치 100");
        expReward.Grant(playerReward);
        Assert.AreEqual(100, playerReward.totalExp);
    }
    ```
  - Unity Test Runner에서 전체 EditMode 테스트 실행 및 GREEN 패스 검증.

- [ ] **Step 5: 변경 사항 커밋**
  ```bash
  git add Assets/_Game/Scripts/EventSystem/Infrastructure/JsonSaveSystem.cs Assets/_Game/Scripts/EventSystem/Conditions/ Assets/_Game/Scripts/EventSystem/Rewards/
  git commit -m "feat: implement JSON saving and Condition/Reward strategies with unit tests"
  ```

---

### Task 3: Factory 클래스 및 핵심 Domain Model (EventModel) 구현

**Files:**
- Create: `Assets/_Game/Scripts/EventSystem/Factories/ConditionFactory.cs`
- Create: `Assets/_Game/Scripts/EventSystem/Factories/RewardFactory.cs`
- Create: `Assets/_Game/Scripts/EventSystem/Models/EventModel.cs`
- Modify: `Assets/_Game/Tests/Editor/EventSystemTests.cs`

- [ ] **Step 1: Condition/Reward Factory 구현**
  - `Assets/_Game/Scripts/EventSystem/Factories/ConditionFactory.cs` 작성:
    ```csharp
    using UnityEngine;
    using BePex.EventSystem.Interfaces;
    using BePex.EventSystem.Data;
    using BePex.EventSystem.Conditions;

    namespace BePex.EventSystem.Factories
    {
        /// <summary>
        /// [기능]: 기획 데이터 기반으로 IEventCondition 전략 인스턴스를 생성하는 팩토리.
        /// [작성자]: 윤승종
        /// </summary>
        public class ConditionFactory
        {
            private readonly ISaveSystem m_saveSystem;

            public ConditionFactory(ISaveSystem saveSystem)
            {
                m_saveSystem = saveSystem;
            }

            public IEventCondition Create(ConditionDefinitionSO definition, string eventId)
            {
                if (definition == null)
                {
                    return null;
                }
                switch (definition.Type)
                {
                    case ConditionDefinitionSO.ConditionType.KillCount:
                        return new KillCountCondition(definition.TargetValue, m_saveSystem, eventId);
                    case ConditionDefinitionSO.ConditionType.StageClear:
                        return new StageClearCondition(definition.TargetValue, m_saveSystem, eventId);
                    case ConditionDefinitionSO.ConditionType.Attendance:
                        return new AttendanceCondition(definition.TargetValue, m_saveSystem, eventId);
                    default:
                        Debug.LogError($"[ConditionFactory] 알 수 없는 조건 타입: {definition.Type}");
                        return null;
                }
            }
        }
    }
    ```
  - `Assets/_Game/Scripts/EventSystem/Factories/RewardFactory.cs` 작성:
    ```csharp
    using UnityEngine;
    using BePex.EventSystem.Interfaces;
    using BePex.EventSystem.Data;
    using BePex.EventSystem.Rewards;

    namespace BePex.EventSystem.Factories
    {
        /// <summary>
        /// [기능]: 기획 데이터 기반으로 IEventReward 전략 인스턴스를 생성하는 팩토리.
        /// [작성자]: 윤승종
        /// </summary>
        public class RewardFactory
        {
            public IEventReward Create(RewardDefinitionSO definition)
            {
                if (definition == null)
                {
                    return null;
                }
                switch (definition.Type)
                {
                    case RewardDefinitionSO.RewardType.Exp:
                        return new ExpReward(definition.Amount, definition.DisplayName);
                    case RewardDefinitionSO.RewardType.Ticket:
                        return new TicketReward(definition.Amount, definition.DisplayName);
                    case RewardDefinitionSO.RewardType.Point:
                        return new PointReward(definition.Amount, definition.DisplayName);
                    default:
                        Debug.LogError($"[RewardFactory] 알 수 없는 보상 타입: {definition.Type}");
                        return null;
                }
            }
        }
    }
    ```

- [ ] **Step 2: EventModel 핵심 도메인 비즈니스 로직 구현**
  - `Assets/_Game/Scripts/EventSystem/Models/EventModel.cs` 작성:
    ```csharp
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using BePex.EventSystem.Interfaces;
    using BePex.EventSystem.Data;
    using BePex.EventSystem.Factories;

    namespace BePex.EventSystem.Models
    {
        /// <summary>
        /// [기능]: 전체 이벤트 데이터를 보유하고 로딩, 진행 상태, 보상 청구를 실질적으로 제어하는 핵심 도메인 모델.
        /// [작성자]: 윤승종
        /// </summary>
        public class EventModel
        {
            private readonly EventTableSO m_eventTable;
            private readonly ConditionFactory m_conditionFactory;
            private readonly RewardFactory m_rewardFactory;

            private readonly List<EventDefinitionSO> m_activeEvents;
            private readonly Dictionary<string, IEventCondition> m_conditions;
            private readonly Dictionary<string, List<IEventReward>> m_rewards;

            public event Action<string> OnEventProgressChanged;
            public event Action<string> OnEventRewardClaimed;

            public EventModel(EventTableSO eventTable, ConditionFactory conditionFactory, RewardFactory rewardFactory)
            {
                m_eventTable = eventTable;
                m_conditionFactory = conditionFactory;
                m_rewardFactory = rewardFactory;

                m_activeEvents = new List<EventDefinitionSO>();
                m_conditions = new Dictionary<string, IEventCondition>();
                m_rewards = new Dictionary<string, List<IEventReward>>();

                Reload();
            }

            public void Reload()
            {
                m_activeEvents.Clear();
                m_conditions.Clear();
                m_rewards.Clear();

                if (m_eventTable == null || m_eventTable.Events == null)
                {
                    return;
                }

                for (int i = 0; i < m_eventTable.Events.Length; i++)
                {
                    var definition = m_eventTable.Events[i];
                    if (definition == null)
                    {
                        continue;
                    }

                    m_activeEvents.Add(definition);

                    // 조건 생성
                    var cond = m_conditionFactory.Create(definition.Condition, definition.EventId);
                    if (cond != null)
                    {
                        m_conditions[definition.EventId] = cond;
                    }

                    // 보상 목록 생성
                    var rewardList = new List<IEventReward>();
                    if (definition.Rewards != null)
                    {
                        for (int j = 0; j < definition.Rewards.Length; j++)
                        {
                            var rew = m_rewardFactory.Create(definition.Rewards[j]);
                            if (rew != null)
                            {
                                rewardList.Add(rew);
                            }
                        }
                    }
                    m_rewards[definition.EventId] = rewardList;
                }
            }

            public List<EventDefinitionSO> GetActiveEvents() => m_activeEvents;

            public IEventCondition GetCondition(string eventId)
            {
                return m_conditions.GetValueOrDefault(eventId);
            }

            public List<IEventReward> GetRewards(string eventId)
            {
                return m_rewards.GetValueOrDefault(eventId, new List<IEventReward>());
            }

            public void Debug_AddProgress(string eventId, int amount, ISaveSystem saveSystem)
            {
                var progress = saveSystem.LoadProgress(eventId);
                progress.currentProgress += amount;
                
                var cond = GetCondition(eventId);
                if (cond != null && progress.currentProgress >= cond.GetTargetValue())
                {
                    progress.isCompleted = true;
                }

                saveSystem.SaveProgress(eventId, progress);
                OnEventProgressChanged?.Invoke(eventId);
            }

            public bool ClaimReward(string eventId, ISaveSystem saveSystem, PlayerRewardModel playerReward)
            {
                var progress = saveSystem.LoadProgress(eventId);
                if (progress.isCompleted == false || progress.isRewardClaimed == true)
                {
                    return false;
                }

                var list = GetRewards(eventId);
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].Grant(playerReward);
                }

                if (playerReward.claimedEventIds.Contains(eventId) == false)
                {
                    playerReward.claimedEventIds.Add(eventId);
                }

                progress.isRewardClaimed = true;
                saveSystem.SaveProgress(eventId, progress);
                saveSystem.SaveRewardState(playerReward);

                OnEventRewardClaimed?.Invoke(eventId);
                return true;
            }
        }
    }
    ```

- [ ] **Step 3: 변경 사항 커밋**
  ```bash
  git add Assets/_Game/Scripts/EventSystem/Factories/ Assets/_Game/Scripts/EventSystem/Models/EventModel.cs
  git commit -m "feat: add Factory and core EventModel domain logic"
  ```

---

### Task 4: MVVM ViewModel 구현 (EventList, EventDetail, RewardPopup)

**Files:**
- Create: `Assets/_Game/Scripts/EventSystem/ViewModels/EventListViewModel.cs`
- Create: `Assets/_Game/Scripts/EventSystem/ViewModels/EventDetailViewModel.cs`
- Create: `Assets/_Game/Scripts/EventSystem/ViewModels/RewardPopupViewModel.cs`

- [ ] **Step 1: EventListViewModel 구현**
  - `Assets/_Game/Scripts/EventSystem/ViewModels/EventListViewModel.cs` 작성:
    ```csharp
    using System;
    using System.Collections.Generic;
    using BePex.EventSystem.Models;
    using BePex.EventSystem.Data;

    namespace BePex.EventSystem.ViewModels
    {
        /// <summary>
        /// [기능]: 이벤트 목록 뷰의 바인딩 상태와 명령을 중개하는 뷰모델.
        /// [작성자]: 윤승종
        /// </summary>
        public class EventListViewModel
        {
            private readonly EventModel m_eventModel;
            public event Action OnListUpdated;

            private string m_selectedEventId;
            public event Action<string> OnEventSelected;

            public EventListViewModel(EventModel eventModel)
            {
                m_eventModel = eventModel;
                m_eventModel.OnEventProgressChanged += HandleEventProgressChanged;
                m_eventModel.OnEventRewardClaimed += HandleEventRewardClaimed;
            }

            private void HandleEventProgressChanged(string eventId)
            {
                OnListUpdated?.Invoke();
            }

            private void HandleEventRewardClaimed(string eventId)
            {
                OnListUpdated?.Invoke();
            }

            public List<EventDefinitionSO> GetEvents()
            {
                return m_eventModel.GetActiveEvents();
            }

            public void SelectEvent(string eventId)
            {
                m_selectedEventId = eventId;
                OnEventSelected?.Invoke(eventId);
            }

            public string GetSelectedEventId() => m_selectedEventId;
        }
    }
    ```

- [ ] **Step 2: EventDetailViewModel 구현**
  - `Assets/_Game/Scripts/EventSystem/ViewModels/EventDetailViewModel.cs` 작성:
    ```csharp
    using System;
    using BePex.EventSystem.Models;
    using BePex.EventSystem.Data;
    using BePex.EventSystem.Interfaces;

    namespace BePex.EventSystem.ViewModels
    {
        /// <summary>
        /// [기능]: 특정 이벤트 상세 화면의 진척도 조회 및 보상 수령 명령을 전달하는 뷰모델.
        /// [작성자]: 윤승종
        /// </summary>
        public class EventDetailViewModel
        {
            private readonly EventModel m_eventModel;
            private readonly ISaveSystem m_saveSystem;
            private string m_currentEventId;

            public event Action OnDetailUpdated;
            public event Action<string> OnRewardClaimSuccess;

            public EventDetailViewModel(EventModel eventModel, ISaveSystem saveSystem)
            {
                m_eventModel = eventModel;
                m_saveSystem = saveSystem;

                m_eventModel.OnEventProgressChanged += HandleProgressChanged;
                m_eventModel.OnEventRewardClaimed += HandleRewardClaimed;
            }

            private void HandleProgressChanged(string eventId)
            {
                if (eventId == m_currentEventId)
                {
                    OnDetailUpdated?.Invoke();
                }
            }

            private void HandleRewardClaimed(string eventId)
            {
                if (eventId == m_currentEventId)
                {
                    OnDetailUpdated?.Invoke();
                }
            }

            public void SetEvent(string eventId)
            {
                m_currentEventId = eventId;
                OnDetailUpdated?.Invoke();
            }

            public EventDefinitionSO GetEventDefinition()
            {
                var list = m_eventModel.GetActiveEvents();
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].EventId == m_currentEventId)
                    {
                        return list[i];
                    }
                }
                return null;
            }

            public (int current, int target, float ratio) GetProgressInfo()
            {
                var cond = m_eventModel.GetCondition(m_currentEventId);
                if (cond == null)
                {
                    return (0, 1, 0f);
                }
                int cur = cond.GetCurrentProgress();
                int tar = cond.GetTargetValue();
                float ratio = tar > 0 ? (float)cur / tar : 0f;
                return (cur, tar, UnityEngine.Mathf.Clamp01(ratio));
            }

            public bool IsRewardClaimed()
            {
                var progress = m_saveSystem.LoadProgress(m_currentEventId);
                return progress.isRewardClaimed;
            }

            public bool CanClaimReward()
            {
                var cond = m_eventModel.GetCondition(m_currentEventId);
                if (cond == null)
                {
                    return false;
                }
                bool completed = cond.IsCompleted();
                bool claimed = IsRewardClaimed();
                return completed && !claimed;
            }

            public void ClaimReward(PlayerRewardModel playerReward)
            {
                if (CanClaimReward() == false)
                {
                    return;
                }
                bool success = m_eventModel.ClaimReward(m_currentEventId, m_saveSystem, playerReward);
                if (success)
                {
                    OnRewardClaimSuccess?.Invoke(m_currentEventId);
                }
            }
        }
    }
    ```

- [ ] **Step 3: RewardPopupViewModel 구현**
  - `Assets/_Game/Scripts/EventSystem/ViewModels/RewardPopupViewModel.cs` 작성:
    ```csharp
    using System;
    using BePex.EventSystem.Models;
    using BePex.EventSystem.Interfaces;

    namespace BePex.EventSystem.ViewModels
    {
        /// <summary>
        /// [기능]: 보상 수령 완료 팝업 연동 및 플레이어의 최종 자산 상태를 노출하는 뷰모델.
        /// [작성자]: 윤승종
        /// </summary>
        public class RewardPopupViewModel
        {
            private readonly PlayerRewardModel m_playerReward;
            private readonly ISaveSystem m_saveSystem;

            public event Action OnRewardDataChanged;

            public RewardPopupViewModel(PlayerRewardModel playerReward, ISaveSystem saveSystem)
            {
                m_playerReward = playerReward;
                m_saveSystem = saveSystem;
            }

            public PlayerRewardModel GetPlayerReward() => m_playerReward;

            public void Refresh()
            {
                OnRewardDataChanged?.Invoke();
            }
        }
    }
    ```

- [ ] **Step 4: 변경 사항 커밋**
  ```bash
  git add Assets/_Game/Scripts/EventSystem/ViewModels/
  git commit -m "feat: implement MVVM ViewModels"
  ```

---

### Task 5: ScriptableObject 데이터 소스 클래스 작성

**Files:**
- Create: `Assets/_Game/Scripts/EventSystem/Data/ConditionDefinitionSO.cs`
- Create: `Assets/_Game/Scripts/EventSystem/Data/RewardDefinitionSO.cs`
- Create: `Assets/_Game/Scripts/EventSystem/Data/EventDefinitionSO.cs`
- Create: `Assets/_Game/Scripts/EventSystem/Data/EventTableSO.cs`

- [ ] **Step 1: ConditionDefinitionSO 작성**
  - `Assets/_Game/Scripts/EventSystem/Data/ConditionDefinitionSO.cs` 작성:
    ```csharp
    using UnityEngine;

    namespace BePex.EventSystem.Data
    {
        /// <summary>
        /// [기능]: 이벤트 완성 조건을 정의하는 SO 파일.
        /// [작성자]: 윤승종
        /// </summary>
        [CreateAssetMenu(fileName = "ConditionDefinition", menuName = "BePex/Event/Condition")]
        public class ConditionDefinitionSO : ScriptableObject
        {
            public enum ConditionType
            {
                KillCount,
                StageClear,
                Attendance
            }

            [SerializeField] private ConditionType m_conditionType;
            [SerializeField] private int m_targetValue;

            public ConditionType Type => m_conditionType;
            public int TargetValue => m_targetValue;
        }
    }
    ```

- [ ] **Step 2: RewardDefinitionSO 작성**
  - `Assets/_Game/Scripts/EventSystem/Data/RewardDefinitionSO.cs` 작성:
    ```csharp
    using UnityEngine;

    namespace BePex.EventSystem.Data
    {
        /// <summary>
        /// [기능]: 지급할 보상의 속성을 정의하는 SO 파일.
        /// [작성자]: 윤승종
        /// </summary>
        [CreateAssetMenu(fileName = "RewardDefinition", menuName = "BePex/Event/Reward")]
        public class RewardDefinitionSO : ScriptableObject
        {
            public enum RewardType
            {
                Exp,
                Ticket,
                Point
            }

            [SerializeField] private RewardType m_rewardType;
            [SerializeField] private int m_amount;
            [SerializeField] private string m_displayName;
            [SerializeField] private Sprite m_icon;

            public RewardType Type => m_rewardType;
            public int Amount => m_amount;
            public string DisplayName => m_displayName;
            public Sprite Icon => m_icon;
        }
    }
    ```

- [ ] **Step 3: EventDefinitionSO 및 EventTableSO 작성**
  - `Assets/_Game/Scripts/EventSystem/Data/EventDefinitionSO.cs` 작성:
    ```csharp
    using UnityEngine;

    namespace BePex.EventSystem.Data
    {
        /// <summary>
        /// [기능]: 개별 이벤트의 콘텐츠 구조를 정의하는 SO 파일.
        /// [작성자]: 윤승종
        /// </summary>
        [CreateAssetMenu(fileName = "EventDefinition", menuName = "BePex/Event/Event")]
        public class EventDefinitionSO : ScriptableObject
        {
            [SerializeField] private string m_eventId;
            [SerializeField] private string m_eventTitle;
            [SerializeField] private string m_eventDescription;
            [SerializeField] private Sprite m_eventIcon;
            [SerializeField] private string m_startDate;
            [SerializeField] private string m_endDate;
            [SerializeField] private ConditionDefinitionSO m_condition;
            [SerializeField] private RewardDefinitionSO[] m_rewards;

            public string EventId => m_eventId;
            public string EventTitle => m_eventTitle;
            public string EventDescription => m_eventDescription;
            public Sprite EventIcon => m_eventIcon;
            public string StartDate => m_startDate;
            public string EndDate => m_endDate;
            public ConditionDefinitionSO Condition => m_condition;
            public RewardDefinitionSO[] Rewards => m_rewards;
        }
    }
    ```
  - `Assets/_Game/Scripts/EventSystem/Data/EventTableSO.cs` 작성:
    ```csharp
    using UnityEngine;

    namespace BePex.EventSystem.Data
    {
        /// <summary>
        /// [기능]: 인게임에 로드될 활성 이벤트들을 모아두는 이벤트 테이블 SO.
        /// [작성자]: 윤승종
        /// </summary>
        [CreateAssetMenu(fileName = "EventTable", menuName = "BePex/Event/Table")]
        public class EventTableSO : ScriptableObject
        {
            [SerializeField] private EventDefinitionSO[] m_events;

            public EventDefinitionSO[] Events => m_events;
        }
    }
    ```

- [ ] **Step 4: 변경 사항 커밋**
  ```bash
  git add Assets/_Game/Scripts/EventSystem/Data/
  git commit -m "feat: add ScriptableObject definitions for Event data"
  ```

---

### Task 6: MVVM Views 및 Composition Root (Initializer) 구현

**Files:**
- Create: `Assets/_Game/Scripts/EventSystem/Views/EventItemCell.cs`
- Create: `Assets/_Game/Scripts/EventSystem/Views/EventListView.cs`
- Create: `Assets/_Game/Scripts/EventSystem/Views/EventDetailView.cs`
- Create: `Assets/_Game/Scripts/EventSystem/Views/RewardPopupView.cs`
- Create: `Assets/_Game/Scripts/EventSystem/Infrastructure/EventSceneInitializer.cs`
- Create: `Assets/_Game/Scripts/EventSystem/DTOs/EventSceneDTO.cs`

- [ ] **Step 1: EventItemCell 구현**
  - `Assets/_Game/Scripts/EventSystem/Views/EventItemCell.cs` 작성:
    ```csharp
    using UnityEngine;
    using UnityEngine.UI;
    using BePex.EventSystem.Data;
    using BePex.EventSystem.ViewModels;

    namespace BePex.EventSystem.Views
    {
        /// <summary>
        /// [기능]: 이벤트 목록 내 개별 셀의 화면 표시 및 입력 연동 제어.
        /// [작성자]: 윤승종
        /// </summary>
        public class EventItemCell : MonoBehaviour
        {
            [SerializeField] private Text m_titleText;
            [SerializeField] private Image m_iconImage;
            [SerializeField] private Button m_selectButton;

            private EventDefinitionSO m_definition;
            private EventListViewModel m_viewModel;

            public void Setup(EventDefinitionSO definition, EventListViewModel viewModel)
            {
                m_definition = definition;
                m_viewModel = viewModel;

                if (m_titleText != null)
                {
                    m_titleText.text = m_definition.EventTitle;
                }
                if (m_iconImage != null)
                {
                    m_iconImage.sprite = m_definition.EventIcon;
                }

                m_selectButton.onClick.RemoveAllListeners();
                m_selectButton.onClick.AddListener(func_OnSelectCell);
            }

            public void func_OnSelectCell()
            {
                if (m_viewModel != null && m_definition != null)
                {
                    m_viewModel.SelectEvent(m_definition.EventId);
                }
            }
        }
    }
    ```

- [ ] **Step 2: EventListView 구현**
  - `Assets/_Game/Scripts/EventSystem/Views/EventListView.cs` 작성:
    ```csharp
    using System.Collections.Generic;
    using UnityEngine;
    using BePex.EventSystem.ViewModels;

    namespace BePex.EventSystem.Views
    {
        /// <summary>
        /// [기능]: 이벤트 전체 목록을 시각화하고 하위 셀을 통제하는 View 클래스.
        /// [작성자]: 윤승종
        /// </summary>
        public class EventListView : MonoBehaviour
        {
            [SerializeField] private Transform m_cellContainer;
            [SerializeField] private GameObject m_cellPrefab;

            private EventListViewModel m_viewModel;
            private readonly List<EventItemCell> m_spawnedCells = new List<EventItemCell>();

            public void Bind(EventListViewModel viewModel)
            {
                m_viewModel = viewModel;
                m_viewModel.OnListUpdated += func_OnListUpdated;

                func_OnListUpdated();
            }

            private void OnDestroy()
            {
                if (m_viewModel != null)
                {
                    m_viewModel.OnListUpdated -= func_OnListUpdated;
                }
            }

            public void func_OnListUpdated()
            {
                // 기존 셀 파괴
                for (int i = 0; i < m_spawnedCells.Count; i++)
                {
                    if (m_spawnedCells[i] != null)
                    {
                        Destroy(m_spawnedCells[i].gameObject);
                    }
                }
                m_spawnedCells.Clear();

                var list = m_viewModel.GetEvents();
                for (int i = 0; i < list.Count; i++)
                {
                    var go = Instantiate(m_cellPrefab, m_cellContainer);
                    var cell = go.GetComponent<EventItemCell>();
                    if (cell != null)
                    {
                        cell.Setup(list[i], m_viewModel);
                        m_spawnedCells.Add(cell);
                    }
                }
            }
        }
    }
    ```

- [ ] **Step 3: EventDetailView 구현**
  - `Assets/_Game/Scripts/EventSystem/Views/EventDetailView.cs` 작성:
    ```csharp
    using UnityEngine;
    using UnityEngine.UI;
    using BePex.EventSystem.ViewModels;

    namespace BePex.EventSystem.Views
    {
        /// <summary>
        /// [기능]: 특정 이벤트의 상세 조건, 보상 정보 및 게이지를 표시하는 상세 UI 뷰.
        /// [작성자]: 윤승종
        /// </summary>
        public class EventDetailView : MonoBehaviour
        {
            [SerializeField] private Text m_titleText;
            [SerializeField] private Text m_descText;
            [SerializeField] private Text m_progressText;
            [SerializeField] private Slider m_progressSlider;
            [SerializeField] private Button m_claimButton;

            private EventDetailViewModel m_viewModel;
            private EventListViewModel m_listViewModel;
            private RewardPopupViewModel m_popupViewModel;

            public void Bind(EventDetailViewModel viewModel, EventListViewModel listViewModel, RewardPopupViewModel popupViewModel)
            {
                m_viewModel = viewModel;
                m_listViewModel = listViewModel;
                m_popupViewModel = popupViewModel;

                m_viewModel.OnDetailUpdated += func_OnDetailUpdated;
                m_listViewModel.OnEventSelected += func_OnEventSelected;

                m_claimButton.onClick.RemoveAllListeners();
                m_claimButton.onClick.AddListener(func_OnClaimButtonClick);

                m_claimButton.interactable = false;
            }

            private void OnDestroy()
            {
                if (m_viewModel != null)
                {
                    m_viewModel.OnDetailUpdated -= func_OnDetailUpdated;
                }
                if (m_listViewModel != null)
                {
                    m_listViewModel.OnEventSelected -= func_OnEventSelected;
                }
            }

            public void func_OnEventSelected(string eventId)
            {
                m_viewModel.SetEvent(eventId);
            }

            public void func_OnDetailUpdated()
            {
                var def = m_viewModel.GetEventDefinition();
                if (def == null)
                {
                    gameObject.SetActive(false);
                    return;
                }
                gameObject.SetActive(true);

                m_titleText.text = def.EventTitle;
                m_descText.text = def.EventDescription;

                var (cur, tar, ratio) = m_viewModel.GetProgressInfo();
                m_progressText.text = $"{cur} / {tar}";
                m_progressSlider.value = ratio;

                bool claimed = m_viewModel.IsRewardClaimed();
                m_claimButton.interactable = m_viewModel.CanClaimReward();
                
                var btnText = m_claimButton.GetComponentInChildren<Text>();
                if (btnText != null)
                {
                    btnText.text = claimed ? "수령 완료" : "보상 받기";
                }
            }

            public void func_OnClaimButtonClick()
            {
                if (m_viewModel != null && m_popupViewModel != null)
                {
                    m_viewModel.ClaimReward(m_popupViewModel.GetPlayerReward());
                }
            }
        }
    }
    ```

- [ ] **Step 4: RewardPopupView 구현**
  - `Assets/_Game/Scripts/EventSystem/Views/RewardPopupView.cs` 작성:
    ```csharp
    using UnityEngine;
    using UnityEngine.UI;
    using BePex.EventSystem.ViewModels;

    namespace BePex.EventSystem.Views
    {
        /// <summary>
        /// [기능]: 보상 획득 상태를 안내하고 플레이어의 총 자산 현황을 표시하는 팝업 View.
        /// [작성자]: 윤승종
        /// </summary>
        public class RewardPopupView : MonoBehaviour
        {
            [SerializeField] private GameObject m_popupRoot;
            [SerializeField] private Text m_expText;
            [SerializeField] private Text m_ticketText;
            [SerializeField] private Text m_pointText;
            [SerializeField] private Button m_closeButton;

            private RewardPopupViewModel m_viewModel;

            public void Bind(RewardPopupViewModel viewModel, EventDetailViewModel detailViewModel)
            {
                m_viewModel = viewModel;
                m_viewModel.OnRewardDataChanged += func_OnRewardDataChanged;
                
                if (detailViewModel != null)
                {
                    detailViewModel.OnRewardClaimSuccess += func_OnShowPopup;
                }

                m_closeButton.onClick.RemoveAllListeners();
                m_closeButton.onClick.AddListener(func_OnCloseClick);

                m_popupRoot.SetActive(false);
            }

            private void OnDestroy()
            {
                if (m_viewModel != null)
                {
                    m_viewModel.OnRewardDataChanged -= func_OnRewardDataChanged;
                }
            }

            public void func_OnShowPopup(string eventId)
            {
                m_popupRoot.SetActive(true);
                m_viewModel.Refresh();
            }

            public void func_OnRewardDataChanged()
            {
                var reward = m_viewModel.GetPlayerReward();
                if (reward != null)
                {
                    m_expText.text = $"누적 경험치: {reward.totalExp}";
                    m_ticketText.text = $"누적 티켓: {reward.totalTickets}";
                    m_pointText.text = $"누적 포인트: {reward.totalPoints}";
                }
            }

            public void func_OnCloseClick()
            {
                m_popupRoot.SetActive(false);
            }
        }
    }
    ```

- [ ] **Step 5: EventSceneInitializer 및 DTO 구현 (Composition Root)**
  - `Assets/_Game/Scripts/EventSystem/DTOs/EventSceneDTO.cs` 작성:
    ```csharp
    using System;

    namespace BePex.EventSystem.DTOs
    {
        /// <summary>
        /// [기능]: 씬 진입 시 전달할 수 있는 로비 상태 데이터 DTO.
        /// [작성자]: 윤승종
        /// </summary>
        [Serializable]
        public class EventSceneDTO
        {
            public string lobbyUserName;
            public int initialSceneCode;
        }
    }
    ```
  - `Assets/_Game/Scripts/EventSystem/Infrastructure/EventSceneInitializer.cs` 작성:
    ```csharp
    using UnityEngine;
    using BePex.EventSystem.Views;
    using BePex.EventSystem.Data;
    using BePex.EventSystem.Factories;
    using BePex.EventSystem.Models;
    using BePex.EventSystem.ViewModels;

    namespace BePex.EventSystem.Infrastructure
    {
        /// <summary>
        /// [기능]: 이벤트 시스템 프로덕션 씬의 모든 POCO 객체 조립 및 의존성 주입을 진행하는 Composition Root.
        /// [작성자]: 윤승종
        /// </summary>
        public class EventSceneInitializer : MonoBehaviour
        {
            [Header("UI Views")]
            [SerializeField] private EventListView m_eventListView;
            [SerializeField] private EventDetailView m_eventDetailView;
            [SerializeField] private RewardPopupView m_rewardPopupView;

            [Header("Data Table Assets")]
            [SerializeField] private EventTableSO m_eventTable;

            private void Awake()
            {
                Initialize();
            }

            private void Initialize()
            {
                // 1. 저장 시스템 구성 (JSON 파일 사용)
                var saveSystem = new JsonSaveSystem();

                // 2. 전략 팩토리 설정
                var condFactory = new ConditionFactory(saveSystem);
                var rewFactory = new RewardFactory();

                // 3. Domain Model 조립
                var eventModel = new EventModel(m_eventTable, condFactory, rewFactory);
                var playerReward = saveSystem.LoadRewardState();

                // 4. MVVM ViewModels 수동 생성자 주입
                var listVM = new EventListViewModel(eventModel);
                var detailVM = new EventDetailViewModel(eventModel, saveSystem);
                var popupVM = new RewardPopupViewModel(playerReward, saveSystem);

                // 5. Views 바인딩 연결
                if (m_eventListView != null)
                {
                    m_eventListView.Bind(listVM);
                }
                if (m_eventDetailView != null)
                {
                    m_eventDetailView.Bind(detailVM, listVM, popupVM);
                }
                if (m_rewardPopupView != null)
                {
                    m_rewardPopupView.Bind(popupVM, detailVM);
                }
            }
        }
    }
    ```

- [ ] **Step 6: 변경 사항 커밋**
  ```bash
  git add Assets/_Game/Scripts/EventSystem/Views/ Assets/_Game/Scripts/EventSystem/Infrastructure/EventSceneInitializer.cs Assets/_Game/Scripts/EventSystem/DTOs/
  git commit -m "feat: implement MVVM UI Views and Composition Root Initializer"
  ```

---

### Task 7: 테스트용 격리 환경 (Test Save System, Debug MVVM & Test Initializer)

**Files:**
- Create: `Assets/_Game/Scripts/EventSystem/Infrastructure/InMemorySaveSystem.cs`
- Create: `Assets/_Game/Scripts/EventSystem/ViewModelsDebug/EventDebugViewModel.cs`
- Create: `Assets/_Game/Scripts/EventSystem/ViewsDebug/EventDebugView.cs`
- Create: `Assets/_Game/Scripts/EventSystem/Infrastructure/EventTestInitializer.cs`

- [ ] **Step 1: InMemorySaveSystem 구현**
  - `Assets/_Game/Scripts/EventSystem/Infrastructure/InMemorySaveSystem.cs` 작성:
    ```csharp
    using System.Collections.Generic;
    using BePex.EventSystem.Interfaces;
    using BePex.EventSystem.Models;

    namespace BePex.EventSystem.Infrastructure
    {
        /// <summary>
        /// [기능]: 파일 I/O를 우회하여 빠른 검증을 지원하는 인메모리 테스트 전용 세이브 장치.
        /// [작성자]: 윤승종
        /// </summary>
        public class InMemorySaveSystem : ISaveSystem
        {
            private readonly Dictionary<string, EventProgressModel> m_progressMap = new Dictionary<string, EventProgressModel>();
            private PlayerRewardModel m_rewardState = new PlayerRewardModel();

            public EventProgressModel LoadProgress(string eventId)
            {
                if (m_progressMap.ContainsKey(eventId) == false)
                {
                    m_progressMap[eventId] = new EventProgressModel { eventId = eventId };
                }
                return m_progressMap[eventId];
            }

            public void SaveProgress(string eventId, EventProgressModel progress)
            {
                m_progressMap[eventId] = progress;
            }

            public PlayerRewardModel LoadRewardState() => m_rewardState;

            public void SaveRewardState(PlayerRewardModel rewardState)
            {
                m_rewardState = rewardState;
            }

            public void ClearAll()
            {
                m_progressMap.Clear();
                m_rewardState = new PlayerRewardModel();
            }
        }
    }
    ```

- [ ] **Step 2: 디버그 MVVM 구현 (EventDebugViewModel, EventDebugView)**
  - `Assets/_Game/Scripts/EventSystem/ViewModelsDebug/EventDebugViewModel.cs` 작성:
    ```csharp
    using BePex.EventSystem.Models;
    using BePex.EventSystem.Interfaces;

    namespace BePex.EventSystem.ViewModelsDebug
    {
        /// <summary>
        /// [기능]: 테스트 화면에서 진행도를 강제 상승시키거나 세이브를 초기화하도록 모델을 조작하는 디버그용 뷰모델.
        /// [작성자]: 윤승종
        /// </summary>
        public class EventDebugViewModel
        {
            private readonly EventModel m_eventModel;
            private readonly ISaveSystem m_saveSystem;

            public EventDebugViewModel(EventModel eventModel, ISaveSystem saveSystem)
            {
                m_eventModel = eventModel;
                m_saveSystem = saveSystem;
            }

            public void SimulateAddProgress(string eventId, int amount)
            {
                m_eventModel.Debug_AddProgress(eventId, amount, m_saveSystem);
            }

            public void ResetAllData()
            {
                m_saveSystem.ClearAll();
                m_eventModel.Reload();
            }
        }
    }
    ```
  - `Assets/_Game/Scripts/EventSystem/ViewsDebug/EventDebugView.cs` 작성:
    ```csharp
    using UnityEngine;
    using UnityEngine.UI;
    using BePex.EventSystem.ViewModelsDebug;

    namespace BePex.EventSystem.ViewsDebug
    {
        /// <summary>
        /// [기능]: 인게임 검증 씬 내 디버그용 조작 버튼들을 바인딩하는 디버그 View.
        /// [작성자]: 윤승종
        /// </summary>
        public class EventDebugView : MonoBehaviour
        {
            [SerializeField] private InputField m_eventIdInput;
            [SerializeField] private InputField m_amountInput;
            [SerializeField] private Button m_addProgressButton;
            [SerializeField] private Button m_resetButton;

            private EventDebugViewModel m_viewModel;

            public void Bind(EventDebugViewModel viewModel)
            {
                m_viewModel = viewModel;

                m_addProgressButton.onClick.RemoveAllListeners();
                m_addProgressButton.onClick.AddListener(func_OnAddProgressClick);

                m_resetButton.onClick.RemoveAllListeners();
                m_resetButton.onClick.AddListener(func_OnResetClick);
            }

            public void func_OnAddProgressClick()
            {
                if (m_viewModel != null)
                {
                    string evId = m_eventIdInput.text;
                    int.TryParse(m_amountInput.text, out int amt);
                    m_viewModel.SimulateAddProgress(evId, amt);
                }
            }

            public void func_OnResetClick()
            {
                if (m_viewModel != null)
                {
                    m_viewModel.ResetAllData();
                }
            }
        }
    }
    ```

- [ ] **Step 3: 테스트용 Composition Root (EventTestInitializer) 구현**
  - `Assets/_Game/Scripts/EventSystem/Infrastructure/EventTestInitializer.cs` 작성:
    ```csharp
    using UnityEngine;
    using BePex.EventSystem.Views;
    using BePex.EventSystem.ViewsDebug;
    using BePex.EventSystem.Data;
    using BePex.EventSystem.Factories;
    using BePex.EventSystem.Models;
    using BePex.EventSystem.ViewModels;
    using BePex.EventSystem.ViewModelsDebug;

    namespace BePex.EventSystem.Infrastructure
    {
        /// <summary>
        /// [기능]: 테스트용 씬을 구성할 때 인메모리 저장 장치와 디버그 뷰를 함께 주입하고 조립하는 전용 Composition Root.
        /// [작성자]: 윤승종
        /// </summary>
        public class EventTestInitializer : MonoBehaviour
        {
            [Header("Production Views")]
            [SerializeField] private EventListView m_eventListView;
            [SerializeField] private EventDetailView m_eventDetailView;
            [SerializeField] private RewardPopupView m_rewardPopupView;

            [Header("Debug Controls")]
            [SerializeField] private EventDebugView m_debugView;

            [Header("Mock Data Assets")]
            [SerializeField] private EventTableSO m_mockTable;

            private void Awake()
            {
                Initialize();
            }

            private void Initialize()
            {
                // 1. 테스트용 인메모리 저장소 사용 (실제 유저 세이브 오염 방지)
                var testSaveSystem = new InMemorySaveSystem();

                // 2. 전략 팩토리 생성
                var condFactory = new ConditionFactory(testSaveSystem);
                var rewFactory = new RewardFactory();

                // 3. Domain Model 생성
                var eventModel = new EventModel(m_mockTable, condFactory, rewFactory);
                var playerReward = testSaveSystem.LoadRewardState();

                // 4. 프로덕션 ViewModels 수동 생성자 주입
                var listVM = new EventListViewModel(eventModel);
                var detailVM = new EventDetailViewModel(eventModel, testSaveSystem);
                var popupVM = new RewardPopupViewModel(playerReward, testSaveSystem);

                // 5. 디버그 전용 ViewModel 수동 주입
                var debugVM = new EventDebugViewModel(eventModel, testSaveSystem);

                // 6. UI Views 바인딩 연결
                if (m_eventListView != null)
                {
                    m_eventListView.Bind(listVM);
                }
                if (m_eventDetailView != null)
                {
                    m_eventDetailView.Bind(detailVM, listVM, popupVM);
                }
                if (m_rewardPopupView != null)
                {
                    m_rewardPopupView.Bind(popupVM, detailVM);
                }
                if (m_debugView != null)
                {
                    m_debugView.Bind(debugVM);
                }
            }
        }
    }
    ```

- [ ] **Step 4: 변경 사항 커밋**
  ```bash
  git add Assets/_Game/Scripts/EventSystem/Infrastructure/InMemorySaveSystem.cs Assets/_Game/Scripts/EventSystem/Infrastructure/EventTestInitializer.cs Assets/_Game/Scripts/EventSystem/ViewModelsDebug/ Assets/_Game/Scripts/EventSystem/ViewsDebug/
  git commit -m "feat: implement isolated debug test components and Initializer"
  ```

---

## Verification Plan

### Automated Tests
- Unity Test Runner에서 `EditMode`의 `BePex.EventSystem.Tests` 전체 유닛 테스트를 작동시킵니다.
- 검증 툴셋을 확장하여 모든 조건/보상 매핑 분기에 대한 팩토리 호출 성공 여부를 테스트 코드로 실행 확인합니다.

### Manual Verification
1.  **Mock 데이터 설정**:
    - 인스펙터 상에서 출석용 `Attendance`와 적 처치용 `KillCount`에 대한 ScriptableObject 에셋을 만들고 `EventTableSO`에 드래그하여 할당합니다.
2.  **독립 구동 테스트**:
    - `Assets/_Game/Scenes/Test_EventSystem.unity` 씬을 열고 플레이 버튼을 클릭합니다.
    - 디버그 패널(`EventDebugView`)에 특정 Event ID를 입력하고 진행도를 점진적으로 가득 채웠을 때 상세 뷰의 게이지가 갱신되는지 수동 검증합니다.
    - 목표치 달성 후 [보상 받기] 버튼을 누르고 `RewardPopupView`에 누적Exp/누적티켓이 즉시 표시 및 누적되는지 확인합니다.
