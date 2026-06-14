using UnityEngine;
using BePex.EventSystem.Attributes;

namespace BePex.EventSystem.Data
{
    /// <summary>
    /// [기능]: 이벤트 완료 타겟 수치 및 조건의 타입을 정의하는 ScriptableObject 에셋 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    [CreateAssetMenu(fileName = "ConditionDefinition", menuName = "BePex/Event/Condition")]
    public class ConditionDefinitionSO : ScriptableObject
    {
        #region 조건 타입 열거형
        public enum ConditionType
        {
            [EventDisplayName("킬 횟수")]
            KillCount,
            [EventDisplayName("스테이지 클리어")]
            StageClear,
            [EventDisplayName("출석체크")]
            Attendance,
            [EventDisplayName("길드 이벤트")]
            GuildEvent,
            [EventDisplayName("월간 이벤트")]
            MonthEvent,
            [EventDisplayName("랭킹 이벤트")]
            RankingEvent,
            [EventDisplayName("광고 이벤트")]
            ADMobEvent
        
        
        
        
        }
        #endregion

        #region UI 참조 (Inspector)
        [SerializeField] private ConditionType m_conditionType;
        [SerializeField] private int m_targetValue;
        #endregion

        #region 공개 프로퍼티 (Public Properties)
        public ConditionType Type => m_conditionType;
        public int TargetValue => m_targetValue;
        #endregion
    }
}
