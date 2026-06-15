using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BePex.EventSystem.ViewModels;

namespace BePex.EventSystem.Views
{
    /// <summary>
    /// [기능]: 상시 화면 상단에 노출되어 플레이어의 골드(Point), 경험치(Exp), 티켓(Ticket), 시즌포인트(SeasonPoint), 재화(Credits) 보유량을 보여주는 UI 뷰 컴포넌트.
    /// [작성자]: 윤승종
    /// </summary>
    public class CurrencyHUDView : MonoBehaviour
    {
        #region UI 참조 (Inspector)
        [Header("재화 텍스트 참조 슬롯")]
        [SerializeField] private TextMeshProUGUI m_goldText;
        [SerializeField] private TextMeshProUGUI m_expText;
        [SerializeField] private TextMeshProUGUI m_ticketText;
        [SerializeField] private TextMeshProUGUI m_seasonPointText;
        [SerializeField] private TextMeshProUGUI m_creditText;
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
        #endregion

        #region 유니티 생명주기
        /// <summary>
        /// [기능]: 오브젝트 파괴 시 뷰모델의 재화 변경 이벤트 리스너를 안전하게 해제합니다.
        /// [작성자]: 윤승종
        /// </summary>
        private void OnDestroy()
        {
            if (m_viewModel != null)
            {
                m_viewModel.OnCurrencyChanged -= RefreshUI;
            }
        }
        #endregion

        #region 내부 갱신 메서드
        /// <summary>
        /// [기능]: 뷰모델의 재화 상태를 각 텍스트 컴포넌트에 실시간 렌더링(포맷팅) 대입합니다. Fake Null 및 안전 널체크 준수.
        /// [작성자]: 윤승종
        /// </summary>
        private void RefreshUI()
        {
            if (m_viewModel == null)
            {
                return;
            }

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

            if (m_seasonPointText != null)
            {
                m_seasonPointText.text = m_viewModel.TotalSeasonPoints.ToString();
            }

            if (m_creditText != null)
            {
                m_creditText.text = m_viewModel.TotalCredits.ToString();
            }
        }
        #endregion
    }
}
