using System;
using BePex.EventSystem.Interfaces;

namespace BePex.EventSystem.Infrastructure
{
    /// <summary>
    /// [기능]: 실제 시스템 로컬 시간을 반환하는 ITimeProvider 상용 구현체.
    /// [작성자]: 윤승종
    /// </summary>
    public class SystemTimeProvider : ITimeProvider
    {
        /// <summary>
        /// [기능]: 실제 시스템의 현재 시간(DateTime.Now)을 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        public DateTime GetCurrentTime()
        {
            return DateTime.Now;
        }
    }
}
