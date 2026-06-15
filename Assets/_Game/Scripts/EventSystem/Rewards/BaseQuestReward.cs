/// <summary>
/// [기능]: 공통적인 퀘스트 보상 정보 관리 및 조회를 제공하는 추상 기반 클래스.
/// [작성자]: 윤승종
/// </summary>

using BePex.EventSystem.Interfaces;
using BePex.EventSystem.Models;

namespace BePex.EventSystem.Rewards
{
    public abstract class BaseQuestReward : IQuestReward
    {
        #region 내부 필드
        protected readonly int m_amount;
        protected readonly string m_displayName;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 지급할 퀘스트 보상 수량 및 표시 이름을 주입받는 생성자.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 명칭 변경
        /// </summary>
        protected BaseQuestReward(int amount, string displayName)
        {
            m_amount = amount;
            m_displayName = displayName;
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 퀘스트 보상의 노출 이름을 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 명칭 변경
        /// </summary>
        public string GetRewardName()
        {
            return m_displayName;
        }

        /// <summary>
        /// [기능]: 퀘스트 보상 수량을 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 명칭 변경
        /// </summary>
        public int GetRewardAmount()
        {
            return m_amount;
        }

        /// <summary>
        /// [기능]: 플레이어에게 퀘스트 보상을 수령 및 적립하도록 파생 클래스에 위임하는 가상 비즈니스 추상 메서드.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 명칭 변경
        /// </summary>
        public abstract void Grant(PlayerRewardModel playerReward);
        #endregion
    }
}
