using UnityEngine;
using System.Collections.Generic;

namespace BePex.EventSystem.Data
{
    /// <summary>
    /// [기능]: 시즌패스의 각 레벨별 필요 경험치와 무료/프리미엄 보상을 정의하는 구조체.
    /// [작성자]: 윤승종
    /// </summary>
    [System.Serializable]
    public class SeasonPassLevelDef
    {
        public int level;
        public int requiredExp;
        public RewardDefinitionSO freeReward;
        public RewardDefinitionSO premiumReward;
    }

    /// <summary>
    /// [기능]: 시즌패스의 전체 메타데이터 및 레벨 데이터를 담는 ScriptableObject.
    /// [작성자]: 윤승종
    /// </summary>
    [CreateAssetMenu(fileName = "NewSeasonPassDef", menuName = "BePex/EventSystem/SeasonPassDefinition")]
    public class SeasonPassDefinitionSO : ScriptableObject
    {
        public string passId;
        public string displayName;
        public List<SeasonPassLevelDef> levels = new List<SeasonPassLevelDef>();
    }
}
