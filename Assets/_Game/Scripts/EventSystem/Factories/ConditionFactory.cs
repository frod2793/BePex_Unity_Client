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
    /// [기능]: ConditionDefinitionSO 기획 데이터를 기반으로 알맞은 IEventCondition 전략 객체를 생성 및 바인딩해 주는 Factory 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    public class ConditionFactory
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
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: ITimeProvider 의존성 주입 추가
        /// </summary>
        public ConditionFactory(ISaveSystem saveSystem, ITimeProvider timeProvider)
        {
            m_saveSystem = saveSystem;
            m_timeProvider = timeProvider;
            m_registry = new Dictionary<ConditionDefinitionSO.ConditionType, Type>();
            BuildRegistry();
        }

        /// <summary>
        /// [기능]: 리플렉션을 통해 어셈블리 내 해당 어트리뷰트 타입 전략 클래스들을 레지스트리에 자동 등록합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: XML 주석 보완
        /// </summary>
        private void BuildRegistry()
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var type in assembly.GetTypes())
            {
                if (typeof(IEventCondition).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                {
                    var attr = type.GetCustomAttribute<EventConditionAttribute>();
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
        /// [기능]: 조건 SO 정의 파일 및 이벤트 ID를 토대로 레지스트리에 매핑된 타입의 인스턴스를 동적 생성합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: CreateInternal 헬퍼 메서드로 생성 위임 리팩토링
        /// </summary>
        public IEventCondition Create(ConditionDefinitionSO definition, string eventId)
        {
            if (definition == null)
            {
                return null;
            }

            return CreateInternal(definition.Type, definition.TargetValue, eventId);
        }

        /// <summary>
        /// [기능]: 조건 DTO 데이터를 토대로 레지스트리에 매핑된 타입의 인스턴스를 동적 생성합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: CreateInternal 헬퍼 메서드로 생성 위임 리팩토링
        /// </summary>
        public IEventCondition Create(ConditionDefinitionDTO definition, string eventId)
        {
            if (definition == null)
            {
                return null;
            }

            if (Enum.TryParse(definition.conditionType, out ConditionDefinitionSO.ConditionType typeEnum))
            {
                return CreateInternal(typeEnum, definition.targetValue, eventId);
            }

            Debug.LogError($"[ConditionFactory] 매핑되지 않은 조건 타입: {definition.conditionType}");
            return null;
        }
        #endregion

        #region 내부 메서드
        /// <summary>
        /// [기능]: 등록된 어셈블리 맵에서 조건 타입을 찾아 IEventCondition 객체로 실질 생성 및 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        private IEventCondition CreateInternal(ConditionDefinitionSO.ConditionType type, int targetValue, string eventId)
        {
            if (m_registry.TryGetValue(type, out Type conditionType))
            {
                return (IEventCondition)Activator.CreateInstance(conditionType, targetValue, m_saveSystem, m_timeProvider, eventId);
            }

            Debug.LogError($"[ConditionFactory] 매핑되지 않은 조건 타입: {type}");
            return null;
        }
        #endregion
    }
}
