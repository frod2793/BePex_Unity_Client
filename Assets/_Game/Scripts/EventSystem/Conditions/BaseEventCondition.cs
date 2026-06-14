/// <summary>
/// [기능]: 공통적인 이벤트 조건의 진행도 로드 및 달성 상태 비교 로직을 제공하는 추상 기반 클래스.
/// [작성자]: 윤승종
/// </summary>

using UnityEngine;
using BePex.EventSystem.Interfaces;

namespace BePex.EventSystem.Conditions
{
    public abstract class BaseEventCondition : IEventCondition
    {
        #region 내부 필드
        protected readonly int m_targetValue;
        protected readonly ISaveSystem m_saveSystem;
        protected readonly string m_eventId;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 기반 조건 인스턴스에 필요한 의존성을 주입받아 초기화합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        protected BaseEventCondition(int targetValue, ISaveSystem saveSystem, string eventId)
        {
            m_targetValue = targetValue;
            m_saveSystem = saveSystem;
            m_eventId = eventId;
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 해당 이벤트의 누적 진행도를 세이브 데이터로부터 비동기 로드합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        public virtual async Awaitable<int> GetCurrentProgressAsync()
        {
            var progress = await m_saveSystem.LoadProgressAsync(m_eventId);
            return progress.currentProgress;
        }

        /// <summary>
        /// [기능]: 설정된 목표 수치를 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        public int GetTargetValue()
        {
            return m_targetValue;
        }

        /// <summary>
        /// [기능]: 목표 진행 수치에 도달하여 완료되었는지 비동기로 판정합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        public virtual async Awaitable<bool> IsCompletedAsync()
        {
            int currentProgress = await GetCurrentProgressAsync();
            return currentProgress >= m_targetValue;
        }
        #endregion
    }
}
