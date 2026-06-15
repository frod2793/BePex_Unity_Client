using System;
using System.Collections.Generic;
using UnityEngine;
using BePex.EventSystem.Interfaces;
using BePex.EventSystem.Models;
using BePex.EventSystem.Data;
using BePex.EventSystem.DTOs;

namespace BePex.EventSystem.ViewModels
{
    /// <summary>
    /// [기능]: 활성화된 이벤트들의 목록 갱신 및 선택 상호작용 상태를 전달하기 위한 ViewModel 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    public class EventListViewModel
    {
        #region 내부 필드
        private readonly EventModel m_eventModel;
        private readonly ISaveSystem m_saveSystem;
        private string m_selectedEventId;
        #endregion

        #region 이벤트 (Observer)
        public event Action OnListUpdated;
        public event Action<string> OnEventSelected;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 비즈니스 로직 도메인 모델과 영속성 세이브 시스템을 주입받고 진행도 및 수령 이벤트를 수신하여 리스트 업데이트를 요청합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: ISaveSystem 주입 및 캐싱 추가
        /// </summary>
        public EventListViewModel(EventModel eventModel, ISaveSystem saveSystem)
        {
            m_eventModel = eventModel;
            m_saveSystem = saveSystem;
            m_eventModel.OnEventProgressChanged += HandleEventProgressChanged;
            m_eventModel.OnEventRewardClaimed += HandleEventRewardClaimed;
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 현재 활성 상태의 모든 이벤트 리스트를 도메인 모델로부터 가공 수집해 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 반환 형식을 EventDefinitionDTO로 변경
        /// </summary>
        public List<EventDefinitionDTO> GetEvents()
        {
            return m_eventModel.GetActiveEvents();
        }

        /// <summary>
        /// [기능]: 특정 이벤트 ID 셀이 선택되었을 때 상태를 변경하고 선택 이벤트를 외부 View들에 전달합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public void SelectEvent(string eventId)
        {
            m_selectedEventId = eventId;
            OnEventSelected?.Invoke(eventId);
        }

        /// <summary>
        /// [기능]: 현재 선택되어 있는 이벤트 ID를 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public string GetSelectedEventId() => m_selectedEventId;

        /// <summary>
        /// [기능]: 해당 이벤트의 보상이 이미 수령 완료되었는지 비동기로 검증합니다.
        /// [작성자]: 윤승종
        /// </summary>
        public async Awaitable<bool> IsRewardClaimedAsync(string eventId)
        {
            if (m_saveSystem == null)
            {
                return false;
            }
            var progress = await m_saveSystem.LoadProgressAsync(eventId);
            return progress.isRewardClaimed;
        }

        /// <summary>
        /// [기능]: 해당 이벤트가 완료되었으나 보상을 아직 받지 않아 수령 가능한 상태인지 비동기로 검증합니다.
        /// [작성자]: 윤승종
        /// </summary>
        public async Awaitable<bool> CanClaimRewardAsync(string eventId)
        {
            var cond = m_eventModel.GetCondition(eventId);
            if (cond == null)
            {
                return false;
            }

            bool completed = await cond.IsCompletedAsync();
            bool claimed = await IsRewardClaimedAsync(eventId);
            return completed && !claimed;
        }
        #endregion

        #region 이벤트 핸들러
        /// <summary>
        /// [기능]: 이벤트 모델의 진행 상태 변경 통지를 수신해 리스트 갱신 알림을 전송합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: XML 주석 보완
        /// </summary>
        private void HandleEventProgressChanged(string eventId)
        {
            OnListUpdated?.Invoke();
        }

        /// <summary>
        /// [기능]: 이벤트 모델의 보상 수령 변경 통지를 수신해 리스트 갱신 알림을 전송합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: XML 주석 보완
        /// </summary>
        private void HandleEventRewardClaimed(string eventId)
        {
            OnListUpdated?.Invoke();
        }
        #endregion
    }
}
