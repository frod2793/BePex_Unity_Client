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
        private readonly Dictionary<ConditionDefinitionSO.ConditionType, Type> m_registry;
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
            m_registry = new Dictionary<ConditionDefinitionSO.ConditionType, Type>();
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
                        m_registry[attr.Type] = type;
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
        /// [수정 내용]: questId 인자로 리네이밍
        /// </summary>
        public IQuestCondition Create(ConditionDefinitionSO definition, string eventId, string questId)
        {
            if (definition == null)
            {
                return null;
            }

            return CreateInternal(definition.Type, definition.TargetValue, eventId, questId);
        }

        /// <summary>
        /// [기능]: 조건 DTO 데이터 및 이벤트 ID, 퀘스트 ID를 토대로 레지스트리에 매핑된 타입의 인스턴스를 동적 생성합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: questId 인자로 리네이밍
        /// </summary>
        public IQuestCondition Create(ConditionDefinitionDTO definition, string eventId, string questId)
        {
            if (definition == null)
            {
                return null;
            }

            if (Enum.TryParse(definition.conditionType, out ConditionDefinitionSO.ConditionType typeEnum))
            {
                return CreateInternal(typeEnum, definition.targetValue, eventId, questId);
            }

            Debug.LogError($"[QuestConditionFactory] 매핑되지 않은 조건 타입: {definition.conditionType}");
            return null;
        }
        #endregion

        #region 내부 메서드
        /// <summary>
        /// [기능]: 등록된 어셈블리 맵에서 조건 타입을 찾아 IQuestCondition 객체로 실질 생성 및 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 생성자 인자에 questId 추가 전달
        /// </summary>
        private IQuestCondition CreateInternal(ConditionDefinitionSO.ConditionType type, int targetValue, string eventId, string questId)
        {
            if (m_registry.TryGetValue(type, out Type conditionType))
            {
                return (IQuestCondition)Activator.CreateInstance(conditionType, targetValue, m_saveSystem, m_timeProvider, eventId, questId);
            }

            Debug.LogError($"[QuestConditionFactory] 매핑되지 않은 조건 타입: {type}");
            return null;
        }
        #endregion
    }
}
