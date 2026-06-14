using UnityEngine;
using UnityEngine.AddressableAssets;
using BePex.EventSystem.Views;
using BePex.EventSystem.ViewsDebug;
using BePex.EventSystem.Data;
using BePex.EventSystem.Factories;
using BePex.EventSystem.Models;
using BePex.EventSystem.ViewModels;
using BePex.EventSystem.ViewModelsDebug;
using BePex.EventSystem.Interfaces;
using BePex.EventSystem.DTOs;

namespace BePex.EventSystem.Infrastructure
{
    /// <summary>
    /// [기능]: 이벤트 센터 프로덕션 및 디버그 통합 씬의 모든 POCO 객체의 라이프사이클 조립 및 MVVM 바인딩 의존성을 해소해 주는 Composition Root.
    /// [작성자]: 윤승종
    /// </summary>
    public class EventSceneInitializer : MonoBehaviour
    {
        #region UI 참조 (Inspector)
        [Header("UI 뷰 목록")]
        [SerializeField] private EventListView m_eventListView;
        [SerializeField] private EventDetailView m_eventDetailView;
        [SerializeField] private RewardPopupView m_rewardPopupView;

        [Header("디버그 뷰 (선택 사항)")]
        [SerializeField] private EventDebugView m_debugView;
        [SerializeField] private bool m_useDebugMode = false;
        #endregion

        #region 데이터 참조 (Inspector)
        [Header("어드레서블 로딩 설정")]
        [SerializeField] private string m_eventJsonAddress = "EventTableJson";
        #endregion

        #region 유니티 생명주기
        private async void Start()
        {
            await InitializeAsync();
        }
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 비즈니스 로직 및 ViewModel 인스턴스를 순차 수동 DI 생성 조립하고 각 View에 바인드합니다. 어드레서블을 통해 비동기로 테이블을 다운로드합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Addressables 기반 비동기 로딩 및 Awaitable 대응
        /// </summary>
        private async Awaitable InitializeAsync()
        {
            // 1단계: 디버그 옵션에 따른 저장 장치 선택 (임시 메모리 vs 로컬 디스크)
            ISaveSystem saveSystem = (m_useDebugMode && m_debugView != null) 
                ? new InMemorySaveSystem() 
                : new JsonSaveSystem();

            // 2단계: Addressables 기반 데이터 로딩 (JSON TextAsset)
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

            // 3단계: 조건 및 보상 Factory 생성
            var condFactory = new ConditionFactory(saveSystem);
            var rewFactory = new RewardFactory();

            // 4단계: 비동기로 유저 누적 보상 정보 로드
            var playerReward = await saveSystem.LoadRewardStateAsync();

            // 5단계: 도메인 Domain Model 생성 
            var eventModel = new EventModel(eventTableDTO, condFactory, rewFactory);

            // 6단계: MVVM ViewModels 수동 생성자 주입 생성
            var listVM = new EventListViewModel(eventModel);
            var detailVM = new EventDetailViewModel(eventModel, saveSystem);
            var popupVM = new RewardPopupViewModel(playerReward, saveSystem);

            // 7단계: Views에 ViewModels 바인딩 연결
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

            // 8단계: [디버그 전용] 디버그 모드가 켜져 있고 조작 뷰가 연결된 경우 바인딩 처리
            if (m_useDebugMode && m_debugView != null)
            {
                var debugVM = new EventDebugViewModel(eventModel, saveSystem);
                m_debugView.Bind(debugVM);
                m_debugView.gameObject.SetActive(true);
            }
            else if (m_debugView != null)
            {
                m_debugView.gameObject.SetActive(false);
            }

            Debug.Log("[EventSceneInitializer] Addressables 기반 비동기 씬 초기화 완료.");
        }
        #endregion
    }
}
