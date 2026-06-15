using System;
using System.Collections.Generic;
using UnityEngine;
using BePex.EventSystem.Data;

namespace BePex.EventSystem.Models
{
    /// <summary>
    /// [기능]: 플레이어의 누적 자산(Exp, Ticket, Point) 및 이미 보상을 수령 완료한 이벤트의 ID 이력을 유지하는 순수 C# 데이터 모델 (POCO).
    /// [작성자]: 윤승종
    /// </summary>
    [Serializable]
    public class PlayerRewardModel
    {
        #region 데이터 멤버
        [SerializeField] private List<string> m_claimedEventIds;
        [SerializeField] private int m_totalExp;
        [SerializeField] private int m_totalTickets;
        [SerializeField] private int m_totalPoints;
        [SerializeField] private int m_totalSeasonPoints;
        [SerializeField] private int m_totalCredits;
        #endregion

        #region 프로퍼티 (호환성 및 캡슐화)
        public List<string> claimedEventIds => m_claimedEventIds;
        public int totalExp => m_totalExp;
        public int totalTickets => m_totalTickets;
        public int totalPoints => m_totalPoints;
        public int totalSeasonPoints => m_totalSeasonPoints;
        public int totalCredits => m_totalCredits;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 기본 생성자로 누적 필드 및 이미 받아진 이벤트 ID 목록 리스트를 할당 및 초기화합니다.
        /// [작성자]: 윤승종
        /// </summary>
        public PlayerRewardModel()
        {
            m_claimedEventIds = new List<string>();
            m_totalExp = 0;
            m_totalTickets = 0;
            m_totalPoints = 0;
            m_totalSeasonPoints = 0;
            m_totalCredits = 0;
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 안전하게 지정한 보상 타입의 자산을 증가시킵니다.
        /// [작성자]: 윤승종
        /// </summary>
        public void AddCurrency(RewardDefinitionSO.RewardType type, int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            switch (type)
            {
                case RewardDefinitionSO.RewardType.Exp:
                    m_totalExp += amount;
                    break;
                case RewardDefinitionSO.RewardType.Ticket:
                    m_totalTickets += amount;
                    break;
                case RewardDefinitionSO.RewardType.Point:
                    m_totalPoints += amount;
                    break;
                case RewardDefinitionSO.RewardType.SeasonPoint:
                    m_totalSeasonPoints += amount;
                    break;
                case RewardDefinitionSO.RewardType.CreditReword:
                    m_totalCredits += amount;
                    break;
            }
        }

        /// <summary>
        /// [기능]: 안전하게 지정한 보상 타입의 자산을 차감합니다. (음수 방지 안전 가드 내재)
        /// [작성자]: 윤승종
        /// </summary>
        public bool TrySpendCurrency(RewardDefinitionSO.RewardType type, int amount)
        {
            if (amount <= 0)
            {
                return false;
            }

            switch (type)
            {
                case RewardDefinitionSO.RewardType.Point:
                    if (m_totalPoints < amount)
                    {
                        return false;
                    }
                    m_totalPoints -= amount;
                    return true;

                case RewardDefinitionSO.RewardType.SeasonPoint:
                    if (m_totalSeasonPoints < amount)
                    {
                        return false;
                    }
                    m_totalSeasonPoints -= amount;
                    return true;

                case RewardDefinitionSO.RewardType.CreditReword:
                    if (m_totalCredits < amount)
                    {
                        return false;
                    }
                    m_totalCredits -= amount;
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// [기능]: 리플렉션 없이 현재 보유한 자산 잔액 상태를 안전하게 딕셔너리로 추출합니다. (OCP 준수)
        /// [작성자]: 윤승종
        /// </summary>
        public Dictionary<string, int> GetBalances()
        {
            return new Dictionary<string, int>
            {
                { "totalExp", m_totalExp },
                { "totalTickets", m_totalTickets },
                { "totalPoints", m_totalPoints },
                { "totalSeasonPoints", m_totalSeasonPoints },
                { "totalCredits", m_totalCredits }
            };
        }
        #endregion
    }
}
