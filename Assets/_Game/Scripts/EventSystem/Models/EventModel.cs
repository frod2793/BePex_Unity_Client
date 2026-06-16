using System;
using System.Collections.Generic;
using System.Threading;
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
        private readonly QuestConditionFactory m_conditionFactory;
        private readonly QuestRewardFactory m_rewardFactory;

        private readonly List<EventDefinitionDTO> m_activeEvents;
        private readonly Dictionary<string, Dictionary<string, IQuestCondition>> m_conditions;
        private readonly Dictionary<string, Dictionary<string, List<IQuestReward>>> m_rewards;
        private readonly ITimeProvider m_timeProvider;
        #endregion

        #region 이벤트 (Observer)
        public event Action<string> OnEventProgressChanged;
        public event Action<string> OnEventRewardClaimed;
        public event Action OnModelReloaded;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: DTO 및 팩토리 객체들을 주입받아 초기화하고 캐싱용 딕셔너리를 초기화합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Quest-related 네이밍 적용 및 팩토리 주입 타입 변경
        /// </summary>
        public EventModel(EventTableDTO eventTable, QuestConditionFactory conditionFactory, QuestRewardFactory rewardFactory, ITimeProvider timeProvider)
        {
            m_eventTable = eventTable;
            m_conditionFactory = conditionFactory;
            m_rewardFactory = rewardFactory;
            m_timeProvider = timeProvider;

            m_activeEvents = new List<EventDefinitionDTO>();
            m_conditions = new Dictionary<string, Dictionary<string, IQuestCondition>>();
            m_rewards = new Dictionary<string, Dictionary<string, List<IQuestReward>>>();

            Reload();
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: DTO 테이블 데이터를 기반으로 각 이벤트 하위 퀘스트들의 조건 및 보상 전략 객체를 생성하여 바인딩합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 퀘스트 계층구조 파싱 및 생성 로직으로 갱신
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

                var eventId = definition.eventId;
                var eventConditions = new Dictionary<string, IQuestCondition>();
                var eventRewards = new Dictionary<string, List<IQuestReward>>();

                if (definition.quests != null)
                {
                    for (int j = 0; j < definition.quests.Count; j++)
                    {
                        var quest = definition.quests[j];
                        if (quest == null)
                        {
                            continue;
                        }

                        var cond = m_conditionFactory.Create(quest.condition, eventId, quest.questId);
                        if (cond != null)
                        {
                            eventConditions[quest.questId] = cond;
                        }

                        var rewardList = new List<IQuestReward>();
                        if (quest.rewards != null)
                        {
                            for (int k = 0; k < quest.rewards.Count; k++)
                            {
                                var rew = m_rewardFactory.Create(quest.rewards[k]);
                                if (rew != null)
                                {
                                    rewardList.Add(rew);
                                }
                            }
                        }
                        eventRewards[quest.questId] = rewardList;
                    }
                }

                m_conditions[eventId] = eventConditions;
                m_rewards[eventId] = eventRewards;
            }

            OnModelReloaded?.Invoke();
        }

        /// <summary>
        /// [기능]: 활성화된 모든 이벤트 목록 중 현재 시간에 유효한 이벤트만 필터링하여 버퍼 리스트에 채워 넣습니다. 가비지 생성을 방지합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 가비지 생성을 방지하는 NonAlloc 방식으로 전면 개편 및 기존 GetActiveEvents 폐기
        /// </summary>
        public void GetActiveEventsNonAlloc(List<EventDefinitionDTO> resultsBuffer)
        {
            if (resultsBuffer == null)
            {
                return;
            }
            resultsBuffer.Clear();

            if (m_timeProvider == null)
            {
                for (int i = 0; i < m_activeEvents.Count; i++)
                {
                    resultsBuffer.Add(m_activeEvents[i]);
                }
                return;
            }
            DateTime currentTime = m_timeProvider.GetCurrentTime();

            for (int i = 0; i < m_activeEvents.Count; i++)
            {
                var evt = m_activeEvents[i];
                if (evt == null)
                {
                    continue;
                }
                bool isValid = true;

                var startDt = evt.GetStartDateTime();
                if (startDt.HasValue)
                {
                    if (currentTime < startDt.Value)
                    {
                        isValid = false;
                    }
                }

                var endDt = evt.GetEndDateTime();
                if (endDt.HasValue)
                {
                    if (currentTime >= endDt.Value.Date.AddDays(1))
                    {
                        isValid = false;
                    }
                }

                if (isValid)
                {
                    resultsBuffer.Add(evt);
                }
            }
        }

        /// <summary>
        /// [기능]: 특정 이벤트의 특정 퀘스트에 매핑된 조건 객체를 찾아 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: eventId 및 questId 기반으로 탐색하도록 시그니처 갱신
        /// </summary>
        public IQuestCondition GetCondition(string eventId, string questId)
        {
            if (m_conditions.TryGetValue(eventId, out var eventConds))
            {
                if (eventConds.TryGetValue(questId, out var cond))
                {
                    return cond;
                }
            }
            return null;
        }

        /// <summary>
        /// [기능]: 특정 이벤트의 특정 퀘스트에 매핑된 보상 전략 리스트를 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: eventId 및 questId 기반으로 탐색하도록 시그니처 갱신
        /// </summary>
        public List<IQuestReward> GetRewards(string eventId, string questId)
        {
            if (m_rewards.TryGetValue(eventId, out var eventRews))
            {
                if (eventRews.TryGetValue(questId, out var rews))
                {
                    return rews;
                }
            }
            return new List<IQuestReward>();
        }

        /// <summary>
        /// [기능]: 개발 환경용 치트 가산 처리를 퀘스트 단위로 실행하고 영속화 및 변경 통지를 수행합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 취소 제어를 위한 CancellationToken 추가
        /// </summary>
        public async Awaitable Debug_AddProgressAsync(string eventId, string questId, int amount, ISaveSystem saveSystem, CancellationToken cancellationToken = default)
        {
            if (saveSystem == null)
            {
                return;
            }

            var progress = await saveSystem.LoadProgressAsync(eventId, cancellationToken);
            if (progress == null)
            {
                return;
            }

            ApplyProgressCalculation(eventId, questId, amount, progress);

            await saveSystem.SaveProgressAsync(eventId, progress, cancellationToken);
            OnEventProgressChanged?.Invoke(eventId);
        }

        /// <summary>
        /// [기능]: 특정 이벤트 내 개별 퀘스트에 대한 보상 청구를 처리하고 세이브 및 이벤트를 발행합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 취소 제어를 위한 CancellationToken 추가
        /// </summary>
        public async Awaitable<bool> ClaimRewardAsync(string eventId, string questId, ISaveSystem saveSystem, PlayerRewardModel playerReward, CancellationToken cancellationToken = default)
        {
            if (saveSystem == null || playerReward == null)
            {
                return false;
            }

            var progress = await saveSystem.LoadProgressAsync(eventId, cancellationToken);
            if (progress == null || progress.quests == null)
            {
                return false;
            }

            progress.TryGetQuestProgress(questId, out QuestProgressModel targetQuest);

            if (targetQuest == null || targetQuest.isCompleted == false || targetQuest.isRewardClaimed == true)
            {
                return false;
            }

            var list = GetRewards(eventId, questId);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null)
                {
                    list[i].Grant(playerReward);
                }
            }

            string claimKey = $"{eventId}_{questId}";
            if (playerReward.claimedEventIds.Contains(claimKey) == false)
            {
                playerReward.claimedEventIds.Add(claimKey);
            }

            targetQuest.isRewardClaimed = true;
            
            // 두 세이브 호출을 Batch 세이브 단일 호출로 묶어 트랜잭션 보장
            await saveSystem.SaveBatchAsync(eventId, progress, playerReward, cancellationToken);

            OnEventRewardClaimed?.Invoke(eventId);
            return true;
        }

        /// <summary>
        /// [기능]: 특정 이벤트 하위의 완료되었으나 아직 보상을 획득하지 않은 모든 퀘스트에 대한 보상을 일괄 청구합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 취소 제어를 위한 CancellationToken 추가
        /// </summary>
        public async Awaitable<bool> ClaimAllRewardsAsync(string eventId, ISaveSystem saveSystem, PlayerRewardModel playerReward, CancellationToken cancellationToken = default)
        {
            if (saveSystem == null || playerReward == null)
            {
                return false;
            }

            var progress = await saveSystem.LoadProgressAsync(eventId, cancellationToken);
            if (progress == null || progress.quests == null)
            {
                return false;
            }

            bool claimedAny = false;
            for (int i = 0; i < progress.quests.Count; i++)
            {
                var quest = progress.quests[i];
                if (quest.isCompleted && !quest.isRewardClaimed)
                {
                    var list = GetRewards(eventId, quest.questId);
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (list[j] != null)
                        {
                            list[j].Grant(playerReward);
                        }
                    }

                    string claimKey = $"{eventId}_{quest.questId}";
                    if (playerReward.claimedEventIds.Contains(claimKey) == false)
                    {
                        playerReward.claimedEventIds.Add(claimKey);
                    }

                    quest.isRewardClaimed = true;
                    claimedAny = true;
                }
            }

            if (claimedAny)
            {
                // 두 세이브 호출을 Batch 세이브 단일 호출로 묶어 트랜잭션 보장
                await saveSystem.SaveBatchAsync(eventId, progress, playerReward, cancellationToken);
                OnEventRewardClaimed?.Invoke(eventId);
                return true;
            }

            return false;
        }

        /// <summary>
        /// [기능]: 디버그 세이브 레이스 방지를 위해, 세이브 처리 없이 메모리(progress) 상에서만 퀘스트 진행도를 동기 가산합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: ApplyProgressCalculation 헬퍼를 도입하여 중복 비즈니스 로직 제거
        /// </summary>
        public void Debug_AddProgressNoSave(string eventId, string questId, int amount, EventProgressModel progress)
        {
            ApplyProgressCalculation(eventId, questId, amount, progress);
        }

        /// <summary>
        /// [기능]: 지정된 이벤트 진척도 모델 내 특정 퀘스트 진행도 데이터의 검색/생성, CanAddProgress 검사, 진척도 증가, 시간 기록, 완료 도달 판정을 일괄 처리합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// </summary>
        private void ApplyProgressCalculation(string eventId, string questId, int amount, EventProgressModel progress)
        {
            if (progress == null)
            {
                return;
            }

            progress.TryGetQuestProgress(questId, out QuestProgressModel targetQuest);

            if (targetQuest == null)
            {
                targetQuest = new QuestProgressModel
                {
                    questId = questId,
                    currentProgress = 0,
                    isCompleted = false,
                    isRewardClaimed = false,
                    lastUpdatedTicks = 0
                };
                if (progress.quests == null)
                {
                    progress.quests = new List<QuestProgressModel>();
                }
                progress.quests.Add(targetQuest);
            }

            var cond = GetCondition(eventId, questId);
            if (cond != null && !cond.CanAddProgress(progress))
            {
                return;
            }

            targetQuest.currentProgress += amount;
            targetQuest.lastUpdatedTicks = m_timeProvider != null ? m_timeProvider.GetCurrentTime().Ticks : DateTime.Now.Ticks;

            if (cond != null && targetQuest.currentProgress >= cond.GetTargetValue())
            {
                targetQuest.isCompleted = true;
            }
        }

        /// <summary>
        /// [기능]: 진행도가 대량으로 바뀐 뒤 일제히 UI에 진행 변경 이벤트를 발행합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 신설
        /// </summary>
        public void TriggerProgressChanged(string eventId)
        {
            OnEventProgressChanged?.Invoke(eventId);
        }
        #endregion
    }
}
