using UnityEngine;

namespace BePex.EventSystem.Data
{
    /// <summary>
    /// [기능]: 활성화 상태의 모든 이벤트를 수집 관리하는 최상위 기획 데이터 테이블 ScriptableObject 에셋 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    [CreateAssetMenu(fileName = "EventTable", menuName = "BePex/Event/Table")]
    public class EventTableSO : ScriptableObject
    {
        #region UI 참조 (Inspector)
        [SerializeField] private EventDefinitionSO[] m_events;
        #endregion

        #region 공개 프로퍼티 (Public Properties)
        public EventDefinitionSO[] Events => m_events;
        #endregion
    }
}
