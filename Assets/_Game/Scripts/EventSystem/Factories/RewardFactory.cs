using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using BePex.EventSystem.Interfaces;
using BePex.EventSystem.Data;
using BePex.EventSystem.Rewards;
using BePex.EventSystem.DTOs;

namespace BePex.EventSystem.Factories
{
    /// <summary>
    /// [기능]: RewardDefinitionSO 기획 데이터를 기반으로 알맞은 IEventReward 전략 객체를 생성해 주는 Factory 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    public class RewardFactory
    {
        #region 내부 필드
        private readonly Dictionary<RewardDefinitionSO.RewardType, Type> m_registry;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 리플렉션을 통한 보상 클래스 자동 등록을 수행합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Reflection 기반 자동화 팩토리로 개편
        /// </summary>
        public RewardFactory()
        {
            m_registry = new Dictionary<RewardDefinitionSO.RewardType, Type>();
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
                if (typeof(IEventReward).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                {
                    var attr = type.GetCustomAttribute<EventRewardAttribute>();
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
        /// [기능]: 보상 SO 설정 데이터를 토대로 해당하는 경험치, 티켓, 포인트 보상 객체를 동적 생성해 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Reflection 레지스트리 기반 생성으로 갱신
        /// </summary>
        public IEventReward Create(RewardDefinitionSO definition)
        {
            if (definition == null)
            {
                return null;
            }

            if (m_registry.TryGetValue(definition.Type, out Type rewardType))
            {
                return (IEventReward)Activator.CreateInstance(rewardType, definition.Amount, definition.DisplayName);
            }

            Debug.LogError($"[RewardFactory] 매핑되지 않은 보상 타입: {definition.Type}");
            return null;
        }

        /// <summary>
        /// [기능]: 보상 DTO 데이터를 토대로 레지스트리에 매핑된 타입의 인스턴스를 동적 생성합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: DTO 지원을 위해 문자열 매핑 로직을 추가하여 작성
        /// </summary>
        public IEventReward Create(RewardDefinitionDTO definition)
        {
            if (definition == null)
            {
                return null;
            }

            if (Enum.TryParse(definition.rewardType, out RewardDefinitionSO.RewardType typeEnum))
            {
                if (m_registry.TryGetValue(typeEnum, out Type rewardType))
                {
                    return (IEventReward)Activator.CreateInstance(rewardType, definition.amount, definition.displayName);
                }
            }

            Debug.LogError($"[RewardFactory] 매핑되지 않은 보상 타입: {definition.rewardType}");
            return null;
        }
        #endregion
    }
}
