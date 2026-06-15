using System;
using BePex.EventSystem.Interfaces;

namespace BePex.EventSystem.Infrastructure
{
    /// <summary>
    /// [기능]: 디버그 환경에서 시스템 시간에 임의의 오프셋을 더하여 반환하는 가상 시간 제공자.
    /// [작성자]: 윤승종
    /// </summary>
    public class DebugTimeProvider : ITimeProvider
    {
        #region 내부 필드
        private TimeSpan m_timeOffset = TimeSpan.Zero;
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 실제 시스템 시간에 조작된 오프셋을 더한 시뮬레이션 시간을 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        public DateTime GetCurrentTime()
        {
            return DateTime.Now.Add(m_timeOffset);
        }

        /// <summary>
        /// [기능]: 가상 시간에 지정된 일(Day) 수만큼 오프셋을 추가합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        public void AddDays(double days)
        {
            m_timeOffset = m_timeOffset.Add(TimeSpan.FromDays(days));
        }

        /// <summary>
        /// [기능]: 가상 시간에 더해진 모든 오프셋을 초기화하여 현재 실제 시간으로 복귀합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        public void ResetOffset()
        {
            m_timeOffset = TimeSpan.Zero;
        }
        #endregion
    }
}
