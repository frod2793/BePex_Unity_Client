using UnityEngine;
using BePex.EventSystem.Interfaces;
using BePex.EventSystem.Data;

namespace BePex.EventSystem.Conditions
{
    /// <summary>
    /// [기능]: 킬카운트를 이벤트 완료 조건으로 달성하였는지 판정하며, 몬스터 처치 판정 및 필터링 비즈니스 로직을 제공하는 예시 전략 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    [QuestCondition("KillCount")]
    public class KillCountQuestCondition : BaseQuestCondition
    {
        #region 내부 필드
        // 기획 데이터 또는 조건 스크립트를 통해 지정될 몬스터 필터 (예시 필드)
        private readonly string m_targetMonsterType;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 팩토리 생성용 5인자 표준 생성자. 기본적으로 모든 몬스터를 가산 대상으로 초기화합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 5인자 기본 생성자 오버로딩 정의
        /// </summary>
        public KillCountQuestCondition(int targetValue, ISaveSystem saveSystem, ITimeProvider timeProvider, string eventId, string questId)
            : base(targetValue, saveSystem, timeProvider, eventId, questId)
        {
            m_targetMonsterType = "All";
        }

        /// <summary>
        /// [기능]: 특정 몬스터 종류만 가산 대상으로 필터링할 수 있는 확장 생성자.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 6인자 확장 생성자 오버로딩 정의
        /// </summary>
        public KillCountQuestCondition(int targetValue, ISaveSystem saveSystem, ITimeProvider timeProvider, string eventId, string questId, string targetMonsterType)
            : base(targetValue, saveSystem, timeProvider, eventId, questId)
        {
            m_targetMonsterType = string.IsNullOrEmpty(targetMonsterType) ? "All" : targetMonsterType;
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 몬스터 처치 이벤트 발생 시, 처치된 몬스터가 해당 기획 조건에 부합하는 가산 대상인지 검사하는 비즈니스 예시 메서드.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 예시 비즈니스 로직 및 헬퍼 추가
        /// </summary>
        public bool EvaluateMonsterKill(string killedMonsterType)
        {
            if (string.IsNullOrEmpty(killedMonsterType))
            {
                return false;
            }

            // "All" 혹은 타겟 몬스터 타입과 대소문자 구분 없이 일치하는 경우 가산 허용
            if (m_targetMonsterType.Equals("All", System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return m_targetMonsterType.Equals(killedMonsterType, System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// [기능]: 조건 달성 가능 여부를 확인합니다. 기본적으로 참을 반환하며 필요 시 재정의합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 가상 메서드 재정의
        /// </summary>
        public override bool CanAddProgress(Models.EventProgressModel progress)
        {
            return true;
        }
        #endregion
    }
}
