using UnityEngine;
using BePex.EventSystem.Interfaces;
using BePex.EventSystem.Data;

namespace BePex.EventSystem.Conditions
{
    /// <summary>
    /// [기능]: 출석 체크 일수를 퀘스트 완료 조건으로 달성하였는지 판정하는 Strategy 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    [QuestCondition("Attendance")]
    public class AttendanceQuestCondition : BaseQuestCondition
    {
        #region 초기화
        /// <summary>
        /// [기능]: 부모 생성자를 경유해 목표 출석 체크 일수, 세이브장치, 시간 제공자, 이벤트 ID 및 퀘스트 ID를 주입받습니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: questId 매개변수 추가 및 base 생성자 전달
        /// </summary>
        public AttendanceQuestCondition(int targetValue, ISaveSystem saveSystem, ITimeProvider timeProvider, string eventId, string questId)
            : base(targetValue, saveSystem, timeProvider, eventId, questId)
        {
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 가장 마지막 갱신 시간과 현재 시간을 비교하여 오늘 이미 출석했는지 퀘스트 단위로 검사합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 퀘스트 리스트에서 questId에 해당하는 퀘스트를 검색하여 중복 출석 여부 체크
        /// </summary>
        public override bool CanAddProgress(Models.EventProgressModel progress)
        {
            if (progress == null)
            {
                return false;
            }

            if (progress.quests == null)
            {
                return true;
            }

            Models.QuestProgressModel targetQuest = null;
            for (int i = 0; i < progress.quests.Count; i++)
            {
                if (progress.quests[i].questId == m_questId)
                {
                    targetQuest = progress.quests[i];
                    break;
                }
            }

            if (targetQuest == null)
            {
                return true;
            }

            if (targetQuest.lastUpdatedTicks == 0)
            {
                return true;
            }

            System.DateTime lastTime = new System.DateTime(targetQuest.lastUpdatedTicks);
            System.DateTime currentTime = m_timeProvider.GetCurrentTime();
            return currentTime.Date > lastTime.Date;
        }
        #endregion
    }
}
