using UnityEngine;
using BePex.EventSystem.Attributes;

namespace BePex.EventSystem.Data
{
    /// <summary>
    /// [기능]: 지급할 보상의 속성(타입, 수량, 아이콘, 텍스트)을 정의하는 ScriptableObject 에셋 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    [CreateAssetMenu(fileName = "RewardDefinition", menuName = "BePex/Event/Reward")]
    public class RewardDefinitionSO : ScriptableObject
    {
        #region 보상 타입 열거형
        public enum RewardType
        {
            [EventDisplayName("경험치")]
            Exp,
            [EventDisplayName("티켓")]
            Ticket,
            [EventDisplayName("이벤트 포인트")]
            Point,
            [EventDisplayName("시즌 포인트")]
            SeasonPoint,
            [EventDisplayName("재화 보상")]
            CreditReword
        
        
        }
        #endregion

        #region UI 참조 (Inspector)
        [SerializeField] private RewardType m_rewardType;
        [SerializeField] private int m_amount;
        [SerializeField] private string m_displayName;
        [SerializeField] private Sprite m_icon;
        #endregion

        #region 공개 프로퍼티 (Public Properties)
        public RewardType Type => m_rewardType;
        public int Amount => m_amount;
        public string DisplayName => m_displayName;
        public Sprite Icon => m_icon;
        #endregion
    }
}
