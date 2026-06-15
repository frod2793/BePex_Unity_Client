using System;
using BePex.EventSystem.Models;

namespace BePex.EventSystem.ViewModels
{
    /// <summary>
    /// [기능]: 플레이어 재화보유 현황 UI(Currency HUD)를 위한 뷰모델 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    public class CurrencyHUDViewModel
    {
        #region 내부 필드
        private readonly PlayerRewardModel m_playerReward;
        #endregion

        #region 이벤트
        public event Action OnCurrencyChanged;
        #endregion

        #region 공개 프로퍼티
        public int TotalExp => m_playerReward != null ? m_playerReward.totalExp : 0;
        public int TotalTickets => m_playerReward != null ? m_playerReward.totalTickets : 0;
        public int TotalPoints => m_playerReward != null ? m_playerReward.totalPoints : 0;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: PlayerRewardModel 인스턴스를 주입받아 데이터 공급원으로 활용합니다.
        /// [작성자]: 윤승종
        /// </summary>
        public CurrencyHUDViewModel(PlayerRewardModel playerReward)
        {
            m_playerReward = playerReward;
        }
        #endregion

        #region 퍼블릭 메소드
        /// <summary>
        /// [기능]: 재화 데이터가 변경되었음을 뷰 단에 통지하여 UI 리프레시를 트리거합니다.
        /// [작성자]: 윤승종
        /// </summary>
        public void NotifyCurrencyChanged()
        {
            OnCurrencyChanged?.Invoke();
        }
        #endregion
    }
}
