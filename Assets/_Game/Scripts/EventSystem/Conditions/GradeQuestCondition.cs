using UnityEngine;
using BePex.EventSystem.Interfaces;
using BePex.EventSystem.Data;

namespace BePex.EventSystem.Conditions
{
    /// <summary>
    /// [기능]: 학년이벤트을 이벤트 완료 조건으로 달성하였는지 판정하는 Strategy 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    [QuestCondition("Grade")]
    public class GradeQuestCondition : BaseQuestCondition
    {
        #region 초기화
        /// <summary>
        /// [기능]: 부모 생성자를 경유해 목표 수치, 세이브장치, 시간 제공자, 이벤트 ID 및 퀘스트 ID를 매핑받습니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public GradeQuestCondition(int targetValue, ISaveSystem saveSystem, ITimeProvider timeProvider, string eventId, string questId)
            : base(targetValue, saveSystem, timeProvider, eventId, questId)
        {
        }
        #endregion

        #region 공개 메서드
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
