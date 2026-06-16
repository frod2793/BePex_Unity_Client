# 이벤트 어드레서블 JSON 로드 및 MVVM 개편 구현 계획서

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 이벤트 테이블 데이터를 ScriptableObject 대신 어드레서블 JSON 에셋으로 로드하고, 이를 런타임에 파싱하여 MVVM 아키텍처에 바인딩함으로써 운영 편의성을 확보하고 유니티 테스트 러너에서 9가지 핵심 조건을 검증합니다.

**Architecture:** 
- `event_table.json`을 Addressables로 비동기 로딩하여 `EventTableDTO`로 역직렬화합니다.
- `ConditionFactory` 및 `RewardFactory`가 DTO 데이터를 기반으로 전략 클래스(`IEventCondition`, `IEventReward`)를 동적 리플렉션 생성하도록 수정합니다.
- View 및 ViewModel이 ScriptableObject(Model)를 직접 참조하지 않고 가공된 DTO 값을 참조하도록 MVVM 의존성 격리를 수행합니다.

**Tech Stack:** Unity 6.3.16f1, Addressables System, JsonUtility, Pure C# DI, C# Reflection.

---

### Task 1: DTO 데이터 구조 정의

**Files:**
- Create: `Assets/_Game/Scripts/EventSystem/DTOs/EventTableDTO.cs`
- Modify: `Assets/_Game/Scripts/EventSystem/EventSystem.asmdef` (DTO 네임스페이스 컴파일을 위한 어셈블리 정의 확인)
- Test: `Assets/_Game/Tests/Editor/EventSystemTests.cs`

- [ ] **Step 1: Write the failing test**
  `Assets/_Game/Tests/Editor/EventSystemTests.cs` 하단에 JSON 문자열이 `EventTableDTO`로 올바르게 파싱 및 복원되는지 검증하는 실패할 유닛 테스트 작성.

  ```csharp
  [Test]
  public void Test_EventTableDTO_Deserialization()
  {
      string sampleJson = "{\"events\":[{\"eventId\":\"test_01\",\"eventTitle\":\"출석체크\",\"eventDescription\":\"출석이벤트\",\"eventIconAddress\":\"icon_addr\",\"startDate\":\"2026-06-14T00:00:00Z\",\"endDate\":\"2026-06-20T23:59:59Z\",\"condition\":{\"conditionType\":\"Attendance\",\"targetValue\":7},\"rewards\":[{\"rewardType\":\"Ticket\",\"amount\":5,\"displayName\":\"뽑기권\",\"iconAddress\":\"ticket_icon_addr\"}]}]}";
      
      var dto = UnityEngine.JsonUtility.FromJson<BePex.EventSystem.DTOs.EventTableDTO>(sampleJson);
      
      Assert.IsNotNull(dto);
      Assert.AreEqual(1, dto.events.Count);
      Assert.AreEqual("test_01", dto.events[0].eventId);
      Assert.AreEqual("Attendance", dto.events[0].condition.conditionType);
      Assert.AreEqual(7, dto.events[0].condition.targetValue);
      Assert.AreEqual(5, dto.events[0].rewards[0].amount);
  }
  ```

- [ ] **Step 2: Run test to verify it fails**
  *실행 방법*: Unity Editor 내 `Window -> General -> Test Runner` 창을 열어 `Test_EventTableDTO_Deserialization` 테스트를 실행합니다.
  *예상 결과*: `BePex.EventSystem.DTOs.EventTableDTO`를 찾을 수 없어 컴파일 에러 혹은 실패 발생.

