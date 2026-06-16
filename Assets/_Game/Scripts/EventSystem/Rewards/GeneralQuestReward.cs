/// <summary>
/// [기능]: 재화 타입의 문자열 식별자를 직접 전달받아 PlayerRewardModel 딕셔너리에 동적으로 적립하는 범용 퀘스트 보상 클래스.
/// [작성자]: 윤승종
/// </summary>

using BePex.EventSystem.Models;

namespace BePex.EventSystem.Rewards
{
    public class GeneralQuestReward : BaseQuestReward
    {
        #region 내부 필드
        private readonly string m_typeName;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 보상 타입 식별자, 지급 수량, 표시 이름을 주입받는 생성자.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// </summary>
        public GeneralQuestReward(string typeName, int amount, string displayName)
            : base(amount, displayName)
        {
            m_typeName = typeName;
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 플레이어의 누적 자산에 보상 타입 식별자 문자열 키를 직접 적립합니다. (OCP 동적 획득 달성)
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// </summary>
        public override void Grant(PlayerRewardModel playerReward)
        {
            if (playerReward != null && !string.IsNullOrEmpty(m_typeName))
            {
                playerReward.AddCurrency(m_typeName, m_amount);
            }
        }
        #endregion
    }
}
