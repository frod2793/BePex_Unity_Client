# 이벤트 시스템 관리자 프로그램(Unity EventAdminScene) 구현 계획서

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 기획자 및 운영진이 유니티 에디터를 직접 건드리지 않고 인게임 씬 상에서 이벤트를 CRUD하고, 규격화된 `event_table.json`을 로컬 저장하거나 Firebase 클라우드 원격지에 모의 업로드하여 배포할 수 있는 유니티 관리자 씬 및 비동기 파이프라인 기초를 구현합니다.

**Architecture:** 
- 수동 의존성 주입(Pure DI)을 수행하는 `EventAdminSceneInitializer` 컴포지션 루트를 갖춘 독립 유니티 씬 형태입니다.
- MVP/MVVM 패턴에 의거하여 상태 및 명령 관리를 POCO C# 객체인 `EventAdminViewModel`로 캡슐화하고, `EventAdminView`는 UI 갱신 및 사용자 입력 전송 역할만 수행합니다.
- `IFirebaseUploadService` 인터페이스와 모의 지연 1.5초를 포함하는 `MockFirebaseUploadService` 구현체를 통해 Firebase 클라우드 REST/SDK 통신 파이프라인의 안전한 확장 여지를 제공합니다.

**Tech Stack:** Unity 6.3.16f1, UGUI (TextMeshPro), C# Async (Awaitable), NUnit (Unity Test Runner).

---

### Task 1: Firebase 업로드 서비스 추상화 및 모킹

**Files:**
- Create: `Assets/_Game/Scripts/EventSystem/Interfaces/IFirebaseUploadService.cs`
- Create: `Assets/_Game/Scripts/EventSystem/Infrastructure/MockFirebaseUploadService.cs`
- Modify: `Assets/_Game/Tests/Editor/EventSystemTests.cs`

- [ ] **Step 1: Firebase 업로드 서비스 인터페이스 정의**
  `IFirebaseUploadService.cs` 파일을 새로 생성하고, 이벤트 DTO를 Firebase에 업로드하는 비동기 메서드를 정의합니다. using 지시문을 적극 활용하여 네임스페이스 지정을 생략합니다.

  ```csharp
  using UnityEngine;
  using BePex.EventSystem.DTOs;

  namespace BePex.EventSystem.Interfaces
  {
      /// <summary>
      /// [기능]: 직렬화된 이벤트 JSON 데이터를 Firebase 서버로 안전하게 업로드하는 통신 인터페이스.
      /// [작성자]: 윤승종
      /// </summary>
      public interface IFirebaseUploadService
      {
          /// <summary>
          /// [기능]: 이벤트 DTO 데이터를 JSON 문자열로 변환하여 Firebase Storage 또는 Realtime DB에 비동기로 업로드합니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 정의
          /// </summary>
          Awaitable<bool> UploadEventTableAsync(EventTableDTO tableDTO);
      }
  }
  ```

- [ ] **Step 2: Mock Firebase 업로드 서비스 구현**
  `MockFirebaseUploadService.cs` 파일을 생성하고, 비동기 지연과 JSON 덤프 로그를 출력하는 가상 업로드 기능을 작성합니다. 모든 if문에 중괄호를 필수 적용합니다.

  ```csharp
  using UnityEngine;
  using BePex.EventSystem.Interfaces;
  using BePex.EventSystem.DTOs;

  namespace BePex.EventSystem.Infrastructure
  {
      /// <summary>
      /// [기능]: 테스트 환경 및 로컬 개발용 Firebase 업로드 모사(Mock) 구현 클래스.
      /// [작성자]: 윤승종
      /// </summary>
      public class MockFirebaseUploadService : IFirebaseUploadService
      {
          /// <summary>
          /// [기능]: JSON 데이터를 인코딩하여 Firebase 가상 엔드포인트 업로드 로그를 출력합니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 구현 및 Awaitable 대응
          /// </summary>
          public async Awaitable<bool> UploadEventTableAsync(EventTableDTO tableDTO)
          {
              Debug.Log("[MockFirebaseUploadService] Firebase 업로드 요청 수락됨.");
              
              if (tableDTO == null)
              {
                  Debug.LogError("[MockFirebaseUploadService] 테이블 DTO가 널입니다.");
                  return false;
              }

              string jsonText = JsonUtility.ToJson(tableDTO, true);
              Debug.Log($"[MockFirebaseUploadService] 변환된 JSON 파일 본문:\n{jsonText}");

              // 1.5초 대기 시뮬레이션
              await Awaitable.WaitForSecondsAsync(1.5f);

              Debug.Log("[MockFirebaseUploadService] Firebase 서버 업로드 완료! (가상 성공)");
              return true;
          }
      }
  }
  ```

- [ ] **Step 3: Mock 서비스 비동기 검증 테스트 작성**
  `EventSystemTests.cs` 최하단에 Mock 서비스 비동기 작동 검증 코드를 작성합니다.

  ```csharp
          /// <summary>
          /// [기능]: 10. Mock Firebase 업로드 비동기 시뮬레이션 검증
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 작성
          /// </summary>
          [Test]
          public async Task Test_10_MockFirebaseUpload_Verify()
          {
              var tableDTO = new EventTableDTO();
              tableDTO.events.Add(new EventDefinitionDTO { eventId = "evt_firebase_test", eventTitle = "파이어베이스 테스트" });

              var firebaseService = new MockFirebaseUploadService();
              bool success = await firebaseService.UploadEventTableAsync(tableDTO);

              Debug.Log($"[EventSystemTests] [Test_10_MockFirebaseUpload_Verify] " +
                        $"입력값: 이벤트 DTO 리스트 1개 전달 | " +
                        $"출력값: Firebase 비동기 가상 업로드 결과={success}");

              Assert.IsTrue(success);
          }
  ```

- [ ] **Step 4: 에디터 테스트 실행 및 통과 검증**
  유니티 에디터 상에서 Test Runner를 기동하여 `Test_10_MockFirebaseUpload_Verify`가 1.5초 대기 후 정상 통과 및 로그가 출력되는지 점검합니다.

- [ ] **Step 5: Commit**
  ```bash
  git add Assets/_Game/Scripts/EventSystem/Interfaces/IFirebaseUploadService.cs Assets/_Game/Scripts/EventSystem/Infrastructure/MockFirebaseUploadService.cs Assets/_Game/Tests/Editor/EventSystemTests.cs
  git commit -m "feat: Firebase 업로드 서비스 추상 인터페이스 및 Mock 업로드 구현 추가"
  ```

---

### Task 2: 이벤트 관리자 뷰모델 (`EventAdminViewModel`) 구현

**Files:**
- Create: `Assets/_Game/Scripts/EventSystem/ViewModels/EventAdminViewModel.cs`
- Modify: `Assets/_Game/Tests/Editor/EventSystemTests.cs`

