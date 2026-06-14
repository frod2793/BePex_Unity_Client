using System;
using BePex.EventSystem.Models;
using BePex.EventSystem.Interfaces;

namespace BePex.EventSystem.ViewModels
{
    /// <summary>
    /// [기능]: 보상 획득 완료 안내 및 플레이어의 최종 누적 자산 현황을 노출하는 ViewModel 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    public class RewardPopupViewModel
    {
        #region 내부 필드
        private readonly PlayerRewardModel m_playerReward;
        private readonly ISaveSystem m_saveSystem;
        #endregion

        #region 이벤트 (Observer)
        public event Action OnRewardDataChanged;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 플레이어 자산 데이터 모델 및 세이브 데이터 모듈을 생성자 주입을 통해 수신받습니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public RewardPopupViewModel(PlayerRewardModel playerReward, ISaveSystem saveSystem)
        {
            m_playerReward = playerReward;
            m_saveSystem = saveSystem;
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 플레이어 자산 데이터 모델을 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public PlayerRewardModel GetPlayerReward() => m_playerReward;

        /// <summary>
        /// [기능]: 보상 데이터 변경 상태를 Observer 패턴(이벤트)을 통하여 외부 View에 전파합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public void Refresh()
        {
            OnRewardDataChanged?.Invoke();
        }
        #endregion
    }
}
