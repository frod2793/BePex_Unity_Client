using System;

namespace BePex.EventSystem.Conditions
{
    /// <summary>
    /// [기능]: 리플렉션 팩토리가 런타임에 동적으로 매핑할 수 있도록 IQuestCondition 구현체에 부착하는 어트리뷰트.
    /// [작성자]: 윤승종
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class QuestConditionAttribute : Attribute
    {
        public string TypeName { get; }

        /// <summary>
        /// [기능]: 퀘스트 조건 타입을 지정하여 어트리뷰트를 생성합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Type Object 패턴에 맞춰 문자열 기반 조건 식별자 매핑으로 전환
        /// </summary>
        public QuestConditionAttribute(string typeName)
        {
            TypeName = typeName;
        }
    }
}