- [ ] **Step 1: 관리자 뷰모델 C# 클래스 구현**
  `EventAdminViewModel.cs` 파일에 CRUD 상태 갱신, 유효성 검사, 로컬 파일 저장 및 Firebase 업로드 명령을 제공하는 비즈니스 POCO 로직을 작성합니다. 모든 메서드에 XML 주석 헤더를 포함하고, 루프 최적화를 위해 `for` 루프를 사용하며, 중괄호 생략을 원천 배제합니다.

  ```csharp
  using System;
  using System.IO;
  using System.Collections.Generic;
  using UnityEngine;
  using BePex.EventSystem.DTOs;
  using BePex.EventSystem.Interfaces;

  namespace BePex.EventSystem.ViewModels
  {
      /// <summary>
      /// [기능]: 이벤트 관리자 씬의 데이터 상태(CRUD, 로컬 저장, 원격 업로드)를 관리하고 View와 통신하는 ViewModel 클래스.
      /// [작성자]: 윤승종
      /// </summary>
      public class EventAdminViewModel
      {
          #region 이벤트 정의
          public event Action OnEventListChanged;
          public event Action<string> OnEventSelected;
          public event Action<string> OnErrorOccurred;
          public event Action<bool> OnSaveCompleted;
          public event Action<bool> OnUploadCompleted;
          #endregion

          #region 내부 필드
          private EventTableDTO m_eventTable;
          private string m_selectedEventId;
          private readonly IFirebaseUploadService m_firebaseService;
          #endregion

          #region 초기화
          /// <summary>
          /// [기능]: Firebase 업로드 서비스 의존성을 주입받아 초기화합니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 생성
          /// </summary>
          public EventAdminViewModel(IFirebaseUploadService firebaseService)
          {
              m_firebaseService = firebaseService;
              m_eventTable = new EventTableDTO();
              m_selectedEventId = string.Empty;
          }
          #endregion

          #region 공개 메서드
          /// <summary>
          /// [기능]: 이벤트 전체 테이블 DTO를 뷰모델의 상태로 등록하고 리스트 변경을 노출합니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 정의
          /// </summary>
          public void SetEventTable(EventTableDTO table)
          {
              if (table != null)
              {
                  m_eventTable = table;
              }
              else
              {
                  m_eventTable = new EventTableDTO();
              }
              m_selectedEventId = string.Empty;
              if (OnEventListChanged != null)
              {
                  OnEventListChanged.Invoke();
              }
          }

          /// <summary>
          /// [기능]: 현재 뷰모델이 보유한 이벤트 테이블 DTO 레퍼런스를 가져옵니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 정의
          /// </summary>
          public EventTableDTO GetEventTable()
          {
              return m_eventTable;
          }

          /// <summary>
          /// [기능]: 등록된 전체 이벤트 정의 리스트를 반환합니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 정의
          /// </summary>
          public List<EventDefinitionDTO> GetEvents()
          {
              return m_eventTable.events;
          }

          /// <summary>
          /// [기능]: 현재 선택된 이벤트 정의 DTO를 반환하며, 선택이 없는 경우 null을 반환합니다. GC 방지를 위해 for 루프를 사용합니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 정의
          /// </summary>
          public EventDefinitionDTO GetSelectedEvent()
          {
              if (string.IsNullOrEmpty(m_selectedEventId))
              {
                  return null;
              }
              for (int i = 0; i < m_eventTable.events.Count; i++)
              {
                  if (m_eventTable.events[i].eventId == m_selectedEventId)
                  {
                      return m_eventTable.events[i];
                  }
              }
              return null;
          }

          /// <summary>
          /// [기능]: 주어진 ID에 해당하는 이벤트를 현재 대상으로 활성화하고 선택 이벤트를 통지합니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 정의
          /// </summary>
          public void SelectEvent(string eventId)
          {
              m_selectedEventId = eventId;
              if (OnEventSelected != null)
              {
                  OnEventSelected.Invoke(m_selectedEventId);
              }
          }

          /// <summary>
          /// [기능]: 신규 디폴트 이벤트를 테이블 리스트에 삽입하고 해당 이벤트를 활성화합니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 정의
          /// </summary>
          public void AddNewEvent()
          {
              string newId = $"evt_new_{DateTime.Now.Ticks}";
              var newEvent = new EventDefinitionDTO()
              {
                  eventId = newId,
                  eventTitle = "새로운 이벤트",
                  eventDescription = "이벤트 설명을 입력하세요.",
                  eventIconAddress = "UI/Icons/Default",
                  startDate = DateTime.Now.ToString("yyyy-MM-dd"),
                  endDate = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd"),
                  condition = new ConditionDefinitionDTO { conditionType = "KillCount", targetValue = 10 },
                  rewards = new List<RewardDefinitionDTO>()
              };

              m_eventTable.events.Add(newEvent);
              SelectEvent(newId);
              if (OnEventListChanged != null)
              {
                  OnEventListChanged.Invoke();
              }
          }

          /// <summary>
          /// [기능]: 특정 ID의 이벤트를 테이블 리스트에서 영구히 삭제합니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 정의
          /// </summary>
          public void RemoveEvent(string eventId)
          {
              int targetIndex = -1;
              for (int i = 0; i < m_eventTable.events.Count; i++)
              {
                  if (m_eventTable.events[i].eventId == eventId)
                  {
                      targetIndex = i;
                      break;
                  }
              }

              if (targetIndex != -1)
              {
                  m_eventTable.events.RemoveAt(targetIndex);
                  if (m_selectedEventId == eventId)
                  {
                      m_selectedEventId = string.Empty;
                  }
                  if (OnEventListChanged != null)
                  {
                      OnEventListChanged.Invoke();
                  }
                  if (OnEventSelected != null)
                  {
                      OnEventSelected.Invoke(m_selectedEventId);
                  }
              }
          }

          /// <summary>
          /// [기능]: 현재 선택된 이벤트 DTO 필드들을 UI 입력 수정값으로 갱신하고 ID 변경 시 중복 검사를 수행합니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 정의
          /// </summary>
          public void UpdateSelectedEvent(EventDefinitionDTO updatedData)
          {
              if (updatedData == null)
              {
                  return;
              }

              var current = GetSelectedEvent();
              if (current != null)
              {
                  current.eventTitle = updatedData.eventTitle;
                  current.eventDescription = updatedData.eventDescription;
                  current.eventIconAddress = updatedData.eventIconAddress;
                  current.startDate = updatedData.startDate;
                  current.endDate = updatedData.endDate;
                  current.condition = updatedData.condition;
                  current.rewards = updatedData.rewards;

                  if (current.eventId != updatedData.eventId)
                  {
                      bool isDuplicate = false;
                      for (int i = 0; i < m_eventTable.events.Count; i++)
                      {
                          if (m_eventTable.events[i].eventId == updatedData.eventId && m_eventTable.events[i] != current)
                          {
                              isDuplicate = true;
                              break;
                          }
                      }

                      if (isDuplicate)
                      {
                          if (OnErrorOccurred != null)
                          {
                              OnErrorOccurred.Invoke("[EventAdminViewModel] 중복된 이벤트 ID가 존재합니다.");
                          }
                          return;
                      }
                      current.eventId = updatedData.eventId;
                      m_selectedEventId = updatedData.eventId;
                  }

                  if (OnEventListChanged != null)
                  {
                      OnEventListChanged.Invoke();
                  }
              }
          }

          /// <summary>
          /// [기능]: 뷰모델 내의 이벤트를 로컬 json 경로에 비동기로 직렬화 저장합니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 정의
          /// </summary>
          public async Awaitable<bool> SaveToLocalFileAsync(string customPath = null)
          {
              string path = customPath;
              if (string.IsNullOrEmpty(path))
              {
                  path = Path.Combine(Application.dataPath, "_Game/Data/event_table.json");
              }

              try
              {
                  string directory = Path.GetDirectoryName(path);
                  if (Directory.Exists(directory) == false)
                  {
                      Directory.CreateDirectory(directory);
                  }

                  string json = JsonUtility.ToJson(m_eventTable, true);
                  await Awaitable.BackgroundThreadAsync();
                  File.WriteAllText(path, json);
                  await Awaitable.MainThreadAsync();

                  Debug.Log($"[EventAdminViewModel] 로컬 파일 저장 완료: {path}");
                  if (OnSaveCompleted != null)
                  {
                      OnSaveCompleted.Invoke(true);
                  }
                  return true;
              }
              catch (Exception ex)
              {
                  Debug.LogError($"[EventAdminViewModel] 로컬 파일 저장 중 오류 발생: {ex.Message}");
                  if (OnSaveCompleted != null)
                  {
                      OnSaveCompleted.Invoke(false);
                  }
                  return false;
              }
          }

          /// <summary>
          /// [기능]: 기획 데이터 유효성(ID 및 Title 공란 검사)을 검증하고, Firebase 업로드 인터페이스를 비동기 호출합니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 정의
          /// </summary>
          public async Awaitable<bool> UploadToFirebaseAsync()
          {
              if (m_firebaseService == null)
              {
                  if (OnErrorOccurred != null)
                  {
                      OnErrorOccurred.Invoke("[EventAdminViewModel] Firebase 업로드 서비스가 연결되지 않았습니다.");
                  }
                  if (OnUploadCompleted != null)
                  {
                      OnUploadCompleted.Invoke(false);
                  }
                  return false;
              }

              for (int i = 0; i < m_eventTable.events.Count; i++)
              {
                  var ev = m_eventTable.events[i];
                  if (string.IsNullOrEmpty(ev.eventId))
                  {
                      if (OnErrorOccurred != null)
                      {
                          OnErrorOccurred.Invoke("[EventAdminViewModel] 이벤트 ID가 공란입니다.");
                      }
                      return false;
                  }
                  if (string.IsNullOrEmpty(ev.eventTitle))
                  {
                      if (OnErrorOccurred != null)
                      {
                          OnErrorOccurred.Invoke($"[EventAdminViewModel] 이벤트 ID ({ev.eventId})의 제목이 공란입니다.");
                      }
                      return false;
                  }
              }

              bool success = await m_firebaseService.UploadEventTableAsync(m_eventTable);
              if (OnUploadCompleted != null)
              {
                  OnUploadCompleted.Invoke(success);
              }
              return success;
          }
          #endregion
      }
  }
  ```

