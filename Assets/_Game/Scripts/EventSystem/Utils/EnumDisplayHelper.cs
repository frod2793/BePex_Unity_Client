using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using BePex.EventSystem.Attributes;

namespace BePex.EventSystem.Utils
{
    /// <summary>
    /// [기능]: 리플렉션을 통해 Enum 디스플레이 명칭을 관리 및 캐싱하고, 양방향 변환을 수행하는 정적 유틸리티 클래스입니다.
    /// [작성자]: 윤승종
    /// </summary>
    public static class EnumDisplayHelper
    {
        #region 내부 필드 (Private Fields)
        private static readonly Dictionary<Type, Dictionary<Enum, string>> m_toDisplayCache = new Dictionary<Type, Dictionary<Enum, string>>();
        private static readonly Dictionary<Type, Dictionary<string, Enum>> m_toEnumCache = new Dictionary<Type, Dictionary<string, Enum>>();
        #endregion

        #region 초기화 (Initialization)
        /// <summary>
        /// [기능]: 대상 Enum 타입의 어트리뷰트를 스캔하여 양방향 변환 딕셔너리를 빌드 및 캐싱합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의 및 캐싱 로직 구현
        /// </summary>
        public static void RegisterEnum<T>() where T : Enum
        {
            Type type = typeof(T);
            if (m_toDisplayCache.ContainsKey(type) == true)
            {
                return;
            }

            var toDisplay = new Dictionary<Enum, string>();
            var toEnum = new Dictionary<string, Enum>();

            FieldInfo[] fields = type.GetFields();
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                if (field.IsLiteral == true)
                {
                    var attr = field.GetCustomAttribute<EventDisplayNameAttribute>();
                    Enum enumValue = (Enum)field.GetValue(null);
                    string displayName = attr != null ? attr.DisplayName : enumValue.ToString();

                    toDisplay[enumValue] = displayName;
                    toEnum[displayName] = enumValue;
                }
            }

            m_toDisplayCache[type] = toDisplay;
            m_toEnumCache[type] = toEnum;
        }
        #endregion

        #region 공개 메서드 (Public Methods)
        /// <summary>
        /// [기능]: 특정 Enum 값을 지정된 한글 표시명으로 변환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        public static string GetDisplayName<T>(T value) where T : Enum
        {
            RegisterEnum<T>();
            if (m_toDisplayCache[typeof(T)].TryGetValue(value, out string name) == true)
            {
                return name;
            }
            return value.ToString();
        }

        /// <summary>
        /// [기능]: 한글 표시명을 통해 원래의 Enum 값으로 환산합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의 및 예외 로그 추가
        /// </summary>
        public static T GetEnumValue<T>(string displayName) where T : Enum
        {
            RegisterEnum<T>();
            if (m_toEnumCache[typeof(T)].TryGetValue(displayName, out Enum value) == true)
            {
                return (T)value;
            }

            Debug.LogWarning($"[EnumDisplayHelper] 매핑되는 {typeof(T).Name} 값을 찾을 수 없습니다: {displayName}");
            return default;
        }

        /// <summary>
        /// [기능]: UI 드롭다운 바인딩을 위해 어트리뷰트 한글 표시명 리스트를 전체 로드합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        public static List<string> GetDisplayNames<T>() where T : Enum
        {
            RegisterEnum<T>();
            return new List<string>(m_toEnumCache[typeof(T)].Keys);
        }
        #endregion
    }
}
