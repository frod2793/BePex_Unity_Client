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
    public class EventDetailViewModel : IDisposable
    {
        #region 내부 필드
        private readonly EventModel m_eventModel;
        private readonly ISaveSystem m_saveSystem;
        private readonly PlayerRewardModel m_playerReward;
        private string m_currentEventId;
        #endregion

        #region 이벤트 (Observer)
        public event Action OnDetailUpdated;
        public event Action<string, string> OnRewardClaimSuccess;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 도메인 모델과 데이터 영속성 장치, 플레이어 누적 보상 정보를 주입받고 진행 상황 변경 감지 이벤트를 처리하는 생성자.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: PlayerRewardModel 주입 인자 추가 및 캡슐화
        /// </summary>
        public EventDetailViewModel(EventModel eventModel, ISaveSystem saveSystem, PlayerRewardModel playerReward)
        {
            m_eventModel = eventModel;
            m_saveSystem = saveSystem;
            m_playerReward = playerReward;

            m_eventModel.OnEventProgressChanged += HandleProgressChanged;
            m_eventModel.OnEventRewardClaimed += HandleRewardClaimed;
            m_eventModel.OnModelReloaded += HandleModelReloaded;
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
        /// [기능]: 대상 조건 전략 객체를 로드해 누적 진행도, 목표치 및 비율(0~1) 정보를 퀘스트 단위로 비동기 반환합니다. static Mathf를 사용해 Clamp01을 단축 호출합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: questId 매개변수 적용 및 조건 조회 로직 갱신
        /// </summary>
        public async Awaitable<(int current, int target, float ratio)> GetProgressInfoAsync(string questId)
        {
            var cond = m_eventModel.GetCondition(m_currentEventId, questId);
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
        /// [기능]: 해당 퀘스트의 보상이 이미 받아졌는지 세이브 파일을 참조해 비동기 검증합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: questId 매개변수 적용 및 개별 퀘스트 진행 정보 순회 검사
        /// </summary>
        public async Awaitable<bool> IsRewardClaimedAsync(string questId)
        {
            var progress = await m_saveSystem.LoadProgressAsync(m_currentEventId);
            if (progress == null || progress.quests == null)
            {
                return false;
            }

            for (int i = 0; i < progress.quests.Count; i++)
            {
                if (progress.quests[i].questId == questId)
                {
                    return progress.quests[i].isRewardClaimed;
                }
            }
            return false;
        }

        /// <summary>
        /// [기능]: 타겟 수치 달성 및 중복 수령 방지 로직에 입각해 특정 퀘스트의 보상 청구가 유효한지 비동기 확인합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: questId 매개변수 적용 및 개별 조건 완료 판정
        /// </summary>
        public async Awaitable<bool> CanClaimRewardAsync(string questId)
        {
            var cond = m_eventModel.GetCondition(m_currentEventId, questId);
            if (cond == null)
            {
                return false;
            }

            bool completed = await cond.IsCompletedAsync();
            bool claimed = await IsRewardClaimedAsync(questId);
            return completed && !claimed;
        }

        /// <summary>
        /// [기능]: 특정 퀘스트의 보상 수령 명령(Command)을 전달하여 도메인 단에서 처리를 비동기로 집행하고 성공 결과를 전파합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: questId 매개변수 적용 및 개별 보상 청구
        /// </summary>
        public async Awaitable ClaimRewardAsync(string questId)
        {
            bool canClaim = await CanClaimRewardAsync(questId);
            if (canClaim == false)
            {
                return;
            }

            bool success = await m_eventModel.ClaimRewardAsync(m_currentEventId, questId, m_saveSystem, m_playerReward);
            if (success)
            {
                OnRewardClaimSuccess?.Invoke(m_currentEventId, questId);
            }
        }

        /// <summary>
        /// [기능]: 이벤트 전체에 대해 완료되었으나 받지 않은 모든 퀘스트 보상을 일괄로 수령 청구합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 일괄 보상 수령 명령 신규 구현 및 퀘스트 네이밍 변경
        /// </summary>
        public async Awaitable ClaimAllRewardsAsync()
        {
            bool success = await m_eventModel.ClaimAllRewardsAsync(m_currentEventId, m_saveSystem, m_playerReward);
            if (success)
            {
                OnRewardClaimSuccess?.Invoke(m_currentEventId, string.Empty);
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
        /// <summary>
        /// [기능]: 모델이 리로드되었을 때 상세 화면 갱신 이벤트를 전파합니다.
        /// [작성자]: 윤승종
        /// </summary>
        private void HandleModelReloaded()
        {
            OnDetailUpdated?.Invoke();
        }

        /// <summary>
        /// [기능]: 메모리 누수 방지를 위한 이벤트 언구독 로직.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: OnModelReloaded 구독 해제 추가 및 IDisposable 구현
        /// </summary>
        public void Dispose()
        {
            if (m_eventModel != null)
            {
                m_eventModel.OnEventProgressChanged -= HandleProgressChanged;
                m_eventModel.OnEventRewardClaimed -= HandleRewardClaimed;
                m_eventModel.OnModelReloaded -= HandleModelReloaded;
            }
        }
        #endregion
    }
}
