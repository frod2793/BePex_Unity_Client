using System;
using System.IO;
using System.Threading;
using UnityEngine;
using BePex.EventSystem.Interfaces;
using BePex.EventSystem.Models;

namespace BePex.EventSystem.Infrastructure
{
    /// <summary>
    /// [기능]: 비동기 세이브 파일 쓰기 도중 예외가 발생할 경우 데이터 불일치 및 손상을 원천 방지하기 위해 백업 및 롤백 메커니즘을 지원하는 안정성 데코레이터.
    /// [작성자]: 윤승종
    /// </summary>
    public class TransactionalSaveSystemDecorator : ISaveSystem
    {
        #region 내부 필드
        private readonly ISaveSystem m_innerSaveSystem;
        private readonly string m_saveDir;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 내장 세이브 시스템을 받아 초기화하고 백업용 로컬 경로를 구성합니다.
        /// [작성자]: 윤승종
        /// </summary>
        public TransactionalSaveSystemDecorator(ISaveSystem innerSaveSystem)
        {
            m_innerSaveSystem = innerSaveSystem;
            m_saveDir = Path.Combine(Application.persistentDataPath, "save");
            if (Directory.Exists(m_saveDir) == false)
            {
                Directory.CreateDirectory(m_saveDir);
            }
        }
        #endregion

        #region 인터페이스 구현 (Forwarding & Rollback Strategy)
        public Awaitable<EventProgressModel> LoadProgressAsync(string eventId, CancellationToken cancellationToken = default)
        {
            return m_innerSaveSystem.LoadProgressAsync(eventId, cancellationToken);
        }

        public Awaitable SaveProgressAsync(string eventId, EventProgressModel progress, CancellationToken cancellationToken = default)
        {
            return m_innerSaveSystem.SaveProgressAsync(eventId, progress, cancellationToken);
        }

        public Awaitable<PlayerRewardModel> LoadRewardStateAsync(CancellationToken cancellationToken = default)
        {
            return m_innerSaveSystem.LoadRewardStateAsync(cancellationToken);
        }

        public Awaitable SaveRewardStateAsync(PlayerRewardModel rewardState, CancellationToken cancellationToken = default)
        {
            return m_innerSaveSystem.SaveRewardStateAsync(rewardState, cancellationToken);
        }

        public Awaitable ClearAllAsync(CancellationToken cancellationToken = default)
        {
            return m_innerSaveSystem.ClearAllAsync(cancellationToken);
        }

        /// <summary>
        /// [기능]: 진행도와 플레이어 보상을 일괄 저장할 때, 기존 원본 데이터의 백업을 먼저 획득하고 두 쓰기 작업 중 하나라도 실패할 시 이전 시점의 원본 상태로 자동 롤백합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// </summary>
        public async Awaitable SaveBatchAsync(string eventId, EventProgressModel progress, PlayerRewardModel rewardState, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string progressFile = Path.Combine(m_saveDir, $"event_progress_{eventId}.json");
            string rewardFile = Path.Combine(m_saveDir, "player_rewards.json");

            string backupProgressFile = progressFile + ".bak";
            string backupRewardFile = rewardFile + ".bak";

            bool hasProgressBackup = false;
            bool hasRewardBackup = false;

            try
            {
                // 1단계: 임시 스레드에서 원본 세이브 파일의 백업 사본 생성
                await Awaitable.BackgroundThreadAsync();
                if (File.Exists(progressFile))
                {
                    File.Copy(progressFile, backupProgressFile, true);
                    hasProgressBackup = true;
                }
                if (File.Exists(rewardFile))
                {
                    File.Copy(rewardFile, backupRewardFile, true);
                    hasRewardBackup = true;
                }
                await Awaitable.MainThreadAsync();

                // 2단계: 주입받은 원본 세이브 시스템에 쓰기 작업 수행
                await m_innerSaveSystem.SaveBatchAsync(eventId, progress, rewardState, cancellationToken);

                // 3단계: 무사 완료 시 백업 임시 파일 소멸 처리
                await Awaitable.BackgroundThreadAsync();
                if (hasProgressBackup && File.Exists(backupProgressFile))
                {
                    File.Delete(backupProgressFile);
                }
                if (hasRewardBackup && File.Exists(backupRewardFile))
                {
                    File.Delete(backupRewardFile);
                }
                await Awaitable.MainThreadAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TransactionalSaveSystemDecorator] 일괄 저장 중 치명적 쓰기 오류가 감지되어 롤백 프로세스를 긴급 작동합니다: {ex.Message}");

                // 4단계: 예외 발생 시 원본으로 강제 롤백 복구
                await Awaitable.BackgroundThreadAsync();
                try
                {
                    if (hasProgressBackup && File.Exists(backupProgressFile))
                    {
                        File.Copy(backupProgressFile, progressFile, true);
                        File.Delete(backupProgressFile);
                    }
                    if (hasRewardBackup && File.Exists(backupRewardFile))
                    {
                        File.Copy(backupRewardFile, rewardFile, true);
                        File.Delete(backupRewardFile);
                    }
                }
                catch (Exception rollbackEx)
                {
                    Debug.LogError($"[TransactionalSaveSystemDecorator] 롤백 복구 시도 중 2차 장애가 발생했습니다: {rollbackEx.Message}");
                }
                await Awaitable.MainThreadAsync();

                throw;
            }
        }
        #endregion
    }
}
