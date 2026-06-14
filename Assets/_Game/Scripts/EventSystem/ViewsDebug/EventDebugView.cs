using UnityEngine;
using UnityEngine.UI;
using BePex.EventSystem.ViewModelsDebug;
using TMPro;

namespace BePex.EventSystem.ViewsDebug
{
    /// <summary>
    /// [기능]: 테스트 환경에서 인풋 필드 입력과 버튼 터치를 모아 디버그 뷰모델에 인가해 주는 조작용 UI View 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    public class EventDebugView : MonoBehaviour
    {
        #region UI 참조 (Inspector)
        [SerializeField] private TMP_Dropdown m_eventIdDropdown;
        [SerializeField] private TMP_InputField m_amountInput;
        [SerializeField] private Button m_addProgressButton;
        [SerializeField] private Button m_resetButton;
        #endregion

        #region 내부 필드
        private EventDebugViewModel m_viewModel;
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 디버그 뷰모델을 주입받아 드롭다운 목록을 채우고 버튼 리스너 바인딩을 집행합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 인풋 필드에서 드롭다운 선택 방식으로 개편
        /// </summary>
        public void Bind(EventDebugViewModel viewModel)
        {
            m_viewModel = viewModel;

            if (m_eventIdDropdown != null && m_viewModel != null)
            {
                m_eventIdDropdown.onValueChanged.RemoveAllListeners();
                m_eventIdDropdown.ClearOptions();
                
                var events = m_viewModel.GetActiveEvents();
                var options = new System.Collections.Generic.List<string>();
                for (int i = 0; i < events.Count; i++)
                {
                    options.Add(string.Format("[{0}] {1}", events[i].eventId, events[i].eventTitle));
                }
                m_eventIdDropdown.AddOptions(options);
            }

            if (m_addProgressButton != null)
            {
                m_addProgressButton.onClick.RemoveAllListeners();
                m_addProgressButton.onClick.AddListener(func_OnAddProgressClick);
            }

            if (m_resetButton != null)
            {
                m_resetButton.onClick.RemoveAllListeners();
                m_resetButton.onClick.AddListener(func_OnResetClick);
            }
        }

        /// <summary>
        /// [기능]: 수치 더하기 버튼 클릭 시 드롭다운에서 선택된 이벤트의 ID를 추출하여 진행도를 비동기로 가산합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 드롭다운 기반 ID 파싱 및 비동기 가산 지시
        /// </summary>
        public async void func_OnAddProgressClick()
        {
            if (m_viewModel != null && m_eventIdDropdown != null && m_amountInput != null)
            {
                if (m_eventIdDropdown.options.Count == 0)
                {
                    return;
                }

                string selectedText = m_eventIdDropdown.options[m_eventIdDropdown.value].text;
                string evId = string.Empty;
                
                int openBracket = selectedText.IndexOf('[');
                int closeBracket = selectedText.IndexOf(']');
                if (openBracket >= 0 && closeBracket > openBracket)
                {
                    evId = selectedText.Substring(openBracket + 1, closeBracket - openBracket - 1);
                }

                if (!string.IsNullOrEmpty(evId))
                {
                    int.TryParse(m_amountInput.text, out int amt);
                    await m_viewModel.SimulateAddProgressAsync(evId, amt);
                }
            }
        }

        /// <summary>
        /// [기능]: 데이터 리셋 버튼 클릭 시 전체 세이브 삭제 및 모델 갱신을 비동기로 지시합니다. func_ 접두사 준수.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Awaitable 비동기 인터페이스로 갱신
        /// </summary>
        public async void func_OnResetClick()
        {
            if (m_viewModel != null)
            {
                await m_viewModel.ResetAllDataAsync();
            }
        }
        #endregion
    }
}
