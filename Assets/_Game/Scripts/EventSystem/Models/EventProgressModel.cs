using System;
using System.Collections.Generic;

namespace BePex.EventSystem.Models
{
    /// <summary>
    /// [기능]: 개별 퀘스트의 진행 상태(진척도, 완료 상태, 보상 수령 여부)를 추적하는 데이터 모델.
    /// [작성자]: 윤승종
    /// </summary>
    [Serializable]
    public class QuestProgressModel
    {
        #region 데이터 멤버
        public string questId;
        public int currentProgress;
        public bool isCompleted;
        public bool isRewardClaimed;
        public long lastUpdatedTicks;
        #endregion

        #region 초기화
        public QuestProgressModel()
        {
            questId = string.Empty;
            currentProgress = 0;
            isCompleted = false;
            isRewardClaimed = false;
            lastUpdatedTicks = 0;
        }
        #endregion
    }

    /// <summary>
    /// [기능]: 개별 이벤트의 고유 정보 및 하위 퀘스트들의 진척도를 관리하는 순수 C# 데이터 모델 (POCO).
    /// [작성자]: 윤승종
    /// [수정 날짜]: 2026-06-16
    /// [마지막 수정 작성자]: 윤승종
    /// [수정 내용]: 1개 이벤트가 N개의 퀘스트를 포함하는 계층형 구조로 스키마 전면 개편
    /// </summary>
    [Serializable]
    public class EventProgressModel
    {
        #region 데이터 멤버
        public string eventId;
        public List<QuestProgressModel> quests;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 기본 생성자로 초기 기본값을 명시적으로 구성합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 퀘스트 리스트 초기화 추가
        /// </summary>
        public EventProgressModel()
        {
            eventId = string.Empty;
            quests = new List<QuestProgressModel>();
        }
        #endregion
    }
}
