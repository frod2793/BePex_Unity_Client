using System;
using System.Collections.Generic;
using UnityEngine;
using BePex.EventSystem.DTOs;

namespace BePex.EventSystem.ViewModels
{
    /// <summary>
    /// [기능]: EventAdminViewModel의 퀘스트 관련 CRUD 및 세부 조건/보상 매핑 비즈니스 로직을 격리한 partial 클래스 파트.
    /// [작성자]: 윤승종
    /// </summary>
    public partial class EventAdminViewModel
    {
        #region 공개 메서드 (퀘스트 CRUD)
        /// <summary>
        /// [기능]: 선택 중인 단일 퀘스트를 반환합니다. GC 방지를 위해 for 루프를 사용합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: partial 분할에 따른 이관
        /// </summary>
        public QuestDefinitionDTO GetSelectedQuest()
        {
            var ev = GetSelectedEvent();
            if (ev == null || string.IsNullOrEmpty(m_selectedQuestId))
            {
                return null;
            }
            for (int i = 0; i < ev.quests.Count; i++)
            {
                if (ev.quests[i].questId == m_selectedQuestId)
                {
                    return ev.quests[i];
                }
            }
            return null;
        }

        /// <summary>
        /// [기능]: 특정 퀘스트 ID를 선택하고 이벤트를 통지합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: partial 분할에 따른 이관
        /// </summary>
        public void SelectQuest(string questId)
        {
            m_selectedQuestId = questId;
            if (OnQuestSelected != null)
            {
                OnQuestSelected.Invoke(m_selectedQuestId);
            }
        }

        /// <summary>
        /// [기능]: 현재 선택된 이벤트에 신규 퀘스트를 추가합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: partial 분할에 따른 이관
        /// </summary>
        public void AddNewQuest()
        {
            var ev = GetSelectedEvent();
            if (ev == null)
            {
                return;
            }

            string newId = $"quest_{DateTime.Now.Ticks}";
            var newQuest = new QuestDefinitionDTO
            {
                questId = newId,
                questTitle = "새로운 퀘스트",
                questDescription = "퀘스트 설명을 입력하세요.",
                condition = new ConditionDefinitionDTO { conditionType = "KillCount", targetValue = 10 },
                rewards = new List<RewardDefinitionDTO>()
            };

            ev.quests.Add(newQuest);
            SelectQuest(newId);
            
            if (OnQuestListChanged != null)
            {
                OnQuestListChanged.Invoke();
            }
        }

        /// <summary>
        /// [기능]: 특정 ID의 퀘스트를 삭제하고 목록을 리렌더링하도록 노출합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: partial 분할에 따른 이관
        /// </summary>
        public void RemoveQuest(string questId)
        {
            var ev = GetSelectedEvent();
            if (ev == null)
            {
                return;
            }

            int targetIndex = -1;
            for (int i = 0; i < ev.quests.Count; i++)
            {
                if (ev.quests[i].questId == questId)
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex != -1)
            {
                ev.quests.RemoveAt(targetIndex);
                if (m_selectedQuestId == questId)
                {
                    m_selectedQuestId = string.Empty;
                }
                
                if (OnQuestListChanged != null)
                {
                    OnQuestListChanged.Invoke();
                }
                if (OnQuestSelected != null)
                {
                    OnQuestSelected.Invoke(m_selectedQuestId);
                }
            }
        }

        /// <summary>
        /// [기능]: 편집 폼을 통해 넘어온 데이터로 퀘스트를 갱신합니다. ID 중복 확인 유효성 검사가 동반됩니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: partial 분할에 따른 이관
        /// </summary>
        public void UpdateSelectedQuest(QuestDefinitionDTO updatedData)
        {
            if (updatedData == null)
            {
                return;
            }

            var current = GetSelectedQuest();
            if (current != null)
            {
                current.questTitle = updatedData.questTitle;
                current.questDescription = updatedData.questDescription;
                current.condition = updatedData.condition;
                current.rewards = updatedData.rewards;

                if (current.questId != updatedData.questId)
                {
                    bool isDuplicate = false;
                    var ev = GetSelectedEvent();
                    if (ev != null)
                    {
                        for (int i = 0; i < ev.quests.Count; i++)
                        {
                            if (ev.quests[i].questId == updatedData.questId && ev.quests[i] != current)
                            {
                                isDuplicate = true;
                                break;
                            }
                        }
                    }

                    if (isDuplicate)
                    {
                        if (OnErrorOccurred != null)
                        {
                            OnErrorOccurred.Invoke("[EventAdminViewModel] 중복된 퀘스트 ID가 존재합니다.");
                        }
                        return;
                    }
                    current.questId = updatedData.questId;
                    m_selectedQuestId = updatedData.questId;
                }

                if (OnQuestListChanged != null)
                {
                    OnQuestListChanged.Invoke();
                }
            }
        }
        #endregion
    }
}
