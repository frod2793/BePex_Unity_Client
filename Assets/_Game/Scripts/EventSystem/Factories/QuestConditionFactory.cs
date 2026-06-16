using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using BePex.EventSystem.Interfaces;
using BePex.EventSystem.Data;
using BePex.EventSystem.Conditions;
using BePex.EventSystem.DTOs;

namespace BePex.EventSystem.Factories
{
    /// <summary>
    /// [기능]: ConditionDefinitionSO 기획 데이터를 기반으로 알맞은 IQuestCondition 전략 객체를 생성 및 바인딩해 주는 Factory 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    public class QuestConditionFactory
    {
        #region 내부 필드
        private readonly ISaveSystem m_saveSystem;
        private readonly ITimeProvider m_timeProvider;
        private readonly Dictionary<string, Type> m_registry;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 영속성 세이브 모듈과 시간 제공자를 주입받고 리플렉션을 통한 조건 클래스 자동 등록을 수행합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Quest-related 네이밍 적용
        /// </summary>
        public QuestConditionFactory(ISaveSystem saveSystem, ITimeProvider timeProvider)
        {
            m_saveSystem = saveSystem;
            m_timeProvider = timeProvider;
            m_registry = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            BuildRegistry();
        }

        /// <summary>
        /// [기능]: 리플렉션을 통해 어셈블리 내 해당 어트리뷰트 타입 전략 클래스들을 레지스트리에 자동 등록합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: IQuestCondition 및 QuestConditionAttribute 검색 적용
        /// </summary>
        private void BuildRegistry()
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var type in assembly.GetTypes())
            {
                if (typeof(IQuestCondition).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                {
                    var attr = type.GetCustomAttribute<QuestConditionAttribute>();
                    if (attr != null)
                    {
                        m_registry[attr.TypeName] = type;
                    }
                }
            }
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 조건 SO 정의 파일 및 이벤트 ID, 퀘스트 ID를 토대로 레지스트리에 매핑된 타입의 인스턴스를 동적 생성합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Type Object 패턴에 맞춰 ConditionTypeSO 참조를 이용해 생성하도록 변경
        /// </summary>
        public IQuestCondition Create(ConditionDefinitionSO definition, string eventId, string questId)
        {
            if (definition == null || definition.Type == null)
            {
                return null;
            }

            return CreateInternal(definition.Type.TypeName, definition.TargetValue, eventId, questId);
        }

        /// <summary>
        /// [기능]: 조건 DTO 데이터 및 이벤트 ID, 퀘스트 ID를 토대로 레지스트리에 매핑된 타입의 인스턴스를 동적 생성합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 이넘 파싱 단계를 제거하고 문자열 타입 식별자를 직접 사용
        /// </summary>
        public IQuestCondition Create(ConditionDefinitionDTO definition, string eventId, string questId)
        {
            if (definition == null)
            {
                return null;
            }

            return CreateInternal(definition.conditionType, definition.targetValue, eventId, questId);
        }
        #endregion

        #region 내부 메서드
        /// <summary>
        /// [기능]: 등록된 어셈블리 맵에서 조건 타입을 찾아 IQuestCondition 객체로 실질 생성 및 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 조건 타입을 문자열 식별자로 받도록 수정
        /// </summary>
        private IQuestCondition CreateInternal(string typeName, int targetValue, string eventId, string questId)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            if (m_registry.TryGetValue(typeName, out Type conditionType))
            {
                return (IQuestCondition)Activator.CreateInstance(conditionType, targetValue, m_saveSystem, m_timeProvider, eventId, questId);
            }

            // [Fallback] 기획상 지정된 클래스가 없다면 범용 StandardQuestCondition 인스턴스로 자동 대체
            return new StandardQuestCondition(targetValue, m_saveSystem, m_timeProvider, eventId, questId);
        }
        #endregion
    }
}
