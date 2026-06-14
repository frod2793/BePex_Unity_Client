using System;
using BePex.EventSystem.Data;

namespace BePex.EventSystem.Rewards
{
    /// <summary>
    /// [기능]: 리플렉션 팩토리가 런타임에 동적으로 매핑할 수 있도록 IEventReward 구현체에 부착하는 어트리뷰트.
    /// [작성자]: 윤승종
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class EventRewardAttribute : Attribute
    {
        public RewardDefinitionSO.RewardType Type { get; }

        public EventRewardAttribute(RewardDefinitionSO.RewardType type)
        {
            Type = type;
        }
    }
}
