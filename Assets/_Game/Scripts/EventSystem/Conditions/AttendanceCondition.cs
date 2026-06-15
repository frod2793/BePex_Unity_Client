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
        /// [기능]: 부모 생성자를 경유해 목표 출석 체크 일수, 세이브장치, 시간 제공자 및 이벤트 ID를 주입받습니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: ITimeProvider 의존성 주입 추가
        /// </summary>
        public AttendanceCondition(int targetValue, ISaveSystem saveSystem, ITimeProvider timeProvider, string eventId)
            : base(targetValue, saveSystem, timeProvider, eventId)
        {
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 가장 마지막 갱신 시간과 현재 시간을 비교하여 오늘 이미 출석했는지 검사합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 타임 기반 출석 방어 로직 추가
        /// </summary>
        public override bool CanAddProgress(Models.EventProgressModel progress)
        {
            if (progress.lastUpdatedTicks == 0) return true;
            System.DateTime lastTime = new System.DateTime(progress.lastUpdatedTicks);
            System.DateTime currentTime = m_timeProvider.GetCurrentTime();
            return currentTime.Date > lastTime.Date;
        }
        #endregion
    }
}