- [ ] **Step 2: 뷰모델 유닛 테스트 코드 작성**
  `EventSystemTests.cs`에 `EventAdminViewModel`의 CRUD, 로컬 파일 쓰기 및 업로드 검증 테스트 3종을 추가합니다.

  ```csharp
          /// <summary>
          /// [기능]: 11. 뷰모델 상의 이벤트 추가, 삭제, 목록 갱신 기능 검증
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 작성
          /// </summary>
          [Test]
          public void Test_11_EventAdminVM_CRUD()
          {
              var firebase = new MockFirebaseUploadService();
              var vm = new EventAdminViewModel(firebase);

              int eventListChangedCount = 0;
              vm.OnEventListChanged += () => { eventListChangedCount++; };

              vm.AddNewEvent();
              var added = vm.GetSelectedEvent();

              Assert.IsNotNull(added);
              Assert.AreEqual(1, vm.GetEvents().Count);

              added.eventTitle = "수정된 제목";
              vm.UpdateSelectedEvent(added);

              Assert.AreEqual("수정된 제목", vm.GetSelectedEvent().eventTitle);

              vm.RemoveEvent(added.eventId);
              Assert.AreEqual(0, vm.GetEvents().Count);
              Assert.GreaterOrEqual(eventListChangedCount, 2);
          }

          /// <summary>
          /// [기능]: 12. 뷰모델을 통해 지정한 JSON 경로에 정상 직렬화 및 로컬 저장 검증
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 작성
          /// </summary>
          [Test]
          public async Task Test_12_EventAdminVM_LocalSave()
          {
              var firebase = new MockFirebaseUploadService();
              var vm = new EventAdminViewModel(firebase);
              
              vm.AddNewEvent();
              var ev = vm.GetSelectedEvent();
              ev.eventId = "evt_save_test";
              ev.eventTitle = "로컬 저장 테스트";
              vm.UpdateSelectedEvent(ev);

              string testPath = Path.Combine(Application.temporaryCachePath, "test_event_table.json");
              bool result = await vm.SaveToLocalFileAsync(testPath);

              Assert.IsTrue(result);
              Assert.IsTrue(File.Exists(testPath));

              string content = File.ReadAllText(testPath);
              Assert.IsTrue(content.Contains("evt_save_test"));

              if (File.Exists(testPath))
              {
                  File.Delete(testPath);
              }
          }

          /// <summary>
          /// [기능]: 13. 이벤트 ID 공란이나 필수란 누락 시 유효성 경고 처리 및 검증
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 작성
          /// </summary>
          [Test]
          public async Task Test_13_EventAdminVM_Validation()
          {
              var firebase = new MockFirebaseUploadService();
              var vm = new EventAdminViewModel(firebase);
              
              vm.AddNewEvent();
              var ev = vm.GetSelectedEvent();
              ev.eventTitle = ""; // 제목 비우기
              vm.UpdateSelectedEvent(ev);

              bool result = await vm.UploadToFirebaseAsync();

              Assert.IsFalse(result); // 업로드 실패해야 함 (유효성 검사 미통과)
          }
  ```

