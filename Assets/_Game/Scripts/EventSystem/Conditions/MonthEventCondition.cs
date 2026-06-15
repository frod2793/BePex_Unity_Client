using UnityEngine;
using BePex.EventSystem.Interfaces;
using BePex.EventSystem.Data;

namespace BePex.EventSystem.Conditions
{
    /// <summary>
    /// [기능]: 월간 이벤트을 이벤트 완료 조건으로 달성하였는지 판정하는 Strategy 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    [EventCondition(ConditionDefinitionSO.ConditionType.MonthEvent)]
    public class MonthEventCondition : BaseEventCondition
    {
        #region 초기화
        /// <summary>
        /// [기능]: 부모 생성자를 경유해 목표 수치, 세이브장치 및 해당 이벤트 ID를 매핑받습니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public MonthEventCondition(int targetValue, ISaveSystem saveSystem, ITimeProvider timeProvider, string eventId)
            : base(targetValue, saveSystem, timeProvider, eventId)
        {
        }
        #endregion
    }
}