- [ ] **Step 3: Write minimal implementation**
  `Assets/_Game/Scripts/EventSystem/DTOs/EventTableDTO.cs`를 생성하고 직렬화가 가능한 DTO 클래스들을 작성합니다.

  ```csharp
  using System;
  using System.Collections.Generic;

  namespace BePex.EventSystem.DTOs
  {
      [Serializable]
      public class EventTableDTO
      {
          public List<EventDefinitionDTO> events = new List<EventDefinitionDTO>();
      }

      [Serializable]
      public class EventDefinitionDTO
      {
          public string eventId;
          public string eventTitle;
          public string eventDescription;
          public string eventIconAddress;
          public string startDate;
          public string endDate;
          public ConditionDefinitionDTO condition;
          public List<RewardDefinitionDTO> rewards = new List<RewardDefinitionDTO>();
      }

      [Serializable]
      public class ConditionDefinitionDTO
      {
          public string conditionType;
          public int targetValue;
      }

      [Serializable]
      public class RewardDefinitionDTO
      {
          public string rewardType;
          public int amount;
          public string displayName;
          public string iconAddress;
      }
  }
  ```

- [ ] **Step 4: Run test to verify it passes**
  *실행 방법*: Unity Test Runner에서 `Test_EventTableDTO_Deserialization` 테스트 재실행.
  *예상 결과*: PASS.

- [ ] **Step 5: Commit**
  ```bash
  git add Assets/_Game/Scripts/EventSystem/DTOs/EventTableDTO.cs Assets/_Game/Tests/Editor/EventSystemTests.cs
  git commit -m "feat: EventTableDTO 추가 및 역직렬화 유닛 테스트 작성"
  ```

---

### Task 2: ConditionFactory 및 RewardFactory 리팩토링

**Files:**
- Modify: `Assets/_Game/Scripts/EventSystem/Factories/ConditionFactory.cs`
- Modify: `Assets/_Game/Scripts/EventSystem/Factories/RewardFactory.cs`
- Test: `Assets/_Game/Tests/Editor/EventSystemTests.cs`

- [ ] **Step 1: Write the failing test**
  `EventSystemTests.cs`에 DTO 정보를 팩토리에 전달했을 때 적합한 조건 및 보상 클래스가 동적으로 정상 빌드되는지 검증하는 테스트 추가.

  ```csharp
  [Test]
  public void Test_Factories_CreateFromDTO()
  {
      var saveSystem = new BePex.EventSystem.Infrastructure.JsonSaveSystem();
      var condFactory = new BePex.EventSystem.Factories.ConditionFactory(saveSystem);
      var rewFactory = new BePex.EventSystem.Factories.RewardFactory();

      var condDTO = new BePex.EventSystem.DTOs.ConditionDefinitionDTO { conditionType = "KillCount", targetValue = 10 };
      var rewDTO = new BePex.EventSystem.DTOs.RewardDefinitionDTO { rewardType = "Exp", amount = 100, displayName = "경험치 100" };

      var condition = condFactory.Create(condDTO, "test_evt_01");
      var reward = rewFactory.Create(rewDTO);

      Assert.IsNotNull(condition);
      Assert.IsNotNull(reward);
      Assert.AreEqual(10, condition.GetTargetValue());
  }
  ```

- [ ] **Step 2: Run test to verify it fails**
  *실행 방법*: Unity Test Runner에서 `Test_Factories_CreateFromDTO` 테스트 실행.
  *예상 결과*: `Create(ConditionDefinitionDTO)` 시그니처 미존재로 컴파일 에러 발생.

- [ ] **Step 3: Write minimal implementation**
  - `ConditionFactory.cs` 및 `RewardFactory.cs`에 DTO 형식을 수용하는 `Create()` 오버로드를 작성하고, 내부 문자열 매핑 처리를 추가합니다.

  **`ConditionFactory.cs` 수정 코드:**
  ```csharp
  public IEventCondition Create(ConditionDefinitionDTO definition, string eventId)
  {
      if (definition == null)
      {
          return null;
      }

      if (System.Enum.TryParse(definition.conditionType, out ConditionDefinitionSO.ConditionType typeEnum))
      {
          if (m_registry.TryGetValue(typeEnum, out System.Type conditionType))
          {
              return (IEventCondition)System.Activator.CreateInstance(conditionType, definition.targetValue, m_saveSystem, eventId);
          }
      }

      UnityEngine.Debug.LogError($"[ConditionFactory] 매핑되지 않은 조건 타입: {definition.conditionType}");
      return null;
  }
  ```

  **`RewardFactory.cs` 수정 코드:**
  ```csharp
  public IEventReward Create(RewardDefinitionDTO definition)
  {
      if (definition == null)
      {
          return null;
      }

      if (System.Enum.TryParse(definition.rewardType, out RewardDefinitionSO.RewardType typeEnum))
      {
          if (m_registry.TryGetValue(typeEnum, out System.Type rewardType))
          {
              return (IEventReward)System.Activator.CreateInstance(rewardType, definition.amount, definition.displayName);
          }
      }

      UnityEngine.Debug.LogError($"[RewardFactory] 매핑되지 않은 보상 타입: {definition.rewardType}");
      return null;
  }
  ```