- [ ] **Step 3: 에디터 테스트 실행 및 통과 검증**
  유니티 테스트 러너에서 `Test_11`, `Test_12`, `Test_13`을 돌려 오류 없이 모두 PASS 상태인지 검증합니다.

- [ ] **Step 4: Commit**
  ```bash
  git add Assets/_Game/Scripts/EventSystem/ViewModels/EventAdminViewModel.cs Assets/_Game/Tests/Editor/EventSystemTests.cs
  git commit -m "feat: 이벤트 관리자 뷰모델 구현 및 유닛 테스트 3종 추가"
  ```

---

### Task 3: 이벤트 관리자 UI 구성 및 뷰 (`EventAdminView`) 구현

**Files:**
- Create: `Assets/_Game/Scripts/EventSystem/Views/EventAdminRewardRowView.cs`
- Create: `Assets/_Game/Scripts/EventSystem/Views/EventAdminView.cs`

- [ ] **Step 1: 보상 동적 입력 행(View) 구현**
  `EventAdminRewardRowView.cs`을 생성하고, 개별 보상 설정용 UI 바인딩 코드를 작성합니다. 모든 if문에 중괄호를 부여하고 상세 XML 주석을 표기합니다.

  ```csharp
  using UnityEngine;
  using UnityEngine.UI;
  using TMPro;
  using BePex.EventSystem.DTOs;
  using System;

  namespace BePex.EventSystem.Views
  {
      /// <summary>
      /// [기능]: 이벤트 관리자 화면에서 보상 항목 리스트의 개별 데이터 입력 행을 표현 및 제어하는 View 클래스.
      /// [작성자]: 윤승종
      /// </summary>
      public class EventAdminRewardRowView : MonoBehaviour
      {
          #region UI 참조 (Inspector)
          [SerializeField] private TMP_Dropdown m_typeDropdown;
          [SerializeField] private TMP_InputField m_amountInput;
          [SerializeField] private TMP_InputField m_nameInput;
          [SerializeField] private TMP_InputField m_iconInput;
          [SerializeField] private Button m_removeButton;
          #endregion

          #region 내부 필드
          private RewardDefinitionDTO m_dto;
          private Action<EventAdminRewardRowView> m_onRemove;
          #endregion

          #region 공개 메서드
          /// <summary>
          /// [기능]: 특정 보상 데이터 구조체와 제거 리스너를 수동 바인딩하고 UI 값을 채웁니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 정의
          /// </summary>
          public void Bind(RewardDefinitionDTO dto, Action<EventAdminRewardRowView> onRemove)
          {
              m_dto = dto;
              m_onRemove = onRemove;

              if (m_typeDropdown != null)
              {
                  m_typeDropdown.onValueChanged.RemoveAllListeners();
                  int optionIndex = GetDropdownOptionIndex(m_dto.rewardType);
                  m_typeDropdown.value = optionIndex;
                  m_typeDropdown.onValueChanged.AddListener(func_OnTypeChanged);
              }

              if (m_amountInput != null)
              {
                  m_amountInput.onValueChanged.RemoveAllListeners();
                  m_amountInput.text = m_dto.amount.ToString();
                  m_amountInput.onValueChanged.AddListener(func_OnAmountChanged);
              }

              if (m_nameInput != null)
              {
                  m_nameInput.onValueChanged.RemoveAllListeners();
                  m_nameInput.text = m_dto.displayName;
                  m_nameInput.onValueChanged.AddListener(func_OnNameChanged);
              }

              if (m_iconInput != null)
              {
                  m_iconInput.onValueChanged.RemoveAllListeners();
                  m_iconInput.text = m_dto.iconAddress;
                  m_iconInput.onValueChanged.AddListener(func_OnIconChanged);
              }

              if (m_removeButton != null)
              {
                  m_removeButton.onClick.RemoveAllListeners();
                  m_removeButton.onClick.AddListener(func_OnRemoveClick);
              }
          }

          /// <summary>
          /// [기능]: 이 뷰가 바인딩된 보상 DTO 객체를 반환합니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 정의
          /// </summary>
          public RewardDefinitionDTO GetDTO()
          {
              return m_dto;
          }
          #endregion

          #region UI 이벤트 핸들러
          /// <summary>
          /// [기능]: 드롭다운의 보상 타입 수정 변경 발생 시 DTO에 인가하는 UI 이벤트 콜백.
          /// [작성자]: 윤승종
          /// </summary>
          private void func_OnTypeChanged(int index)
          {
              if (m_dto != null && m_typeDropdown != null)
              {
                  m_dto.rewardType = m_typeDropdown.options[index].text;
              }
          }

          /// <summary>
          /// [기능]: 수량 InputField 변경 발생 시 DTO에 인가하는 UI 이벤트 콜백.
          /// [작성자]: 윤승종
          /// </summary>
          private void func_OnAmountChanged(string val)
          {
              if (m_dto != null)
              {
                  int.TryParse(val, out int result);
                  m_dto.amount = result;
              }
          }

          /// <summary>
          /// [기능]: 노출 이름 InputField 변경 발생 시 DTO에 인가하는 UI 이벤트 콜백.
          /// [작성자]: 윤승종
          /// </summary>
          private void func_OnNameChanged(string val)
          {
              if (m_dto != null)
              {
                  m_dto.displayName = val;
              }
          }

          /// <summary>
          /// [기능]: 아이콘 어드레서블 주소 InputField 변경 발생 시 DTO에 인가하는 UI 이벤트 콜백.
          /// [작성자]: 윤승종
          /// </summary>
          private void func_OnIconChanged(string val)
          {
              if (m_dto != null)
              {
                  m_dto.iconAddress = val;
              }
          }

          /// <summary>
          /// [기능]: 삭제 버튼 클릭 감지 시 제거 리스너를 호출하는 UI 이벤트 콜백.
          /// [작성자]: 윤승종
          /// </summary>
          private void func_OnRemoveClick()
          {
              if (m_onRemove != null)
              {
                  m_onRemove.Invoke(this);
              }
          }
          #endregion

          #region 헬퍼 메서드
          /// <summary>
          /// [기능]: 보상 타입 스트링과 일치하는 드롭다운의 인덱스를 선별합니다.
          /// [작성자]: 윤승종
          /// </summary>
          private int GetDropdownOptionIndex(string rewardType)
          {
              if (m_typeDropdown == null)
              {
                  return 0;
              }
              for (int i = 0; i < m_typeDropdown.options.Count; i++)
              {
                  if (m_typeDropdown.options[i].text.Equals(rewardType, StringComparison.OrdinalIgnoreCase))
                  {
                      return i;
                  }
              }
              return 0;
          }
          #endregion
      }
  }
  ```

