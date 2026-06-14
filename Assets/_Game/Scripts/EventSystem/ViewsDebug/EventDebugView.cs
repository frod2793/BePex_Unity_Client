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
        [SerializeField] private TMP_InputField m_eventIdInput;
        [SerializeField] private TMP_InputField m_amountInput;
        [SerializeField] private Button m_addProgressButton;
        [SerializeField] private Button m_resetButton;
        #endregion

        #region 내부 필드
        private EventDebugViewModel m_viewModel;
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 디버그 뷰모델을 주입받아 버튼 리스너 바인딩을 집행합니다. Fake Null 우회 준수.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public void Bind(EventDebugViewModel viewModel)
        {
            m_viewModel = viewModel;

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
        /// [기능]: 수치 더하기 버튼 클릭 시 뷰모델에 수치 가산을 비동기로 지시합니다. func_ 접두사 준수.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Awaitable 비동기 인터페이스로 갱신
        /// </summary>
        public async void func_OnAddProgressClick()
        {
            if (m_viewModel != null && m_eventIdInput != null && m_amountInput != null)
            {
                string evId = m_eventIdInput.text;
                int.TryParse(m_amountInput.text, out int amt);
                await m_viewModel.SimulateAddProgressAsync(evId, amt);
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
