

using System.Collections.Generic;
using UnityEngine;
using BePex.EventSystem.ViewModels;
using System.Threading;

namespace BePex.EventSystem.Views
{
    /// <summary>
    /// [기능]: 뷰모델의 목록 상태 변경을 관찰하여 UI 상에 활성 이벤트를 동적 생성하고 갱신하는 리스트 뷰 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    public class EventListView : MonoBehaviour
    {
        #region UI 참조 (Inspector)
        [SerializeField] private Transform m_cellContainer;
        [SerializeField] private GameObject m_cellPrefab;
        #endregion

        #region 내부 필드
        private EventListViewModel m_viewModel;
        private readonly List<EventItemCell> m_spawnedCells = new List<EventItemCell>();
        private CancellationTokenSource m_cts;
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 뷰모델을 주입받아 리스트 갱신 알림 액션을 구독 바인딩하고 첫 화면 드로우를 지시합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public void Bind(EventListViewModel viewModel)
        {
            m_viewModel = viewModel;
            if (m_viewModel != null)
            {
                m_viewModel.OnListUpdated += func_OnListUpdated;
            }

            func_OnListUpdated();
        }
        #endregion

        #region 유니티 생명주기
        private void Awake()
        {
            m_cts = new CancellationTokenSource();
        }

        /// <summary>
        /// [기능]: 리스트 뷰 오브젝트 소멸 시 뷰모델 데이터 변경 리스너를 일제히 구독 해제합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: CancellationTokenSource 정리 추가
        /// </summary>
        private void OnDestroy()
        {
            if (m_viewModel != null)
            {
                m_viewModel.OnListUpdated -= func_OnListUpdated;
            }

            if (m_cts != null)
            {
                m_cts.Cancel();
                m_cts.Dispose();
                m_cts = null;
            }
        }
        #endregion

        #region UI 이벤트 핸들러 및 갱신
        /// <summary>
        /// [기능]: 뷰모델 상태가 변경되었을 때 호출되어 기존 인스턴스 셀들을 정리하고 재생성합니다. for 루프 사용 및 Unity Object 널 조건부 회피 규칙 준수.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public void func_OnListUpdated()
        {
            if (m_viewModel == null)
            {
                return;
            }

            // 기존 동적 생성 셀 파괴
            for (int i = 0; i < m_spawnedCells.Count; i++)
            {
                if (m_spawnedCells[i] != null)
                {
                    Destroy(m_spawnedCells[i].gameObject);
                }
            }
            m_spawnedCells.Clear();

            var list = m_viewModel.GetEvents();
            if (list == null)
            {
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == null || m_cellPrefab == null || m_cellContainer == null)
                {
                    continue;
                }

                var go = Instantiate(m_cellPrefab, m_cellContainer);
                if (go != null)
                {
                    var cell = go.GetComponent<EventItemCell>();
                    if (cell != null)
                    {
                        cell.Setup(list[i], m_viewModel);
                        m_spawnedCells.Add(cell);
                    }
                }
            }
        }
        #endregion
    }
}
