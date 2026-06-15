using UnityEngine;

namespace BePex.EventSystem.Interfaces
{
    /// <summary>
    /// [기능]: 이벤트 목표 달성 조건(처치수, 스테이지 클리어 등)의 진척도를 검증하기 위한 Strategy 인터페이스.
    /// [작성자]: 윤승종
    /// </summary>
    public interface IEventCondition
    {
        /// <summary>
        /// [기능]: 현재 조건의 진행 수치를 파일/저장소로부터 비동기로 조회해 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Awaitable 비동기 인터페이스로 갱신
        /// </summary>
        Awaitable<int> GetCurrentProgressAsync();

        /// <summary>
        /// [기능]: 조건 달성을 위한 목표 수치를 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        int GetTargetValue();

        /// <summary>
        /// [기능]: 조건이 만족되어 완료되었는지 여부를 비동기로 확인합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Awaitable 비동기 인터페이스로 갱신
        /// </summary>
        Awaitable<bool> IsCompletedAsync();

        /// <summary>
        /// [기능]: 특정 이벤트 진척 상태 데이터를 바탕으로 현재 진척도를 가산할 수 있는지(쿨타임, 중복 출석 방지 등) 검사합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: OCP 확장을 위해 검증 메서드 신규 추가
        /// </summary>
        bool CanAddProgress(Models.EventProgressModel progress);
    }
}
