using System;
using System.Threading;
using UnityEngine;
using BePex.EventSystem.Interfaces;
using BePex.EventSystem.Models;

namespace BePex.EventSystem.Infrastructure
{
    /// <summary>
    /// [기능]: ISaveSystem 인터페이스를 구현하며 지수 백오프 비동기 재시도 로직을 래핑 제공하는 데코레이터.
    /// [작성자]: 윤승종
    /// </summary>
    public class RetrySaveSystemDecorator : ISaveSystem
    {
        #region 내부 필드
        private readonly ISaveSystem m_innerSaveSystem;
        private readonly int m_maxRetryAttempts;
        private readonly float m_baseDelaySeconds;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 실제 저장소 시스템 객체 및 최대 재시도 횟수, 기본 지연 초 단위를 주입받아 초기화합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        public RetrySaveSystemDecorator(ISaveSystem innerSaveSystem, int maxRetryAttempts = 3, float baseDelaySeconds = 1f)
        {
            m_innerSaveSystem = innerSaveSystem;
            m_maxRetryAttempts = maxRetryAttempts;
            m_baseDelaySeconds = baseDelaySeconds;
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 특정 이벤트의 진행 상태를 로드합니다. 로드 연산은 일반적으로 재시도하지 않으나 필요한 경우 지수 백오프를 적용해 시도합니다.
        /// [작성자]: 윤승종
        /// </summary>
        public async Awaitable<EventProgressModel> LoadProgressAsync(string eventId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await m_innerSaveSystem.LoadProgressAsync(eventId, cancellationToken);
        }

        /// <summary>
        /// [기능]: 특정 이벤트의 진행 상태를 저장합니다. 지수 백오프 비동기 재시도 로직을 적용합니다.
        /// [작성자]: 윤승종
        /// </summary>
        public async Awaitable SaveProgressAsync(string eventId, EventProgressModel progress, CancellationToken cancellationToken = default)
        {
            int retryCount = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await m_innerSaveSystem.SaveProgressAsync(eventId, progress, cancellationToken);
                    return;
                }
                catch (Exception ex)
                {
                    if (ex is OperationCanceledException)
                    {
                        throw;
                    }
                    
                    retryCount++;
                    if (retryCount > m_maxRetryAttempts)
                    {
                        Debug.LogError($"[RetrySaveSystemDecorator] 진행 상태 저장에 최종 실패했습니다. (시도 횟수: {retryCount}): {ex.Message}");
                        throw;
                    }

                    float delay = m_baseDelaySeconds * Mathf.Pow(2, retryCount - 1);
                    Debug.LogWarning($"[RetrySaveSystemDecorator] 진행 상태 저장 실패. 지수 백오프 대기 후 재시도합니다. (현재 재시도 횟수: {retryCount}, 대기 시간: {delay}초, 에러: {ex.Message})");

                    await Awaitable.WaitForSecondsAsync(delay, cancellationToken);
                }
            }
        }

        /// <summary>
        /// [기능]: 플레이어의 누적 보상 현황을 로드합니다.
        /// [작성자]: 윤승종
        /// </summary>
        public async Awaitable<PlayerRewardModel> LoadRewardStateAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await m_innerSaveSystem.LoadRewardStateAsync(cancellationToken);
        }

        /// <summary>
        /// [기능]: 플레이어의 누적 보상 현황을 저장합니다. 지수 백오프 재시도를 적용합니다.
        /// [작성자]: 윤승종
        /// </summary>
        public async Awaitable SaveRewardStateAsync(PlayerRewardModel rewardState, CancellationToken cancellationToken = default)
        {
            int retryCount = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await m_innerSaveSystem.SaveRewardStateAsync(rewardState, cancellationToken);
                    return;
                }
                catch (Exception ex)
                {
                    if (ex is OperationCanceledException)
                    {
                        throw;
                    }

                    retryCount++;
                    if (retryCount > m_maxRetryAttempts)
                    {
                        Debug.LogError($"[RetrySaveSystemDecorator] 보상 상태 저장에 최종 실패했습니다. (시도 횟수: {retryCount}): {ex.Message}");
                        throw;
                    }

                    float delay = m_baseDelaySeconds * Mathf.Pow(2, retryCount - 1);
                    Debug.LogWarning($"[RetrySaveSystemDecorator] 보상 상태 저장 실패. 지수 백오프 대기 후 재시도합니다. (현재 재시도 횟수: {retryCount}, 대기 시간: {delay}초, 에러: {ex.Message})");

                    await Awaitable.WaitForSecondsAsync(delay, cancellationToken);
                }
            }
        }

        /// <summary>
        /// [기능]: 저장소의 모든 세이브 데이터를 완전히 리셋합니다.
        /// [작성자]: 윤승종
        /// </summary>
        public async Awaitable ClearAllAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await m_innerSaveSystem.ClearAllAsync(cancellationToken);
        }

        /// <summary>
        /// [기능]: 진행 상태와 자산 상태를 트랜잭션 단위로 일괄(Batch) 비동기 저장합니다. 지수 백오프 재시도를 적용합니다.
        /// [작성자]: 윤승종
        /// </summary>
        public async Awaitable SaveBatchAsync(string eventId, EventProgressModel progress, PlayerRewardModel rewardState, CancellationToken cancellationToken = default)
        {
            int retryCount = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await m_innerSaveSystem.SaveBatchAsync(eventId, progress, rewardState, cancellationToken);
                    return;
                }
                catch (Exception ex)
                {
                    if (ex is OperationCanceledException)
                    {
                        throw;
                    }

                    retryCount++;
                    if (retryCount > m_maxRetryAttempts)
                    {
                        Debug.LogError($"[RetrySaveSystemDecorator] 일괄 배치 저장에 최종 실패했습니다. (시도 횟수: {retryCount}): {ex.Message}");
                        throw;
                    }

                    float delay = m_baseDelaySeconds * Mathf.Pow(2, retryCount - 1);
                    Debug.LogWarning($"[RetrySaveSystemDecorator] 일괄 배치 저장 실패. 지수 백오프 대기 후 재시도합니다. (현재 재시도 횟수: {retryCount}, 대기 시간: {delay}초, 에러: {ex.Message})");

                    await Awaitable.WaitForSecondsAsync(delay, cancellationToken);
                }
            }
        }
        #endregion
    }
}