- [ ] **Step 2: 메인 관리자 뷰(View) 구현**
  `EventAdminView.cs`을 생성하여, 전체 목록 갱신 및 선택 처리, 필드 폼 데이터의 ViewModel 동기화 코드를 작성합니다. `if`문 단일 행 중괄호를 철저하게 보강하였습니다.

  ```csharp
  using UnityEngine;
  using UnityEngine.UI;
  using TMPro;
  using System.Collections.Generic;
  using BePex.EventSystem.ViewModels;
  using BePex.EventSystem.DTOs;
  using System;

  namespace BePex.EventSystem.Views
  {
      /// <summary>
      /// [기능]: 이벤트 관리자 UI 씬의 모든 입력/갱신 시각 인터렉션을 제어하고 ViewModel을 중개하는 View 클래스.
      /// [작성자]: 윤승종
      /// </summary>
      public class EventAdminView : MonoBehaviour
      {
          #region UI 참조 (Inspector) - 좌측 패널
          [Header("좌측 패널")]
          [SerializeField] private RectTransform m_eventListContent;
          [SerializeField] private GameObject m_eventItemPrefab;
          [SerializeField] private Button m_addEventButton;
          #endregion

          #region UI 참조 (Inspector) - 우측 패널
          [Header("우측 패널 (상세 편집)")]
          [SerializeField] private GameObject m_detailPanel;
          [SerializeField] private TMP_InputField m_eventIdInput;
          [SerializeField] private TMP_InputField m_titleInput;
          [SerializeField] private TMP_InputField m_descInput;
          [SerializeField] private TMP_InputField m_iconAddressInput;
          [SerializeField] private TMP_InputField m_startDateInput;
          [SerializeField] private TMP_InputField m_endDateInput;

          [Header("조건 편집")]
          [SerializeField] private TMP_Dropdown m_condTypeDropdown;
          [SerializeField] private TMP_InputField m_condTargetInput;

          [Header("보상 편집")]
          [SerializeField] private RectTransform m_rewardListContent;
          [SerializeField] private GameObject m_rewardRowPrefab;
          [SerializeField] private Button m_addRewardButton;
          #endregion

          #region UI 참조 (Inspector) - 제어 바 & 상태 메시지
          [Header("하단 제어 바")]
          [SerializeField] private Button m_saveLocalButton;
          [SerializeField] private Button m_uploadFirebaseButton;
          [SerializeField] private Button m_removeEventButton;
          [SerializeField] private TextMeshProUGUI m_statusText;
          #endregion

          #region 내부 필드
          private EventAdminViewModel m_viewModel;
          private readonly List<GameObject> m_spawnedItems = new List<GameObject>();
          private readonly List<EventAdminRewardRowView> m_spawnedRewardRows = new List<EventAdminRewardRowView>();
          #endregion

          #region 초기화 및 바인딩
          /// <summary>
          /// [기능]: 뷰모델 의존성을 바인딩하고 이벤트를 구독합니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 정의
          /// </summary>
          public void Bind(EventAdminViewModel viewModel)
          {
              m_viewModel = viewModel;

              if (m_viewModel != null)
              {
                  m_viewModel.OnEventListChanged += func_OnEventListChanged;
                  m_viewModel.OnEventSelected += func_OnEventSelected;
                  m_viewModel.OnErrorOccurred += func_OnErrorOccurred;
                  m_viewModel.OnSaveCompleted += func_OnSaveCompleted;
                  m_viewModel.OnUploadCompleted += func_OnUploadCompleted;
              }

              if (m_addEventButton != null)
              {
                  m_addEventButton.onClick.RemoveAllListeners();
                  m_addEventButton.onClick.AddListener(func_OnAddEventClick);
              }

              if (m_removeEventButton != null)
              {
                  m_removeEventButton.onClick.RemoveAllListeners();
                  m_removeEventButton.onClick.AddListener(func_OnRemoveEventClick);
              }

              if (m_addRewardButton != null)
              {
                  m_addRewardButton.onClick.RemoveAllListeners();
                  m_addRewardButton.onClick.AddListener(func_OnAddRewardClick);
              }

              if (m_saveLocalButton != null)
              {
                  m_saveLocalButton.onClick.RemoveAllListeners();
                  m_saveLocalButton.onClick.AddListener(func_OnSaveLocalClick);
              }

              if (m_uploadFirebaseButton != null)
              {
                  m_uploadFirebaseButton.onClick.RemoveAllListeners();
                  m_uploadFirebaseButton.onClick.AddListener(func_OnUploadFirebaseClick);
              }

              func_RegisterFormListeners();
              func_OnEventListChanged();
              func_OnEventSelected(string.Empty);
          }
          #endregion

          #region 유니티 생명주기
          /// <summary>
          /// [기능]: 뷰 디바인딩 해제 및 뷰모델 이벤트 연결을 영구 취소하여 메모리 누수를 원천 방지합니다.
          /// [작성자]: 윤승종
          /// </summary>
          private void OnDestroy()
          {
              if (m_viewModel != null)
              {
                  m_viewModel.OnEventListChanged -= func_OnEventListChanged;
                  m_viewModel.OnEventSelected -= func_OnEventSelected;
                  m_viewModel.OnErrorOccurred -= func_OnErrorOccurred;
                  m_viewModel.OnSaveCompleted -= func_OnSaveCompleted;
                  m_viewModel.OnUploadCompleted -= func_OnUploadCompleted;
              }
          }
          #endregion

          #region 이벤트 구독 반응 UI 갱신
          /// <summary>
          /// [기능]: 뷰모델 내의 전체 이벤트 리스트가 변경되었을 때 좌측 Scroll View 항목을 갱신 렌더링합니다.
          /// [작성자]: 윤승종
          /// </summary>
          private void func_OnEventListChanged()
          {
              for (int i = 0; i < m_spawnedItems.Count; i++)
              {
                  if (m_spawnedItems[i] != null)
                  {
                      Destroy(m_spawnedItems[i]);
                  }
              }
              m_spawnedItems.Clear();

              if (m_viewModel == null || m_eventListContent == null || m_eventItemPrefab == null)
              {
                  return;
              }

              var events = m_viewModel.GetEvents();
              for (int i = 0; i < events.Count; i++)
              {
                  var ev = events[i];
                  GameObject go = Instantiate(m_eventItemPrefab, m_eventListContent);
                  m_spawnedItems.Add(go);

                  var text = go.GetComponentInChildren<TextMeshProUGUI>();
                  if (text != null)
                  {
                      text.text = string.IsNullOrEmpty(ev.eventTitle) ? "(제목 없음)" : ev.eventTitle;
                  }

                  var button = go.GetComponent<Button>();
                  if (button != null)
                  {
                      button.onClick.AddListener(() => { func_OnSelectItem(ev.eventId); });
                  }
              }
          }

          /// <summary>
          /// [기능]: 목록 아이템 선택 클릭을 뷰모델에 전달합니다.
          /// [작성자]: 윤승종
          /// </summary>
          private void func_OnSelectItem(string eventId)
          {
              if (m_viewModel != null)
              {
                  m_viewModel.SelectEvent(eventId);
              }
          }

          /// <summary>
          /// [기능]: 뷰모델에서 특정 이벤트의 활성화 변경 신호를 보냈을 때 우측 상세 폼 데이터를 채우고 활성화합니다.
          /// [작성자]: 윤승종
          /// </summary>
          private void func_OnEventSelected(string eventId)
          {
              if (m_viewModel == null)
              {
                  return;
              }

              var selected = m_viewModel.GetSelectedEvent();
              if (selected == null)
              {
                  if (m_detailPanel != null)
                  {
                      m_detailPanel.SetActive(false);
                  }
                  return;
              }

              if (m_detailPanel != null)
              {
                  m_detailPanel.SetActive(true);
              }

              func_UnregisterFormListeners();

              if (m_eventIdInput != null)
              {
                  m_eventIdInput.text = selected.eventId;
              }
              if (m_titleInput != null)
              {
                  m_titleInput.text = selected.eventTitle;
              }
              if (m_descInput != null)
              {
                  m_descInput.text = selected.eventDescription;
              }
              if (m_iconAddressInput != null)
              {
                  m_iconAddressInput.text = selected.eventIconAddress;
              }
              if (m_startDateInput != null)
              {
                  m_startDateInput.text = selected.startDate;
              }
              if (m_endDateInput != null)
              {
                  m_endDateInput.text = selected.endDate;
              }

              if (m_condTypeDropdown != null)
              {
                  int condIdx = 0;
                  for (int i = 0; i < m_condTypeDropdown.options.Count; i++)
                  {
                      if (m_condTypeDropdown.options[i].text.Equals(selected.condition.conditionType, StringComparison.OrdinalIgnoreCase))
                      {
                          condIdx = i;
                          break;
                      }
                  }
                  m_condTypeDropdown.value = condIdx;
              }

              if (m_condTargetInput != null)
              {
                  m_condTargetInput.text = selected.condition.targetValue.ToString();
              }

              func_RegisterFormListeners();
              func_RenderRewards(selected.rewards);
          }

          /// <summary>
          /// [기능]: 보상 목록을 뷰 하이어라키 내에 동적으로 인스턴싱하고 바인딩합니다.
          /// [작성자]: 윤승종
          /// </summary>
          private void func_RenderRewards(List<RewardDefinitionDTO> rewards)
          {
              for (int i = 0; i < m_spawnedRewardRows.Count; i++)
              {
                  if (m_spawnedRewardRows[i] != null)
                  {
                      Destroy(m_spawnedRewardRows[i].gameObject);
                  }
              }
              m_spawnedRewardRows.Clear();

              if (m_rewardListContent == null || m_rewardRowPrefab == null)
              {
                  return;
              }

              for (int i = 0; i < rewards.Count; i++)
              {
                  var rew = rewards[i];
                  GameObject go = Instantiate(m_rewardRowPrefab, m_rewardListContent);
                  var rowView = go.GetComponent<EventAdminRewardRowView>();
                  if (rowView != null)
                  {
                      rowView.Bind(rew, func_OnRemoveRewardRow);
                      m_spawnedRewardRows.Add(rowView);
                  }
              }
          }

          /// <summary>
          /// [기능]: 개별 보상 줄에서 삭제 요청이 넘어왔을 때 이를 소거하고 보상 리스트를 재생성합니다.
          /// [작성자]: 윤승종
          /// </summary>
          private void func_OnRemoveRewardRow(EventAdminRewardRowView row)
          {
              if (m_viewModel == null || row == null)
              {
                  return;
              }
              var selected = m_viewModel.GetSelectedEvent();
              if (selected != null)
              {
                  selected.rewards.Remove(row.GetDTO());
                  func_RenderRewards(selected.rewards);
              }
          }

          /// <summary>
          /// [기능]: 경고 발생 시 텍스트 컴포넌트의 컬러를 빨간색으로 변경하고 경고 내용을 갱신합니다.
          /// [작성자]: 윤승종
          /// </summary>
          private void func_OnErrorOccurred(string error)
          {
              if (m_statusText != null)
              {
                  m_statusText.color = Color.red;
                  m_statusText.text = error;
              }
          }

          /// <summary>
          /// [기능]: 로컬 세이브 파일 출력 완료 결과를 하단 상태 창에 출력합니다.
          /// [작성자]: 윤승종
          /// </summary>
          private void func_OnSaveCompleted(bool success)
          {
              if (m_statusText != null)
              {
                  m_statusText.color = success ? Color.green : Color.red;
                  m_statusText.text = success ? "[EventAdminView] 로컬 저장에 성공하였습니다." : "[EventAdminView] 로컬 저장에 실패하였습니다.";
              }
          }

          /// <summary>
          /// [기능]: Firebase 비동기 업로드 완료 결과를 하단 상태 창에 출력합니다.
          /// [작성자]: 윤승종
          /// </summary>
          private void func_OnUploadCompleted(bool success)
          {
              if (m_statusText != null)
              {
                  m_statusText.color = success ? Color.green : Color.red;
                  m_statusText.text = success ? "[EventAdminView] Firebase 서버 업로드에 성공하였습니다." : "[EventAdminView] Firebase 서버 업로드에 실패하였습니다.";
              }
          }
          #endregion

          #region 사용자 입력 버튼 콜백
          /// <summary>
          /// [기능]: 신규 이벤트 생성 버튼 클릭 핸들러.
          /// [작성자]: 윤승종
          /// </summary>
          private void func_OnAddEventClick()
          {
              if (m_viewModel != null)
              {
                  m_viewModel.AddNewEvent();
              }
          }

          /// <summary>
          /// [기능]: 이벤트 삭제 버튼 클릭 핸들러.
          /// [작성자]: 윤승종
          /// </summary>
          private void func_OnRemoveEventClick()
          {
              if (m_viewModel != null)
              {
                  var selected = m_viewModel.GetSelectedEvent();
                  if (selected != null)
                  {
                      m_viewModel.RemoveEvent(selected.eventId);
                  }
              }
          }

          /// <summary>
          /// [기능]: 보상 추가 버튼 클릭 핸들러.
          /// [작성자]: 윤승종
          /// </summary>
          private void func_OnAddRewardClick()
          {
              if (m_viewModel == null)
              {
                  return;
              }
              var selected = m_viewModel.GetSelectedEvent();
              if (selected != null)
              {
                  var newRew = new RewardDefinitionDTO
                  {
                      rewardType = "Exp",
                      amount = 10,
                      displayName = "경험치",
                      iconAddress = "UI/Icons/Default"
                  };
                  selected.rewards.Add(newRew);
                  func_RenderRewards(selected.rewards);
              }
          }

          /// <summary>
          /// [기능]: 로컬 파일 저장 버튼 클릭 비동기 핸들러.
          /// [작성자]: 윤승종
          /// </summary>
          private async void func_OnSaveLocalClick()
          {
              if (m_viewModel != null)
              {
                  func_SubmitFormChanges();
                  if (m_statusText != null)
                  {
                      m_statusText.text = "저장 중...";
                  }
                  await m_viewModel.SaveToLocalFileAsync();
              }
          }

          /// <summary>
          /// [기능]: Firebase 서버 배포 버튼 클릭 비동기 핸들러.
          /// [작성자]: 윤승종
          /// </summary>
          private async void func_OnUploadFirebaseClick()
          {
              if (m_viewModel != null)
              {
                  func_SubmitFormChanges();
                  if (m_statusText != null)
                  {
                      m_statusText.text = "업로드 중...";
                  }
                  await m_viewModel.UploadToFirebaseAsync();
              }
          }
          #endregion

          #region 폼 입력 실시간 동기화
          /// <summary>
          /// [기능]: 우측 폼 입력 컴포넌트들의 데이터 변경 리스너를 일제히 등록합니다. 단일 행 중괄호 규칙 준수.
          /// [작성자]: 윤승종
          /// </summary>
          private void func_RegisterFormListeners()
          {
              if (m_eventIdInput != null)
              {
                  m_eventIdInput.onEndEdit.AddListener(func_OnFormInputEndEdit);
              }
              if (m_titleInput != null)
              {
                  m_titleInput.onEndEdit.AddListener(func_OnFormInputEndEdit);
              }
              if (m_descInput != null)
              {
                  m_descInput.onEndEdit.AddListener(func_OnFormInputEndEdit);
              }
              if (m_iconAddressInput != null)
              {
                  m_iconAddressInput.onEndEdit.AddListener(func_OnFormInputEndEdit);
              }
              if (m_startDateInput != null)
              {
                  m_startDateInput.onEndEdit.AddListener(func_OnFormInputEndEdit);
              }
              if (m_endDateInput != null)
              {
                  m_endDateInput.onEndEdit.AddListener(func_OnFormInputEndEdit);
              }
              if (m_condTypeDropdown != null)
              {
                  m_condTypeDropdown.onValueChanged.AddListener(func_OnDropdownValueChanged);
              }
              if (m_condTargetInput != null)
              {
                  m_condTargetInput.onEndEdit.AddListener(func_OnFormInputEndEdit);
              }
          }

          /// <summary>
          /// [기능]: 리스너 등록을 해제하여 중복 감지를 차단합니다.
          /// [작성자]: 윤승종
          /// </summary>
          private void func_UnregisterFormListeners()
          {
              if (m_eventIdInput != null)
              {
                  m_eventIdInput.onEndEdit.RemoveAllListeners();
              }
              if (m_titleInput != null)
              {
                  m_titleInput.onEndEdit.RemoveAllListeners();
              }
              if (m_descInput != null)
              {
                  m_descInput.onEndEdit.RemoveAllListeners();
              }
              if (m_iconAddressInput != null)
              {
                  m_iconAddressInput.onEndEdit.RemoveAllListeners();
              }
              if (m_startDateInput != null)
              {
                  m_startDateInput.onEndEdit.RemoveAllListeners();
              }
              if (m_endDateInput != null)
              {
                  m_endDateInput.onEndEdit.RemoveAllListeners();
              }
              if (m_condTypeDropdown != null)
              {
                  m_condTypeDropdown.onValueChanged.RemoveAllListeners();
              }
              if (m_condTargetInput != null)
              {
                  m_condTargetInput.onEndEdit.RemoveAllListeners();
              }
          }

          /// <summary>
          /// [기능]: 인풋 필드 폼 수정 완료 시 바인딩.
          /// [작성자]: 윤승종
          /// </summary>
          private void func_OnFormInputEndEdit(string val)
          {
              func_SubmitFormChanges();
          }

          /// <summary>
          /// [기능]: 드롭다운 설정 변경 시 바인딩.
          /// [작성자]: 윤승종
          /// </summary>
          private void func_OnDropdownValueChanged(int index)
          {
              func_SubmitFormChanges();
          }

          /// <summary>
          /// [기능]: UI 상의 모든 편집 텍스트와 드롭다운 값을 수집하여 뷰모델 갱신 명령을 인가합니다.
          /// [작성자]: 윤승종
          /// </summary>
          private void func_SubmitFormChanges()
          {
              if (m_viewModel == null)
              {
                  return;
              }
              var selected = m_viewModel.GetSelectedEvent();
              if (selected == null)
              {
                  return;
              }

              var updated = new EventDefinitionDTO
              {
                  eventId = m_eventIdInput != null ? m_eventIdInput.text : selected.eventId,
                  eventTitle = m_titleInput != null ? m_titleInput.text : selected.eventTitle,
                  eventDescription = m_descInput != null ? m_descInput.text : selected.eventDescription,
                  eventIconAddress = m_iconAddressInput != null ? m_iconAddressInput.text : selected.eventIconAddress,
                  startDate = m_startDateInput != null ? m_startDateInput.text : selected.startDate,
                  endDate = m_endDateInput != null ? m_endDateInput.text : selected.endDate,
                  condition = new ConditionDefinitionDTO
                  {
                      conditionType = m_condTypeDropdown != null ? m_condTypeDropdown.options[m_condTypeDropdown.value].text : selected.condition.conditionType,
                      targetValue = m_condTargetInput != null ? (int.TryParse(m_condTargetInput.text, out int target) ? target : 0) : selected.condition.targetValue
                  },
                  rewards = selected.rewards
              };

              m_viewModel.UpdateSelectedEvent(updated);
          }
          #endregion
      }
  }
  ```

