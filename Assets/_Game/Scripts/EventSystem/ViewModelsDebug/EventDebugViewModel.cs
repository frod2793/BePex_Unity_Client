using UnityEngine;
using BePex.EventSystem.Models;
using BePex.EventSystem.Interfaces;
using BePex.EventSystem.DTOs;
using BePex.EventSystem.ViewModels;
using BePex.EventSystem.Data;

namespace BePex.EventSystem.ViewModelsDebug
{
    /// <summary>
    /// [기능]: 테스트 환경에서 인위적으로 조건 수치를 더하고 세이브 상태를 조작/초기화하여 이벤트를 시뮬레이션하는 Debug 전용 ViewModel 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    public class EventDebugViewModel
    {
        #region 내부 필드
        private readonly EventModel m_eventModel;
        private readonly ISaveSystem m_saveSystem;
        private readonly ITimeProvider m_timeProvider;
        private readonly PlayerRewardModel m_playerReward;
        private readonly CurrencyHUDViewModel m_hudViewModel;
        private readonly ConditionTypeRegistrySO m_conditionTypeRegistry;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 비즈니스 로직 모델, 저장 상태 장치, 시간 제공자, 보상 모델 및 HUD 뷰모델을 주입받는 생성자.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Type Object 패턴을 위한 ConditionTypeRegistrySO 주입 추가 및 기본값 설정
        /// </summary>
        public EventDebugViewModel(EventModel eventModel, ISaveSystem saveSystem, ITimeProvider timeProvider, PlayerRewardModel playerReward, CurrencyHUDViewModel hudViewModel, ConditionTypeRegistrySO conditionTypeRegistry = null)
        {
            m_eventModel = eventModel;
            m_saveSystem = saveSystem;
            m_timeProvider = timeProvider;
            m_playerReward = playerReward;
            m_hudViewModel = hudViewModel;
            m_conditionTypeRegistry = conditionTypeRegistry;
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 지정 이벤트 ID와 퀘스트 ID에 대해 인위적인 가산 처리를 도메인 단에 하달해 게이지 변화 및 클리어 상태를 비동기로 모의 조작합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: questId 매개변수 적용 및 전달
        /// </summary>
        public async Awaitable SimulateAddProgressAsync(string eventId, string questId, int amount)
        {
            if (m_eventModel != null)
            {
                await m_eventModel.Debug_AddProgressAsync(eventId, questId, amount, m_saveSystem);
            }
        }

        /// <summary>
        /// [기능]: 현재 로드된 활성 이벤트 목록을 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 추가
        /// </summary>
        public System.Collections.Generic.List<EventDefinitionDTO> GetActiveEvents()
        {
            return m_eventModel != null ? m_eventModel.GetActiveEvents() : new System.Collections.Generic.List<EventDefinitionDTO>();
        }

        /// <summary>
        /// [기능]: 세이브 데이터를 비동기로 완전히 밀어버리고 모델을 재적재(Reload)하여 화면 상태를 공백으로 되돌립니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Awaitable 비동기 인터페이스로 갱신
        /// </summary>
        public async Awaitable ResetAllDataAsync()
        {
            if (m_saveSystem != null && m_eventModel != null)
            {
                await m_saveSystem.ClearAllAsync();
                m_eventModel.Reload();
                if (m_hudViewModel != null)
                {
                    m_hudViewModel.NotifyCurrencyChanged();
                }
            }
        }

        /// <summary>
        /// [기능]: 디버그 환경에서 지정된 일수만큼 가상 시간을 오프셋하고 모델을 재적재합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 타임 시뮬레이션 기능 신규 추가
        /// </summary>
        public async Awaitable SimulateTimeOffsetAsync(int days)
        {
            if (m_timeProvider is BePex.EventSystem.Infrastructure.DebugTimeProvider debugTime)
            {
                for (int d = 0; d < days; d++)
                {
                    // 1. 가상 시간 1일 증가 및 이벤트 테이블 리로드
                    debugTime.AddDays(1);
                    if (m_eventModel != null)
                    {
                        m_eventModel.Reload();
                    }

                    // 2. 가상 출석체크 행동 모의 연동
                    await SimulateAttendanceInternalAsync();
                }
            }
        }

        /// <summary>
        /// [기능]: 가상 시간에 적용된 모든 시간 오프셋을 초기화하고 이벤트를 다시 로드합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 시간 오프셋 리셋 기능 추가
        /// </summary>
        public void ResetTimeOffset()
        {
            if (m_timeProvider is BePex.EventSystem.Infrastructure.DebugTimeProvider debugTime)
            {
                debugTime.ResetOffset();
                if (m_eventModel != null)
                {
                    m_eventModel.Reload();
                }
            }
        }

        /// <summary>
        /// [기능]: 시스템에 등록된 모든 조건/행동 타입의 이름 목록을 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Type Object 레지스트리를 통한 동적 타입 이름 조회 적용
        /// </summary>
        public string[] GetAvailableActionTypes()
        {
            if (m_conditionTypeRegistry != null && m_conditionTypeRegistry.ConditionTypes != null)
            {
                var types = m_conditionTypeRegistry.ConditionTypes;
                var validNames = new System.Collections.Generic.List<string>();
                for (int i = 0; i < types.Count; i++)
                {
                    if (types[i] != null && !string.IsNullOrEmpty(types[i].TypeName))
                    {
                        // 출석체크(Attendance)는 날짜 추가 버튼과 통합하므로 동적 리스트 버튼 생성에서 제외
                        if (types[i].TypeName != "Attendance")
                        {
                            validNames.Add(types[i].TypeName);
                        }
                    }
                }
                return validNames.ToArray();
            }
            return System.Array.Empty<string>();
        }

        /// <summary>
        /// [기능]: 특정 타입의 행동이 발생했다고 모의하여, 조건 타입이 일치하는 모든 활성 이벤트 하위의 퀘스트 진척도를 일괄 안전하게 가산합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Attendance 타입 시뮬레이션 시 가상 시간을 자동으로 +1일 경과시킨 뒤 진행도를 가산하도록 변경
        /// </summary>
        public async Awaitable SimulateActionAsync(string actionType)
        {
            if (actionType == "Attendance")
            {
                // 테스트 코드나 호환성을 위해 직접 SimulateActionAsync("Attendance")를 부를 경우의 예외 처리
                if (m_timeProvider is BePex.EventSystem.Infrastructure.DebugTimeProvider debugTime)
                {
                    debugTime.AddDays(1);
                    if (m_eventModel != null)
                    {
                        m_eventModel.Reload();
                    }
                }
                await SimulateAttendanceInternalAsync();
                return;
            }

            var events = GetActiveEvents();
            for (int i = 0; i < events.Count; i++)
            {
                var evt = events[i];
                if (evt.quests != null)
                {
                    // 이벤트 단위로 progress 데이터를 단 1회 로드합니다.
                    var progress = await m_saveSystem.LoadProgressAsync(evt.eventId);
                    bool isAnyProgressAdded = false;

                    // 동일 조건에 매핑된 하위 퀘스트 진행도를 메모리 상에서 모두 누적합니다.
                    for (int j = 0; j < evt.quests.Count; j++)
                    {
                        var quest = evt.quests[j];
                        if (quest.condition != null && quest.condition.conditionType == actionType)
                        {
                            m_eventModel.Debug_AddProgressNoSave(evt.eventId, quest.questId, 1, progress);
                            isAnyProgressAdded = true;
                        }
                    }

                    // 메모리 상의 연산이 끝난 후, 단 1회만 세이브 파일에 씁니다.
                    if (isAnyProgressAdded)
                    {
                        await m_saveSystem.SaveProgressAsync(evt.eventId, progress);
                        m_eventModel.TriggerProgressChanged(evt.eventId); // 변경 사항 UI 일제 전파
                    }
                }
            }

            if (m_hudViewModel != null)
            {
                m_hudViewModel.NotifyCurrencyChanged();
            }
        }

        /// <summary>
        /// [기능]: 가상 시간의 직접 가산 처리 없이 활성화된 출석체크(Attendance) 퀘스트의 진행도를 안전하게 1씩 가산하는 내부 비동기 헬퍼 메서드.
        /// [작성자]: 윤승종
        /// </summary>
        private async Awaitable SimulateAttendanceInternalAsync()
        {
            var events = GetActiveEvents();
            for (int i = 0; i < events.Count; i++)
            {
                var evt = events[i];
                if (evt.quests != null)
                {
                    var progress = await m_saveSystem.LoadProgressAsync(evt.eventId);
                    bool isAnyProgressAdded = false;

                    for (int j = 0; j < evt.quests.Count; j++)
                    {
                        var quest = evt.quests[j];
                        if (quest.condition != null && quest.condition.conditionType == "Attendance")
                        {
                            m_eventModel.Debug_AddProgressNoSave(evt.eventId, quest.questId, 1, progress);
                            isAnyProgressAdded = true;
                        }
                    }

                    if (isAnyProgressAdded)
                    {
                        await m_saveSystem.SaveProgressAsync(evt.eventId, progress);
                        m_eventModel.TriggerProgressChanged(evt.eventId); // 변경 사항 UI 전파
                    }
                }
            }

            if (m_hudViewModel != null)
            {
                m_hudViewModel.NotifyCurrencyChanged();
            }
        }

        /// <summary>
        /// [기능]: 플레이어 리워드 모델의 자산 상태 딕셔너리를 OCP 기반 안전 방식으로 조회하여 반환합니다. (리플렉션 완전 제거)
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 리플렉션 조회를 GetBalances API로 대체
        /// </summary>
        public System.Collections.Generic.Dictionary<string, int> GetRewardStatus()
        {
            if (m_playerReward == null)
            {
                return new System.Collections.Generic.Dictionary<string, int>();
            }
            return m_playerReward.GetBalances();
        }

        /// <summary>
        /// [기능]: 디버그 환경에서 인위적으로 플레이어의 포인트를 소모하고 저장소에 저장 및 UI를 갱신합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: PlayerRewardModel.TrySpendCurrency 캡슐화 가드 적용
        /// </summary>
        public async Awaitable SimulateSpendPointsAsync(int amount)
        {
            if (m_playerReward != null && m_saveSystem != null)
            {
                m_playerReward.TrySpendCurrency("Point", amount);
                await m_saveSystem.SaveRewardStateAsync(m_playerReward);
                if (m_hudViewModel != null)
                {
                    m_hudViewModel.NotifyCurrencyChanged();
                }
            }
        }

        /// <summary>
        /// [기능]: 디버그 환경에서 인위적으로 플레이어의 시즌 포인트를 소모하고 저장소에 저장 및 UI를 갱신합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: PlayerRewardModel.TrySpendCurrency 캡슐화 가드 적용
        /// </summary>
        public async Awaitable SimulateSpendSeasonPointsAsync(int amount)
        {
            if (m_playerReward != null && m_saveSystem != null)
            {
                m_playerReward.TrySpendCurrency("SeasonPoint", amount);
                await m_saveSystem.SaveRewardStateAsync(m_playerReward);
                if (m_hudViewModel != null)
                {
                    m_hudViewModel.NotifyCurrencyChanged();
                }
            }
        }

        /// <summary>
        /// [기능]: 디버그 환경에서 인위적으로 플레이어의 크레딧(재화)을 소모하고 저장소에 저장 및 UI를 갱신합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: PlayerRewardModel.TrySpendCurrency 캡슐화 가드 적용
        /// </summary>
        public async Awaitable SimulateSpendCreditsAsync(int amount)
        {
            if (m_playerReward != null && m_saveSystem != null)
            {
                m_playerReward.TrySpendCurrency("CreditReward", amount);
                await m_saveSystem.SaveRewardStateAsync(m_playerReward);
                if (m_hudViewModel != null)
                {
                    m_hudViewModel.NotifyCurrencyChanged();
                }
            }
        }
        #endregion
    }
}
