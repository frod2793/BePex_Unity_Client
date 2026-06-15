using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using BePex.EventSystem.DTOs;
using BePex.EventSystem.Interfaces;

namespace BePex.EventSystem.ViewModels
{
    /// <summary>
    /// [기능]: 이벤트 관리자 씬의 데이터 상태(CRUD, 로컬 저장, 원격 업로드)를 관리하고 View와 통신하는 ViewModel 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    public class EventAdminViewModel
    {
        #region 이벤트 정의
        public event Action OnEventListChanged;
        public event Action<string> OnEventSelected;
        public event Action<string> OnErrorOccurred;
        public event Action<bool> OnSaveCompleted;
        public event Action<bool> OnUploadCompleted;
        #endregion

        #region 내부 필드
        private EventTableDTO m_eventTable;
        private string m_selectedEventId;
        private readonly IFirebaseUploadService m_firebaseService;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: Firebase 업로드 서비스 의존성을 주입받아 초기화합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 생성
        /// </summary>
        public EventAdminViewModel(IFirebaseUploadService firebaseService)
        {
            m_firebaseService = firebaseService;
            m_eventTable = new EventTableDTO();
            m_selectedEventId = string.Empty;
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 뷰모델에 관리 대상이 될 DTO 테이블 정보를 설정하고 목록 갱신 이벤트를 노출합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
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
            if (OnEventListChanged != null)
            {
                OnEventListChanged.Invoke();
            }
        }

        /// <summary>
        /// [기능]: 현재 뷰모델이 소유한 이벤트 테이블 DTO를 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        public EventTableDTO GetEventTable()
        {
            return m_eventTable;
        }

        /// <summary>
        /// [기능]: 등록된 전체 이벤트 리스트를 조회합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        public List<EventDefinitionDTO> GetEvents()
        {
            return m_eventTable.events;
        }

        /// <summary>
        /// [기능]: 선택 중인 단일 이벤트를 반환합니다. GC 방지를 위해 for 루프를 사용합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
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
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        public void SelectEvent(string eventId)
        {
            m_selectedEventId = eventId;
            if (OnEventSelected != null)
            {
                OnEventSelected.Invoke(m_selectedEventId);
            }
        }

        /// <summary>
        /// [기능]: 디폴트 정보의 신규 이벤트를 추가 생성하고 활성화 상태로 만듭니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
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
                condition = new ConditionDefinitionDTO { conditionType = "KillCount", targetValue = 10 },
                rewards = new List<RewardDefinitionDTO>()
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
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
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
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
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
                current.condition = updatedData.condition;
                current.rewards = updatedData.rewards;

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

        /// <summary>
        /// [기능]: 현재 테이블 정보를 JSON으로 직렬화하여 로컬 디스크 파일에 비동기로 덤프합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        public async Awaitable<bool> SaveToLocalFileAsync(string customPath = null)
        {
            string path = customPath;
            if (string.IsNullOrEmpty(path))
            {
                path = Path.Combine(Application.dataPath, "_Game/Data/event_table.json");
            }

            try
            {
                string directory = Path.GetDirectoryName(path);
                if (Directory.Exists(directory) == false)
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonUtility.ToJson(m_eventTable, true);
                await Awaitable.BackgroundThreadAsync();
                File.WriteAllText(path, json);
                await Awaitable.MainThreadAsync();

                Debug.Log($"[EventAdminViewModel] 로컬 파일 저장 완료: {path}");
                if (OnSaveCompleted != null)
                {
                    OnSaveCompleted.Invoke(true);
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EventAdminViewModel] 로컬 파일 저장 중 오류 발생: {ex.Message}");
                if (OnSaveCompleted != null)
                {
                    OnSaveCompleted.Invoke(false);
                }
                return false;
            }
        }

        /// <summary>
        /// [기능]: DTO 전체 유효성 검사(ID 및 제목 공란 검출)를 실행하고 Firebase 비동기 업로드를 실행합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        public async Awaitable<bool> UploadToFirebaseAsync()
        {
            if (m_firebaseService == null)
            {
                if (OnErrorOccurred != null)
                {
                    OnErrorOccurred.Invoke("[EventAdminViewModel] Firebase 업로드 서비스가 연결되지 않았습니다.");
                }
                if (OnUploadCompleted != null)
                {
                    OnUploadCompleted.Invoke(false);
                }
                return false;
            }

            for (int i = 0; i < m_eventTable.events.Count; i++)
            {
                var ev = m_eventTable.events[i];
                if (string.IsNullOrEmpty(ev.eventId))
                {
                    if (OnErrorOccurred != null)
                    {
                        OnErrorOccurred.Invoke("[EventAdminViewModel] 이벤트 ID가 공란입니다.");
                    }
                    return false;
                }
                if (string.IsNullOrEmpty(ev.eventTitle))
                {
                    if (OnErrorOccurred != null)
                    {
                        OnErrorOccurred.Invoke($"[EventAdminViewModel] 이벤트 ID ({ev.eventId})의 제목이 공란입니다.");
                    }
                    return false;
                }
            }

            bool success = await m_firebaseService.UploadEventTableAsync(m_eventTable);
            if (OnUploadCompleted != null)
            {
                OnUploadCompleted.Invoke(success);
            }
            return success;
        }
        #endregion
    }
}
