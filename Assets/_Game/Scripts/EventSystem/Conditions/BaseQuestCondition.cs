/// <summary>
/// [기능]: 공통적인 퀘스트 조건의 진행도 로드 및 달성 상태 비교 로직을 제공하는 추상 기반 클래스.
/// [작성자]: 윤승종
/// </summary>

using UnityEngine;
using BePex.EventSystem.Interfaces;

namespace BePex.EventSystem.Conditions
{
    public abstract class BaseQuestCondition : IQuestCondition
    {
        #region 내부 필드
        protected readonly int m_targetValue;
        protected readonly ISaveSystem m_saveSystem;
        protected readonly ITimeProvider m_timeProvider;
        protected readonly string m_eventId;
        protected readonly string m_questId;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 기반 조건 인스턴스에 필요한 의존성과 퀘스트 ID를 주입받아 초기화합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: m_questId 필드 및 초기화 추가
        /// </summary>
        protected BaseQuestCondition(int targetValue, ISaveSystem saveSystem, ITimeProvider timeProvider, string eventId, string questId)
        {
            m_targetValue = targetValue;
            m_saveSystem = saveSystem;
            m_timeProvider = timeProvider;
            m_eventId = eventId;
            m_questId = questId;
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 해당 이벤트의 누적 진행도를 세이브 데이터로부터 비동기 로드합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 퀘스트 리스트에서 questId로 개별 진행도를 검사하여 반환하는 로직으로 개편
        /// </summary>
        public virtual async Awaitable<int> GetCurrentProgressAsync()
        {
            var progress = await m_saveSystem.LoadProgressAsync(m_eventId);
            if (progress == null || progress.quests == null)
            {
                return 0;
            }

            for (int i = 0; i < progress.quests.Count; i++)
            {
                if (progress.quests[i].questId == m_questId)
                {
                    return progress.quests[i].currentProgress;
                }
            }

            return 0;
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
        /// [기능]: 조건이 만족되어 완료되었는지 비동기로 판정합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 퀘스트 리스트에서 questId로 개별 완료 상태를 판정하도록 갱신
        /// </summary>
        public virtual async Awaitable<bool> IsCompletedAsync()
        {
            var progress = await m_saveSystem.LoadProgressAsync(m_eventId);
            if (progress == null || progress.quests == null)
            {
                return false;
            }

            for (int i = 0; i < progress.quests.Count; i++)
            {
                if (progress.quests[i].questId == m_questId)
                {
                    return progress.quests[i].isCompleted;
                }
            }

            int currentProgress = await GetCurrentProgressAsync();
            return currentProgress >= m_targetValue;
        }

        /// <summary>
        /// [기능]: 진척도를 더할 수 있는지 기본적으로 판별합니다. (기본: 항상 true)
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        public virtual bool CanAddProgress(Models.EventProgressModel progress)
        {
            return true;
        }
        #endregion
    }
}
