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
