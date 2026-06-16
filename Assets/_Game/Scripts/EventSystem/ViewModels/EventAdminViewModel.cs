using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using BePex.EventSystem.DTOs;
using BePex.EventSystem.Interfaces;
using BePex.EventSystem.Data;

namespace BePex.EventSystem.ViewModels
{
    /// <summary>
    /// [기능]: 이벤트 관리자 씬의 데이터 상태(CRUD, 로컬 저장, 원격 업로드)를 관리하고 View와 통신하는 ViewModel 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    public partial class EventAdminViewModel
    {
        #region 이벤트 정의
        public event Action OnEventListChanged;
        public event Action<string> OnEventSelected;
        public event Action OnQuestListChanged;
        public event Action<string> OnQuestSelected;
        public event Action<string> OnErrorOccurred;
        public event Action<bool> OnSaveCompleted;
        public event Action<bool> OnUploadCompleted;
        #endregion

        #region 내부 필드
        private EventTableDTO m_eventTable;
        private string m_selectedEventId;
        private string m_selectedQuestId;
        private readonly IFirebaseUploadService m_firebaseService;
        private readonly ConditionTypeRegistrySO m_conditionTypeRegistry;
        private readonly RewardTypeRegistrySO m_rewardTypeRegistry;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: Firebase 업로드 서비스, 조건 타입 레지스트리 및 보상 타입 레지스트리 의존성을 주입받아 초기화합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Type Object 패턴을 위한 RewardTypeRegistrySO 주입 추가 및 기본값 설정
        /// </summary>
        public EventAdminViewModel(IFirebaseUploadService firebaseService, ConditionTypeRegistrySO conditionTypeRegistry = null, RewardTypeRegistrySO rewardTypeRegistry = null)
        {
            m_firebaseService = firebaseService;
            m_conditionTypeRegistry = conditionTypeRegistry;
            m_rewardTypeRegistry = rewardTypeRegistry;
            m_eventTable = new EventTableDTO();
            m_selectedEventId = string.Empty;
            m_selectedQuestId = string.Empty;
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 레지스트리에 등록된 사용 가능한 조건 타입 목록을 조회합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        public IReadOnlyList<ConditionTypeSO> GetAvailableConditionTypes()
        {
            if (m_conditionTypeRegistry != null && m_conditionTypeRegistry.ConditionTypes != null)
            {
                var rawList = m_conditionTypeRegistry.ConditionTypes;
                var filtered = new List<ConditionTypeSO>();
                for (int i = 0; i < rawList.Count; i++)
                {
                    if (rawList[i] != null)
                    {
                        filtered.Add(rawList[i]);
                    }
                }
                return filtered;
            }
            return Array.Empty<ConditionTypeSO>();
        }

        /// <summary>
        /// [기능]: 레지스트리에 등록된 사용 가능한 보상 타입 목록을 조회합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        public IReadOnlyList<RewardTypeSO> GetAvailableRewardTypes()
        {
            if (m_rewardTypeRegistry != null && m_rewardTypeRegistry.RewardTypes != null)
            {
                var rawList = m_rewardTypeRegistry.RewardTypes;
                var filtered = new List<RewardTypeSO>();
                for (int i = 0; i < rawList.Count; i++)
                {
                    if (rawList[i] != null)
                    {
                        filtered.Add(rawList[i]);
                    }
                }
                return filtered;
            }
            return Array.Empty<RewardTypeSO>();
        }







        /// <summary>
        /// [기능]: 현재 테이블 정보를 JSON으로 직렬화하여 로컬 디스크 파일에 비동기로 덤프합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: EditMode 테스트 환경에서 Awaitable 스케줄러 데드락 정지 해결을 위해 Application.isPlaying 분기 가드 적용
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
                if (Application.isPlaying)
                {
                    await Awaitable.BackgroundThreadAsync();
                    File.WriteAllText(path, json);
                    await Awaitable.MainThreadAsync();
                }
                else
                {
                    File.WriteAllText(path, json);
                }

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
                    if (OnUploadCompleted != null)
                    {
                        OnUploadCompleted.Invoke(false);
                    }
                    return false;
                }
                if (string.IsNullOrEmpty(ev.eventTitle))
                {
                    if (OnErrorOccurred != null)
                    {
                        OnErrorOccurred.Invoke($"[EventAdminViewModel] 이벤트 ID ({ev.eventId})의 제목이 공란입니다.");
                    }
                    if (OnUploadCompleted != null)
                    {
                        OnUploadCompleted.Invoke(false);
                    }
                    return false;
                }
            }

            // 로컬 디스크 저장 절차 자동 삽입
            bool saveSuccess = await SaveToLocalFileAsync();
            if (saveSuccess == false)
            {
                if (OnErrorOccurred != null)
                {
                    OnErrorOccurred.Invoke("[EventAdminViewModel] 로컬 파일 저장 실패로 인해 서버 업로드가 차단되었습니다.");
                }
                if (OnUploadCompleted != null)
                {
                    OnUploadCompleted.Invoke(false);
                }
                return false;
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
