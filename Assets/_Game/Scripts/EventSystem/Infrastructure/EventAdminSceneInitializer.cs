using UnityEngine;
using BePex.EventSystem.Views;
using BePex.EventSystem.ViewModels;
using BePex.EventSystem.DTOs;
using BePex.EventSystem.Data;
using UnityEngine.AddressableAssets;
using System.IO;
using System.Threading;

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

        #region 내부 필드 (Private Fields)
        private CancellationTokenSource m_cts;
        #endregion

        #region 유니티 생명주기
        private void Awake()
        {
            m_cts = new CancellationTokenSource();
        }

        /// <summary>
        /// [기능]: 씬 기동 시 수동 비동기 초기화를 트리거합니다.
        /// [작성자]: 윤승종
        /// </summary>
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
                Debug.Log("[EventAdminSceneInitializer] 씬 초기화 프로세스가 정상적으로 취소되었습니다.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[EventAdminSceneInitializer] 초기화 도중 예외가 발생했습니다: {ex.Message}");
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
        }
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 데이터 파일(로컬 JSON 파일 또는 Addressables)을 로드하고 뷰와 뷰모델, 모킹 서비스를 인스턴싱 및 수동 DI 결합합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 취소 제어를 위한 CancellationToken 매개변수 도입 및 전달 연계
        /// </summary>
        private async Awaitable InitializeAsync(CancellationToken cancellationToken)
        {
            EventTableDTO eventTableDTO = null;

            cancellationToken.ThrowIfCancellationRequested();

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

                        cancellationToken.ThrowIfCancellationRequested();

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

#if UNITY_EDITOR
            // 에디터 상에서 저장 완료 시 에셋 데이터베이스 새로고침 및 어드레서블 자산 자동 재빌드 연동
            adminVM.OnSaveCompleted += (success) =>
            {
                if (success)
                {
                    UnityEditor.AssetDatabase.Refresh();
                    Debug.Log("[EventAdminSceneInitializer] 에디터 에셋 데이터베이스가 성공적으로 새로고침되었습니다.");
                    RebuildAddressablesOnEditor();
                }
            };
#endif

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

#if UNITY_EDITOR
        /// <summary>
        /// [기능]: 에디터 재생 환경에서 event_table JSON 에셋의 변경 사항을 어드레서블 로컬 빌드 데이터에 강제 반영합니다. (리플렉션을 통한 에디터 컴파일 의존성 제거)
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 컴파일 CS0234 에러 방지를 위해 리플렉션 동적 호출 적용
        /// </summary>
        private void RebuildAddressablesOnEditor()
        {
            try
            {
                var settingsDefaultObjectType = System.Type.GetType("UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject, Unity.Addressables.Editor");
                if (settingsDefaultObjectType == null)
                {
                    return;
                }

                var settingsProperty = settingsDefaultObjectType.GetProperty("Settings", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (settingsProperty == null)
                {
                    return;
                }

                var settings = settingsProperty.GetValue(null);
                if (settings == null)
                {
                    return;
                }

                var settingsType = System.Type.GetType("UnityEditor.AddressableAssets.Settings.AddressableAssetSettings, Unity.Addressables.Editor");
                if (settingsType == null)
                {
                    return;
                }

                var buildMethod = settingsType.GetMethod("BuildPlayerContent", new System.Type[] { settingsType });
                if (buildMethod != null)
                {
                    buildMethod.Invoke(null, new object[] { settings });
                    Debug.Log("[EventAdminSceneInitializer] Addressables 빌드 컨텐츠가 자동으로 재빌드되었습니다.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[EventAdminSceneInitializer] Addressables 자동 빌드 중 오류 발생 (무시 가능): {ex.Message}");
            }
        }
#endif
        #endregion
    }
}
