using BePex.EventSystem.Models;

namespace BePex.EventSystem.Interfaces
{
    /// <summary>
    /// [기능]: 유저 자산에 퀘스트 보상을 지급하기 위한 Strategy 인터페이스.
    /// [작성자]: 윤승종
    /// </summary>
    public interface IQuestReward
    {
        /// <summary>
        /// [기능]: 퀘스트 보상의 표시 이름을 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 명칭 변경
        /// </summary>
        string GetRewardName();

        /// <summary>
        /// [기능]: 퀘스트 보상의 수량을 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 명칭 변경
        /// </summary>
        int GetRewardAmount();

        /// <summary>
        /// [기능]: 퀘스트 보상을 플레이어의 보상 모델 데이터에 직접 적립(지급)합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 명칭 변경
        /// </summary>
        void Grant(PlayerRewardModel playerReward);
    }
}
