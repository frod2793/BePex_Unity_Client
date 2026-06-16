using BePex.EventSystem.Models;
using BePex.EventSystem.Data;

namespace BePex.EventSystem.Rewards
{
    /// <summary>
    /// [기능]: 플레이어 자산에 퀘스트 완료 보상으로 경험치를 부여해 주는 Strategy 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    [QuestReward("Exp")]
    public class ExpQuestReward : BaseQuestReward
    {
        #region 초기화
        /// <summary>
        /// [기능]: 부모 생성자를 경유해 지급할 경험치 수량 및 표시 이름을 주입받습니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Quest-related 네이밍 적용
        /// </summary>
        public ExpQuestReward(int amount, string displayName)
            : base(amount, displayName)
        {
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 플레이어의 누적 경험치에 보상 수량을 더해 지급합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 퀘스트 명칭 변경
        /// </summary>
        public override void Grant(PlayerRewardModel playerReward)
        {
            if (playerReward != null)
            {
                playerReward.AddCurrency("Exp", m_amount);
            }
        }
        #endregion
    }
}
