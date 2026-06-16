/// <summary>
/// [기능]: 실제 저장소(ISaveSystem)를 래핑하여 메모리 캐싱 및 비동기 쓰기 락을 지원하는 데코레이터 세이브 시스템.
/// [작성자]: 윤승종
/// </summary>

using System.Collections.Generic;
using UnityEngine;
using BePex.EventSystem.Interfaces;
using BePex.EventSystem.Models;
using System.Threading;

namespace BePex.EventSystem.Infrastructure
{
    public class CachedSaveSystem : ISaveSystem
    {
        #region 내부 필드
        private readonly ISaveSystem m_innerSaveSystem;
        private readonly Dictionary<string, EventProgressModel> m_progressCache;
        private readonly HashSet<string> m_activeWrites;
        private PlayerRewardModel m_rewardCache;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 원본 저장 장치 인스턴스를 주입받아 데코레이터 캐시를 구성합니다.
        /// [작성자]: 윤승종
        /// </summary>
        public CachedSaveSystem(ISaveSystem innerSaveSystem)
        {
            m_innerSaveSystem = innerSaveSystem;
            m_progressCache = new Dictionary<string, EventProgressModel>();
            m_activeWrites = new HashSet<string>();
            m_rewardCache = null;
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 지정 이벤트 ID에 대해 쓰기 락 상태를 검증하고 캐시 혹은 원본 파일로부터 데이터를 로드합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Awaitable detached state 예외 해결을 위한 HashSet 기반 프레임 대기 기작 적용
        /// </summary>
        public async Awaitable<EventProgressModel> LoadProgressAsync(string eventId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (Application.isPlaying)
            {
                while (m_activeWrites.Contains(eventId))
                {
                    await Awaitable.NextFrameAsync(cancellationToken);
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (m_progressCache.TryGetValue(eventId, out var cached))
            {
                return cached;
            }

            var progress = await m_innerSaveSystem.LoadProgressAsync(eventId, cancellationToken);
            m_progressCache[eventId] = progress;
            return progress;
        }

        /// <summary>
        /// [기능]: 캐시를 즉시 갱신하고 원본 장치에 비동기 저장을 수행하는 동안 쓰기 락을 선언합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Awaitable detached state 예외 해결을 위한 HashSet 기반 락 관리 적용
        /// </summary>
        public async Awaitable SaveProgressAsync(string eventId, EventProgressModel progress, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            m_progressCache[eventId] = progress;

            m_activeWrites.Add(eventId);
            try
            {
                await m_innerSaveSystem.SaveProgressAsync(eventId, progress, cancellationToken);
            }
            finally
            {
                m_activeWrites.Remove(eventId);
            }
        }

        /// <summary>
        /// [기능]: 캐시로부터 플레이어 보상 상태를 즉시 반환하거나 원본에서 로드합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 취소 제어를 위한 CancellationToken 추가
        /// </summary>
        public async Awaitable<PlayerRewardModel> LoadRewardStateAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (m_rewardCache != null)
            {
                return m_rewardCache;
            }

            m_rewardCache = await m_innerSaveSystem.LoadRewardStateAsync(cancellationToken);
            return m_rewardCache;
        }

        /// <summary>
        /// [기능]: 플레이어 보상 캐시 상태를 갱신하고 물리 저장을 수행합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 취소 제어를 위한 CancellationToken 추가
        /// </summary>
        public async Awaitable SaveRewardStateAsync(PlayerRewardModel rewardState, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            m_rewardCache = rewardState;
            await m_innerSaveSystem.SaveRewardStateAsync(rewardState, cancellationToken);
        }

        /// <summary>
        /// [기능]: 캐시 맵과 락 리스트를 완전히 비우고 내부 저장소를 리셋합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 취소 제어를 위한 CancellationToken 추가
        /// </summary>
        public async Awaitable ClearAllAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            m_progressCache.Clear();
            m_rewardCache = null;
            m_activeWrites.Clear();
            await m_innerSaveSystem.ClearAllAsync(cancellationToken);
        }

        /// <summary>
        /// [기능]: 배치 세이브 시 쓰기 락을 적용하며 캐시를 동기화합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Awaitable detached state 예외 해결을 위한 HashSet 기반 락 관리 적용
        /// </summary>
        public async Awaitable SaveBatchAsync(string eventId, EventProgressModel progress, PlayerRewardModel rewardState, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            m_progressCache[eventId] = progress;
            m_rewardCache = rewardState;

            m_activeWrites.Add(eventId);
            try
            {
                await m_innerSaveSystem.SaveBatchAsync(eventId, progress, rewardState, cancellationToken);
            }
            finally
            {
                m_activeWrites.Remove(eventId);
            }
        }
        #endregion
    }
}
