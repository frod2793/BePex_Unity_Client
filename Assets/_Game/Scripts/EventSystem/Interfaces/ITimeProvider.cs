using System;

namespace BePex.EventSystem.Interfaces
{
    /// <summary>
    /// [기능]: 시스템 시간에 대한 의존성을 캡슐화하여 기간 및 출석 체크 로직의 테스트 및 시뮬레이션을 돕는 시간 제공자 인터페이스.
    /// [작성자]: 윤승종
    /// </summary>
    public interface ITimeProvider
    {
        /// <summary>
        /// [기능]: 현재 적용된 가상 혹은 실제 로컬 시간을 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        DateTime GetCurrentTime();
    }
}
