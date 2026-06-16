using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace BePex.EventSystem.Models
{
    /// <summary>
    /// [기능]: 기획 변경 및 신규 재화 보상 추가에 무한 확장이 가능하도록 Dictionary 기반으로 개선된 플레이어 자산 데이터 모델.
    /// [작성자]: 윤승종
    /// </summary>
    [Serializable]
    public class PlayerRewardModel
    {
        #region 데이터 멤버
        [JsonProperty("claimedEventIds")]
        [SerializeField] private List<string> m_claimedEventIds;

        // 런타임 고속 조회 및 유연성 보장을 위한 딕셔너리
        private Dictionary<string, int> m_currencies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        #endregion

        #region 프로퍼티 (호환성 제공)
        [JsonIgnore]
        public List<string> claimedEventIds => m_claimedEventIds;
        [JsonIgnore]
        public int totalExp => GetCurrencyAmount("Exp");
        [JsonIgnore]
        public int totalTickets => GetCurrencyAmount("Ticket");
        [JsonIgnore]
        public int totalPoints => GetCurrencyAmount("Point");
        [JsonIgnore]
        public int totalSeasonPoints => GetCurrencyAmount("SeasonPoint");
        [JsonIgnore]
        public int totalCredits => GetCurrencyAmount("Credit");

        /// <summary>
        /// [기능]: Newtonsoft.Json 직렬화/역직렬화를 직접 수납하기 위한 딕셔너리 연계 프로퍼티. 대소문자 무시 속성을 상시 동기화합니다.
        /// [작성자]: 윤승종
        /// </summary>
        [JsonProperty("currencies")]
        public Dictionary<string, int> Currencies
        {
            get => m_currencies;
            set
            {
                if (value != null)
                {
                    m_currencies = new Dictionary<string, int>(value, StringComparer.OrdinalIgnoreCase);
                }
                else
                {
                    m_currencies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                }
            }
        }
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 기본 자산 목록 및 데이터 구조체를 할당하고 초기화합니다.
        /// [작성자]: 윤승종
        /// </summary>
        public PlayerRewardModel()
        {
            m_claimedEventIds = new List<string>();
            m_currencies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }
        #endregion



        #region 내부 보조 메서드
        /// <summary>
        /// [기능]: 지정 재화 타입의 수량을 안전하게 조회합니다.
        /// [작성자]: 윤승종
        /// </summary>
        private int GetCurrencyAmount(string typeName)
        {
            string normKey = NormalizeCurrencyKey(typeName);
            if (m_currencies.TryGetValue(normKey, out int amount))
            {
                return amount;
            }
            return 0;
        }



        /// <summary>
        /// [기능]: 재화의 식별자 키값을 정규화합니다.
        /// [작성자]: 윤승종
        /// </summary>
        private string NormalizeCurrencyKey(string typeName)
        {
            return typeName;
        }
        #endregion

        #region 공개 비즈니스 로직 메서드
        /// <summary>
        /// [기능]: 지정한 보상 타입(문자열 식별자)의 자산을 안전하게 증가시킵니다. (OCP 극대화)
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 하드코딩 switch 분기를 제거하고 딕셔너리 연산으로 단순화
        /// </summary>
        public void AddCurrency(string typeName, int amount)
        {
            if (amount <= 0 || string.IsNullOrEmpty(typeName))
            {
                return;
            }

            string normKey = NormalizeCurrencyKey(typeName);

            if (m_currencies.ContainsKey(normKey))
            {
                m_currencies[normKey] += amount;
            }
            else
            {
                m_currencies.Add(normKey, amount);
            }
            
            Debug.Log($"[PlayerRewardModel] {normKey} 보상이 적립되었습니다: +{amount} (현재 잔액: {m_currencies[normKey]})");
        }

        /// <summary>
        /// [기능]: 지정한 보상 타입의 자산을 안전하게 차감합니다. (잔액 부족 가드 내장)
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 딕셔너리 기반 차감 처리로 개선
        /// </summary>
        public bool TrySpendCurrency(string typeName, int amount)
        {
            if (amount <= 0 || string.IsNullOrEmpty(typeName))
            {
                return false;
            }

            string normKey = NormalizeCurrencyKey(typeName);

            if (m_currencies.TryGetValue(normKey, out int currentAmount))
            {
                if (currentAmount < amount)
                {
                    return false;
                }

                m_currencies[normKey] = currentAmount - amount;
                return true;
            }

            return false;
        }

        /// <summary>
        /// [기능]: 현재 보유한 전체 자산의 잔액 상태를 복사하여 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 딕셔너리 복사본 직접 반환으로 가독성 증대
        /// </summary>
        public Dictionary<string, int> GetBalances()
        {
            return new Dictionary<string, int>(m_currencies, StringComparer.OrdinalIgnoreCase);
        }
        #endregion
    }
}
