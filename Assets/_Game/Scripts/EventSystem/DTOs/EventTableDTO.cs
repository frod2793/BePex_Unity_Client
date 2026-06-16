using System;
using System.Collections.Generic;

namespace BePex.EventSystem.DTOs
{
    /// <summary>
    /// [기능]: 이벤트 테이블 전체 기획 데이터를 담는 최상위 데이터 전송 객체(DTO).
    /// [작성자]: 윤승종
    /// </summary>
    [Serializable]
    public class EventTableDTO
    {
        public List<EventDefinitionDTO> events = new List<EventDefinitionDTO>();
    }

    /// <summary>
    /// [기능]: 개별 퀘스트의 기획 데이터를 담는 데이터 전송 객체(DTO).
    /// [작성자]: 윤승종
    /// </summary>
    [Serializable]
    public class QuestDefinitionDTO
    {
        public string questId;
        public string questTitle;
        public string questDescription;
        public ConditionDefinitionDTO condition;
        public List<RewardDefinitionDTO> rewards = new List<RewardDefinitionDTO>();
    }

    /// <summary>
    /// [기능]: 개별 이벤트 기획 메타데이터 정보를 직렬화하기 위한 데이터 전송 객체(DTO).
    /// [작성자]: 윤승종
    /// [수정 날짜]: 2026-06-16
    /// [마지막 수정 작성자]: 윤승종
    /// [수정 내용]: 날짜 파싱 캐싱 필드 및 헬퍼 추가로 성능 최적화
    /// </summary>
    [Serializable]
    public class EventDefinitionDTO
    {
        public string eventId;
        public string eventTitle;
        public string eventDescription;
        public string eventIconAddress;
        public string startDate;
        public string endDate;
        public List<QuestDefinitionDTO> quests = new List<QuestDefinitionDTO>();

        [NonSerialized] private DateTime? m_cachedStartDateTime;
        [NonSerialized] private DateTime? m_cachedEndDateTime;
        [NonSerialized] private bool m_isStartParsed = false;
        [NonSerialized] private bool m_isEndParsed = false;

        public DateTime? GetStartDateTime()
        {
            if (m_isStartParsed == false)
            {
                if (DateTime.TryParse(startDate, out DateTime startDt))
                {
                    m_cachedStartDateTime = startDt;
                }
                else
                {
                    m_cachedStartDateTime = null;
                }
                m_isStartParsed = true;
            }
            return m_cachedStartDateTime;
        }

        public DateTime? GetEndDateTime()
        {
            if (m_isEndParsed == false)
            {
                if (DateTime.TryParse(endDate, out DateTime endDt))
                {
                    m_cachedEndDateTime = endDt;
                }
                else
                {
                    m_cachedEndDateTime = null;
                }
                m_isEndParsed = true;
            }
            return m_cachedEndDateTime;
        }
    }

    /// <summary>
    /// [기능]: 이벤트 달성 조건 설정 기획 수치를 담는 데이터 전송 객체(DTO).
    /// [작성자]: 윤승종
    /// </summary>
    [Serializable]
    public class ConditionDefinitionDTO
    {
        public string conditionType;
        public int targetValue;
    }

    /// <summary>
    /// [기능]: 이벤트 완료 시 지급되는 단일 보상 명세를 담는 데이터 전송 객체(DTO).
    /// [작성자]: 윤승종
    /// </summary>
    [Serializable]
    public class RewardDefinitionDTO
    {
        public string rewardType;
        public int amount;
        public string displayName;
        public string iconAddress;
    }
}
