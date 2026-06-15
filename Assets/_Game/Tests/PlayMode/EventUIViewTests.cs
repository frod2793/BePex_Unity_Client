using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using BePex.EventSystem.Views;
using BePex.EventSystem.ViewModels;
using BePex.EventSystem.Models;
using BePex.EventSystem.DTOs;
using BePex.EventSystem.Factories;
using BePex.EventSystem.Infrastructure;

namespace BePex.EventSystem.Tests.PlayMode
{
    /// <summary>
    /// [기능]: 씬 내에서 UI 뷰(EventListView, EventDetailView)들이 ViewModel과 바인딩되고 상호작용하는지 검증하는 PlayMode 테스트.
    /// [작성자]: 윤승종
    /// </summary>
    public class EventUIViewTests
    {
        private GameObject m_rootGO;
        private EventListView m_listView;
        private EventDetailView m_detailView;
        private EventListViewModel m_listVM;
        private EventDetailViewModel m_detailVM;
        private EventModel m_model;
        private InMemorySaveSystem m_saveSystem;

        /// <summary>
        /// [기능]: 의존성 수동 조립 및 테스트 대상 씬 환경 셋업.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: ConditionFactory 및 EventModel 생성자에 ITimeProvider(SystemTimeProvider) 의존성 주입 추가
        /// </summary>
        [SetUp]
        public void Setup()
        {
            m_rootGO = new GameObject("TestRoot");

            // View 컴포넌트 추가
            var listGO = new GameObject("ListView");
            listGO.transform.SetParent(m_rootGO.transform);
            m_listView = listGO.AddComponent<EventListView>();

            var detailGO = new GameObject("DetailView");
            detailGO.transform.SetParent(m_rootGO.transform);
            m_detailView = detailGO.AddComponent<EventDetailView>();

            // Mock Prefab 및 Container 생성
            var mockCellPrefab = new GameObject("MockCellPrefab");
            mockCellPrefab.AddComponent<EventItemCell>();
            mockCellPrefab.SetActive(false);
            
            var mockContainer = new GameObject("MockContainer").transform;
            mockContainer.SetParent(listGO.transform);

            // Reflection으로 EventListView에 의존성 강제 주입
            var listViewType = typeof(EventListView);
            var prefabField = listViewType.GetField("m_cellPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var containerField = listViewType.GetField("m_cellContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            prefabField?.SetValue(m_listView, mockCellPrefab);
            containerField?.SetValue(m_listView, mockContainer);

            Debug.Log($"[EventUIViewTests] Field Init - prefabField null? {prefabField == null}, containerField null? {containerField == null}");
            Debug.Log($"[EventUIViewTests] Field Value - m_cellPrefab: {prefabField?.GetValue(m_listView)}, m_cellContainer: {containerField?.GetValue(m_listView)}");

            // 의존성 수동 조립
            m_saveSystem = new InMemorySaveSystem();
            var timeProvider = new SystemTimeProvider();
            var condFactory = new ConditionFactory(m_saveSystem, timeProvider);
            var rewFactory = new RewardFactory();

            var tableDTO = new EventTableDTO();
            tableDTO.events.Add(new EventDefinitionDTO
            {
                eventId = "evt_ui_test",
                eventTitle = "UI 바인딩 테스트",
                condition = new ConditionDefinitionDTO { conditionType = "KillCount", targetValue = 10 }
            });

            m_model = new EventModel(tableDTO, condFactory, rewFactory, timeProvider);
            m_listVM = new EventListViewModel(m_model, m_saveSystem);
            var playerReward = new PlayerRewardModel();
            m_detailVM = new EventDetailViewModel(m_model, m_saveSystem, playerReward);

            // Bind
            m_listView.Bind(m_listVM);
            
            // EventDetailView는 Bind(DetailVM, ListVM, PopupVM)을 받으므로, PopupVM은 임시로 null 전달 (사용 안 함)
            m_detailView.Bind(m_detailVM, m_listVM, null);
        }

        [TearDown]
        public void Teardown()
        {
            m_listVM.Dispose();
            m_detailVM.Dispose();
            if (m_rootGO != null)
            {
                Object.Destroy(m_rootGO);
            }
        }

        /// <summary>
        /// [기능]: PlayMode 1. EventListView 바인딩 검증
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        [UnityTest]
        public IEnumerator Test_UI_ListView_Binding()
        {
            // ListView 갱신 로직이 MonoBehaviour Start 이후 또는 Bind 직후 일어날 수 있음.
            // 여기선 1프레임 대기 후 검증
            yield return null;

            // ListView 내부에 EventItemCell이 몇 개 생성되었는지 확인하기 위해 자식 수를 센다.
            // EventListView.cs의 구현에 따라 다를 수 있으나 보통 Content Transform 밑에 생성.
            // public 속성이 없으므로, m_listView 하위의 모든 EventItemCell 컴포넌트를 찾는다.
            var cells = m_listView.GetComponentsInChildren<EventItemCell>(true);
            
            Debug.Log($"[EventUIViewTests] [Test_UI_ListView_Binding] " +
                      $"입력값: 1개의 이벤트를 가진 ListVM 바인딩 | " +
                      $"출력값: 생성된 EventItemCell 개수={cells.Length}");

            Assert.AreEqual(1, cells.Length, "이벤트 목록에 맞게 셀이 생성되어야 합니다.");
        }

        /// <summary>
        /// [기능]: PlayMode 2. DetailView 업데이트 검증
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        [UnityTest]
        public IEnumerator Test_UI_DetailView_Update()
        {
            yield return null;

            // 이벤트를 선택
            m_listVM.SelectEvent("evt_ui_test");
            
            yield return null; // 이벤트 전파 대기

            // DetailVM에 "evt_ui_test"가 세팅되었는지 확인
            var def = m_detailVM.GetEventDefinition();
            
            Debug.Log($"[EventUIViewTests] [Test_UI_DetailView_Update] " +
                      $"입력값: evt_ui_test 선택 명령 | " +
                      $"출력값: DetailVM의 활성 이벤트 ID={def?.eventId}");

            Assert.IsNotNull(def);
            Assert.AreEqual("evt_ui_test", def.eventId);
        }

        /// <summary>
        /// [기능]: PlayMode 3. 보상 수령 시뮬레이션
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        [UnityTest]
        public IEnumerator Test_UI_RewardClaim_Simulation()
        {
            yield return null;

            m_listVM.SelectEvent("evt_ui_test");
            yield return null;

            bool isProgressDone = false;
            System.Action addProgress = async () =>
            {
                await m_model.Debug_AddProgressAsync("evt_ui_test", 10, m_saveSystem);
                isProgressDone = true;
            };
            addProgress();
            while (!isProgressDone) yield return null;

            yield return null;

            // ViewModel의 CanClaimRewardAsync 확인
            bool isClaimDone = false;
            bool canClaim = false;
            System.Action checkClaim = async () =>
            {
                canClaim = await m_detailVM.CanClaimRewardAsync();
                isClaimDone = true;
            };
            checkClaim();
            while (!isClaimDone) yield return null;

            Debug.Log($"[EventUIViewTests] [Test_UI_RewardClaim_Simulation] " +
                      $"입력값: 조건 달성 후 보상 수령 여부 확인 | " +
                      $"출력값: 수령 가능 상태={canClaim}");

            Assert.IsTrue(canClaim, "목표치를 달성했으므로 보상 수령이 가능해야 합니다.");
        }
    }
}
