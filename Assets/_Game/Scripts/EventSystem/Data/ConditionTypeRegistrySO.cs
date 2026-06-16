/// <summary>
/// [기능]: 프로젝트에 등록된 모든 ConditionTypeSO 에셋 목록을 관리하고 제공하는 레지스트리 클래스.
/// [작성자]: 윤승종
/// </summary>

using System.Collections.Generic;
using UnityEngine;

namespace BePex.EventSystem.Data
{
    [CreateAssetMenu(fileName = "ConditionTypeRegistry", menuName = "BePex/Event/Condition Type Registry")]
    public class ConditionTypeRegistrySO : ScriptableObject
    {
        #region 내부 필드 (Private Fields)
        [SerializeField] private List<ConditionTypeSO> m_conditionTypes = new List<ConditionTypeSO>();
        #endregion

        #region 공개 프로퍼티 (Public Properties)
        public IReadOnlyList<ConditionTypeSO> ConditionTypes => m_conditionTypes;
        #endregion

        #region 공개 메서드 (Public Methods)
        /// <summary>
        /// [기능]: 신규 조건 타입을 에셋 목록에 중복되지 않게 동적으로 추가 등록합니다. (에디터 지원용)
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        public void Register(ConditionTypeSO type)
        {
            if (type != null && !m_conditionTypes.Contains(type))
            {
                m_conditionTypes.Add(type);
            }
        }
        #endregion
    }
}
