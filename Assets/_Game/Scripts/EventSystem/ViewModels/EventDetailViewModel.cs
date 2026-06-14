using System;
using UnityEngine;
using BePex.EventSystem.Models;
using BePex.EventSystem.Data;
using BePex.EventSystem.Interfaces;
using BePex.EventSystem.DTOs;
using static UnityEngine.Mathf;

namespace BePex.EventSystem.ViewModels
{
    /// <summary>
    /// [기능]: 선택된 특정 이벤트의 상세 정보 및 진행 슬라이더 수치, 보상 수령 명령 처리를 담당하는 ViewModel 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    public class EventDetailViewModel
    {
        #region 내부 필드
        private readonly EventModel m_eventModel;
        private readonly ISaveSystem m_saveSystem;
        private string m_currentEventId;
        #endregion

        #region 이벤트 (Observer)
        public event Action OnDetailUpdated;
        public event Action<string> OnRewardClaimSuccess;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 도메인 모델과 데이터 영속성 장치를 주입받고 진행 상황 변경 감지 이벤트를 처리하는 생성자.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public EventDetailViewModel(EventModel eventModel, ISaveSystem saveSystem)
        {
            m_eventModel = eventModel;
            m_saveSystem = saveSystem;

            m_eventModel.OnEventProgressChanged += HandleProgressChanged;
            m_eventModel.OnEventRewardClaimed += HandleRewardClaimed;
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 상세 화면을 렌더링할 타겟 이벤트 ID를 갱신하고 화면 동기화를 전달합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public void SetEvent(string eventId)
        {
            m_currentEventId = eventId;
            OnDetailUpdated?.Invoke();
        }

        /// <summary>
        /// [기능]: 현재 상세 정보 조회 중인 이벤트의 기본 스펙 정의 DTO 객체를 수집해 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 반환 목록 타입을 DTO로 변경 및 대소문자 필드 교정
        /// </summary>
        public EventDefinitionDTO GetEventDefinition()
        {
            var list = m_eventModel.GetActiveEvents();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].eventId == m_currentEventId)
                {
                    return list[i];
                }
            }
            return null;
        }

        /// <summary>
        /// [기능]: 대상 조건 전략 객체를 로드해 누적 진행도, 목표치 및 비율(0~1) 정보를 튜플로 비동기 반환합니다. static Mathf를 사용해 Clamp01을 단축 호출합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Awaitable 비동기 인터페이스로 갱신
        /// </summary>
        public async Awaitable<(int current, int target, float ratio)> GetProgressInfoAsync()
        {
            var cond = m_eventModel.GetCondition(m_currentEventId);
            if (cond == null)
            {
                return (0, 1, 0f);
            }

            int cur = await cond.GetCurrentProgressAsync();
            int tar = cond.GetTargetValue();
            float ratio = tar > 0 ? (float)cur / tar : 0f;
            return (cur, tar, Clamp01(ratio));
        }

        /// <summary>
        /// [기능]: 해당 이벤트의 보상이 이미 받아졌는지 세이브 파일을 참조해 비동기 검증합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Awaitable 비동기 인터페이스로 갱신
        /// </summary>
        public async Awaitable<bool> IsRewardClaimedAsync()
        {
            var progress = await m_saveSystem.LoadProgressAsync(m_currentEventId);
            return progress.isRewardClaimed;
        }

        /// <summary>
        /// [기능]: 타겟 수치 달성 및 중복 수령 방지 로직에 입각해 보상 청구가 유효한지 비동기 확인합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Awaitable 비동기 인터페이스로 갱신
        /// </summary>
        public async Awaitable<bool> CanClaimRewardAsync()
        {
            var cond = m_eventModel.GetCondition(m_currentEventId);
            if (cond == null)
            {
                return false;
            }

            bool completed = await cond.IsCompletedAsync();
            bool claimed = await IsRewardClaimedAsync();
            return completed && !claimed;
        }

        /// <summary>
        /// [기능]: 보상 수령 명령(Command)을 전달하여 도메인 단에서 처리를 비동기로 집행하고 성공 결과를 전파합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Awaitable 비동기 인터페이스로 갱신
        /// </summary>
        public async Awaitable ClaimRewardAsync(PlayerRewardModel playerReward)
        {
            bool canClaim = await CanClaimRewardAsync();
            if (canClaim == false)
            {
                return;
            }

            bool success = await m_eventModel.ClaimRewardAsync(m_currentEventId, m_saveSystem, playerReward);
            if (success)
            {
                OnRewardClaimSuccess?.Invoke(m_currentEventId);
            }
        }
        #endregion

        #region 내부 이벤트 핸들러
        /// <summary>
        /// [기능]: 타겟 이벤트의 진척도가 변경되었음을 관찰하여 UI 상세 화면 갱신 이벤트를 전파합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: XML 주석 보완
        /// </summary>
        private void HandleProgressChanged(string eventId)
        {
            if (eventId == m_currentEventId)
            {
                OnDetailUpdated?.Invoke();
            }
        }

        /// <summary>
        /// [기능]: 타겟 이벤트의 보상 수령 상태가 변경되었음을 관찰하여 UI 상세 화면 갱신 이벤트를 전파합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: XML 주석 보완
        /// </summary>
        private void HandleRewardClaimed(string eventId)
        {
            if (eventId == m_currentEventId)
            {
                OnDetailUpdated?.Invoke();
            }
        }
        #endregion
    }
}
