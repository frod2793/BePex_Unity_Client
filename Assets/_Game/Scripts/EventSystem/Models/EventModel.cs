using System;
using System.Collections.Generic;
using UnityEngine;
using BePex.EventSystem.Interfaces;
using BePex.EventSystem.Data;
using BePex.EventSystem.Factories;
using BePex.EventSystem.DTOs;

namespace BePex.EventSystem.Models
{
    /// <summary>
    /// [기능]: 전체 이벤트 기획 데이터를 관리하고 이벤트 진행 진척도 수정 및 보상 지급 판단을 중개하는 핵심 도메인 모델.
    /// [작성자]: 윤승종
    /// </summary>
    public class EventModel
    {
        #region 내부 필드
        private readonly EventTableDTO m_eventTable;
        private readonly ConditionFactory m_conditionFactory;
        private readonly RewardFactory m_rewardFactory;

        private readonly List<EventDefinitionDTO> m_activeEvents;
        private readonly Dictionary<string, IEventCondition> m_conditions;
        private readonly Dictionary<string, List<IEventReward>> m_rewards;
        private readonly ITimeProvider m_timeProvider;
        #endregion

        #region 이벤트 (Observer)
        public event Action<string> OnEventProgressChanged;
        public event Action<string> OnEventRewardClaimed;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 테이블 DTO 데이터 및 팩토리 인스턴스, ITimeProvider를 수동 DI 주입받고 초기 이벤트를 캐싱 처리하는 생성자.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: ITimeProvider 의존성 주입 추가
        /// </summary>
        public EventModel(EventTableDTO eventTable, ConditionFactory conditionFactory, RewardFactory rewardFactory, ITimeProvider timeProvider)
        {
            m_eventTable = eventTable;
            m_conditionFactory = conditionFactory;
            m_rewardFactory = rewardFactory;
            m_timeProvider = timeProvider;

            m_activeEvents = new List<EventDefinitionDTO>();
            m_conditions = new Dictionary<string, IEventCondition>();
            m_rewards = new Dictionary<string, List<IEventReward>>();

            Reload();
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: DTO 테이블 데이터를 검사하여 목록을 갱신하고 각 이벤트의 조건 및 보상 전략 객체를 생성해 바인딩합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: DTO 리스트 기반으로 데이터 순회 및 조건/보상 매핑 로직 갱신
        /// </summary>
        public void Reload()
        {
            m_activeEvents.Clear();
            m_conditions.Clear();
            m_rewards.Clear();

            if (m_eventTable == null || m_eventTable.events == null)
            {
                return;
            }

            for (int i = 0; i < m_eventTable.events.Count; i++)
            {
                var definition = m_eventTable.events[i];
                if (definition == null)
                {
                    continue;
                }

                m_activeEvents.Add(definition);

                // 조건 전략 인스턴스 팩토리 위임 생성 (DTO 버전에 맞춰 필드 참조를 소문자로 변경)
                var cond = m_conditionFactory.Create(definition.condition, definition.eventId);
                if (cond != null)
                {
                    m_conditions[definition.eventId] = cond;
                }

                // 보상 전략 리스트 팩토리 위임 생성 (DTO 버전에 맞춰 필드 참조를 소문자로 변경)
                var rewardList = new List<IEventReward>();
                if (definition.rewards != null)
                {
                    for (int j = 0; j < definition.rewards.Count; j++)
                    {
                        var rew = m_rewardFactory.Create(definition.rewards[j]);
                        if (rew != null)
                        {
                            rewardList.Add(rew);
                        }
                    }
                }
                m_rewards[definition.eventId] = rewardList;
            }
        }

        /// <summary>
        /// [기능]: 로드되어 활성화된 모든 이벤트 정의 리스트 중 현재 시간(ITimeProvider 기준)에 유효한 이벤트만 필터링하여 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: startDate 및 endDate를 기반으로 기간 만료 체크 필터링 로직 추가
        /// </summary>
        public List<EventDefinitionDTO> GetActiveEvents()
        {
            if (m_timeProvider == null) return m_activeEvents;
            DateTime currentTime = m_timeProvider.GetCurrentTime();
            var validEvents = new List<EventDefinitionDTO>();

            for (int i = 0; i < m_activeEvents.Count; i++)
            {
                var evt = m_activeEvents[i];
                bool isValid = true;

                if (!string.IsNullOrEmpty(evt.startDate) && DateTime.TryParse(evt.startDate, out DateTime startDt))
                {
                    if (currentTime < startDt) isValid = false;
                }

                if (!string.IsNullOrEmpty(evt.endDate) && DateTime.TryParse(evt.endDate, out DateTime endDt))
                {
                    // endDate의 자정(0시 0분)까지를 기한으로 간주한다면, 보통 끝나는 날의 23:59:59를 포함시키기 위해 Date 비교만 수행하거나 시간을 추가 처리합니다.
                    // 단순화를 위해 endDt에 +1일 하여 그 전까지 유효하다고 칩니다 (예: 6월 7일까지면 6월 8일 0시 전까지).
                    if (currentTime >= endDt.Date.AddDays(1)) isValid = false;
                }

                if (isValid)
                {
                    validEvents.Add(evt);
                }
            }

            return validEvents;
        }

        /// <summary>
        /// [기능]: 특정 이벤트에 매핑된 조건 객체를 찾아 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public IEventCondition GetCondition(string eventId)
        {
            if (m_conditions.ContainsKey(eventId))
            {
                return m_conditions[eventId];
            }
            return null;
        }

        /// <summary>
        /// [기능]: 특정 이벤트에 매핑된 지급 보상 리스트를 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public List<IEventReward> GetRewards(string eventId)
        {
            if (m_rewards.ContainsKey(eventId))
            {
                return m_rewards[eventId];
            }
            return new List<IEventReward>();
        }

        /// <summary>
        /// [기능]: 개발 환경 및 치트 조작용으로 진행 수치를 비동기로 가산 처리 및 영속화하고 이벤트를 통지합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Awaitable 비동기 인터페이스로 갱신
        /// </summary>
        public async Awaitable Debug_AddProgressAsync(string eventId, int amount, ISaveSystem saveSystem)
        {
            if (saveSystem == null)
            {
                return;
            }

            var progress = await saveSystem.LoadProgressAsync(eventId);
            var cond = GetCondition(eventId);
            if (cond != null && !cond.CanAddProgress(progress))
            {
                return;
            }

            progress.currentProgress += amount;
            progress.lastUpdatedTicks = m_timeProvider != null ? m_timeProvider.GetCurrentTime().Ticks : System.DateTime.Now.Ticks;

            if (cond != null && progress.currentProgress >= cond.GetTargetValue())
            {
                progress.isCompleted = true;
            }

            await saveSystem.SaveProgressAsync(eventId, progress);
            OnEventProgressChanged?.Invoke(eventId);
        }

        /// <summary>
        /// [기능]: 특정 이벤트의 보상 청구를 집행하고 플레이어 자산 데이터에 Grant를 가한 뒤 비동기로 최종 세이브 처리를 수행합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Awaitable 비동기 인터페이스로 갱신
        /// </summary>
        public async Awaitable<bool> ClaimRewardAsync(string eventId, ISaveSystem saveSystem, PlayerRewardModel playerReward)
        {
            if (saveSystem == null || playerReward == null)
            {
                return false;
            }

            var progress = await saveSystem.LoadProgressAsync(eventId);
            if (progress.isCompleted == false || progress.isRewardClaimed == true)
            {
                return false;
            }

            var list = GetRewards(eventId);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null)
                {
                    list[i].Grant(playerReward);
                }
            }

            if (playerReward.claimedEventIds.Contains(eventId) == false)
            {
                playerReward.claimedEventIds.Add(eventId);
            }

            progress.isRewardClaimed = true;
            await saveSystem.SaveProgressAsync(eventId, progress);
            await saveSystem.SaveRewardStateAsync(playerReward);

            OnEventRewardClaimed?.Invoke(eventId);
            return true;
        }
        #endregion
    }
}