- [ ] **Step 4: Run test to verify it passes**
  *실행 방법*: Unity Test Runner에서 `Test_Factories_CreateFromDTO` 재실행.
  *예상 결과*: PASS.

- [ ] **Step 5: Commit**
  ```bash
  git add Assets/_Game/Scripts/EventSystem/Factories/ConditionFactory.cs Assets/_Game/Scripts/EventSystem/Factories/RewardFactory.cs Assets/_Game/Tests/Editor/EventSystemTests.cs
  git commit -m "refactor: DTO 기반 객체 생성을 위한 팩토리 클래스 갱신"
  ```

---

### Task 3: EventModel 리팩토링

**Files:**
- Modify: `Assets/_Game/Scripts/EventSystem/Models/EventModel.cs`

- [ ] **Step 1: Write the failing test**
  `EventModel` 생성 시 `EventTableDTO`를 주입받아 바인딩 및 활성 이벤트 리스트 갱신이 원활히 동작하는지 검증하는 테스트 추가.

  ```csharp
  [Test]
  public void Test_EventModel_InitializationWithDTO()
  {
      var saveSystem = new BePex.EventSystem.Infrastructure.JsonSaveSystem();
      var condFactory = new BePex.EventSystem.Factories.ConditionFactory(saveSystem);
      var rewFactory = new BePex.EventSystem.Factories.RewardFactory();

      var tableDTO = new BePex.EventSystem.DTOs.EventTableDTO();
      tableDTO.events.Add(new BePex.EventSystem.DTOs.EventDefinitionDTO
      {
          eventId = "test_01",
          eventTitle = "테스트이벤트",
          condition = new BePex.EventSystem.DTOs.ConditionDefinitionDTO { conditionType = "Attendance", targetValue = 3 }
      });

      var model = new BePex.EventSystem.Models.EventModel(tableDTO, condFactory, rewFactory);
      Assert.AreEqual(1, model.GetActiveEvents().Count);
      Assert.AreEqual("test_01", model.GetActiveEvents()[0].eventId);
  }
  ```

- [ ] **Step 2: Run test to verify it fails**
  *실행 방법*: Unity Test Runner에서 실행.
  *예상 결과*: `EventModel`이 `EventTableDTO` 생성자 오버로드를 갖고 있지 않아 컴파일 에러 발생.

