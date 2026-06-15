using System.Collections.Generic;
using UnityEngine;
using BePex.EventSystem.Interfaces;
using BePex.EventSystem.Models;

namespace BePex.EventSystem.Infrastructure
{
    /// <summary>
    /// [기능]: 실제 파일 I/O 없이 메모리 상(Dictionary)에서만 모의 저장을 처리하여 테스트 편의를 돕는 테스트용 세이브 모듈.
    /// [작성자]: 윤승종
    /// </summary>
    public class InMemorySaveSystem : ISaveSystem
    {
        #region 내부 필드
        private readonly Dictionary<string, EventProgressModel> m_progressMap = new Dictionary<string, EventProgressModel>();
        private PlayerRewardModel m_rewardState = new PlayerRewardModel();
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 메모리 맵 객체로부터 진행 정보를 꺼내어 반환합니다. 데이터가 없으면 새로 생성해 세팅합니다. (비동기 시뮬레이션)
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Awaitable 비동기 인터페이스로 갱신
        /// </summary>
        public async Awaitable<EventProgressModel> LoadProgressAsync(string eventId)
        {
            await Awaitable.BackgroundThreadAsync(); // 모의 비동기 대기
            if (m_progressMap.ContainsKey(eventId) == false)
            {
                m_progressMap[eventId] = new EventProgressModel { eventId = eventId };
            }
            await Awaitable.MainThreadAsync();
            return m_progressMap[eventId];
        }

        /// <summary>
        /// [기능]: 메모리 맵의 진행 상태 정보를 갱신하여 덮어씁니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Awaitable 비동기 인터페이스로 갱신
        /// </summary>
        public async Awaitable SaveProgressAsync(string eventId, EventProgressModel progress)
        {
            await Awaitable.MainThreadAsync();
            m_progressMap[eventId] = progress;
        }

        /// <summary>
        /// [기능]: 메모리에 보관된 모의 누적 보상 현황을 로드합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Awaitable 비동기 인터페이스로 갱신
        /// </summary>
        public async Awaitable<PlayerRewardModel> LoadRewardStateAsync()
        {
            await Awaitable.MainThreadAsync();
            return m_rewardState;
        }

        /// <summary>
        /// [기능]: 메모리에 모의 누적 보상 현황을 갱신합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Awaitable 비동기 인터페이스로 갱신
        /// </summary>
        public async Awaitable SaveRewardStateAsync(PlayerRewardModel rewardState)
        {
            await Awaitable.MainThreadAsync();
            m_rewardState = rewardState;
        }

        /// <summary>
        /// [기능]: 인메모리에 할당된 모든 사전 정보와 보상 적립 상태를 리셋하여 초기화합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Awaitable 비동기 인터페이스로 갱신
        /// </summary>
        public async Awaitable ClearAllAsync()
        {
            await Awaitable.MainThreadAsync();
            m_progressMap.Clear();
            m_rewardState = new PlayerRewardModel();
        }

        /// <summary>
        /// [기능]: 메모리 맵 상의 진행 상태와 플레이어 자산 현황을 일괄 동기화 갱신합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 구현
        /// </summary>
        public async Awaitable SaveBatchAsync(string eventId, EventProgressModel progress, PlayerRewardModel rewardState)
        {
            await Awaitable.MainThreadAsync();
            m_progressMap[eventId] = progress;
            m_rewardState = rewardState;
        }
        #endregion
    }
}
