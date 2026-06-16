using UnityEngine;

namespace BePex.EventSystem.Data
{
    /// <summary>
    /// [기능]: 이벤트 완료 타겟 수치 및 조건의 타입을 정의하는 ScriptableObject 에셋 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    [CreateAssetMenu(fileName = "ConditionDefinition", menuName = "BePex/Event/Condition")]
    public class ConditionDefinitionSO : ScriptableObject
    {
        #region UI 참조 (Inspector)
        [SerializeField] private ConditionTypeSO m_conditionType;
        [SerializeField] private int m_targetValue;
        #endregion

        #region 공개 프로퍼티 (Public Properties)
        public ConditionTypeSO Type => m_conditionType;
        public int TargetValue => m_targetValue;
        #endregion
    }
}
