using UnityEngine;
using UnityEngine.UI;
using BePex.EventSystem.ViewModels;
using BePex.EventSystem.DTOs;
using TMPro;

namespace BePex.EventSystem.Views
{
    /// <summary>
    /// [기능]: 개별 퀘스트의 상세 정보(타이틀, 진행도 게이지 바, 보상 리스트) 및 보상 획득 조작 입력을 제어하는 뷰 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    public class EventQuestRowView : MonoBehaviour
    {
        #region UI 참조 (Inspector)
        [SerializeField] private TextMeshProUGUI m_questTitleText;
        [SerializeField] private TextMeshProUGUI m_progressText;
        [SerializeField] private Slider m_progressSlider;
        [SerializeField] private TextMeshProUGUI m_rewardText;
        [SerializeField] private Button m_claimButton;
        #endregion

        #region 내부 필드
        private EventDetailViewModel m_detailViewModel;
        private string m_questId;
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 퀘스트 정보 및 상세 화면 뷰모델을 받아와 UI 컴포넌트들을 바인딩하고 진행 상태를 업데이트합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public void Bind(QuestDefinitionDTO quest, EventDetailViewModel detailViewModel)
        {
            m_detailViewModel = detailViewModel;
            if (quest != null)
            {
                m_questId = quest.questId;

                if (m_questTitleText != null)
                {
                    m_questTitleText.text = quest.questTitle;
                }

                if (m_rewardText != null)
                {
                    var sb = new System.Text.StringBuilder();
                    if (quest.rewards != null && quest.rewards.Count > 0)
                    {
                        for (int i = 0; i < quest.rewards.Count; i++)
                        {
                            if (i > 0)
                            {
                                sb.Append(", ");
                            }
                            sb.Append($"{quest.rewards[i].displayName} x{quest.rewards[i].amount}");
                        }
                    }
                    else
                    {
                        sb.Append("보상 없음");
                    }
                    m_rewardText.text = sb.ToString();
                }
            }

            if (m_claimButton != null)
            {
                m_claimButton.onClick.RemoveAllListeners();
                m_claimButton.onClick.AddListener(func_OnClaimButtonClick);
            }

            func_UpdateProgressAsync();
        }
        #endregion

        #region 내부 메서드 및 이벤트 핸들러
        /// <summary>
        /// [기능]: 퀘스트의 실시간 진척도와 보상 획득 가능 유무 상태를 갱신합니다. Fake Null 우회 준수.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        private async void func_UpdateProgressAsync()
        {
            if (m_detailViewModel == null || string.IsNullOrEmpty(m_questId))
            {
                return;
            }

            var (cur, tar, ratio) = await m_detailViewModel.GetProgressInfoAsync(m_questId);

            if (m_progressText != null)
            {
                int percent = Mathf.RoundToInt(ratio * 100f);
                m_progressText.text = $"{cur} / {tar} ({percent}%)";
            }

            if (m_progressSlider != null)
            {
                m_progressSlider.minValue = 0f;
                m_progressSlider.maxValue = 1f;
                m_progressSlider.value = ratio;
            }

            bool claimed = await m_detailViewModel.IsRewardClaimedAsync(m_questId);
            bool canClaim = await m_detailViewModel.CanClaimRewardAsync(m_questId);

            if (m_claimButton != null)
            {
                m_claimButton.interactable = canClaim;
                var btnText = m_claimButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.text = claimed ? "수령 완료" : "보상 받기";
                }
            }
        }

        /// <summary>
        /// [기능]: 보상 받기 버튼을 눌렀을 때 뷰모델의 명령을 호출합니다. func_ 접두사 준수.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public async void func_OnClaimButtonClick()
        {
            if (m_detailViewModel != null && !string.IsNullOrEmpty(m_questId))
            {
                await m_detailViewModel.ClaimRewardAsync(m_questId);
            }
        }
        #endregion
    }
}
