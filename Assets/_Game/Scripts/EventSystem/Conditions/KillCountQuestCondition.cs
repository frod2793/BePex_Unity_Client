using UnityEngine;
using BePex.EventSystem.Interfaces;
using BePex.EventSystem.Data;

namespace BePex.EventSystem.Conditions
{
    /// <summary>
    /// [기능]: 적 처치 수를 퀘스트 완료 조건으로 달성하였는지 판정하는 Strategy 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    [QuestCondition(ConditionDefinitionSO.ConditionType.KillCount)]
    public class KillCountQuestCondition : BaseQuestCondition
    {
        #region 초기화
        /// <summary>
        /// [기능]: 부모 생성자를 경유해 목표 처치 횟수, 세이브장치, 시간 제공자, 이벤트 ID 및 퀘스트 ID를 주입받습니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Quest-related 네이밍 적용
        /// </summary>
        public KillCountQuestCondition(int targetValue, ISaveSystem saveSystem, ITimeProvider timeProvider, string eventId, string questId)
            : base(targetValue, saveSystem, timeProvider, eventId, questId)
        {
        }
        #endregion
    }
}