- [.] **Step 3: Compile 확인**
  컴파일 오류가 발생하지 않는지 프로젝트를 빌드/정적 컴파일하여 확인합니다.

- [ ] **Step 4: Commit**
  ```bash
  git add Assets/_Game/Scripts/EventSystem/Views/EventAdminRewardRowView.cs Assets/_Game/Scripts/EventSystem/Views/EventAdminView.cs
  git commit -m "feat: EventAdminView 및 EventAdminRewardRowView UI 바인딩 클래스 추가"
  ```

---

### Task 4: 이벤트 관리자 씬 구성 및 DI 바인딩

**Files:**
- Create: `Assets/_Game/Scripts/EventSystem/Infrastructure/EventAdminSceneInitializer.cs`

- [ ] **Step 1: 컴포지션 루트Initializer 구현**
  `EventAdminSceneInitializer.cs` 파일을 생성하고, 수동 DI 의존성 해소 및 씬 진입 시 데이터 로딩 코드를 작성합니다.

  ```csharp
  using UnityEngine;
  using BePex.EventSystem.Views;
  using BePex.EventSystem.ViewModels;
  using BePex.EventSystem.Infrastructure;
  using BePex.EventSystem.DTOs;
  using UnityEngine.AddressableAssets;
  using System.IO;

  namespace BePex.EventSystem.Infrastructure
  {
      /// <summary>
      /// [기능]: 이벤트 관리자 씬의 뷰모델, 뷰 및 모의 업로드 서비스 의존성을 수동 조립하고 바인딩하는 Composition Root 클래스.
      /// [작성자]: 윤승종
      /// </summary>
      public class EventAdminSceneInitializer : MonoBehaviour
      {
          #region UI 참조 (Inspector)
          [SerializeField] private EventAdminView m_adminView;
          #endregion

          #region 데이터 참조 (Inspector)
          [SerializeField] private string m_eventJsonAddress = "EventTableJson";
          #endregion

          #region 유니티 생명주기
          /// <summary>
          /// [기능]: 씬 기동 시 수동 비동기 초기화를 트리거합니다.
          /// [작성자]: 윤승종
          /// </summary>
          private async void Start()
          {
              await InitializeAsync();
          }
          #endregion

          #region 초기화
          /// <summary>
          /// [기능]: 데이터 파일(로컬 JSON 파일 또는 Addressables)을 로드하고 뷰와 뷰모델, 모킹 서비스를 인스턴싱 및 수동 DI 결합합니다.
          /// [작성자]: 윤승종
          /// [수정 날짜]: 2026-06-14
          /// [마지막 수정 작성자]: 윤승종
          /// [수정 내용]: 최초 정의
          /// </summary>
          private async Awaitable InitializeAsync()
          {
              EventTableDTO eventTableDTO = null;

              string localPath = Path.Combine(Application.dataPath, "_Game/Data/event_table.json");
              if (File.Exists(localPath))
              {
                  try
                  {
                      string json = File.ReadAllText(localPath);
                      eventTableDTO = JsonUtility.FromJson<EventTableDTO>(json);
                  }
                  catch (System.Exception ex)
                  {
                      Debug.LogWarning($"[EventAdminSceneInitializer] 로컬 파일 로드 실패, 어드레서블 시도: {ex.Message}");
                  }
              }

              if (eventTableDTO == null && !string.IsNullOrEmpty(m_eventJsonAddress))
              {
                  try
                  {
                      var handle = Addressables.LoadAssetAsync<TextAsset>(m_eventJsonAddress);
                      TextAsset jsonAsset = await handle.Task;

                      if (jsonAsset != null)
                      {
                          eventTableDTO = JsonUtility.FromJson<EventTableDTO>(jsonAsset.text);
                      }
                  }
                  catch (System.Exception ex)
                  {
                      Debug.LogError($"[EventAdminSceneInitializer] 어드레서블 로드 오류: {ex.Message}");
                  }
              }

              if (eventTableDTO == null)
              {
                  eventTableDTO = new EventTableDTO();
                  Debug.LogWarning("[EventAdminSceneInitializer] 신규 빈 이벤트 테이블 DTO 생성.");
              }

              var firebaseService = new MockFirebaseUploadService();
              var adminVM = new EventAdminViewModel(firebaseService);
              adminVM.SetEventTable(eventTableDTO);

              if (m_adminView != null)
              {
                  m_adminView.Bind(adminVM);
              }
              else
              {
                  Debug.LogError("[EventAdminSceneInitializer] EventAdminView 참조가 하이어라키에서 할당되지 않았습니다.");
              }

              Debug.Log("[EventAdminSceneInitializer] 이벤트 관리자 씬 의존성 주입 및 수동 DI 조립 완료.");
          }
          #endregion
      }
  }
  ```

