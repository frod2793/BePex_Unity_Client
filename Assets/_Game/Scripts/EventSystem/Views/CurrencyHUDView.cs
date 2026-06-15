using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BePex.EventSystem.ViewModels;

namespace BePex.EventSystem.Views
{
    /// <summary>
    /// [기능]: 상시 화면 상단에 노출되어 플레이어의 골드(Point), 경험치(Exp), 티켓(Ticket) 보유량을 보여주는 UI 뷰 컴포넌트.
    /// [작성자]: 윤승종
    /// </summary>
    public class CurrencyHUDView : MonoBehaviour
    {
        #region UI 참조 (Inspector)
        [Header("Currency Text References")]
        [SerializeField] private TextMeshProUGUI m_goldText;
        [SerializeField] private TextMeshProUGUI m_expText;
        [SerializeField] private TextMeshProUGUI m_ticketText;
        #endregion

        #region 내부 필드
        private CurrencyHUDViewModel m_viewModel;
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 뷰모델을 주입받아 이벤트를 구독하고 초기 UI 갱신을 실행합니다.
        /// [작성자]: 윤승종
        /// </summary>
        public void Bind(CurrencyHUDViewModel viewModel)
        {
            m_viewModel = viewModel;
            if (m_viewModel != null)
            {
                m_viewModel.OnCurrencyChanged += RefreshUI;
            }
            RefreshUI();
        }

        private void OnDestroy()
        {
            if (m_viewModel != null)
            {
                m_viewModel.OnCurrencyChanged -= RefreshUI;
            }
        }
        #endregion

        #region UI 갱신
        /// <summary>
        /// [기능]: 뷰모델로부터 최신 재화 상태를 얻어와 텍스트를 업데이트합니다.
        /// [작성자]: 윤승종
        /// </summary>
        private void RefreshUI()
        {
            if (m_viewModel == null) return;

            if (m_goldText != null)
            {
                m_goldText.text = m_viewModel.TotalPoints.ToString();
            }
            if (m_expText != null)
            {
                m_expText.text = m_viewModel.TotalExp.ToString();
            }
            if (m_ticketText != null)
            {
                m_ticketText.text = m_viewModel.TotalTickets.ToString();
            }
        }
        #endregion
    }
}
