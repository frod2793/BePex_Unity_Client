/// <summary>
/// [기능]: 별도의 예외 가드 로직이 필요 없는 일반적인 퀘스트 조건(단순 누적값 비교)을 공통 처리하는 범용 조건 클래스.
/// [작성자]: 윤승종
/// </summary>

using BePex.EventSystem.Interfaces;

namespace BePex.EventSystem.Conditions
{
    public class StandardQuestCondition : BaseQuestCondition
    {
        #region 초기화
        /// <summary>
        /// [기능]: 부모 생성자를 경유해 목표 수치, 세이브 모듈, 시간 제공자, 이벤트 및 퀘스트 ID를 주입받습니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// </summary>
        public StandardQuestCondition(int targetValue, ISaveSystem saveSystem, ITimeProvider timeProvider, string eventId, string questId)
            : base(targetValue, saveSystem, timeProvider, eventId, questId)
        {
        }
        #endregion
    }
}
