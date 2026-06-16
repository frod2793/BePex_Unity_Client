using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BePex.EventSystem.Models;
using BePex.EventSystem.ViewModels;
using BePex.EventSystem.ViewModelsDebug;
using BePex.EventSystem.DTOs;
using BePex.EventSystem.Factories;
using BePex.EventSystem.Infrastructure;
using BePex.EventSystem.Data;

namespace BePex.EventSystem.Tests.PlayMode
{
    /// <summary>
    /// [기능]: 9대 핵심 기능(목록/진행도 표시, 완료, 수령, 포인트 적립/소모, 중복 방지, 세이브/로드)을
    ///        순차적 통합 시나리오로 유기적 검증을 처리하는 플레이 모드 통합 테스트 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    public class EventIntegrationTests
    {
        private InMemorySaveSystem m_saveSystem;
        private MockTimeProvider m_timeProvider;
        private QuestConditionFactory m_condFactory;
        private QuestRewardFactory m_rewardFactory;
        private EventModel m_model;
        private PlayerRewardModel m_playerReward;
        private EventDetailViewModel m_detailVM;
        private EventDebugViewModel m_debugVM;
        private CurrencyHUDViewModel m_hudVM;
        private RewardPopupViewModel m_popupVM;
        private EventTableDTO m_tableDTO;

        /// <summary>
        /// [기능]: 통합 테스트 구동에 수반되는 순수 C# 의존성 모듈들을 수동 조립 및 초기화합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Quest-related 팩토리 교체 및 퀘스트 기반 DTO 셋업 수정
        /// </summary>
        [SetUp]
        public void Setup()
        {
            m_saveSystem = new InMemorySaveSystem();
            m_timeProvider = new MockTimeProvider();
            m_timeProvider.CurrentTime = new System.DateTime(2026, 6, 15, 12, 0, 0); // 고정된 기준 시간
            m_condFactory = new QuestConditionFactory(m_saveSystem, m_timeProvider);
            m_rewardFactory = new QuestRewardFactory();

            // 1. DTO 설계 및 데이터 모의 주입
            m_tableDTO = new EventTableDTO();
            
            var eventDto = new EventDefinitionDTO
            {
                eventId = "evt_integration_test_id",
                eventTitle = "통합 테스트 이벤트",
                eventDescription = "9대 기능을 동시에 유기적으로 검증하는 통합 이벤트"
            };

            var questDto = new QuestDefinitionDTO
            {
                questId = "qst_integration_test_id",
                questTitle = "통합 테스트 퀘스트",
                condition = new ConditionDefinitionDTO
                {
                    conditionType = "KillCount",
                    targetValue = 10
                }
            };
            
            questDto.rewards.Add(new RewardDefinitionDTO
            {
                rewardType = "Point",
                amount = 100,
                displayName = "테스트포인트"
            });
            questDto.rewards.Add(new RewardDefinitionDTO
            {
                rewardType = "SeasonPoint",
                amount = 20,
                displayName = "테스트시즌포인트"
            });
            questDto.rewards.Add(new RewardDefinitionDTO
            {
                rewardType = "CreditReward",
                amount = 5,
                displayName = "테스트크레딧"
            });

            eventDto.quests.Add(questDto);
            m_tableDTO.events.Add(eventDto);

            // 2. 모델 및 뷰모델 인스턴스 생성
            m_model = new EventModel(m_tableDTO, m_condFactory, m_rewardFactory, m_timeProvider);
            m_playerReward = new PlayerRewardModel();
            
            m_hudVM = new CurrencyHUDViewModel(m_playerReward);
            m_detailVM = new EventDetailViewModel(m_model, m_saveSystem, m_playerReward);
            m_debugVM = new EventDebugViewModel(m_model, m_saveSystem, m_timeProvider, m_playerReward, m_hudVM);
            m_popupVM = new RewardPopupViewModel(m_playerReward, m_saveSystem, m_model);

            // Detail VM 초기 이벤트 선택
            m_detailVM.SetEvent("evt_integration_test_id");
        }

        /// <summary>
        /// [기능]: 사용 후 자원 정리 및 뷰모델 IDisposable 해제 처리.
        /// [작성자]: 윤승종
        /// </summary>
        [TearDown]
        public void Teardown()
        {
            if (m_detailVM != null)
            {
                m_detailVM.Dispose();
            }
        }

        /// <summary>
        /// [기능]: NUnit 및 PlayMode 상에서 9대 핵심 시나리오가 데이터 무결성을 보장하며
        ///        유기적으로 관통 구동되는지 한 단계씩 확인하는 다단계 어서트 테스트.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 퀘스트 ID 매개변수 추가 전달 및 검증
        /// </summary>
        [UnityTest]
        public IEnumerator Test_Nine_Core_Features_Integrative_Flow()
        {
            bool isDone = false;
            System.Action runFlow = async () =>
            {
                try
                {
                    Debug.Log("[EventIntegrationTests] ======= [1단계: 이벤트 목록 표시 및 진행도 표시 검증] =======");
                    // 1. 이벤트 목록 표시 검증
                    var activeEvents = new System.Collections.Generic.List<EventDefinitionDTO>();
                    m_model.GetActiveEventsNonAlloc(activeEvents);
                    Assert.AreEqual(1, activeEvents.Count, "[EventIntegrationTests] 활성화된 이벤트 목록 개수가 일치하지 않습니다.");
                    Assert.AreEqual("evt_integration_test_id", activeEvents[0].eventId, "[EventIntegrationTests] 활성 이벤트 ID가 일치하지 않습니다.");

                    // 2. 진행도 표시 검증
                    var progressInfo = await m_detailVM.GetProgressInfoAsync("qst_integration_test_id");
                    Assert.AreEqual(0, progressInfo.current, "[EventIntegrationTests] 초기 진척도는 0이어야 합니다.");
                    Assert.AreEqual(10, progressInfo.target, "[EventIntegrationTests] 목표 진척도는 10이어야 합니다.");
                    Assert.AreEqual(0f, progressInfo.ratio, "[EventIntegrationTests] 초기 진척 비율은 0f이어야 합니다.");
                    Debug.Log("[EventIntegrationTests] 1단계 완료: 목록 및 진척도 데이터 바인딩 확인 완료.");


                    Debug.Log("[EventIntegrationTests] ======= [2단계: 이벤트 조건 달성 및 완료 처리 검증] =======");
                    // 3. 진척도 가산 처리 (이벤트 완료 유도)
                    await m_debugVM.SimulateAddProgressAsync("evt_integration_test_id", "qst_integration_test_id", 10);
                    
                    // 진행도 갱신 확인
                    progressInfo = await m_detailVM.GetProgressInfoAsync("qst_integration_test_id");
                    Assert.AreEqual(10, progressInfo.current, "[EventIntegrationTests] 가산 후 진척도가 10이어야 합니다.");
                    Assert.AreEqual(1.0f, progressInfo.ratio, "[EventIntegrationTests] 가산 후 진척 비율은 1.0f이어야 합니다.");

                    // 4. 완료 상태 충족 확인
                    bool canClaim = await m_detailVM.CanClaimRewardAsync("qst_integration_test_id");
                    Assert.IsTrue(canClaim, "[EventIntegrationTests] 조건을 달성했으므로 보상 수령이 가능해야 합니다.");
                    Debug.Log("[EventIntegrationTests] 2단계 완료: 조건 가산 및 클리어 완료 판정 확인 완료.");


                    Debug.Log("[EventIntegrationTests] ======= [3단계: 보상 수령 처리 및 이벤트 포인트 획득 검증] =======");
                    // 5. 보상 수령
                    Assert.AreEqual(0, m_playerReward.totalPoints, "[EventIntegrationTests] 초기 포인트는 0이어야 합니다.");
                    await m_detailVM.ClaimRewardAsync("qst_integration_test_id");

                    // 6. 보상 획득 및 수령 완료 처리 확인
                    Assert.AreEqual(100, m_playerReward.totalPoints, "[EventIntegrationTests] 보상 수령 후 포인트가 100이어야 합니다.");
                    Assert.AreEqual(20, m_playerReward.totalSeasonPoints, "[EventIntegrationTests] 보상 수령 후 시즌 포인트가 20이어야 합니다.");
                    Assert.AreEqual(5, m_playerReward.totalCredits, "[EventIntegrationTests] 보상 수령 후 크레딧이 5여야 합니다.");

                    // 팝업 뷰모델 상태 동기화 확인
                    m_popupVM.Refresh("evt_integration_test_id");
                    var claimedRewards = m_popupVM.GetClaimedRewards();
                    Assert.AreEqual(3, claimedRewards.Count, "[EventIntegrationTests] 팝업 뷰모델이 보유한 획득 보상 개수가 일치하지 않습니다.");
                    Assert.AreEqual("테스트포인트", claimedRewards[0].displayName);
                    Assert.AreEqual("테스트시즌포인트", claimedRewards[1].displayName);
                    Assert.AreEqual("테스트크레딧", claimedRewards[2].displayName);

                    bool isClaimed = await m_detailVM.IsRewardClaimedAsync("qst_integration_test_id");
                    Assert.IsTrue(isClaimed, "[EventIntegrationTests] 수령 완료 상태가 참(true)이어야 합니다.");
                    Debug.Log("[EventIntegrationTests] 3단계 완료: 보상 갱신, 포인트/시즌포인트/크레딧 획득 및 팝업 뷰모델 연동 확인 완료.");


                    Debug.Log("[EventIntegrationTests] ======= [4단계: 이미 수령한 보상 중복 수령 방지 검증] =======");
                    // 7. 중복 수령 조건 거절 확인
                    bool canClaimAgain = await m_detailVM.CanClaimRewardAsync("qst_integration_test_id");
                    Assert.IsFalse(canClaimAgain, "[EventIntegrationTests] 이미 수령한 이벤트는 다시 수령할 수 없어야 합니다.");

                    // 강제 재호출 시도 시 데이터에 영향이 없어야 함
                    await m_detailVM.ClaimRewardAsync("qst_integration_test_id");
                    Assert.AreEqual(100, m_playerReward.totalPoints, "[EventIntegrationTests] 중복 수령 거부로 인해 포인트가 증가하지 않아야 합니다.");
                    Debug.Log("[EventIntegrationTests] 4단계 완료: 이미 수령한 보상에 대한 중복 청구 차단 확인 완료.");


                    Debug.Log("[EventIntegrationTests] ======= [5단계: 이벤트 포인트 사용(교환) 검증] =======");
                    // 8. 포인트 소모
                    await m_debugVM.SimulateSpendPointsAsync(40);
                    Assert.AreEqual(60, m_playerReward.totalPoints, "[EventIntegrationTests] 40포인트 소모 후 잔여 포인트는 60이어야 합니다.");
                    Debug.Log("[EventIntegrationTests] 5단계 완료: SimulateSpendPointsAsync를 통한 재화 차감 및 HUD 갱신 확인 완료.");


                    Debug.Log("[EventIntegrationTests] ======= [6단계: 진행도 저장 및 로드 데이터 복구 검증] =======");
                    // 9. 새로운 모델 인스턴스 셋업 (세이브 연동 확인)
                    var loadedModel = new EventModel(m_tableDTO, m_condFactory, m_rewardFactory, m_timeProvider);
                    var loadedDetailVM = new EventDetailViewModel(loadedModel, m_saveSystem, m_playerReward);
                    loadedDetailVM.SetEvent("evt_integration_test_id");

                    // 새로 로드한 VM에서도 기존 수령 상태가 영속화되어 복구되는지 확인
                    bool loadedClaimed = await loadedDetailVM.IsRewardClaimedAsync("qst_integration_test_id");
                    Assert.IsTrue(loadedClaimed, "[EventIntegrationTests] 새로 인스턴싱하여 복구한 모델에서도 수령 이력이 그대로 로드되어야 합니다.");

                    // 누적 진행도 수치가 10으로 잘 불러와지는지 복합 검증
                    var loadedProgress = await loadedDetailVM.GetProgressInfoAsync("qst_integration_test_id");
                    Assert.AreEqual(10, loadedProgress.current, "[EventIntegrationTests] 로드된 진행 수치가 10으로 일치하지 않습니다.");
                    
                    loadedDetailVM.Dispose();
                    Debug.Log("[EventIntegrationTests] 6단계 완료: ISaveSystem에 의한 비동기 영속화 데이터 복원 무결성 확인 완료.");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[EventIntegrationTests] 통합 테스트 도중 오류 발생: {ex.Message}\n{ex.StackTrace}");
                    throw;
                }
                finally
                {
                    isDone = true;
                }
            };

            runFlow();

            while (isDone == false)
            {
                yield return null;
            }
        }

        /// <summary>
        /// [기능]: 다양한 난이도(목표치)와 여러 형태의 조건(KillCount, StageClear, Attendance)이 
        ///        동시에 존재할 때, 각각이 독립적으로 잘 가산되고 완료 처리되는지 검증합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 퀘스트 하위 DTO 생성 및 퀘스트 ID 개별 전달 검증
        /// </summary>
        [UnityTest]
        public IEnumerator Test_Multi_Condition_And_Difficulties()
        {
            bool isDone = false;
            System.Action runFlow = async () =>
            {
                try
                {
                    // 기존 이벤트 지우고 새로 세팅
                    m_tableDTO.events.Clear();
                    
                    var easyEvent = new EventDefinitionDTO
                    {
                        eventId = "evt_easy_kill",
                        eventTitle = "쉬운 처치 미션",
                    };
                    var easyQuest = new QuestDefinitionDTO
                    {
                        questId = "easy_quest_id",
                        questTitle = "쉬운 퀘스트",
                        condition = new ConditionDefinitionDTO { conditionType = "KillCount", targetValue = 5 }
                    };
                    easyQuest.rewards.Add(new RewardDefinitionDTO { rewardType = "Point", amount = 10, displayName = "쉬운보상" });
                    easyEvent.quests.Add(easyQuest);
                    m_tableDTO.events.Add(easyEvent);

                    var hardEvent = new EventDefinitionDTO
                    {
                        eventId = "evt_hard_stage",
                        eventTitle = "어려운 스테이지 클리어",
                    };
                    var hardQuest = new QuestDefinitionDTO
                    {
                        questId = "hard_quest_id",
                        questTitle = "어려운 퀘스트",
                        condition = new ConditionDefinitionDTO { conditionType = "StageClear", targetValue = 50 }
                    };
                    hardQuest.rewards.Add(new RewardDefinitionDTO { rewardType = "Point", amount = 500, displayName = "어려운보상" });
                    hardEvent.quests.Add(hardQuest);
                    m_tableDTO.events.Add(hardEvent);

                    var midEvent = new EventDefinitionDTO
                    {
                        eventId = "evt_mid_attendance",
                        eventTitle = "출석 체크",
                    };
                    var midQuest = new QuestDefinitionDTO
                    {
                        questId = "mid_quest_id",
                        questTitle = "출석 퀘스트",
                        condition = new ConditionDefinitionDTO { conditionType = "Attendance", targetValue = 3 }
                    };
                    midQuest.rewards.Add(new RewardDefinitionDTO { rewardType = "Point", amount = 50, displayName = "출석보상" });
                    midEvent.quests.Add(midQuest);
                    m_tableDTO.events.Add(midEvent);

                    m_model.Reload();

                    var activeEvents = new System.Collections.Generic.List<EventDefinitionDTO>();
                    m_model.GetActiveEventsNonAlloc(activeEvents);
                    Assert.AreEqual(3, activeEvents.Count, "[Test_Multi_Condition] 활성화된 이벤트는 3개여야 합니다.");

                    // 1. 쉬운 미션 5만큼 달성 시도
                    await m_debugVM.SimulateAddProgressAsync("evt_easy_kill", "easy_quest_id", 5);
                    m_detailVM.SetEvent("evt_easy_kill");
                    var easyProgress = await m_detailVM.GetProgressInfoAsync("easy_quest_id");
                    Assert.AreEqual(5, easyProgress.current, "[Test_Multi_Condition] 쉬운 미션 진행도가 5여야 합니다.");
                    Assert.IsTrue(await m_detailVM.CanClaimRewardAsync("easy_quest_id"), "[Test_Multi_Condition] 쉬운 미션은 완료되어야 합니다.");

                    // 2. 어려운 미션은 아직 미완료인지 확인
                    m_detailVM.SetEvent("evt_hard_stage");
                    var hardProgress = await m_detailVM.GetProgressInfoAsync("hard_quest_id");
                    Assert.AreEqual(0, hardProgress.current, "[Test_Multi_Condition] 독립된 이벤트이므로 어려운 미션은 진행도가 0이어야 합니다.");

                    // 어려운 미션 20 달성
                    await m_debugVM.SimulateAddProgressAsync("evt_hard_stage", "hard_quest_id", 20);
                    hardProgress = await m_detailVM.GetProgressInfoAsync("hard_quest_id");
                    Assert.AreEqual(20, hardProgress.current, "[Test_Multi_Condition] 어려운 미션 진행도가 20이어야 합니다.");
                    Assert.IsFalse(await m_detailVM.CanClaimRewardAsync("hard_quest_id"), "[Test_Multi_Condition] 어려운 미션은 아직 완료되지 않아야 합니다.");
                    
                    Debug.Log("[EventIntegrationTests] 다중 난이도/조건 상호 독립성 및 가산 검증 완료");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[EventIntegrationTests] Test_Multi_Condition_And_Difficulties 도중 오류 발생: {ex.Message}");
                    throw;
                }
                finally
                {
                    isDone = true;
                }
            };

            runFlow();

            while (isDone == false)
            {
                yield return null;
            }
        }

        /// <summary>
        /// [기능]: 이벤트 기간(startDate, endDate)을 다양하게 설정하고,
        ///        시간의 흐름에 따라 활성화/만료 처리가 정상적으로 이루어지는지 검증합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 퀘스트 하위 DTO 생성 셋업으로 수정
        /// </summary>
        [UnityTest]
        public IEnumerator Test_Time_Expiration_Scenarios()
        {
            bool isDone = false;
            System.Action runFlow = () =>
            {
                try
                {
                    m_tableDTO.events.Clear();
                    
                    // 시작 전 이벤트
                    var futureEvt = new EventDefinitionDTO
                    {
                        eventId = "evt_future",
                        startDate = "2026-07-01",
                        endDate = "2026-07-31",
                    };
                    futureEvt.quests.Add(new QuestDefinitionDTO
                    {
                        questId = "future_qst",
                        condition = new ConditionDefinitionDTO { conditionType = "KillCount", targetValue = 1 }
                    });
                    m_tableDTO.events.Add(futureEvt);

                    // 진행 중 이벤트
                    var currentEvt = new EventDefinitionDTO
                    {
                        eventId = "evt_current",
                        startDate = "2026-06-01",
                        endDate = "2026-06-30",
                    };
                    currentEvt.quests.Add(new QuestDefinitionDTO
                    {
                        questId = "current_qst",
                        condition = new ConditionDefinitionDTO { conditionType = "KillCount", targetValue = 1 }
                    });
                    m_tableDTO.events.Add(currentEvt);

                    // 만료된 이벤트
                    var expiredEvt = new EventDefinitionDTO
                    {
                        eventId = "evt_expired",
                        startDate = "2026-05-01",
                        endDate = "2026-05-31",
                    };
                    expiredEvt.quests.Add(new QuestDefinitionDTO
                    {
                        questId = "expired_qst",
                        condition = new ConditionDefinitionDTO { conditionType = "KillCount", targetValue = 1 }
                    });
                    m_tableDTO.events.Add(expiredEvt);

                    m_model.Reload();

                    // 현재 시간: 2026-06-15
                    m_timeProvider.CurrentTime = new System.DateTime(2026, 6, 15);
                    var activeEvents = new System.Collections.Generic.List<EventDefinitionDTO>();
                    m_model.GetActiveEventsNonAlloc(activeEvents);
                    
                    Assert.AreEqual(1, activeEvents.Count, "[Test_Time_Expiration] 활성 이벤트는 1개여야 합니다.");
                    Assert.AreEqual("evt_current", activeEvents[0].eventId, "[Test_Time_Expiration] 현재 진행 중인 이벤트만 필터링되어야 합니다.");

                    // 시간을 만료 기간인 2026-08-01로 강제 변경
                    m_timeProvider.CurrentTime = new System.DateTime(2026, 8, 1);
                    m_model.GetActiveEventsNonAlloc(activeEvents);
                    
                    Assert.AreEqual(0, activeEvents.Count, "[Test_Time_Expiration] 시간이 만료되어 활성화된 이벤트가 없어야 합니다.");
                    
                    Debug.Log("[EventIntegrationTests] 기간 만료에 따른 이벤트 필터링 검증 완료");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[EventIntegrationTests] Test_Time_Expiration_Scenarios 도중 오류 발생: {ex.Message}");
                    throw;
                }
                finally
                {
                    isDone = true;
                }
            };

            runFlow();

            while (isDone == false)
            {
                yield return null;
            }
        }

        /// <summary>
        /// [기능]: 22. EventDebugView의 func_ToggleDrawer 빠르게 스팸 클릭 호출 시 NullReferenceException 방지 검증
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        [UnityTest]
        public IEnumerator Test_22_EventDebugView_ToggleDrawer_SpamClick_Safety()
        {
            // 1. 테스트 오브젝트 및 뷰 컴포넌트 생성
            var viewGo = new GameObject("TestDebugView", typeof(RectTransform), typeof(BePex.EventSystem.ViewsDebug.EventDebugView));
            var view = viewGo.GetComponent<BePex.EventSystem.ViewsDebug.EventDebugView>();

            // 2. 드로어 패널 모킹용 자식 RectTransform 생성
            var panelGo = new GameObject("DrawerPanel", typeof(RectTransform));
            panelGo.transform.SetParent(viewGo.transform);
            var panelRect = panelGo.GetComponent<RectTransform>();

            // 3. 리플렉션으로 드로어 패널 및 멤버 정보 바인딩
            var panelField = typeof(BePex.EventSystem.ViewsDebug.EventDebugView).GetField("m_drawerPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (panelField != null)
            {
                panelField.SetValue(view, panelRect);
            }
            
            var widthField = typeof(BePex.EventSystem.ViewsDebug.EventDebugView).GetField("m_drawerWidth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (widthField != null)
            {
                widthField.SetValue(view, 100f);
            }

            var durationField = typeof(BePex.EventSystem.ViewsDebug.EventDebugView).GetField("m_slideDuration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (durationField != null)
            {
                durationField.SetValue(view, 0.1f);
            }

            // 4. 뷰모델 바이패스 셋업 및 바인딩
            var conditionRegistry = ScriptableObject.CreateInstance<ConditionTypeRegistrySO>();
            var debugVM = new EventDebugViewModel(m_model, m_saveSystem, m_timeProvider, m_playerReward, m_hudVM, conditionRegistry);
            view.Bind(debugVM);

            // 5. 프레임을 나누어 슬라이드 애니메이션 스팸 클릭 시뮬레이션
            bool hasException = false;
            try
            {
                // 연속 토글 호출 (0.01초 간격으로 스팸 클릭 모사)
                for (int i = 0; i < 5; i++)
                {
                    view.func_ToggleDrawer();
                }
            }
            catch (System.Exception ex)
            {
                hasException = true;
                Debug.LogError($"[EventIntegrationTests] [Test_22] 스팸 클릭 시 오류 발생: {ex.Message}");
            }

            Assert.IsFalse(hasException, "[EventIntegrationTests] EventDebugView 토글 스팸 클릭 도중 널 참조 예외가 검출되었습니다.");

            // 0.2초간 플레이모드 대기하며 비동기 연출 가라앉을 때까지 대기
            yield return new WaitForSeconds(0.2f);

            // 6. 메모리 정리
            GameObject.Destroy(viewGo);
            ScriptableObject.DestroyImmediate(conditionRegistry);
            
            Debug.Log("[EventIntegrationTests] Test_22 완료: 스팸 클릭 토글 시 NullReferenceException 누출 방지 입증.");
        }
    }
}