- [ ] **Step 2: 유니티 에디터 내 관리자 씬 설정 및 프리팹 조립**
  - 유니티에서 `Assets/_Game/Scenes/EventAdminScene.unity` 씬을 신규 생성합니다. (수동 작업 유도)
  - 씬 내에 UI Canvas를 얹고, 좌측 Scroll View(이벤트 목록), 우측 Vertical Layout(기본 입력란, 조건 Dropdown, 보상 목록 Scroll View), 하단 제어 버튼 3종을 배치합니다.
  - 빈 GameObject를 생성하여 `EventAdminSceneInitializer` 컴포넌트를 붙이고, UI Canvas 내의 `EventAdminView` 컴포넌트를 SerializedField 참조로 연결합니다.
  - 보상 항목을 채워줄 동적 프리팹 `EventAdminRewardRow` 및 이벤트 목록 버튼 프리팹 `EventItem`을 구성하여 각각 View의 인스펙터 슬롯에 끌어다 할당합니다.

- [ ] **Step 3: Commit**
  ```bash
  git add Assets/_Game/Scripts/EventSystem/Infrastructure/EventAdminSceneInitializer.cs
  git commit -m "feat: EventAdminSceneInitializer 컴포지션 루트 클래스 구현 완료"
  ```

---

### Task 5: 수동 검증 및 프로젝트 문서 갱신

