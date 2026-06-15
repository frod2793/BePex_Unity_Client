using System;
using UnityEngine;
using BePex.EventSystem.Models;
using BePex.EventSystem.Interfaces;
using BePex.EventSystem.DTOs;
using BePex.EventSystem.ViewModels;

namespace BePex.EventSystem.ViewModelsDebug
{
    /// <summary>
    /// [기능]: 테스트 환경에서 인위적으로 조건 수치를 더하고 세이브 상태를 조작/초기화하여 이벤트를 시뮬레이션하는 Debug 전용 ViewModel 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    public class EventDebugViewModel : IDisposable
    {
        #region 내부 필드
        private readonly EventModel m_eventModel;
        private readonly ISaveSystem m_saveSystem;
        private readonly ITimeProvider m_timeProvider;
        private readonly PlayerRewardModel m_playerReward;
        private readonly CurrencyHUDViewModel m_hudViewModel;
        #endregion

        #region 이벤트 (Observer)
        public event Action OnStatusChanged;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 비즈니스 로직 모델, 저장 상태 장치, 시간 제공자, 보상 모델 및 HUD 뷰모델을 주입받는 생성자. 재화 갱신 이벤트를 연결합니다.
        /// [작성자]: 윤승종
        /// </summary>
        public EventDebugViewModel(EventModel eventModel, ISaveSystem saveSystem, ITimeProvider timeProvider, PlayerRewardModel playerReward, CurrencyHUDViewModel hudViewModel)
        {
            m_eventModel = eventModel;
            m_saveSystem = saveSystem;
            m_timeProvider = timeProvider;
            m_playerReward = playerReward;
            m_hudViewModel = hudViewModel;

            if (m_hudViewModel != null)
            {
                m_hudViewModel.OnCurrencyChanged += func_HandleCurrencyChanged;
            }
        }

        /// <summary>
        /// [기능]: HUD 뷰모델의 재화 상태 변화 이벤트를 감지하여 디버그 뷰 단으로 갱신 통지를 중계합니다.
        /// [작성자]: 윤승종
        /// </summary>
        private void func_HandleCurrencyChanged()
        {
            OnStatusChanged?.Invoke();
        }

        /// <summary>
        /// [기능]: 등록된 이벤트를 해제하여 메모리 누수를 사전에 예방합니다.
        /// [작성자]: 윤승종
        /// </summary>
        public void Dispose()
        {
            if (m_hudViewModel != null)
            {
                m_hudViewModel.OnCurrencyChanged -= func_HandleCurrencyChanged;
            }
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 지정 이벤트 ID에 대해 인위적인 가산 처리를 도메인 단에 하달해 게이지 변화 및 클리어 상태를 비동기로 모의 조작합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Awaitable 비동기 인터페이스로 갱신
        /// </summary>
        public async Awaitable SimulateAddProgressAsync(string eventId, int amount)
        {
            if (m_eventModel != null)
            {
                await m_eventModel.Debug_AddProgressAsync(eventId, amount, m_saveSystem);
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
        public void SimulateTimeOffset(int days)
        {
            if (m_timeProvider is BePex.EventSystem.Infrastructure.DebugTimeProvider debugTime)
            {
                debugTime.AddDays(days);
                m_eventModel?.Reload();
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
                m_eventModel?.Reload();
            }
        }

        /// <summary>
        /// [기능]: 시스템에 등록된 모든 조건/행동 타입의 이름 목록을 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: OCP 기반 동적 UI 생성을 위한 조회 메서드 추가
        /// </summary>
        public string[] GetAvailableActionTypes()
        {
            return System.Enum.GetNames(typeof(BePex.EventSystem.Data.ConditionDefinitionSO.ConditionType));
        }

        /// <summary>
        /// [기능]: 특정 타입의 행동이 발생했다고 모의하여, 조건 타입이 일치하는 모든 활성 이벤트의 진척도를 가산합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 다중 이벤트 동시 처리 시뮬레이션 신규 추가
        /// </summary>
        public async Awaitable SimulateActionAsync(string actionType)
        {
            var events = GetActiveEvents();
            for (int i = 0; i < events.Count; i++)
            {
                var evt = events[i];
                if (evt.condition != null && evt.condition.conditionType == actionType)
                {
                    await m_eventModel.Debug_AddProgressAsync(evt.eventId, 1, m_saveSystem);
                }
            }
            if (m_hudViewModel != null)
            {
                m_hudViewModel.NotifyCurrencyChanged();
            }
        }

        /// <summary>
        /// [기능]: 플레이어 리워드 모델의 수치형 자산들을 리플렉션으로 읽어들여 딕셔너리로 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: OCP 기반 재화 동적 모니터링을 위한 조회 메서드 추가
        /// </summary>
        public System.Collections.Generic.Dictionary<string, int> GetRewardStatus()
        {
            var result = new System.Collections.Generic.Dictionary<string, int>();
            if (m_playerReward == null)
            {
                return result;
            }

            var fields = typeof(PlayerRewardModel).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].FieldType == typeof(int))
                {
                    string displayName = fields[i].Name switch
                    {
                        "totalExp" => "경험치",
                        "totalTickets" => "티켓",
                        "totalPoints" => "이벤트 포인트",
                        "totalSeasonPoints" => "시즌 포인트",
                        "totalCredits" => "재화",
                        _ => fields[i].Name
                    };
                    result[displayName] = (int)fields[i].GetValue(m_playerReward);
                }
            }
            return result;
        }

        /// <summary>
        /// [기능]: 임의의 포인트를 소모(교환)하고 실시간 세이브 영속성 적용 및 HUD를 갱신합니다.
        /// [작성자]: 윤승종
        /// </summary>
        public async Awaitable SimulateSpendPointsAsync(int amount)
        {
            if (m_playerReward != null && m_playerReward.totalPoints >= amount)
            {
                m_playerReward.totalPoints -= amount;
                if (m_saveSystem != null)
                {
                    await m_saveSystem.SaveRewardStateAsync(m_playerReward);
                }
                if (m_hudViewModel != null)
                {
                    m_hudViewModel.NotifyCurrencyChanged();
                }
            }
        }

        /// <summary>
        /// [기능]: 임의의 시즌 포인트를 소모하고 실시간 세이브 영속성 적용 및 HUD를 갱신합니다.
        /// [작성자]: 윤승종
        /// </summary>
        public async Awaitable SimulateSpendSeasonPointsAsync(int amount)
        {
            if (m_playerReward != null && m_playerReward.totalSeasonPoints >= amount)
            {
                m_playerReward.totalSeasonPoints -= amount;
                if (m_saveSystem != null)
                {
                    await m_saveSystem.SaveRewardStateAsync(m_playerReward);
                }
                if (m_hudViewModel != null)
                {
                    m_hudViewModel.NotifyCurrencyChanged();
                }
            }
        }

        /// <summary>
        /// [기능]: 임의의 코인성 재화(Credits)를 소모하고 실시간 세이브 영속성 적용 및 HUD를 갱신합니다.
        /// [작성자]: 윤승종
        /// </summary>
        public async Awaitable SimulateSpendCreditsAsync(int amount)
        {
            if (m_playerReward != null && m_playerReward.totalCredits >= amount)
            {
                m_playerReward.totalCredits -= amount;
                if (m_saveSystem != null)
                {
                    await m_saveSystem.SaveRewardStateAsync(m_playerReward);
                }
                if (m_hudViewModel != null)
                {
                    m_hudViewModel.NotifyCurrencyChanged();
                }
            }
        }
        #endregion
    }
}
