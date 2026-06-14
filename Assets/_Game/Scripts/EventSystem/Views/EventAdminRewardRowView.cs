using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BePex.EventSystem.DTOs;
using BePex.EventSystem.Data;
using BePex.EventSystem.Utils;
using System;

namespace BePex.EventSystem.Views
{
    /// <summary>
    /// [기능]: 이벤트 관리자 화면에서 보상 항목 리스트의 개별 데이터 입력 행을 표현 및 제어하는 View 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    public class EventAdminRewardRowView : MonoBehaviour
    {
        #region UI 참조 (Inspector)
        [SerializeField] private TMP_Dropdown m_typeDropdown;
        [SerializeField] private TMP_InputField m_amountInput;
        [SerializeField] private TMP_InputField m_nameInput;
        [SerializeField] private TMP_InputField m_iconInput;
        [SerializeField] private Button m_removeButton;
        #endregion

        #region 내부 필드
        private RewardDefinitionDTO m_dto;
        private Action<EventAdminRewardRowView> m_onRemove;
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 특정 보상 데이터 구조체와 제거 리스너를 수동 바인딩하고 UI 값을 채웁니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        public void Bind(RewardDefinitionDTO dto, Action<EventAdminRewardRowView> onRemove)
        {
            m_dto = dto;
            m_onRemove = onRemove;

            if (m_typeDropdown != null)
            {
                m_typeDropdown.onValueChanged.RemoveAllListeners();
                m_typeDropdown.ClearOptions();
                var options = EnumDisplayHelper.GetDisplayNames<RewardDefinitionSO.RewardType>();
                m_typeDropdown.AddOptions(options);

                int optionIndex = GetDropdownOptionIndex(m_dto.rewardType);
                m_typeDropdown.value = optionIndex;
                m_typeDropdown.onValueChanged.AddListener(func_OnTypeChanged);
            }

            if (m_amountInput != null)
            {
                m_amountInput.onValueChanged.RemoveAllListeners();
                m_amountInput.text = m_dto.amount.ToString();
                m_amountInput.onValueChanged.AddListener(func_OnAmountChanged);
            }

            if (m_nameInput != null)
            {
                m_nameInput.onValueChanged.RemoveAllListeners();
                m_nameInput.text = m_dto.displayName;
                m_nameInput.onValueChanged.AddListener(func_OnNameChanged);
            }

            if (m_iconInput != null)
            {
                m_iconInput.onValueChanged.RemoveAllListeners();
                m_iconInput.text = m_dto.iconAddress;
                m_iconInput.onValueChanged.AddListener(func_OnIconChanged);
            }

            if (m_removeButton != null)
            {
                m_removeButton.onClick.RemoveAllListeners();
                m_removeButton.onClick.AddListener(func_OnRemoveClick);
            }
        }

        /// <summary>
        /// [기능]: 이 뷰가 바인딩된 보상 DTO 객체를 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        public RewardDefinitionDTO GetDTO()
        {
            return m_dto;
        }
        #endregion

        #region UI 이벤트 핸들러
        /// <summary>
        /// [기능]: 드롭다운의 보상 타입 수정 변경 발생 시 DTO에 인가하는 UI 이벤트 콜백.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        private void func_OnTypeChanged(int index)
        {
            if (m_dto != null)
            {
                if (m_typeDropdown != null)
                {
                    string displayName = m_typeDropdown.options[index].text;
                    var typeEnum = EnumDisplayHelper.GetEnumValue<RewardDefinitionSO.RewardType>(displayName);
                    m_dto.rewardType = typeEnum.ToString();
                }
            }
        }

        /// <summary>
        /// [기능]: 수량 InputField 변경 발생 시 DTO에 인가하는 UI 이벤트 콜백.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        private void func_OnAmountChanged(string val)
        {
            if (m_dto != null)
            {
                int.TryParse(val, out int result);
                m_dto.amount = result;
            }
        }

        /// <summary>
        /// [기능]: 노출 이름 InputField 변경 발생 시 DTO에 인가하는 UI 이벤트 콜백.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        private void func_OnNameChanged(string val)
        {
            if (m_dto != null)
            {
                m_dto.displayName = val;
            }
        }

        /// <summary>
        /// [기능]: 아이콘 어드레서블 주소 InputField 변경 발생 시 DTO에 인가하는 UI 이벤트 콜백.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        private void func_OnIconChanged(string val)
        {
            if (m_dto != null)
            {
                m_dto.iconAddress = val;
            }
        }

        /// <summary>
        /// [기능]: 삭제 버튼 클릭 감지 시 제거 리스너를 호출하는 UI 이벤트 콜백.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        private void func_OnRemoveClick()
        {
            if (m_onRemove != null)
            {
                m_onRemove.Invoke(this);
            }
        }
        #endregion

        #region 헬퍼 메서드
        /// <summary>
        /// [기능]: 보상 타입 스트링과 일치하는 드롭다운의 인덱스를 선별합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        private int GetDropdownOptionIndex(string rewardType)
        {
            if (m_typeDropdown == null)
            {
                return 0;
            }
            string searchName = rewardType;
            if (Enum.TryParse<RewardDefinitionSO.RewardType>(rewardType, true, out var typeEnum))
            {
                searchName = EnumDisplayHelper.GetDisplayName(typeEnum);
            }

            for (int i = 0; i < m_typeDropdown.options.Count; i++)
            {
                if (m_typeDropdown.options[i].text.Equals(searchName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return 0;
        }
        #endregion
    }
}
