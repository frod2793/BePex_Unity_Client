using UnityEngine;
using BePex.EventSystem.Models;
using BePex.EventSystem.Interfaces;
using BePex.EventSystem.DTOs;

namespace BePex.EventSystem.ViewModelsDebug
{
    /// <summary>
    /// [기능]: 테스트 환경에서 인위적으로 조건 수치를 더하고 세이브 상태를 조작/초기화하여 이벤트를 시뮬레이션하는 Debug 전용 ViewModel 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    public class EventDebugViewModel
    {
        #region 내부 필드
        private readonly EventModel m_eventModel;
        private readonly ISaveSystem m_saveSystem;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 비즈니스 로직 모델 및 저장 상태 장치를 주입받는 생성자.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public EventDebugViewModel(EventModel eventModel, ISaveSystem saveSystem)
        {
            m_eventModel = eventModel;
            m_saveSystem = saveSystem;
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 지정 이벤트 ID에 대해 인위적인 가산 처리를 도메인 단에 하달해 게이지 변화 및 클리어 상태를 비동기로 모의 조작합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Awaitable 비동기 인터페이스로 갱신
        /// </summary>
        public async Awaitable SimulateAddProgressAsync(string eventId, int amount)
        {
            if (m_eventModel != null)
            {
                await m_eventModel.Debug_AddProgressAsync(eventId, amount, m_saveSystem);
            }
        }

        /// <summary>
        /// [기능]: 현재 로드된 활성 이벤트 목록을 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 추가
        /// </summary>
        public System.Collections.Generic.List<EventDefinitionDTO> GetActiveEvents()
        {
            return m_eventModel != null ? m_eventModel.GetActiveEvents() : new System.Collections.Generic.List<EventDefinitionDTO>();
        }

        /// <summary>
        /// [기능]: 세이브 데이터를 비동기로 완전히 밀어버리고 모델을 재적재(Reload)하여 화면 상태를 공백으로 되돌립니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Awaitable 비동기 인터페이스로 갱신
        /// </summary>
        public async Awaitable ResetAllDataAsync()
        {
            if (m_saveSystem != null && m_eventModel != null)
            {
                await m_saveSystem.ClearAllAsync();
                m_eventModel.Reload();
            }
        }
        #endregion
    }
}
