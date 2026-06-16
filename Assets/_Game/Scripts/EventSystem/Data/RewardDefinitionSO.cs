using UnityEngine;

namespace BePex.EventSystem.Data
{
    /// <summary>
    /// [기능]: 지급할 보상의 속성(타입, 수량, 아이콘, 텍스트)을 정의하는 ScriptableObject 에셋 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    [CreateAssetMenu(fileName = "RewardDefinition", menuName = "BePex/Event/Reward")]
    public class RewardDefinitionSO : ScriptableObject
    {
        #region UI 참조 (Inspector)
        [SerializeField] private RewardTypeSO m_rewardType;
        [SerializeField] private int m_amount;
        [SerializeField] private string m_displayName;
        [SerializeField] private Sprite m_icon;
        #endregion

        #region 공개 프로퍼티 (Public Properties)
        public RewardTypeSO Type => m_rewardType;
        public int Amount => m_amount;
        public string DisplayName => m_displayName;
        public Sprite Icon => m_icon;
        #endregion
    }
}
