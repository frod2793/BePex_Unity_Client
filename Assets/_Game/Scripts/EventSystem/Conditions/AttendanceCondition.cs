using UnityEngine;
using BePex.EventSystem.Interfaces;
using BePex.EventSystem.Data;

namespace BePex.EventSystem.Conditions
{
    /// <summary>
    /// [기능]: 출석 체크 일수를 이벤트 완료 조건으로 달성하였는지 판정하는 Strategy 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    [EventCondition(ConditionDefinitionSO.ConditionType.Attendance)]
    public class AttendanceCondition : BaseEventCondition
    {
        #region 초기화
        /// <summary>
        /// [기능]: 부모 생성자를 경유해 목표 출석 체크 일수, 세이브장치 및 이벤트 ID를 주입받습니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: BaseEventCondition 상속을 활용해 단순화
        /// </summary>
        public AttendanceCondition(int targetValue, ISaveSystem saveSystem, string eventId)
            : base(targetValue, saveSystem, eventId)
        {
        }
        #endregion
    }
}
