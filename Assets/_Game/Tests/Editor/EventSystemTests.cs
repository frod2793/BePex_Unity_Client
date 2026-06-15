using NUnit.Framework;
using System;
using System.Threading.Tasks;
using BePex.EventSystem.DTOs;
using BePex.EventSystem.Models;
using BePex.EventSystem.Factories;
using BePex.EventSystem.ViewModels;
using BePex.EventSystem.Infrastructure;
using BePex.EventSystem.Interfaces;
using UnityEngine;
using BePex.EventSystem.Data;

namespace BePex.EventSystem.Tests
{
    /// <summary>
    /// [기능]: 유니티 테스트 러너 9대 세부 조건에 대해 입력값과 출력값을 한글 로그로 상세 표시하고 검증하는 EditMode 유닛 테스트 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    public class EventSystemTests
    {
        #region 내부 필드 (Private Fields)
        private JsonSaveSystem m_saveSystem;
        private QuestConditionFactory m_condFactory;
        private QuestRewardFactory m_rewFactory;
        private ITimeProvider m_timeProvider;
        #endregion

        #region 초기화 및 셋업
        /// <summary>
        /// [기능]: 각 테스트 케이스 실행 전에 세이브 파일 시스템과 DTO 기반 팩토리들을 매번 새로 셋업합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Quest-related 팩토리로 교체
        /// </summary>
        [SetUp]
        public void Setup()
        {
            m_saveSystem = new JsonSaveSystem();
            m_timeProvider = new BePex.EventSystem.Infrastructure.DebugTimeProvider();
            m_condFactory = new QuestConditionFactory(m_saveSystem, m_timeProvider);
            m_rewFactory = new QuestRewardFactory();
        }
        #endregion

        #region 유닛 테스트 메서드
        /// <summary>
        /// [기능]: 1. 이벤트 목록 표시 검증
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 완료 로그 입력값/출력값 한글 표기 반영 및 퀘스트 네이밍 변경
        /// </summary>
        [Test]
        public void Test_01_EventList_Display()
        {
            var tableDTO = new EventTableDTO();
            tableDTO.events.Add(new EventDefinitionDTO { eventId = "evt_01", eventTitle = "테스트 1" });
            tableDTO.events.Add(new EventDefinitionDTO { eventId = "evt_02", eventTitle = "테스트 2" });

            var model = new EventModel(tableDTO, m_condFactory, m_rewFactory, m_timeProvider);
            var listVM = new EventListViewModel(model, m_saveSystem);

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
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: EventDetailViewModel 생성자 의존성 및 미션->퀘스트 변경 반영
        /// </summary>
        [Test]
        public async Task Test_02_EventProgress_Display()
        {
            await m_saveSystem.ClearAllAsync();
            var tableDTO = new EventTableDTO();
            var ev = new EventDefinitionDTO { eventId = "evt_progress" };
            ev.quests.Add(new QuestDefinitionDTO
            {
                questId = "quest_01",
                condition = new ConditionDefinitionDTO { conditionType = "KillCount", targetValue = 10 }
            });
            tableDTO.events.Add(ev);

            var model = new EventModel(tableDTO, m_condFactory, m_rewFactory, m_timeProvider);
            var playerReward = new PlayerRewardModel();
            var detailVM = new EventDetailViewModel(model, m_saveSystem, playerReward);

            detailVM.SetEvent("evt_progress");
            await model.Debug_AddProgressAsync("evt_progress", "quest_01", 4, m_saveSystem);

            var (cur, tar, ratio) = await detailVM.GetProgressInfoAsync("quest_01");

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
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 완료 로그 입력값/출력값 한글 표기 반영 및 퀘스트 네이밍 변경
        /// </summary>
        [Test]
        public async Task Test_03_EventCompletion_Processing()
        {
            await m_saveSystem.ClearAllAsync();
            var tableDTO = new EventTableDTO();
            var ev = new EventDefinitionDTO { eventId = "evt_complete" };
            ev.quests.Add(new QuestDefinitionDTO
            {
                questId = "quest_01",
                condition = new ConditionDefinitionDTO { conditionType = "KillCount", targetValue = 10 }
            });
            tableDTO.events.Add(ev);

            var model = new EventModel(tableDTO, m_condFactory, m_rewFactory, m_timeProvider);
            var condition = model.GetCondition("evt_complete", "quest_01");

            await model.Debug_AddProgressAsync("evt_complete", "quest_01", 10, m_saveSystem);
            bool isCompleted = await condition.IsCompletedAsync();

            Debug.Log($"[EventSystemTests] [Test_03_EventCompletion_Processing] " +
                      $"입력값: 목표치=10, 가산진행도=10 | " +
                      $"출력값: 완료 여부={isCompleted}");

            Assert.IsTrue(isCompleted);
        }

        /// <summary>
        /// [기능]: 4. 보상 수령 처리 검증
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 완료 로그 입력값/출력값 한글 표기 반영 및 퀘스트 네이밍 변경
        /// </summary>
        [Test]
        public async Task Test_04_Reward_Claim_Processing()
        {
            await m_saveSystem.ClearAllAsync();
            var tableDTO = new EventTableDTO();
            var ev = new EventDefinitionDTO { eventId = "evt_claim" };
            var quest = new QuestDefinitionDTO
            {
                questId = "quest_01",
                condition = new ConditionDefinitionDTO { conditionType = "KillCount", targetValue = 5 }
            };
            quest.rewards.Add(new RewardDefinitionDTO { rewardType = "Exp", amount = 100, displayName = "경험치" });
            ev.quests.Add(quest);
            tableDTO.events.Add(ev);

            var model = new EventModel(tableDTO, m_condFactory, m_rewFactory, m_timeProvider);
            var playerReward = new PlayerRewardModel();

            await model.Debug_AddProgressAsync("evt_claim", "quest_01", 5, m_saveSystem);
            
            bool success = await model.ClaimRewardAsync("evt_claim", "quest_01", m_saveSystem, playerReward);

            Debug.Log($"[EventSystemTests] [Test_04_Reward_Claim_Processing] " +
                      $"입력값: 경험치 100 지급이벤트 완료 상태 수령 시도 | " +
                      $"출력값: 보상 수령 성공여부={success}, 플레이어 총 획득 경험치={playerReward.totalExp}");

            Assert.IsTrue(success);
            Assert.AreEqual(100, playerReward.totalExp);
        }

        /// <summary>
        /// [기능]: 5. 이벤트 포인트 획득 검증
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 완료 로그 입력값/출력값 한글 표기 반영 및 퀘스트 네이밍 변경
        /// </summary>
        [Test]
        public async Task Test_05_EventPoint_Acquisition()
        {
            await m_saveSystem.ClearAllAsync();
            var tableDTO = new EventTableDTO();
            var ev = new EventDefinitionDTO { eventId = "evt_point" };
            var quest = new QuestDefinitionDTO
            {
                questId = "quest_01",
                condition = new ConditionDefinitionDTO { conditionType = "KillCount", targetValue = 5 }
            };
            quest.rewards.Add(new RewardDefinitionDTO { rewardType = "Point", amount = 50, displayName = "포인트" });
            ev.quests.Add(quest);
            tableDTO.events.Add(ev);

            var model = new EventModel(tableDTO, m_condFactory, m_rewFactory, m_timeProvider);
            var playerReward = new PlayerRewardModel();

            await model.Debug_AddProgressAsync("evt_point", "quest_01", 5, m_saveSystem);
            await model.ClaimRewardAsync("evt_point", "quest_01", m_saveSystem, playerReward);

            Debug.Log($"[EventSystemTests] [Test_05_EventPoint_Acquisition] " +
                      $"입력값: 포인트 50 지급이벤트 완료 상태 수령 | " +
                      $"출력값: 플레이어 총 획득 포인트={playerReward.totalPoints}");

            Assert.AreEqual(50, playerReward.totalPoints);
        }

        /// <summary>
        /// [기능]: 6. 이벤트 포인트 사용 검증
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 완료 로그 입력값/출력값 한글 표기 반영 및 퀘스트 네이밍 변경
        /// </summary>
        [Test]
        public async Task Test_06_EventPoint_Consumption()
        {
            await m_saveSystem.ClearAllAsync();
            var playerReward = new PlayerRewardModel();
            playerReward.AddCurrency(RewardDefinitionSO.RewardType.Point, 100);

            int spendAmount = 40;
            playerReward.TrySpendCurrency(RewardDefinitionSO.RewardType.Point, spendAmount);

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
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 완료 로그 입력값/출력값 한글 표기 반영 및 퀘스트 네이밍 변경
        /// </summary>
        [Test]
        public async Task Test_07_Duplicate_Reward_Claim_Prevention()
        {
            await m_saveSystem.ClearAllAsync();
            var tableDTO = new EventTableDTO();
            var ev = new EventDefinitionDTO { eventId = "evt_dup" };
            var quest = new QuestDefinitionDTO
            {
                questId = "quest_01",
                condition = new ConditionDefinitionDTO { conditionType = "KillCount", targetValue = 5 }
            };
            quest.rewards.Add(new RewardDefinitionDTO { rewardType = "Ticket", amount = 1, displayName = "티켓" });
            ev.quests.Add(quest);
            tableDTO.events.Add(ev);

            var model = new EventModel(tableDTO, m_condFactory, m_rewFactory, m_timeProvider);
            var playerReward = new PlayerRewardModel();

            await model.Debug_AddProgressAsync("evt_dup", "quest_01", 5, m_saveSystem);
            
            bool claim1 = await model.ClaimRewardAsync("evt_dup", "quest_01", m_saveSystem, playerReward);
            bool claim2 = await model.ClaimRewardAsync("evt_dup", "quest_01", m_saveSystem, playerReward);

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
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 완료 로그 입력값/출력값 한글 표기 반영 및 퀘스트 네이밍 변경
        /// </summary>
        [Test]
        public async Task Test_08_Progress_Serialization_Save()
        {
            await m_saveSystem.ClearAllAsync();
            var progress = new EventProgressModel { eventId = "evt_save" };
            progress.quests.Add(new QuestProgressModel
            {
                questId = "quest_01",
                currentProgress = 7,
                isCompleted = false,
                isRewardClaimed = false,
                lastUpdatedTicks = 123456789012345L
            });

            await m_saveSystem.SaveProgressAsync("evt_save", progress);
            var loaded = await m_saveSystem.LoadProgressAsync("evt_save");

            Debug.Log($"[EventSystemTests] [Test_08_Progress_Serialization_Save] " +
                      $"입력값: 진행도 7 수치 및 틱스 123456789012345 저장 | " +
                      $"출력값: 물리 저장 장치 로드 결과={loaded.quests[0].currentProgress}, 틱스={loaded.quests[0].lastUpdatedTicks}");

            Assert.IsNotNull(loaded);
            Assert.AreEqual(7, loaded.quests[0].currentProgress);
            Assert.AreEqual(123456789012345L, loaded.quests[0].lastUpdatedTicks);
        }

        /// <summary>
        /// [기능]: 9. 저장 된 진행도 불러오기 검증
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 완료 로그 입력값/출력값 한글 표기 반영 및 퀘스트 네이밍 변경
        /// </summary>
        [Test]
        public async Task Test_09_Progress_Deserialization_Load()
        {
            await m_saveSystem.ClearAllAsync();
            var progress = new EventProgressModel { eventId = "evt_load" };
            progress.quests.Add(new QuestProgressModel
            {
                questId = "quest_01",
                currentProgress = 15,
                isCompleted = true,
                isRewardClaimed = true
            });
            
            await m_saveSystem.SaveProgressAsync("evt_load", progress);
            var loaded = await m_saveSystem.LoadProgressAsync("evt_load");

            Debug.Log($"[EventSystemTests] [Test_09_Progress_Deserialization_Load] " +
                      $"입력값: evt_load 세이브 데이터 (진행도=15, 완료=true, 수령=true) 저장 | " +
                      $"출력값: 로드 복원된 ID={loaded.eventId}, 진행도={loaded.quests[0].currentProgress}, 완료여부={loaded.quests[0].isCompleted}, 수령여부={loaded.quests[0].isRewardClaimed}");
            
            Assert.AreEqual("evt_load", loaded.eventId);
            Assert.AreEqual(15, loaded.quests[0].currentProgress);
            Assert.IsTrue(loaded.quests[0].isCompleted);
            Assert.IsTrue(loaded.quests[0].isRewardClaimed);
        }

        /// <summary>
        /// [기능]: 10. Mock Firebase 업로드 비동기 시뮬레이션 검증
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
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
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성 및 퀘스트 네이밍 변경
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
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성 및 퀘스트 네이밍 변경
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
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성 및 퀘스트 네이밍 변경
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

        /// <summary>
        /// [기능]: 14. 날짜 변경/출석 이벤트 조건 검증
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성 및 퀘스트 네이밍 변경
        /// </summary>
        [Test]
        public async Task Test_14_AttendanceEvent_Calculation()
        {
            await m_saveSystem.ClearAllAsync();
            var tableDTO = new EventTableDTO();
            var ev = new EventDefinitionDTO { eventId = "evt_attendance" };
            ev.quests.Add(new QuestDefinitionDTO
            {
                questId = "quest_01",
                condition = new ConditionDefinitionDTO { conditionType = "Attendance", targetValue = 3 }
            });
            tableDTO.events.Add(ev);

            var debugTime = m_timeProvider as BePex.EventSystem.Infrastructure.DebugTimeProvider;
            if (debugTime != null)
            {
                debugTime.ResetOffset();
            }

            var model = new EventModel(tableDTO, m_condFactory, m_rewFactory, m_timeProvider);
            var condition = model.GetCondition("evt_attendance", "quest_01");

            await model.Debug_AddProgressAsync("evt_attendance", "quest_01", 1, m_saveSystem);
            bool isCompleted1 = await condition.IsCompletedAsync();
            
            if (debugTime != null)
            {
                debugTime.AddDays(1);
            }

            await model.Debug_AddProgressAsync("evt_attendance", "quest_01", 1, m_saveSystem);
            
            if (debugTime != null)
            {
                debugTime.AddDays(1);
            }

            await model.Debug_AddProgressAsync("evt_attendance", "quest_01", 1, m_saveSystem);
            bool isCompleted2 = await condition.IsCompletedAsync();

            Debug.Log($"[EventSystemTests] [Test_14_AttendanceEvent_Calculation] " +
                      $"입력값: 목표 출석=3, 누적 출석=1 -> 1 -> 1 (총 3일) | " +
                      $"출력값: 1회 출석 완료여부={isCompleted1}, 3회 출석 완료여부={isCompleted2}");

            Assert.IsFalse(isCompleted1);
            Assert.IsTrue(isCompleted2);
        }

        /// <summary>
        /// [기능]: 15. 스테이지 클리어 조건 달성 검증
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성 및 퀘스트 네이밍 변경
        /// </summary>
        [Test]
        public async Task Test_15_StageClearCondition_Validation()
        {
            await m_saveSystem.ClearAllAsync();
            var tableDTO = new EventTableDTO();
            var ev = new EventDefinitionDTO { eventId = "evt_stage" };
            ev.quests.Add(new QuestDefinitionDTO
            {
                questId = "quest_01",
                condition = new ConditionDefinitionDTO { conditionType = "StageClear", targetValue = 5 }
            });
            tableDTO.events.Add(ev);

            var model = new EventModel(tableDTO, m_condFactory, m_rewFactory, m_timeProvider);
            var condition = model.GetCondition("evt_stage", "quest_01");

            await model.Debug_AddProgressAsync("evt_stage", "quest_01", 5, m_saveSystem);
            bool isCompleted = await condition.IsCompletedAsync();

            Debug.Log($"[EventSystemTests] [Test_15_StageClearCondition_Validation] " +
                      $"입력값: 목표 스테이지 클리어 횟수=5, 진행도=5 | " +
                      $"출력값: 달성 여부={isCompleted}");

            Assert.IsTrue(isCompleted);
        }

        /// <summary>
        /// [기능]: 16. ViewModel 생성 및 해제 후 메모리 누수 방지(Unsubscribe) 테스트
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성 및 퀘스트 네이밍 변경
        /// </summary>
        [Test]
        public void Test_16_ViewModel_MemoryLeak_Check()
        {
            var tableDTO = new EventTableDTO();
            var model = new EventModel(tableDTO, m_condFactory, m_rewFactory, m_timeProvider);
            
            var listVM = new EventListViewModel(model, m_saveSystem);
            int callCount = 0;
            listVM.OnListUpdated += () => { callCount++; };

            if (listVM is IDisposable disposableVM)
            {
                disposableVM.Dispose();
            }

            // Reflection을 통해 OnEventProgressChanged 강제 호출
            var eventField = typeof(EventModel).GetField("OnEventProgressChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (eventField != null)
            {
                var eventDelegate = (MulticastDelegate)eventField.GetValue(model);
                if (eventDelegate != null)
                {
                    foreach (var handler in eventDelegate.GetInvocationList())
                    {
                        handler.Method.Invoke(handler.Target, new object[] { "test_event" });
                    }
                }
            }

            Debug.Log($"[EventSystemTests] [Test_16_ViewModel_MemoryLeak_Check] " +
                      $"입력값: ViewModel Dispose 후 EventModel 통지 시뮬레이션 | " +
                      $"출력값: 콜백 호출 횟수={callCount} (0이어야 메모리 누수 없음)");

            Assert.AreEqual(0, callCount);
        }

        /// <summary>
        /// [기능]: 17. 다중 퀘스트 일괄 보상 수령 검증
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성 및 퀘스트 명세 반영
        /// </summary>
        [Test]
        public async Task Test_17_ClaimAllRewards_Processing()
        {
            await m_saveSystem.ClearAllAsync();
            var tableDTO = new EventTableDTO();
            var ev = new EventDefinitionDTO { eventId = "evt_claim_all" };

            var q1 = new QuestDefinitionDTO
            {
                questId = "quest_01",
                condition = new ConditionDefinitionDTO { conditionType = "KillCount", targetValue = 5 }
            };
            q1.rewards.Add(new RewardDefinitionDTO { rewardType = "Exp", amount = 100, displayName = "경험치" });

            var q2 = new QuestDefinitionDTO
            {
                questId = "quest_02",
                condition = new ConditionDefinitionDTO { conditionType = "StageClear", targetValue = 3 }
            };
            q2.rewards.Add(new RewardDefinitionDTO { rewardType = "Point", amount = 50, displayName = "포인트" });

            ev.quests.Add(q1);
            ev.quests.Add(q2);
            tableDTO.events.Add(ev);

            var model = new EventModel(tableDTO, m_condFactory, m_rewFactory, m_timeProvider);
            var playerReward = new PlayerRewardModel();

            // 두 퀘스트를 완료시킴
            await model.Debug_AddProgressAsync("evt_claim_all", "quest_01", 5, m_saveSystem);
            await model.Debug_AddProgressAsync("evt_claim_all", "quest_02", 3, m_saveSystem);

            bool success = await model.ClaimAllRewardsAsync("evt_claim_all", m_saveSystem, playerReward);

            Debug.Log($"[EventSystemTests] [Test_17_ClaimAllRewards_Processing] " +
                      $"입력값: 경험치 100, 포인트 50 지급 퀘스트 2개 완료 후 일괄 수령 | " +
                      $"출력값: 일괄수령 성공여부={success}, 플레이어 경험치={playerReward.totalExp}, 포인트={playerReward.totalPoints}");

            Assert.IsTrue(success);
            Assert.AreEqual(100, playerReward.totalExp);
            Assert.AreEqual(50, playerReward.totalPoints);
        }
        #endregion
    }
}