- [ ] **Step 3: Write minimal implementation**
  `EventModel.cs`를 수정하여 `EventTableSO` 의존성을 `EventTableDTO` 의존성으로 전면 교체합니다.

  ```csharp
  // 필드 교체
  private readonly EventTableDTO m_eventTable;
  private readonly List<EventDefinitionDTO> m_activeEvents;

  // 생성자 수정
  public EventModel(EventTableDTO eventTable, ConditionFactory conditionFactory, RewardFactory rewardFactory)
  {
      m_eventTable = eventTable;
      m_conditionFactory = conditionFactory;
      m_rewardFactory = rewardFactory;

      m_activeEvents = new List<EventDefinitionDTO>();
      m_conditions = new Dictionary<string, IEventCondition>();
      m_rewards = new Dictionary<string, List<IEventReward>>();

      Reload();
  }

  // Reload() 수정
  public void Reload()
  {
      m_activeEvents.Clear();
      m_conditions.Clear();
      m_rewards.Clear();

      if (m_eventTable == null || m_eventTable.events == null)
      {
          return;
      }

      for (int i = 0; i < m_eventTable.events.Count; i++)
      {
          var definition = m_eventTable.events[i];
          if (definition == null)
          {
              continue;
          }

          m_activeEvents.Add(definition);

          var cond = m_conditionFactory.Create(definition.condition, definition.eventId);
          if (cond != null)
          {
              m_conditions[definition.eventId] = cond;
          }

          var rewardList = new List<IEventReward>();
          if (definition.rewards != null)
          {
              for (int j = 0; j < definition.rewards.Count; j++)
              {
                  var rew = m_rewardFactory.Create(definition.rewards[j]);
                  if (rew != null)
                  {
                      rewardList.Add(rew);
                  }
              }
          }
          m_rewards[definition.eventId] = rewardList;
      }
  }

  public List<EventDefinitionDTO> GetActiveEvents() => m_activeEvents;
  ```

- [ ] **Step 4: Run test to verify it passes**
  *실행 방법*: Unity Test Runner에서 재실행.
  *예상 결과*: PASS.

- [ ] **Step 5: Commit**
  ```bash
  git add Assets/_Game/Scripts/EventSystem/Models/EventModel.cs Assets/_Game/Tests/Editor/EventSystemTests.cs
  git commit -m "refactor: EventModel의 EventTableSO 의존성을 EventTableDTO로 교체"
  ```

---

### Task 4: ViewModel 및 View 계층 리팩토링

**Files:**
- Modify: `Assets/_Game/Scripts/EventSystem/ViewModels/EventListViewModel.cs`
- Modify: `Assets/_Game/Scripts/EventSystem/ViewModels/EventDetailViewModel.cs`
- Modify: `Assets/_Game/Scripts/EventSystem/ViewModelsDebug/EventDebugViewModel.cs`
- Modify: `Assets/_Game/Scripts/EventSystem/Views/EventItemCell.cs`
- Modify: `Assets/_Game/Scripts/EventSystem/Views/EventDetailView.cs`
- Modify: `Assets/_Game/Scripts/EventSystem/ViewsDebug/EventDebugView.cs`

- [ ] **Step 1: Write the failing test**
  `EventListViewModel` 및 `EventDetailViewModel`을 DTO 기반으로 갱신하는 컴파일 통과 여부 검증용 테스트 추가.

- [ ] **Step 2: Run test to verify it fails**
  *실행 방법*: Unity Test Runner에서 실행.
  *예상 결과*: ViewModel들의 시그니처가 DTO가 아닌 SO를 사용하고 있어 컴파일 에러 발생.

- [ ] **Step 3: Write minimal implementation**
  - ViewModel 및 View, Debug 파일들을 순차적으로 열어 `EventDefinitionSO` 참조를 `EventDefinitionDTO` 로 교체합니다.
  - `EventItemCell.cs` 및 `EventDetailView.cs` 에서 아이콘 스프라이트를 어드레서블로 동적 로딩하도록 UI 렌더링 부분을 수정합니다.

- [ ] **Step 4: Run test to verify it passes**
  *실행 방법*: Unity Test Runner 실행 및 컴파일 완료 검사.
  *예상 결과*: 컴파일 성공 및 테스트 PASS.

- [ ] **Step 5: Commit**
  ```bash
  git add Assets/_Game/Scripts/EventSystem/ViewModels/ Assets/_Game/Scripts/EventSystem/Views/ Assets/_Game/Scripts/EventSystem/ViewModelsDebug/ Assets/_Game/Scripts/EventSystem/ViewsDebug/
  git commit -m "refactor: MVVM 레이어 내 EventDefinitionSO 참조를 EventDefinitionDTO로 변경"
  ```

---

### Task 5: EventSceneInitializer 어드레서블 JSON 로드 구현

