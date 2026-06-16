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
    /// [기능]: RewardDefinitionSO 기획 데이터를 기반으로 알맞은 IQuestReward 전략 객체를 생성해 주는 Factory 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    public class QuestRewardFactory
    {
        #region 내부 필드
        private readonly Dictionary<string, Type> m_registry;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 리플렉션을 통한 보상 클래스 자동 등록을 수행합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Quest-related 네이밍 적용
        /// </summary>
        public QuestRewardFactory()
        {
            m_registry = new Dictionary<string, Type>();
            BuildRegistry();
        }

        /// <summary>
        /// [기능]: 리플렉션을 통해 어셈블리 내 해당 어트리뷰트 타입 전략 클래스들을 레지스트리에 자동 등록합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: IQuestReward 및 QuestRewardAttribute 검색 적용
        /// </summary>
        private void BuildRegistry()
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var type in assembly.GetTypes())
            {
                if (typeof(IQuestReward).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                {
                    var attr = type.GetCustomAttribute<QuestRewardAttribute>();
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
        /// [기능]: 보상 SO 설정 데이터를 토대로 해당하는 경험치, 티켓, 포인트 보상 객체를 동적 생성해 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: IQuestReward 반환 타입 명칭 변경
        /// </summary>
        public IQuestReward Create(RewardDefinitionSO definition)
        {
            if (definition == null)
            {
                return null;
            }

            string typeName = string.Empty;
            if (definition.Type != null)
            {
                typeName = definition.Type.TypeName;
            }

            return CreateInternal(typeName, definition.Amount, definition.DisplayName);
        }

        /// <summary>
        /// [기능]: 보상 DTO 데이터를 토대로 레지스트리에 매핑된 타입의 인스턴스를 동적 생성합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: IQuestReward 반환 타입 명칭 변경
        /// </summary>
        public IQuestReward Create(RewardDefinitionDTO definition)
        {
            if (definition == null)
            {
                return null;
            }

            return CreateInternal(definition.rewardType, definition.amount, definition.displayName);
        }
        #endregion

        #region 내부 메서드
        /// <summary>
        /// [기능]: 등록된 어셈블리 맵에서 보상 타입을 찾아 IQuestReward 객체로 실질 생성 및 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: IQuestReward 반환 적용
        /// </summary>
        private IQuestReward CreateInternal(string typeName, int amount, string displayName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            if (m_registry.TryGetValue(typeName, out Type rewardType))
            {
                return (IQuestReward)Activator.CreateInstance(rewardType, amount, displayName);
            }

            Debug.LogError($"[QuestRewardFactory] 매핑되지 않은 보상 타입: {typeName}");
            return null;
        }
        #endregion
    }
}
