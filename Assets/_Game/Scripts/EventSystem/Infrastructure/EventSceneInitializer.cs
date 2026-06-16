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
using System.IO;
using System.Threading;

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
        [SerializeField] private CurrencyHUDView m_currencyHUDView;

        [Header("디버그 뷰 (선택 사항)")]
        [SerializeField] private EventDebugView m_debugView;
        [SerializeField] private bool m_useDebugMode = false;
        [SerializeField] private ConditionTypeRegistrySO m_conditionTypeRegistry;
        #endregion

        #region 데이터 참조 (Inspector)
        [Header("어드레서블 로딩 설정")]
        [SerializeField] private string m_eventJsonAddress = "EventTableJson";
        #endregion

        #region 내부 필드 (Private Fields)
        private CancellationTokenSource m_cts;
        private EventListViewModel m_listViewModel;
        private EventDetailViewModel m_detailViewModel;
        private EventDebugViewModel m_debugViewModel;
        #endregion

        #region 유니티 생명주기
        private void Awake()
        {
            m_cts = new CancellationTokenSource();
        }

        private async void Start()
        {
            try
            {
                if (m_cts != null)
                {
                    await InitializeAsync(m_cts.Token);
                }
            }
            catch (System.OperationCanceledException)
            {
                Debug.Log("[EventSceneInitializer] 씬 초기화 프로세스가 정상적으로 취소되었습니다.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[EventSceneInitializer] 초기화 도중 예외가 발생했습니다: {ex.Message}");
            }
        }

        private void OnDestroy()
        {
            if (m_cts != null)
            {
                m_cts.Cancel();
                m_cts.Dispose();
                m_cts = null;
            }

            if (m_listViewModel != null)
            {
                m_listViewModel.Dispose();
                m_listViewModel = null;
            }

            if (m_detailViewModel != null)
            {
                m_detailViewModel.Dispose();
                m_detailViewModel = null;
            }

            if (m_debugViewModel is System.IDisposable disposableDebugVM)
            {
                disposableDebugVM.Dispose();
                m_debugViewModel = null;
            }
        }
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 비즈니스 로직 및 ViewModel 인스턴스를 순차 수동 DI 생성 조립하고 각 View에 바인드합니다. 
        ///         원격 카탈로그 업데이트가 존재할 경우 다운로드 패치 후 테이블 데이터를 로드합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 어드레서블 런타임 원격 핫패치 체크 및 persistentDataPath 우선 로딩 정책 적용
        /// </summary>
        private async Awaitable InitializeAsync(CancellationToken cancellationToken)
        {
            // 1단계: 저장 장치 및 시간 제공자 선택 (디버그 모드와 관계없이 JsonSaveSystem을 기본 사용하여 씬 재시작 시 상태 유지)
            ISaveSystem rawSaveSystem = new JsonSaveSystem();
            ISaveSystem cachedSaveSystem = new CachedSaveSystem(rawSaveSystem);
            ISaveSystem retrySaveSystem = new RetrySaveSystemDecorator(cachedSaveSystem);
            ISaveSystem saveSystem = new TransactionalSaveSystemDecorator(retrySaveSystem);
                
            ITimeProvider timeProvider = (m_useDebugMode && m_debugView != null)
                ? new BePex.EventSystem.Infrastructure.DebugTimeProvider()
                : new BePex.EventSystem.Infrastructure.SystemTimeProvider();

            cancellationToken.ThrowIfCancellationRequested();

            // 1.5단계: 실시간 어드레서블 원격 카탈로그 및 다운로드 업데이트 체크
            await UpdateAddressableCatalogAsync(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            // 2단계: 최신 공유 세이브 경로 파일(1순위) 또는 어드레서블 에셋 번들(2순위 폴백) 데이터 로딩
            EventTableDTO eventTableDTO = null;
            string sharedPath = System.IO.Path.Combine(Application.persistentDataPath, "event_table.json");

            if (System.IO.File.Exists(sharedPath))
            {
                try
                {
                    string json = System.IO.File.ReadAllText(sharedPath);
                    eventTableDTO = JsonUtility.FromJson<EventTableDTO>(json);
                    Debug.Log($"[EventSceneInitializer] 공유 persistentDataPath에서 최신 이벤트 테이블을 로드했습니다: {sharedPath}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[EventSceneInitializer] 공유 폴더 데이터 로딩 실패, 어드레서블 시도: {ex.Message}");
                }
            }

            if (eventTableDTO == null && !string.IsNullOrEmpty(m_eventJsonAddress))
            {
                try
                {
                    var handle = Addressables.LoadAssetAsync<TextAsset>(m_eventJsonAddress);
                    TextAsset jsonAsset = await handle.Task;

                    cancellationToken.ThrowIfCancellationRequested();

                    if (jsonAsset != null)
                    {
                        eventTableDTO = JsonUtility.FromJson<EventTableDTO>(jsonAsset.text);
                        Debug.Log("[EventSceneInitializer] 어드레서블 에셋으로부터 최신 이벤트 테이블을 로드했습니다.");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[EventSceneInitializer] 어드레서블 에셋 로드 최종 실패: {ex.Message}");
                }
            }

            if (eventTableDTO == null)
            {
                Debug.LogError("[EventSceneInitializer] 데이터 테이블 로드에 최종 실패했습니다.");
                return;
            }

            // 3단계: 조건 및 보상 Factory 생성
            var condFactory = new QuestConditionFactory(saveSystem, timeProvider);
            var rewFactory = new QuestRewardFactory();

            // 4단계: 비동기로 유저 누적 보상 정보 로드
            var playerReward = await saveSystem.LoadRewardStateAsync(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            // 5단계: 도메인 Domain Model 생성 
            var eventModel = new EventModel(eventTableDTO, condFactory, rewFactory, timeProvider);

            // 6단계: MVVM ViewModels 수동 생성자 주입 생성
            m_listViewModel = new EventListViewModel(eventModel, saveSystem);
            m_detailViewModel = new EventDetailViewModel(eventModel, saveSystem, playerReward);
            var popupVM = new RewardPopupViewModel(playerReward, saveSystem, eventModel);
            var hudVM = new CurrencyHUDViewModel(playerReward);

            // 보상 데이터 변경에 따른 상단 HUD 동기화 이벤트 체이닝
            if (popupVM != null && hudVM != null)
            {
                popupVM.OnRewardDataChanged += hudVM.NotifyCurrencyChanged;
            }

            // 7단계: Views에 ViewModels 바인딩 연결
            if (m_eventListView != null)
            {
                m_eventListView.Bind(m_listViewModel);
            }

            if (m_eventDetailView != null)
            {
                m_eventDetailView.Bind(m_detailViewModel, m_listViewModel, popupVM);
            }

            if (m_rewardPopupView != null)
            {
                m_rewardPopupView.Bind(popupVM, m_detailViewModel);
            }

            if (m_currencyHUDView != null)
            {
                m_currencyHUDView.Bind(hudVM);
            }

            // 8단계: [디버그 전용] 디버그 모드가 켜져 있고 조작 뷰가 연결된 경우 바인딩 처리
            if (m_useDebugMode && m_debugView != null)
            {
                m_debugViewModel = new EventDebugViewModel(eventModel, saveSystem, timeProvider, playerReward, hudVM, m_conditionTypeRegistry);
                m_debugView.Bind(m_debugViewModel);
                m_debugView.gameObject.SetActive(true);
            }
            else if (m_debugView != null)
            {
                m_debugView.gameObject.SetActive(false);
            }

            Debug.Log("[EventSceneInitializer] Addressables 기반 비동기 씬 초기화 완료.");
        }

        /// <summary>
        /// [기능]: 원격 어드레서블 서버의 카탈로그 업데이트를 체크하고 갱신된 에셋 번들을 자동 다운로드합니다. (예외 가드 탑재)
        /// [작성자]: 윤승종
        /// </summary>
        private async Awaitable UpdateAddressableCatalogAsync(CancellationToken cancellationToken)
        {
            try
            {
                Debug.Log("[EventSceneInitializer] 어드레서블 카탈로그 업데이트 검사를 시작합니다.");
                
                // 1. 카탈로그 업데이트 여부 비동기 확인
                var checkHandle = Addressables.CheckForCatalogUpdates(false);
                var catalogsToUpdate = await checkHandle.Task;

                cancellationToken.ThrowIfCancellationRequested();

                if (catalogsToUpdate != null && catalogsToUpdate.Count > 0)
                {
                    Debug.Log($"[EventSceneInitializer] {catalogsToUpdate.Count}개의 신규 어드레서블 카탈로그 변경 발견. 업데이트 진행...");
                    
                    // 2. 카탈로그 정보 갱신 적용
                    var updateHandle = Addressables.UpdateCatalogs(catalogsToUpdate, false);
                    await updateHandle.Task;

                    cancellationToken.ThrowIfCancellationRequested();

                    // 3. 갱신된 최신 에셋 디펜던시(번들) 자동 사전 다운로드
                    var downloadHandle = Addressables.DownloadDependenciesAsync(m_eventJsonAddress, true);
                    await downloadHandle.Task;
                    
                    Debug.Log("[EventSceneInitializer] 원격 카탈로그 핫패치 및 에셋 다운로드 완료.");
                }
                else
                {
                    Debug.Log("[EventSceneInitializer] 어드레서블 카탈로그가 이미 최신 상태입니다.");
                }
            }
            catch (System.OperationCanceledException)
            {
                throw;
            }
            catch (System.Exception ex)
            {
                // 인터넷 연결 부재나 서버 응답 에러 발생 시 부드러운 오프라인 폴백 허용
                Debug.LogWarning($"[EventSceneInitializer] 카탈로그 업데이트 검사 중 오류 발생 (로컬 캐시로 진행): {ex.Message}");
            }
        }
        #endregion
    }
}
