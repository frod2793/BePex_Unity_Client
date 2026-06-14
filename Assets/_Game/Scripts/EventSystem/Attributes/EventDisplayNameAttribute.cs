using System;

namespace BePex.EventSystem.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EventDisplayNameAttribute : Attribute
    {
        #region 내부 필드
        private readonly string m_displayName;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 어트리뷰트에 한글 명칭을 전달받아 초기화합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public EventDisplayNameAttribute(string displayName)
        {
            m_displayName = displayName;
        }
        #endregion

        #region 공개 프로퍼티 (Public Properties)
        public string DisplayName => m_displayName;
        #endregion
    }
}
