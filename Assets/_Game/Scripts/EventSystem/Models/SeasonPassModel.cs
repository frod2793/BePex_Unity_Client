using System.Collections.Generic;

namespace BePex.EventSystem.Models
{
    /// <summary>
    /// [기능]: 사용자의 시즌패스 진행 레벨, 획득한 EXP, 수령 완료한 보상 리스트 상태를 저장하는 순수 데이터 모델 (DTO).
    /// [작성자]: 윤승종
    /// </summary>
    [System.Serializable]
    public class SeasonPassModel
    {
        #region 데이터 멤버
        public string passId;
        public int currentExp;
        public int currentLevel;
        public bool isPremiumActive;

        public List<int> claimedFreeLevels;
        public List<int> claimedPremiumLevels;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 기본 생성자를 호출해 무료/프리미엄 보상 클레임 레벨 리스트를 안전하게 인스턴싱합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 필드 이니셜라이저를 생성자 초기화 방식으로 이관
        /// </summary>
        public SeasonPassModel()
        {
            claimedFreeLevels = new List<int>();
            claimedPremiumLevels = new List<int>();
            passId = string.Empty;
            currentExp = 0;
            currentLevel = 0;
            isPremiumActive = false;
        }
        #endregion
    }
}
