using BePex.EventSystem.Models;
using BePex.EventSystem.Data;

namespace BePex.EventSystem.Rewards
{
    /// <summary>
    /// [기능]: 플레이어 자산에 이벤트 완료 보상으로 경험치를 부여해 주는 Strategy 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    [EventReward(RewardDefinitionSO.RewardType.Exp)]
    public class ExpReward : BaseEventReward
    {
        #region 초기화
        /// <summary>
        /// [기능]: 부모 생성자를 경유해 지급할 경험치 수량 및 표시 이름을 주입받습니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: BaseEventReward 상속을 활용해 단순화
        /// </summary>
        public ExpReward(int amount, string displayName)
            : base(amount, displayName)
        {
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 플레이어의 누적 경험치에 보상 수량을 더해 지급합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public override void Grant(PlayerRewardModel playerReward)
        {
            if (playerReward != null)
            {
                playerReward.totalExp += m_amount;
            }
        }
        #endregion
    }
}
