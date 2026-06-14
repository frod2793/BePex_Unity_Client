using NUnit.Framework;
using System.Threading.Tasks;
using BePex.EventSystem.DTOs;
using BePex.EventSystem.Models;
using BePex.EventSystem.Factories;
using BePex.EventSystem.ViewModels;
using BePex.EventSystem.Infrastructure;
using UnityEngine;

namespace BePex.EventSystem.Tests
{
    /// <summary>
    /// [기능]: 유니티 테스트 러너 9대 세부 조건에 대해 입력값과 출력값을 한글 로그로 상세 표시하고 검증하는 EditMode 유닛 테스트 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    public class EventSystemTests
    {
        #region 테스트 대상 및 팩토리 필드
        private JsonSaveSystem m_saveSystem;
        private ConditionFactory m_condFactory;
        private RewardFactory m_rewFactory;
        #endregion

        #region 초기화 및 셋업
        /// <summary>
        /// [기능]: 각 테스트 케이스 실행 전에 세이브 파일 시스템과 DTO 기반 팩토리들을 매번 새로 셋업합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        [SetUp]
        public void Setup()
        {
            m_saveSystem = new JsonSaveSystem();
            m_condFactory = new ConditionFactory(m_saveSystem);
            m_rewFactory = new RewardFactory();
        }
        #endregion

        #region 유닛 테스트 메서드
        /// <summary>
        /// [기능]: 1. 이벤트 목록 표시 검증
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 완료 로그 입력값/출력값 한글 표기 반영
        /// </summary>
        [Test]
        public void Test_01_EventList_Display()
        {
            var tableDTO = new EventTableDTO();
            tableDTO.events.Add(new EventDefinitionDTO { eventId = "evt_01", eventTitle = "테스트 1" });
            tableDTO.events.Add(new EventDefinitionDTO { eventId = "evt_02", eventTitle = "테스트 2" });

            var model = new EventModel(tableDTO, m_condFactory, m_rewFactory);
            var listVM = new EventListViewModel(model);

            int count = listVM.GetEvents().Count;
            string title1 = listVM.GetEvents()[0].eventTitle;
            string title2 = listVM.GetEvents()[1].eventTitle;

            Debug.Log($"[EventSystemTests] [Test_01_EventList_Display] " +
                      $"입력값: 이벤트 2개 등록 (evt_01: 테스트 1, evt_02: 테스트 2) | " +
                      $"출력값: 수집된 이벤트 갯수={count}, 첫 번째 제목={title1}, 두 번째 제목={title2}");

            Assert.AreEqual(2, count);
            Assert.AreEqual("테스트 1", title1);
            Assert.AreEqual("테스트 2", title2);
        }

        /// <summary>
        /// [기능]: 2. 이벤트 진행도 표시 검증
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 완료 로그 입력값/출력값 한글 표기 반영
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

            Debug.Log($"[EventSystemTests] [Test_02_EventProgress_Display] " +
                      $"입력값: 목표치=10, 가산진행도=4 | " +
                      $"출력값: 현재 진행도={cur}, 목표치={tar}, 진행 비율={ratio}");

            Assert.AreEqual(4, cur);
            Assert.AreEqual(10, tar);
            Assert.AreEqual(0.4f, ratio);
        }

        /// <summary>
        /// [기능]: 3. 이벤트 완료 처리 검증
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 완료 로그 입력값/출력값 한글 표기 반영
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
            bool isCompleted = await condition.IsCompletedAsync();

            Debug.Log($"[EventSystemTests] [Test_03_EventCompletion_Processing] " +
                      $"입력값: 목표치=10, 가산진행도=10 | " +
                      $"출력값: 완료 여부={isCompleted}");

