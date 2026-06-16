using UnityEngine;
using BePex.EventSystem.Views;
using BePex.EventSystem.ViewModels;
using BePex.EventSystem.DTOs;
using BePex.EventSystem.Data;
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
        [SerializeField] private ConditionTypeRegistrySO m_conditionTypeRegistry;
        [SerializeField] private RewardTypeRegistrySO m_rewardTypeRegistry;
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

            if (eventTableDTO == null)
            {
                if (!string.IsNullOrEmpty(m_eventJsonAddress))
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
            }

            if (eventTableDTO == null)
            {
                eventTableDTO = new EventTableDTO();
                Debug.LogWarning("[EventAdminSceneInitializer] 신규 빈 이벤트 테이블 DTO 생성.");
            }

            var firebaseService = new MockFirebaseUploadService();
            var adminVM = new EventAdminViewModel(firebaseService, m_conditionTypeRegistry, m_rewardTypeRegistry);
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
