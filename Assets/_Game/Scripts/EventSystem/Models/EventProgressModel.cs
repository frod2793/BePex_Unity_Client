using System;

namespace BePex.EventSystem.Models
{
    /// <summary>
    /// [기능]: 개별 이벤트의 고유 정보, 진척도 및 완료/수령 상태를 가지는 순수 C# 데이터 모델 (POCO).
    /// [작성자]: 윤승종
    /// [수정 날짜]: 2026-06-14
    /// [마지막 수정 작성자]: 윤승종
    /// [수정 내용]: [Serializable] 어트리뷰트와 XML 주석의 배치 순서 교정
    /// </summary>
    [Serializable]
    public class EventProgressModel
    {
        #region 데이터 멤버
        public string eventId;
        public int currentProgress;
        public bool isCompleted;
        public bool isRewardClaimed;
        public long lastUpdatedTicks;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 기본 생성자로 초기 기본값을 명시적으로 구성합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public EventProgressModel()
        {
            eventId = string.Empty;
            currentProgress = 0;
            isCompleted = false;
            isRewardClaimed = false;
            lastUpdatedTicks = 0;
        }
        #endregion
    }
}
