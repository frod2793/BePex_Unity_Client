using System;
using BePex.EventSystem.Data;

namespace BePex.EventSystem.Rewards
{
    /// <summary>
    /// [기능]: 리플렉션 팩토리가 런타임에 동적으로 매핑할 수 있도록 IQuestReward 구현체에 부착하는 어트리뷰트.
    /// [작성자]: 윤승종
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class QuestRewardAttribute : Attribute
    {
        public string TypeName { get; }

        /// <summary>
        /// [기능]: 퀘스트 보상 타입 이름을 지정하여 어트리뷰트를 생성합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Enum에서 문자열 식별자 타입명으로 변경
        /// </summary>
        public QuestRewardAttribute(string typeName)
        {
            TypeName = typeName;
        }
    }
}
