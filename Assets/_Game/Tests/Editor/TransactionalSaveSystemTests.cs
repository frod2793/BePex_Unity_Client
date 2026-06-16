using NUnit.Framework;
using System;
using System.IO;
using System.Threading;
using UnityEngine;
using BePex.EventSystem.Interfaces;
using BePex.EventSystem.Models;
using BePex.EventSystem.Infrastructure;

namespace BePex.EventSystem.Tests.Editor
{
    [TestFixture]
    public class TransactionalSaveSystemTests
    {
        private string m_testSaveDir;
        private string m_testEventId;

        [SetUp]
        public void SetUp()
        {
            m_testSaveDir = Path.Combine(Application.persistentDataPath, "save");
            m_testEventId = "test_transactional_event";
            
            // 기존 테스트 찌꺼기 파일 정리
            CleanFiles();
        }

        [TearDown]
        public void TearDown()
        {
            CleanFiles();
        }

        private void CleanFiles()
        {
            string progressFile = Path.Combine(m_testSaveDir, $"event_progress_{m_testEventId}.json");
            string rewardFile = Path.Combine(m_testSaveDir, "player_rewards.json");

            if (File.Exists(progressFile))
            {
                File.Delete(progressFile);
            }
            if (File.Exists(rewardFile))
            {
                File.Delete(rewardFile);
            }
            if (File.Exists(progressFile + ".bak"))
            {
                File.Delete(progressFile + ".bak");
            }
            if (File.Exists(rewardFile + ".bak"))
            {
                File.Delete(rewardFile + ".bak");
            }
        }

        /// <summary>
        /// [기능]: 예외를 강제로 방출시키는 모의 세이브 구현체.
        /// </summary>
        private class FaultySaveSystem : ISaveSystem
        {
            public bool shouldFailOnSaveBatch = false;

            public async Awaitable<EventProgressModel> LoadProgressAsync(string eventId, CancellationToken cancellationToken = default)
            {
                await Awaitable.MainThreadAsync();
                return new EventProgressModel { eventId = eventId };
            }
            
            public async Awaitable SaveProgressAsync(string eventId, EventProgressModel progress, CancellationToken cancellationToken = default)
            {
                await Awaitable.MainThreadAsync();
            }
            
            public async Awaitable<PlayerRewardModel> LoadRewardStateAsync(CancellationToken cancellationToken = default)
            {
                await Awaitable.MainThreadAsync();
                return new PlayerRewardModel();
            }
            
            public async Awaitable SaveRewardStateAsync(PlayerRewardModel rewardState, CancellationToken cancellationToken = default)
            {
                await Awaitable.MainThreadAsync();
            }
            
            public async Awaitable ClearAllAsync(CancellationToken cancellationToken = default)
            {
                await Awaitable.MainThreadAsync();
            }

            public async Awaitable SaveBatchAsync(string eventId, EventProgressModel progress, PlayerRewardModel rewardState, CancellationToken cancellationToken = default)
            {
                await Awaitable.MainThreadAsync();
                if (shouldFailOnSaveBatch)
                {
                    throw new IOException("Simulated disk error during SaveBatchAsync");
                }
            }
        }

        [Test]
        public void Test_SaveBatch_WhenExceptionOccurs_ShouldRollbackToPreviousFiles()
        {
            // Arrange: 1. 기존 파일 세팅 (초기 파일 존재)
            string progressFile = Path.Combine(m_testSaveDir, $"event_progress_{m_testEventId}.json");
            string rewardFile = Path.Combine(m_testSaveDir, "player_rewards.json");

            File.WriteAllText(progressFile, "{\"eventId\":\"test_transactional_event\",\"quests\":[]}");
            File.WriteAllText(rewardFile, "{\"claimedEventIds\":[],\"currencies\":{}}");

            var faultySystem = new FaultySaveSystem { shouldFailOnSaveBatch = true };
            var decorator = new TransactionalSaveSystemDecorator(faultySystem);

            var newProgress = new EventProgressModel { eventId = m_testEventId };
            var newReward = new PlayerRewardModel();

            // Act & Assert: 2. 예외 던짐 확인 및 롤백 확인
            Assert.ThrowsAsync<IOException>(async () =>
            {
                await decorator.SaveBatchAsync(m_testEventId, newProgress, newReward);
            });

            // 원본 파일이 유지(혹은 백업으로부터 원상 복구)되었는지 검증
            Assert.IsTrue(File.Exists(progressFile), "Progress 세이브 파일이 유실되지 않고 원본이 유지되어야 합니다.");
            Assert.IsTrue(File.Exists(rewardFile), "Reward 세이브 파일이 유실되지 않고 원본이 유지되어야 합니다.");
            
            string progressContent = File.ReadAllText(progressFile);
            Assert.IsTrue(progressContent.Contains("test_transactional_event"), "세이브 파일 내용이 이전 시점의 원본 상태여야 합니다.");
        }
    }
}