            Assert.IsTrue(isCompleted);
        }

        /// <summary>
        /// [기능]: 4. 보상 수령 처리 검증
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 완료 로그 입력값/출력값 한글 표기 반영
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

            Debug.Log($"[EventSystemTests] [Test_04_Reward_Claim_Processing] " +
                      $"입력값: 경험치 100 지급이벤트 완료 상태 수령 시도 | " +
                      $"출력값: 보상 수령 성공여부={success}, 플레이어 총 획득 경험치={playerReward.totalExp}");

            Assert.IsTrue(success);
            Assert.AreEqual(100, playerReward.totalExp);
        }

        /// <summary>
        /// [기능]: 5. 이벤트 포인트 획득 검증
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 완료 로그 입력값/출력값 한글 표기 반영
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

            Debug.Log($"[EventSystemTests] [Test_05_EventPoint_Acquisition] " +
                      $"입력값: 포인트 50 지급이벤트 완료 상태 수령 | " +
                      $"출력값: 플레이어 총 획득 포인트={playerReward.totalPoints}");

            Assert.AreEqual(50, playerReward.totalPoints);
        }

        /// <summary>
        /// [기능]: 6. 이벤트 포인트 사용 검증
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 완료 로그 입력값/출력값 한글 표기 반영
        /// </summary>
        [Test]
        public async Task Test_06_EventPoint_Consumption()
        {
            await m_saveSystem.ClearAllAsync();
            var playerReward = new PlayerRewardModel();
            playerReward.totalPoints = 100;

            int spendAmount = 40;
            if (playerReward.totalPoints >= spendAmount)
            {
                playerReward.totalPoints -= spendAmount;
            }

            await m_saveSystem.SaveRewardStateAsync(playerReward);
            var loadedReward = await m_saveSystem.LoadRewardStateAsync();

            Debug.Log($"[EventSystemTests] [Test_06_EventPoint_Consumption] " +
                      $"입력값: 초기 보유 포인트=100, 소모 포인트액=40 | " +
                      $"출력값: 소모 후 잔여 포인트={playerReward.totalPoints}, 세이브 재로드 잔여 포인트={loadedReward.totalPoints}");

            Assert.AreEqual(60, playerReward.totalPoints);
            Assert.AreEqual(60, loadedReward.totalPoints);
        }

        /// <summary>
        /// [기능]: 7. 이미수령한 보상 중복 수령 방지 검증
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 완료 로그 입력값/출력값 한글 표기 반영
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

            Debug.Log($"[EventSystemTests] [Test_07_Duplicate_Reward_Claim_Prevention] " +
                      $"입력값: 단일 완료 이벤트 보상에 대해 2회 연속 수령 시도 | " +
                      $"출력값: 1차 수령 결과={claim1}, 2차 중복 수령 결과={claim2}, 플레이어 총 티켓 갯수={playerReward.totalTickets}");

            Assert.IsTrue(claim1);
            Assert.IsFalse(claim2);
            Assert.AreEqual(1, playerReward.totalTickets);
        }

        /// <summary>
        /// [기능]: 8. 진행도 저장 검증
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 완료 로그 입력값/출력값 한글 표기 반영
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

            Debug.Log($"[EventSystemTests] [Test_08_Progress_Serialization_Save] " +
                      $"입력값: 진행도 7 수치 세이브 파일에 저장 | " +
                      $"출력값: 물리 저장 장치 로드 결과={loaded.currentProgress}");

            Assert.IsNotNull(loaded);
            Assert.AreEqual(7, loaded.currentProgress);
        }

        /// <summary>
        /// [기능]: 9. 저장 된 진행도 불러오기 검증
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 완료 로그 입력값/출력값 한글 표기 반영
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

            Debug.Log($"[EventSystemTests] [Test_09_Progress_Deserialization_Load] " +
                      $"입력값: evt_load 세이브 데이터 (진행도=15, 완료=true, 수령=true) 저장 | " +
                      $"출력값: 로드 복원된 ID={loaded.eventId}, 진행도={loaded.currentProgress}, 완료여부={loaded.isCompleted}, 수령여부={loaded.isRewardClaimed}");
            
            Assert.AreEqual("evt_load", loaded.eventId);
            Assert.AreEqual(15, loaded.currentProgress);
            Assert.IsTrue(loaded.isCompleted);
            Assert.IsTrue(loaded.isRewardClaimed);
        }

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
            vm.OnEventListChanged += () => 
            { 
                eventListChangedCount++; 
            };

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

            Debug.Log($"[EventSystemTests] [Test_11_EventAdminVM_CRUD] " +
                      $"입력값: 뷰모델에 이벤트 추가 -> 수정 -> 삭제 조작 진행 | " +
                      $"출력값: 최종 이벤트 갯수={vm.GetEvents().Count}, 리스트 변경 알림 횟수={eventListChangedCount}");
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

            string testPath = System.IO.Path.Combine(Application.temporaryCachePath, "test_event_table.json");
            bool result = await vm.SaveToLocalFileAsync(testPath);

            Assert.IsTrue(result);
            Assert.IsTrue(System.IO.File.Exists(testPath));

            string content = System.IO.File.ReadAllText(testPath);
            Assert.IsTrue(content.Contains("evt_save_test"));

            Debug.Log($"[EventSystemTests] [Test_12_EventAdminVM_LocalSave] " +
                      $"입력값: evt_save_test 이벤트 생성 후 경로={testPath} 에 저장 시도 | " +
                      $"출력값: 저장 결과={result}, 파일 존재 여부={System.IO.File.Exists(testPath)}");

            if (System.IO.File.Exists(testPath))
            {
                System.IO.File.Delete(testPath);
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

            Debug.Log($"[EventSystemTests] [Test_13_EventAdminVM_Validation] " +
                      $"입력값: 이벤트 제목을 공란으로 설정 후 Firebase 업로드 시도 | " +
                      $"출력값: 업로드 성공 여부={result} (False 기대)");
        }
        #endregion
    }
}
