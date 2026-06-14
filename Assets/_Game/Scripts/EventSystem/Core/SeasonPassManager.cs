using UnityEngine;
using BePex.EventSystem.Models;
using BePex.EventSystem.Data;
using BePex.EventSystem.Factories;
using BePex.EventSystem.Interfaces;

namespace BePex.EventSystem.Core
{
    /// <summary>
    /// [기능]: 시즌패스의 진행도 갱신, 레벨 업그레이드, 프리미엄 패스 활성화, 보상 수령 로직을 관장하는 시스템.
    /// [작성자]: 윤승종
    /// </summary>
    public class SeasonPassManager
    {
        #region 내부 필드
        private readonly SeasonPassDefinitionSO m_definition;
        private readonly ISaveSystem m_saveSystem;
        private readonly RewardFactory m_rewardFactory;
        
        private SeasonPassModel m_model;
        #endregion

        #region 프로퍼티
        public SeasonPassModel Model => m_model;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 정의 파일, 세이브 모듈, 보상 팩토리를 주입받는 생성자.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public SeasonPassManager(SeasonPassDefinitionSO definition, ISaveSystem saveSystem, RewardFactory rewardFactory)
        {
            m_definition = definition;
            m_saveSystem = saveSystem;
            m_rewardFactory = rewardFactory;
        }

        /// <summary>
        /// [기능]: 서버 또는 로컬 저장소로부터 시즌패스 모델 데이터를 로드합니다. (이벤트용 LoadProgressAsync 재활용 또는 별도 확장)
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public async Awaitable InitializeAsync()
        {
            // 여기서는 ISaveSystem에 SeasonPass용 로드가 추가되었다고 가정하거나 EventProgressModel 대신 직접 로드하는 형태로 구현합니다.
            // 단순화를 위해 임시 모델을 생성하거나, 확장이 필요한 경우 별도의 ISeasonPassSaveSystem 도입 가능.
            // 데모에서는 기본 인스턴스를 메모리에서 사용
            m_model = new SeasonPassModel
            {
                passId = m_definition.passId,
                currentExp = 0,
                currentLevel = 0,
                isPremiumActive = false
            };
            
            // 실제 구현 시: m_model = await m_saveSystem.LoadSeasonPassAsync(m_definition.passId);
            await Awaitable.MainThreadAsync();
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 시즌패스에 경험치를 추가하고, 레벨업 조건을 달성하면 레벨을 상승시킵니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 예외 가드 구문에 Allman Style 중괄호 `{}` 추가 보완
        /// </summary>
        public void AddExp(int expAmount)
        {
            if (m_model == null || m_definition == null)
            {
                return;
            }

            m_model.currentExp += expAmount;
            CheckLevelUp();
        }

        /// <summary>
        /// [기능]: 프리미엄 패스를 활성화합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public void ActivatePremium()
        {
            if (m_model != null)
            {
                m_model.isPremiumActive = true;
            }
        }

        /// <summary>
        /// [기능]: 특정 레벨의 무료 또는 프리미엄 보상을 수령합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public async Awaitable ClaimRewardAsync(int level, bool isPremium)
        {
            if (m_model.currentLevel < level)
            {
                Debug.LogWarning($"[SeasonPassManager] {level}레벨은 아직 도달하지 못했습니다.");
                return;
            }

            if (isPremium && !m_model.isPremiumActive)
            {
                Debug.LogWarning($"[SeasonPassManager] 프리미엄 패스가 활성화되지 않았습니다.");
                return;
            }

            var claimedList = isPremium ? m_model.claimedPremiumLevels : m_model.claimedFreeLevels;
            if (claimedList.Contains(level))
            {
                Debug.LogWarning($"[SeasonPassManager] {level}레벨 보상은 이미 수령했습니다.");
                return;
            }

            var levelDef = m_definition.levels.Find(x => x.level == level);
            if (levelDef != null)
            {
                var rewardDef = isPremium ? levelDef.premiumReward : levelDef.freeReward;
                if (rewardDef != null)
                {
                    IEventReward reward = m_rewardFactory.Create(rewardDef);
                    if (reward != null)
                    {
                        var playerReward = await m_saveSystem.LoadRewardStateAsync();
                        reward.Grant(playerReward);
                        await m_saveSystem.SaveRewardStateAsync(playerReward);
                        
                        claimedList.Add(level);
                        Debug.Log($"[SeasonPassManager] {level}레벨 보상 수령 완료!");
                    }
                }
            }
        }
        #endregion

        #region 내부 메서드
        /// <summary>
        /// [기능]: 레벨 조건 목록을 검사하여 레벨업 달성 여부를 판정합니다. GC 방지용 for 루프 최적화 완료.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: foreach 루프를 index 기반 for 루프로 최적화 교정
        /// </summary>
        private void CheckLevelUp()
        {
            bool leveledUp = false;
            for (int i = 0; i < m_definition.levels.Count; i++)
            {
                var levelDef = m_definition.levels[i];
                if (levelDef != null && m_model.currentLevel < levelDef.level && m_model.currentExp >= levelDef.requiredExp)
                {
                    m_model.currentLevel = levelDef.level;
                    leveledUp = true;
                }
            }

            if (leveledUp)
            {
                Debug.Log($"[SeasonPassManager] 시즌패스 레벨 업! 현재 레벨: {m_model.currentLevel}");
            }
        }
        #endregion
    }
}