**Files:**
- Modify: `README.md`

- [ ] **Step 1: 로컬 파일 저장 및 Firebase 배포 수동 기능 확인**
  - 유니티 에디터에서 `EventAdminScene.unity`를 기동합니다.
  - `+ 신규 이벤트`를 누르고 정보를 수정한 뒤 `[로컬 파일 저장]`을 실행해 `Assets/_Game/Data/event_table.json` 파일이 알맞게 생성되는지 기획자가 작성할 구조와 대조합니다.
  - `[Firebase 서버 배포]`를 실행하여 콘솔 창에 1.5초 후 Mock 가상 업로드 완료 로그가 출력되는지 점검합니다.
  - 인게임 씬(`EventScene`)으로 넘어가서 어드민 씬에서 내보낸 JSON 데이터가 오류 없이 로드되어 동작하는지 최종 수동 점검을 완료합니다.

- [ ] **Step 2: README.md 내용 갱신**
  `README.md` 파일에 유니티 이벤트 관리자 씬(EventAdminScene) 설명 및 IFirebaseUploadService/MockFirebaseUploadService 확장 구조에 관한 한글 개발 문서 설명을 최신화합니다. (`readme_rules.md` 규칙 엄격 준수)

- [ ] **Step 3: Commit**
  ```bash
  git add README.md
  git commit -m "docs: 유니티 이벤트 관리자 씬 및 Firebase 연동 아키텍처 설명 README 추가"
  ```
