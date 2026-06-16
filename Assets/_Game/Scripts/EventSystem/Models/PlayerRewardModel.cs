using System;
using System.Collections.Generic;
using UnityEngine;

namespace BePex.EventSystem.Models
{
    /// <summary>
    /// [기능]: 기획 변경 및 신규 재화 보상 추가에 무한 확장이 가능하도록 Dictionary 기반으로 개선된 플레이어 자산 데이터 모델.
    /// [작성자]: 윤승종
    /// </summary>
    [Serializable]
    public class PlayerRewardModel : ISerializationCallbackReceiver
    {
        #region 내부 구조체
        /// <summary>
        /// [기능]: Unity JsonUtility 직렬화 지원을 위한 Key-Value 엔트리 구조체.
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
        [SerializeField] private List<string> m_claimedEventIds;
        
        // Unity JsonUtility 직렬화 대상 가변 리스트
        [SerializeField] private List<CurrencyEntry> m_currenciesList;

        // 런타임 고속 조회 및 유연성 보장을 위한 딕셔너리
        private Dictionary<string, int> m_currencies;

        // [하위 호환성 유지용 임시 필드]
        [SerializeField] private int m_totalExp;
        [SerializeField] private int m_totalTickets;
        [SerializeField] private int m_totalPoints;
        [SerializeField] private int m_totalSeasonPoints;
        [SerializeField] private int m_totalCredits;
        #endregion

        #region 프로퍼티 (호환성 제공)
        public List<string> claimedEventIds => m_claimedEventIds;
        public int totalExp => GetCurrencyAmount("Exp");
        public int totalTickets => GetCurrencyAmount("Ticket");
        public int totalPoints => GetCurrencyAmount("Point");
        public int totalSeasonPoints => GetCurrencyAmount("SeasonPoint");
        public int totalCredits => GetCurrencyAmount("CreditReword");
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

        #region 직렬화 콜백 (Serialization Callbacks)
        /// <summary>
        /// [기능]: 직렬화 직전 딕셔너리의 최신 상태를 직렬화 가능한 리스트로 동기화합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: ISerializationCallbackReceiver 구현에 따른 동기화
        /// </summary>
        public void OnBeforeSerialize()
        {
            m_currenciesList.Clear();
            foreach (var pair in m_currencies)
            {
                m_currenciesList.Add(new CurrencyEntry { key = pair.Key, value = pair.Value });
            }
        }

        /// <summary>
        /// [기능]: 역직렬화 직후 리스트 데이터를 딕셔너리로 마이그레이션하고, 구버전 필드의 호환성 마이그레이션을 처리합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 구버전 데이터 복구를 위한 호환성 마이그레이션 포함
        /// </summary>
        public void OnAfterDeserialize()
        {
            m_currencies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
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

            // [Legacy Migration] 구버전 세이브 복구 처리
            MigrateLegacyCurrency("Exp", ref m_totalExp);
            MigrateLegacyCurrency("Ticket", ref m_totalTickets);
            MigrateLegacyCurrency("Point", ref m_totalPoints);
            MigrateLegacyCurrency("SeasonPoint", ref m_totalSeasonPoints);
            MigrateLegacyCurrency("CreditReword", ref m_totalCredits);
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

            if (typeName.Equals("Credit", StringComparison.OrdinalIgnoreCase))
            {
                return "CreditReword";
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
