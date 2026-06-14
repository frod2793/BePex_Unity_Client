using UnityEngine;

namespace BePex.EventSystem.Data
{
    /// <summary>
    /// [기능]: 단일 이벤트 콘텐츠(아이디, 기간, 조건, 보상 정보) 명세를 정의하는 ScriptableObject 에셋 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    [CreateAssetMenu(fileName = "EventDefinition", menuName = "BePex/Event/Event")]
    public class EventDefinitionSO : ScriptableObject
    {
        #region UI 참조 (Inspector)
        [SerializeField] private string m_eventId;
        [SerializeField] private string m_eventTitle;
        [SerializeField] private string m_eventDescription;
        [SerializeField] private Sprite m_eventIcon;
        [SerializeField] private string m_startDate;
        [SerializeField] private string m_endDate;
        [SerializeField] private ConditionDefinitionSO m_condition;
        [SerializeField] private RewardDefinitionSO[] m_rewards;
        #endregion

        #region 공개 프로퍼티 (Public Properties)
        public string EventId => m_eventId;
        public string EventTitle => m_eventTitle;
        public string EventDescription => m_eventDescription;
        public Sprite EventIcon => m_eventIcon;
        public string StartDate => m_startDate;
        public string EndDate => m_endDate;
        public ConditionDefinitionSO Condition => m_condition;
        public RewardDefinitionSO[] Rewards => m_rewards;
        #endregion
    }
}
