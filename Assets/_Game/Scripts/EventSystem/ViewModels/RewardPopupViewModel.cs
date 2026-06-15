using System;
using BePex.EventSystem.Models;
using BePex.EventSystem.Interfaces;

namespace BePex.EventSystem.ViewModels
{
    /// <summary>
    /// [기능]: 보상 획득 완료 안내 및 플레이어의 최종 누적 자산 현황을 노출하는 ViewModel 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    public class RewardPopupViewModel
    {
        #region 내부 필드
        private readonly PlayerRewardModel m_playerReward;
        private readonly ISaveSystem m_saveSystem;
        private readonly EventModel m_eventModel;
        private string m_lastClaimedEventId;
        #endregion

        #region 가공 프로퍼티
        /// <summary>
        /// [기능]: 플레이어의 누적 경험치 반환 프로퍼티.
        /// [작성자]: 윤승종
        /// </summary>
        public int TotalExp
        {
            get
            {
                return m_playerReward.totalExp;
            }
        }

        /// <summary>
        /// [기능]: 플레이어의 누적 티켓 개수 반환 프로퍼티.
        /// [작성자]: 윤승종
        /// </summary>
        public int TotalTickets
        {
            get
            {
                return m_playerReward.totalTickets;
            }
        }

        /// <summary>
        /// [기능]: 플레이어의 누적 포인트 반환 프로퍼티.
        /// [작성자]: 윤승종
        /// </summary>
        public int TotalPoints
        {
            get
            {
                return m_playerReward.totalPoints;
            }
        }

        /// <summary>
        /// [기능]: 플레이어의 누적 시즌 포인트 반환 프로퍼티.
        /// [작성자]: 윤승종
        /// </summary>
        public int TotalSeasonPoints
        {
            get
            {
                return m_playerReward.totalSeasonPoints;
            }
        }

        /// <summary>
        /// [기능]: 플레이어의 누적 재화 반환 프로퍼티.
        /// [작성자]: 윤승종
        /// </summary>
        public int TotalCredits
        {
            get
            {
                return m_playerReward.totalCredits;
            }
        }
        #endregion


        #region 이벤트 (Observer)
        public event Action OnRewardDataChanged;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 플레이어 자산 데이터 모델, 세이브 데이터 모듈 및 이벤트 모델을 생성자 주입을 통해 수신받습니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: EventModel 의존성 주입 추가
        /// </summary>
        public RewardPopupViewModel(PlayerRewardModel playerReward, ISaveSystem saveSystem, EventModel eventModel)
        {
            m_playerReward = playerReward;
            m_saveSystem = saveSystem;
            m_eventModel = eventModel;
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 보상 데이터 변경 상태를 Observer 패턴(이벤트)을 통하여 외부 View에 전파합니다.
        /// [작성자]: 윤승종
        /// </summary>
        public void Refresh()
        {
            OnRewardDataChanged?.Invoke();
        }

        /// <summary>
        /// [기능]: 특정 이벤트 클리어 시점의 ID를 받아 캐싱하고 보상 변경 통지를 알립니다.
        /// [작성자]: 윤승종
        /// </summary>
        public void Refresh(string eventId)
        {
            m_lastClaimedEventId = eventId;
            OnRewardDataChanged?.Invoke();
        }

        /// <summary>
        /// [기능]: 마지막으로 획득한 이벤트의 보상 상세 목록 DTO 리스트를 반환합니다.
        /// [작성자]: 윤승종
        /// </summary>
        public System.Collections.Generic.List<DTOs.RewardDefinitionDTO> GetClaimedRewards()
        {
            if (m_eventModel == null || string.IsNullOrEmpty(m_lastClaimedEventId))
            {
                return new System.Collections.Generic.List<DTOs.RewardDefinitionDTO>();
            }

            var events = m_eventModel.GetActiveEvents();
            for (int i = 0; i < events.Count; i++)
            {
                if (events[i].eventId == m_lastClaimedEventId)
                {
                    return events[i].rewards;
                }
            }

            return new System.Collections.Generic.List<DTOs.RewardDefinitionDTO>();
        }
        #endregion
    }
}
