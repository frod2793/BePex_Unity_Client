/// <summary>
/// [기능]: 단일 퀘스트/이벤트 조건 타입을 정의하는 ScriptableObject 에셋 클래스.
/// [작성자]: 윤승종
/// </summary>

using UnityEngine;

namespace BePex.EventSystem.Data
{
    [CreateAssetMenu(fileName = "ConditionType_", menuName = "BePex/Event/Condition Type")]
    public class ConditionTypeSO : ScriptableObject
    {
        #region 내부 필드 (Private Fields)
        [SerializeField] private string m_typeName;
        [SerializeField] private string m_displayName;
        #endregion

        #region 공개 프로퍼티 (Public Properties)
        public string TypeName => m_typeName;
        public string DisplayName => m_displayName;
        #endregion
    }
}