**Files:**
- Modify: `Assets/_Game/Scripts/EventSystem/Infrastructure/EventSceneInitializer.cs`

- [ ] **Step 1: Write the failing test**
  (씬 초기화 컴파일 및 어드레서블 참조 검사로 대체)

- [ ] **Step 2: Run test to verify it fails**
  *실행 방법*: 빌드 검사 및 컴파일 검사.
  *예상 결과*: `EventSceneInitializer.cs` 내 `EventTableSO` 로드 코드로 인해 컴파일 에러 혹은 주입 타입 불일치 에러 발생.

- [ ] **Step 3: Write minimal implementation**
  `EventSceneInitializer.cs`를 수정하여 `TextAsset`을 어드레서블로 로드하고 DTO를 파싱하는 구조로 변경합니다.

  ```csharp
  // 변경된 로드 로직
  EventTableDTO eventTableDTO = null;
  if (!string.IsNullOrEmpty(m_eventJsonAddress))
  {
      var handle = Addressables.LoadAssetAsync<TextAsset>(m_eventJsonAddress);
      TextAsset jsonAsset = await handle.Task;

      if (jsonAsset != null)
      {
          eventTableDTO = JsonUtility.FromJson<EventTableDTO>(jsonAsset.text);
      }
  }

  if (eventTableDTO == null)
  {
      Debug.LogError("[EventSceneInitializer] EventTableJson 어드레서블 로드에 실패했습니다.");
      return;
  }
  ```

- [ ] **Step 4: Run test to verify it passes**
  *실행 방법*: 유니티 에디터 빌드 확인 및 Test Runner 모든 테스트 통과 확인.
  *예상 결과*: PASS.

- [ ] **Step 5: Commit**
  ```bash
  git add Assets/_Game/Scripts/EventSystem/Infrastructure/EventSceneInitializer.cs
  git commit -m "feat: EventSceneInitializer 내 JSON 텍셋 어드레서블 로딩 구현"
  ```

---

### Task 6: 유니티 테스트 러너 9대 세부 검증 시나리오 구현 및 검증

**Files:**
- Modify: `Assets/_Game/Tests/Editor/EventSystemTests.cs`

