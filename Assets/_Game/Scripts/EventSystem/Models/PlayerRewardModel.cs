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
        #region 내부 구조체
        /// <summary>
        /// [기능]: Unity JsonUtility 직렬화 지원을 위한 Key-Value 엔트리 구조체 (레거시 하위 호환용).
        /// [작성자]: 윤승종
        /// </summary>
        [Serializable]
        public struct CurrencyEntry
        {
            public string key;
            public int value;
        }
        #endregion

        #region 데이터 멤버
        [JsonProperty("claimedEventIds")]
        [SerializeField] private List<string> m_claimedEventIds;
        
        // Unity JsonUtility 직렬화 대상 가변 리스트 (레거시 세이브 파일 파싱용)
        [JsonProperty("m_currenciesList")]
        [SerializeField] private List<CurrencyEntry> m_currenciesList;

        // 런타임 고속 조회 및 유연성 보장을 위한 딕셔너리
        private Dictionary<string, int> m_currencies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        // [하위 호환성 유지용 임시 필드]
        [JsonProperty("m_totalExp")]
        [SerializeField] private int m_totalExp;
        [JsonProperty("m_totalTickets")]
        [SerializeField] private int m_totalTickets;
        [JsonProperty("m_totalPoints")]
        [SerializeField] private int m_totalPoints;
        [JsonProperty("m_totalSeasonPoints")]
        [SerializeField] private int m_totalSeasonPoints;
        [JsonProperty("m_totalCredits")]
        [SerializeField] private int m_totalCredits;
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
        public int totalCredits => GetCurrencyAmount("CreditReward");

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
            m_currenciesList = new List<CurrencyEntry>();
            m_currencies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }
        #endregion

        #region 직렬화 콜백 (Newtonsoft.Json OnDeserialized)
        /// <summary>
        /// [기능]: 역직렬화 직후 구버전 세이브 리스트 데이터를 딕셔너리로 복구하고, 레거시 필드들의 호환 마이그레이션을 자동 처리합니다.
        /// [작성자]: 윤승종
        /// </summary>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (m_currencies == null)
            {
                m_currencies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            }

            // 1. 구버전 m_currenciesList 리스트 세이브 데이터를 딕셔너리로 마이그레이션
            if (m_currenciesList != null && m_currenciesList.Count > 0)
            {
                for (int i = 0; i < m_currenciesList.Count; i++)
                {
                    string normKey = NormalizeCurrencyKey(m_currenciesList[i].key);
                    if (m_currencies.ContainsKey(normKey))
                    {
                        m_currencies[normKey] += m_currenciesList[i].value;
                    }
                    else
                    {
                        m_currencies[normKey] = m_currenciesList[i].value;
                    }
                }
                m_currenciesList.Clear();
            }

            // 2. [Legacy Migration] 구버전 단일 변수 세이브 복구 처리
            MigrateLegacyCurrency("Exp", ref m_totalExp);
            MigrateLegacyCurrency("Ticket", ref m_totalTickets);
            MigrateLegacyCurrency("Point", ref m_totalPoints);
            MigrateLegacyCurrency("SeasonPoint", ref m_totalSeasonPoints);
            MigrateLegacyCurrency("CreditReward", ref m_totalCredits);

            // 3. [Modernization Migration] "CreditReword" 레거시 오타 키가 딕셔너리에 존재하는 경우 "CreditReward"로 병합 및 현대화
            if (m_currencies.TryGetValue("CreditReword", out int legacyAmount))
            {
                if (m_currencies.ContainsKey("CreditReward"))
                {
                    m_currencies["CreditReward"] += legacyAmount;
                }
                else
                {
                    m_currencies["CreditReward"] = legacyAmount;
                }
                m_currencies.Remove("CreditReword");

                Debug.Log($"[PlayerRewardModel] 레거시 재화 데이터('CreditReword')를 현대적인 포맷('CreditReward')으로 변환 및 통합 완료했습니다. (합산된 금액: {legacyAmount})");
            }
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
        /// [기능]: 레거시 세이브 데이터를 신규 딕셔너리로 마이그레이션하고 레거시 변수 값을 초기화합니다.
        /// [작성자]: 윤승종
        /// </summary>
        private void MigrateLegacyCurrency(string typeName, ref int legacyField)
        {
            if (legacyField > 0)
            {
                string normKey = NormalizeCurrencyKey(typeName);
                if (m_currencies.ContainsKey(normKey))
                {
                    m_currencies[normKey] += legacyField;
                }
                else
                {
                    m_currencies.Add(normKey, legacyField);
                }
                legacyField = 0;
            }
        }

        /// <summary>
        /// [기능]: 재화의 식별자 키값을 런타임 표준("CreditReword")으로 일관되게 정규화합니다.
        /// [작성자]: 윤승종
        /// </summary>
        private string NormalizeCurrencyKey(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return typeName;
            }

            if (typeName.Equals("Credit", StringComparison.OrdinalIgnoreCase) || 
                typeName.Equals("CreditReword", StringComparison.OrdinalIgnoreCase))
            {
                return "CreditReward";
            }

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
