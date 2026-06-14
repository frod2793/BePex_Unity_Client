using System;
using System.Collections.Generic;

namespace BePex.EventSystem.Models
{
    /// <summary>
    /// [기능]: 플레이어의 누적 자산(Exp, Ticket, Point) 및 이미 보상을 수령 완료한 이벤트의 ID 이력을 유지하는 순수 C# 데이터 모델 (POCO).
    /// [작성자]: 윤승종
    /// [수정 날짜]: 2026-06-14
    /// [마지막 수정 작성자]: 윤승종
    /// [수정 내용]: [Serializable] 어트리뷰트와 XML 주석의 배치 순서 교정
    /// </summary>
    [Serializable]
    public class PlayerRewardModel
    {
        #region 데이터 멤버
        public List<string> claimedEventIds;
        public int totalExp;
        public int totalTickets;
        public int totalPoints;
        public int totalSeasonPoints;
        public int totalCredits;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 기본 생성자로 누적 필드 및 이미 받아진 이벤트 ID 목록 리스트를 할당 및 초기화합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: totalSeasonPoints, totalCredits 추가 초기화
        /// </summary>
        public PlayerRewardModel()
        {
            claimedEventIds = new List<string>();
            totalExp = 0;
            totalTickets = 0;
            totalPoints = 0;
            totalSeasonPoints = 0;
            totalCredits = 0;
        }
        #endregion
    }
}