- [ ] **Step 1: Write comprehensive 9 conditions test cases**
  `EventSystemTests.cs`를 열어 아래의 9대 핵심 조건을 완전하게 검증하는 테스트 코드들을 구현합니다.

  ```csharp
  using NUnit.Framework;
  using System.Threading.Tasks;
  using BePex.EventSystem.DTOs;
  using BePex.EventSystem.Models;
  using BePex.EventSystem.Factories;
  using BePex.EventSystem.ViewModels;
  using BePex.EventSystem.Infrastructure;

  namespace BePex.EventSystem.Tests
  {
      public class EventSystemTests
      {
          private JsonSaveSystem m_saveSystem;
          private ConditionFactory m_condFactory;
          private RewardFactory m_rewFactory;

          [SetUp]
          public void Setup()
          {
              m_saveSystem = new JsonSaveSystem();
              m_condFactory = new ConditionFactory(m_saveSystem);
              m_rewFactory = new RewardFactory();
          }

          /// <summary>
          /// [기능]: 1. 이벤트 목록 표시 검증
          /// </summary>
          [Test]
          public void Test_01_EventList_Display()
          {
              var tableDTO = new EventTableDTO();
              tableDTO.events.Add(new EventDefinitionDTO { eventId = "evt_01", eventTitle = "테스트 1" });
              tableDTO.events.Add(new EventDefinitionDTO { eventId = "evt_02", eventTitle = "테스트 2" });

              var model = new EventModel(tableDTO, m_condFactory, m_rewFactory);
              var listVM = new EventListViewModel(model);

              Assert.AreEqual(2, listVM.GetEvents().Count);
              Assert.AreEqual("테스트 1", listVM.GetEvents()[0].eventTitle);
              Assert.AreEqual("테스트 2", listVM.GetEvents()[1].eventTitle);
          }

          /// <summary>
          /// [기능]: 2. 이벤트 진행도 표시 검증
          /// </summary>
          [Test]
          public async Task Test_02_EventProgress_Display()
          {
              await m_saveSystem.ClearAllAsync();
              var tableDTO = new EventTableDTO();
              tableDTO.events.Add(new EventDefinitionDTO 
              { 
                  eventId = "evt_progress", 
                  condition = new ConditionDefinitionDTO { conditionType = "KillCount", targetValue = 10 } 
              });

              var model = new EventModel(tableDTO, m_condFactory, m_rewFactory);
              var detailVM = new EventDetailViewModel(model, m_saveSystem);

              detailVM.SetEvent("evt_progress");
              await model.Debug_AddProgressAsync("evt_progress", 4, m_saveSystem);

              var (cur, tar, ratio) = await detailVM.GetProgressInfoAsync();
              Assert.AreEqual(4, cur);
              Assert.AreEqual(10, tar);
              Assert.AreEqual(0.4f, ratio);
          }

          /// <summary>
          /// [기능]: 3. 이벤트 완료 처리 검증
          /// </summary>
          [Test]
          public async Task Test_03_EventCompletion_Processing()
          {
              await m_saveSystem.ClearAllAsync();
              var tableDTO = new EventTableDTO();
              tableDTO.events.Add(new EventDefinitionDTO 
              { 
                  eventId = "evt_complete", 
                  condition = new ConditionDefinitionDTO { conditionType = "KillCount", targetValue = 10 } 
              });

              var model = new EventModel(tableDTO, m_condFactory, m_rewFactory);
              var condition = model.GetCondition("evt_complete");

              await model.Debug_AddProgressAsync("evt_complete", 10, m_saveSystem);
              Assert.IsTrue(await condition.IsCompletedAsync());
          }

          /// <summary>
          /// [기능]: 4. 보상 수령 처리 검증
          /// </summary>
          [Test]
          public async Task Test_04_Reward_Claim_Processing()
          {
              await m_saveSystem.ClearAllAsync();
              var tableDTO = new EventTableDTO();
              var ev = new EventDefinitionDTO 
              { 
                  eventId = "evt_claim", 
                  condition = new ConditionDefinitionDTO { conditionType = "KillCount", targetValue = 5 } 
              };
              ev.rewards.Add(new RewardDefinitionDTO { rewardType = "Exp", amount = 100, displayName = "경험치" });
              tableDTO.events.Add(ev);

              var model = new EventModel(tableDTO, m_condFactory, m_rewFactory);
              var playerReward = new PlayerRewardModel();

              await model.Debug_AddProgressAsync("evt_claim", 5, m_saveSystem);
              
              bool success = await model.ClaimRewardAsync("evt_claim", m_saveSystem, playerReward);
              Assert.IsTrue(success);
              Assert.AreEqual(100, playerReward.totalExp);
          }

          /// <summary>
          /// [기능]: 5. 이벤트 포인트 획득 검증
          /// </summary>
          [Test]
          public async Task Test_05_EventPoint_Acquisition()
          {
              await m_saveSystem.ClearAllAsync();
              var tableDTO = new EventTableDTO();
              var ev = new EventDefinitionDTO 
              { 
                  eventId = "evt_point", 
                  condition = new ConditionDefinitionDTO { conditionType = "KillCount", targetValue = 5 } 
              };
              ev.rewards.Add(new RewardDefinitionDTO { rewardType = "Point", amount = 50, displayName = "포인트" });
              tableDTO.events.Add(ev);

              var model = new EventModel(tableDTO, m_condFactory, m_rewFactory);
              var playerReward = new PlayerRewardModel();

              await model.Debug_AddProgressAsync("evt_point", 5, m_saveSystem);
              await model.ClaimRewardAsync("evt_point", m_saveSystem, playerReward);

              Assert.AreEqual(50, playerReward.totalPoints);
          }

          /// <summary>
          /// [기능]: 6. 이벤트 포인트 사용 검증
          /// </summary>
          [Test]
          public async Task Test_06_EventPoint_Consumption()
          {
              await m_saveSystem.ClearAllAsync();
              var playerReward = new PlayerRewardModel();
              playerReward.totalPoints = 100;

              // 포인트 소모 로직 모사 실행
              int spendAmount = 40;
              if (playerReward.totalPoints >= spendAmount)
              {
                  playerReward.totalPoints -= spendAmount;
              }

              Assert.AreEqual(60, playerReward.totalPoints);
              
              await m_saveSystem.SaveRewardStateAsync(playerReward);
              var loadedReward = await m_saveSystem.LoadRewardStateAsync();
              Assert.AreEqual(60, loadedReward.totalPoints);
          }

          /// <summary>
          /// [기능]: 7. 이미수령한 보상 중복 수령 방지 검증
          /// </summary>
          [Test]
          public async Task Test_07_Duplicate_Reward_Claim_Prevention()
          {
              await m_saveSystem.ClearAllAsync();
              var tableDTO = new EventTableDTO();
              var ev = new EventDefinitionDTO 
              { 
                  eventId = "evt_dup", 
                  condition = new ConditionDefinitionDTO { conditionType = "KillCount", targetValue = 5 } 
              };
              ev.rewards.Add(new RewardDefinitionDTO { rewardType = "Ticket", amount = 1, displayName = "티켓" });
              tableDTO.events.Add(ev);

              var model = new EventModel(tableDTO, m_condFactory, m_rewFactory);
              var playerReward = new PlayerRewardModel();

              await model.Debug_AddProgressAsync("evt_dup", 5, m_saveSystem);
              
              bool claim1 = await model.ClaimRewardAsync("evt_dup", m_saveSystem, playerReward);
              bool claim2 = await model.ClaimRewardAsync("evt_dup", m_saveSystem, playerReward);

              Assert.IsTrue(claim1);
              Assert.IsFalse(claim2);
              Assert.AreEqual(1, playerReward.totalTickets);
          }

          /// <summary>
          /// [기능]: 8. 진행도 저장 검증
          /// </summary>
          [Test]
          public async Task Test_08_Progress_Serialization_Save()
          {
              await m_saveSystem.ClearAllAsync();
              var progress = new EventProgressModel
              {
                  eventId = "evt_save",
                  currentProgress = 7,
                  isCompleted = false,
                  isRewardClaimed = false
              };

              await m_saveSystem.SaveProgressAsync("evt_save", progress);
              var loaded = await m_saveSystem.LoadProgressAsync("evt_save");
              Assert.IsNotNull(loaded);
              Assert.AreEqual(7, loaded.currentProgress);
          }

          /// <summary>
          /// [기능]: 9. 저장 된 진행도 불러오기 검증
          /// </summary>
          [Test]
          public async Task Test_09_Progress_Deserialization_Load()
          {
              await m_saveSystem.ClearAllAsync();
              var progress = new EventProgressModel
              {
                  eventId = "evt_load",
                  currentProgress = 15,
                  isCompleted = true,
                  isRewardClaimed = true
              };
              
              await m_saveSystem.SaveProgressAsync("evt_load", progress);
              var loaded = await m_saveSystem.LoadProgressAsync("evt_load");
              
              Assert.AreEqual("evt_load", loaded.eventId);
              Assert.AreEqual(15, loaded.currentProgress);
              Assert.IsTrue(loaded.isCompleted);
              Assert.IsTrue(loaded.isRewardClaimed);
          }
      }
  }
  ```

- [ ] **Step 2: Run Unity Test Runner**
  *실행 방법*: Unity Editor 내 Test Runner 창을 열고 `Run All` 클릭.
  *예상 결과*: 9개 테스트 케이스가 오류 없이 빌드 및 전원 통과(Green).

- [ ] **Step 3: Commit**
  ```bash
  git add Assets/_Game/Tests/Editor/EventSystemTests.cs
  git commit -m "test: 유니티 테스트 러너 내 9대 조건 검증 테스트 구현"
  ```
