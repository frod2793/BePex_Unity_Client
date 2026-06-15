using UnityEngine;
using UnityEngine.UI;
using BePex.EventSystem.ViewModels;
using TMPro;

namespace BePex.EventSystem.Views
{
    /// <summary>
    /// [기능]: 보상 획득 연출 및 플레이어의 최종 자산 적립 통계를 표출하는 팝업 View 클래스.
    /// [작성자]: 윤승종
    /// </summary>  
    public class RewardPopupView : MonoBehaviour
    {
        #region UI 참조 (Inspector)
        [SerializeField] private GameObject m_popupRoot; // 팝업 UI의 최상위 루트 오브젝트
        [SerializeField] private TextMeshProUGUI m_earnedRewardsText; // 이번에 획득한 보상 목록 표시 텍스트
        [SerializeField] private TextMeshProUGUI m_cumulativeRewardsText; // 누적 보유 재화 목록 표시 텍스트
        [SerializeField] private Button m_closeButton; // 팝업을 닫는 버튼
        #endregion

        #region 내부 필드
        private RewardPopupViewModel m_viewModel;
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 보상 팝업 뷰모델과 상세 정보 뷰모델의 상태 변화 및 완료 성공 트리거 이벤트를 수신 등록합니다. Fake Null 우회 준수.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 뷰 통폐합 리팩토링 반영
        /// </summary>
        public void Bind(RewardPopupViewModel viewModel, EventDetailViewModel detailViewModel)
        {
            m_viewModel = viewModel;

            if (m_viewModel != null)
            {
                m_viewModel.OnRewardDataChanged += func_OnRewardDataChanged;
            }

            if (detailViewModel != null)
            {
                detailViewModel.OnRewardClaimSuccess += func_OnShowPopup;
            }

            if (m_closeButton != null)
            {
                m_closeButton.onClick.RemoveAllListeners();
                m_closeButton.onClick.AddListener(func_OnCloseClick);
            }

            if (m_popupRoot != null)
            {
                m_popupRoot.SetActive(false);
            }
        }
        #endregion

        #region 유니티 생명주기
        /// <summary>
        /// [기능]: 팝업이 파괴될 때 뷰모델 구독 리스너를 해제하여 리소스 누수를 차단합니다.
        /// [작성자]: 윤승종
        /// </summary>
        private void OnDestroy()
        {
            if (m_viewModel != null)
            {
                m_viewModel.OnRewardDataChanged -= func_OnRewardDataChanged;
            }
        }
        #endregion

        #region UI 이벤트 핸들러 및 갱신
        /// <summary>
        /// [기능]: 상세 화면으로부터 보상 수령 청구 성공 이벤트를 받아 팝업을 활성화하고 내용 갱신을 지시합니다. func_ 접두사 준수.
        /// [작성자]: 윤승종
        /// </summary>
        public void func_OnShowPopup(string eventId)
        {
            if (m_popupRoot != null)
            {
                m_popupRoot.SetActive(true);
            }

            if (m_viewModel != null)
            {
                m_viewModel.Refresh(eventId);
            }
        }

        /// <summary>
        /// [기능]: 뷰모델의 자산 상태 갱신 알림을 수신해 텍스트 요소를 갱신합니다. Fake Null 우회 준수.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 개별 텍스트 변수들을 m_cumulativeRewardsText 하나로 통합하여 효율화
        /// </summary>
        public void func_OnRewardDataChanged()
        {
            if (m_viewModel == null)
            {
                return;
            }

            if (m_cumulativeRewardsText != null)
            {
                m_cumulativeRewardsText.color = Color.black;
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("★ 누적 보유 재화:");
                sb.AppendLine($"- 경험치: {m_viewModel.TotalExp}");
                sb.AppendLine($"- 티켓: {m_viewModel.TotalTickets}");
                sb.AppendLine($"- 이벤트 포인트: {m_viewModel.TotalPoints}");
                sb.AppendLine($"- 시즌 포인트: {m_viewModel.TotalSeasonPoints}");
                sb.AppendLine($"- 재화: {m_viewModel.TotalCredits}");
                m_cumulativeRewardsText.text = sb.ToString();
            }

            if (m_earnedRewardsText != null)
            {
                m_earnedRewardsText.color = Color.black;
                var rewards = m_viewModel.GetClaimedRewards();
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("★ 이번에 획득한 보상:");
                if (rewards != null && rewards.Count > 0)
                {
                    for (int i = 0; i < rewards.Count; i++)
                    {
                        sb.AppendLine($"- {rewards[i].displayName} +{rewards[i].amount}");
                    }
                }
                else
                {
                    sb.AppendLine("- 없음");
                }
                m_earnedRewardsText.text = sb.ToString();
            }
        }

        /// <summary>
        /// [기능]: 닫기 버튼 선택 시 오버레이 루트 오브젝트를 비활성화합니다. func_ 접두사 준수.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public void func_OnCloseClick()
        {
            if (m_popupRoot != null)
            {
                m_popupRoot.SetActive(false);
            }
        }
        #endregion
    }
}
