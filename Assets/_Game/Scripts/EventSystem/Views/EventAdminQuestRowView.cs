using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BePex.EventSystem.DTOs;
using System;

namespace BePex.EventSystem.Views
{
    /// <summary>
    /// [기능]: 이벤트 관리자 화면에서 특정 이벤트에 종속된 퀘스트 목록의 개별 행을 표시하고 편집/삭제 명령을 전달하는 View 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    public class EventAdminQuestRowView : MonoBehaviour
    {
        #region UI 참조 (Inspector)
        [SerializeField] private TextMeshProUGUI m_titleText;
        [SerializeField] private TextMeshProUGUI m_idText;
        [SerializeField] private Button m_editButton;
        [SerializeField] private Button m_removeButton;
        #endregion

        #region 내부 필드
        private QuestDefinitionDTO m_dto;
        private Action<string> m_onEdit;
        private Action<string> m_onRemove;
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 특정 퀘스트 데이터 구조체와 편집/제거 리스너를 바인딩하고 UI 텍스트를 구성합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        public void Bind(QuestDefinitionDTO dto, Action<string> onEdit, Action<string> onRemove)
        {
            m_dto = dto;
            m_onEdit = onEdit;
            m_onRemove = onRemove;

            if (m_titleText != null)
            {
                m_titleText.text = m_dto.questTitle;
            }

            if (m_idText != null)
            {
                m_idText.text = $"ID: {m_dto.questId}";
            }

            if (m_editButton != null)
            {
                m_editButton.onClick.RemoveAllListeners();
                m_editButton.onClick.AddListener(func_OnEditClick);
            }

            if (m_removeButton != null)
            {
                m_removeButton.onClick.RemoveAllListeners();
                m_removeButton.onClick.AddListener(func_OnRemoveClick);
            }
        }

        /// <summary>
        /// [기능]: 이 뷰가 바인딩된 퀘스트 DTO 객체를 반환합니다.
        /// [작성자]: 윤승종
        /// </summary>
        public QuestDefinitionDTO GetDTO()
        {
            return m_dto;
        }
        #endregion

        #region UI 이벤트 핸들러
        /// <summary>
        /// [기능]: 편집 버튼 클릭 감지 시 편집 리스너를 호출하는 UI 이벤트 콜백.
        /// [작성자]: 윤승종
        /// </summary>
        private void func_OnEditClick()
        {
            if (m_onEdit != null && m_dto != null)
            {
                m_onEdit.Invoke(m_dto.questId);
            }
        }

        /// <summary>
        /// [기능]: 삭제 버튼 클릭 감지 시 제거 리스너를 호출하는 UI 이벤트 콜백.
        /// [작성자]: 윤승종
        /// </summary>
        private void func_OnRemoveClick()
        {
            if (m_onRemove != null && m_dto != null)
            {
                m_onRemove.Invoke(m_dto.questId);
            }
        }
        #endregion
    }
}
