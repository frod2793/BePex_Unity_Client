using System;
using System.Collections.Generic;
using UnityEngine;
using BePex.EventSystem.DTOs;

namespace BePex.EventSystem.ViewModels
{
    /// <summary>
    /// [기능]: EventAdminViewModel의 이벤트 관련 CRUD 및 목록 관리 상태 비즈니스 로직을 격리한 partial 클래스 파트.
    /// [작성자]: 윤승종
    /// </summary>
    public partial class EventAdminViewModel
    {
        #region 공개 메서드 (이벤트 CRUD)
        /// <summary>
        /// [기능]: 뷰모델에 관리 대상이 될 DTO 테이블 정보를 설정하고 목록 갱신 이벤트를 노출합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: partial 분할에 따른 이관
        /// </summary>
        public void SetEventTable(EventTableDTO table)
        {
            if (table != null)
            {
                m_eventTable = table;
            }
            else
            {
                m_eventTable = new EventTableDTO();
            }
            m_selectedEventId = string.Empty;
            m_selectedQuestId = string.Empty;
            if (OnEventListChanged != null)
            {
                OnEventListChanged.Invoke();
            }
        }

        /// <summary>
        /// [기능]: 현재 뷰모델이 소유한 이벤트 테이블 DTO를 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: partial 분할에 따른 이관
        /// </summary>
        public EventTableDTO GetEventTable()
        {
            return m_eventTable;
        }

        /// <summary>
        /// [기능]: 등록된 전체 이벤트 리스트를 조회합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: partial 분할에 따른 이관
        /// </summary>
        public List<EventDefinitionDTO> GetEvents()
        {
            return m_eventTable.events;
        }

        /// <summary>
        /// [기능]: 선택 중인 단일 이벤트를 반환합니다. GC 방지를 위해 for 루프를 사용합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: partial 분할에 따른 이관
        /// </summary>
        public EventDefinitionDTO GetSelectedEvent()
        {
            if (string.IsNullOrEmpty(m_selectedEventId))
            {
                return null;
            }
            for (int i = 0; i < m_eventTable.events.Count; i++)
            {
                if (m_eventTable.events[i].eventId == m_selectedEventId)
                {
                    return m_eventTable.events[i];
                }
            }
            return null;
        }

        /// <summary>
        /// [기능]: 특정 이벤트 ID를 뷰모델의 선택 정보로 설정하고 이벤트를 통지합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: partial 분할에 따른 이관
        /// </summary>
        public void SelectEvent(string eventId)
        {
            m_selectedEventId = eventId;
            m_selectedQuestId = string.Empty;
            if (OnEventSelected != null)
            {
                OnEventSelected.Invoke(m_selectedEventId);
            }
        }

        /// <summary>
        /// [기능]: 디폴트 정보의 신규 이벤트를 추가 생성하고 활성화 상태로 만듭니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: partial 분할에 따른 이관 및 네이밍 정리
        /// </summary>
        public void AddNewEvent()
        {
            string newId = $"evt_new_{DateTime.Now.Ticks}";
            var newEvent = new EventDefinitionDTO()
            {
                eventId = newId,
                eventTitle = "새로운 이벤트",
                eventDescription = "이벤트 설명을 입력하세요.",
                eventIconAddress = "item_Sheet[item_Sheet_0]",
                startDate = DateTime.Now.ToString("yyyy-MM-dd"),
                endDate = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd"),
                quests = new List<QuestDefinitionDTO>()
                {
                    new QuestDefinitionDTO
                    {
                        questId = $"quest_{newId}_1",
                        questTitle = "기본 퀘스트",
                        questDescription = "적을 처치하세요.",
                        condition = new ConditionDefinitionDTO { conditionType = "KillCount", targetValue = 10 },
                        rewards = new List<RewardDefinitionDTO>()
                    }
                }
            };

            m_eventTable.events.Add(newEvent);
            SelectEvent(newId);
            if (OnEventListChanged != null)
            {
                OnEventListChanged.Invoke();
            }
        }

        /// <summary>
        /// [기능]: 특정 ID의 이벤트를 삭제하고 목록을 리렌더링하도록 이벤트를 노출합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: partial 분할에 따른 이관
        /// </summary>
        public void RemoveEvent(string eventId)
        {
            int targetIndex = -1;
            for (int i = 0; i < m_eventTable.events.Count; i++)
            {
                if (m_eventTable.events[i].eventId == eventId)
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex != -1)
            {
                m_eventTable.events.RemoveAt(targetIndex);
                if (m_selectedEventId == eventId)
                {
                    m_selectedEventId = string.Empty;
                }
                if (OnEventListChanged != null)
                {
                    OnEventListChanged.Invoke();
                }
                if (OnEventSelected != null)
                {
                    OnEventSelected.Invoke(m_selectedEventId);
                }
            }
        }

        /// <summary>
        /// [기능]: 편집 폼을 통해 넘어온 데이터로 이벤트를 갱신합니다. ID 중복 확인 유효성 검사가 동반됩니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: partial 분할에 따른 이관
        /// </summary>
        public void UpdateSelectedEvent(EventDefinitionDTO updatedData)
        {
            if (updatedData == null)
            {
                return;
            }

            var current = GetSelectedEvent();
            if (current != null)
            {
                current.eventTitle = updatedData.eventTitle;
                current.eventDescription = updatedData.eventDescription;
                current.eventIconAddress = updatedData.eventIconAddress;
                current.startDate = updatedData.startDate;
                current.endDate = updatedData.endDate;
                current.quests = updatedData.quests;

                if (current.eventId != updatedData.eventId)
                {
                    bool isDuplicate = false;
                    for (int i = 0; i < m_eventTable.events.Count; i++)
                    {
                        if (m_eventTable.events[i].eventId == updatedData.eventId && m_eventTable.events[i] != current)
                        {
                            isDuplicate = true;
                            break;
                        }
                    }

                    if (isDuplicate)
                    {
                        if (OnErrorOccurred != null)
                        {
                            OnErrorOccurred.Invoke("[EventAdminViewModel] 중복된 이벤트 ID가 존재합니다.");
                        }
                        return;
                    }
                    current.eventId = updatedData.eventId;
                    m_selectedEventId = updatedData.eventId;
                }

                if (OnEventListChanged != null)
                {
                    OnEventListChanged.Invoke();
                }
            }
        }
        #endregion
    }
}
