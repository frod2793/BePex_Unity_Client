using UnityEngine;
using UnityEngine.UI;
using BePex.EventSystem.ViewModels;
using BePex.EventSystem.DTOs;
using TMPro;
using System.Collections.Generic;

namespace BePex.EventSystem.Views
{
    /// <summary>
    /// [기능]: 선택된 특정 이벤트의 디테일 정보(타이틀, 설명) 및 하위 퀘스트 목록, 일괄 보상 획득 조작 입력을 제어하는 View 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    public class EventDetailView : MonoBehaviour
    {
        #region UI 참조 (Inspector)
        [SerializeField] private TextMeshProUGUI m_titleText;
        [SerializeField] private TextMeshProUGUI m_descText;
        [SerializeField] private Transform m_questListContent;
        [SerializeField] private EventQuestRowView m_questRowPrefab;
        [SerializeField] private Button m_claimAllButton; // 일괄 보상 수령 버튼
        #endregion

        #region 내부 필드
        private EventDetailViewModel m_viewModel;
        private EventListViewModel m_listViewModel;
        private RewardPopupViewModel m_popupViewModel;
        private readonly List<EventQuestRowView> m_spawnedQuestRows = new List<EventQuestRowView>();
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 상세 뷰모델, 목록 뷰모델, 보상 뷰모델 의존성을 수동 주입(Bind)받아 각종 데이터 갱신/선택 이벤트를 관찰 등록하고 UI 리스너를 바인딩합니다. Unity Safe 널체크 준수.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 다중 퀘스트 목록 렌더링에 맞게 UI 리팩토링 및 일괄 보상 버튼 바인딩
        /// </summary>
        public void Bind(EventDetailViewModel viewModel, EventListViewModel listViewModel, RewardPopupViewModel popupViewModel)
        {
            m_viewModel = viewModel;
            m_listViewModel = listViewModel;
            m_popupViewModel = popupViewModel;

            if (m_viewModel != null)
            {
                m_viewModel.OnDetailUpdated += func_OnDetailUpdatedWrapper;
            }

            if (m_listViewModel != null)
            {
                m_listViewModel.OnEventSelected += func_OnEventSelected;
            }

            if (m_claimAllButton != null)
            {
                m_claimAllButton.onClick.RemoveAllListeners();
                m_claimAllButton.onClick.AddListener(func_OnClaimAllButtonClick);
                m_claimAllButton.interactable = false;
            }
        }
        #endregion

        #region 유니티 생명주기
        /// <summary>
        /// [기능]: 뷰의 의존성 바인딩을 끊어 이벤트 리스너 누수로 인한 가상 에러 및 메모리 누수를 원천 차단합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: XML 주석 보완 및 정리
        /// </summary>
        private void OnDestroy()
        {
            if (m_viewModel != null)
            {
                m_viewModel.OnDetailUpdated -= func_OnDetailUpdatedWrapper;
            }

            if (m_listViewModel != null)
            {
                m_listViewModel.OnEventSelected -= func_OnEventSelected;
            }
        }
        #endregion

        #region UI 이벤트 핸들러 및 바인딩
        /// <summary>
        /// [기능]: 목록에서 새로운 이벤트가 선택되었음을 감지했을 때 타겟 상세 정보를 갱신하도록 뷰모델 명령을 호출합니다. func_ 접두사 준수.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public void func_OnEventSelected(string eventId)
        {
            if (m_viewModel != null)
            {
                m_viewModel.SetEvent(eventId);
            }
        }

        /// <summary>
        /// [기능]: 비동기 상세 업데이트 연산을 감싸 동기 이벤트 핸들러 인터페이스 형식으로 반환합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        private async void func_OnDetailUpdatedWrapper()
        {
            await func_OnDetailUpdatedAsync();
        }

        /// <summary>
        /// [기능]: 뷰모델 상태값에 입각해 상세 설명 텍스트, 퀘스트 목록 스폰 및 일괄 [보상 받기] 버튼 활성화 유무를 비동기로 갱신합니다. Fake Null 우회 준수.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 다중 퀘스트 리스트 인스턴싱 및 일괄 보상 상태 체크 구현
        /// </summary>
        public async Awaitable func_OnDetailUpdatedAsync()
        {
            if (m_viewModel == null)
            {
                return;
            }

            var def = m_viewModel.GetEventDefinition();
            if (def == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            if (m_titleText != null)
            {
                m_titleText.text = def.eventTitle;
            }

            if (m_descText != null)
            {
                m_descText.text = def.eventDescription;
            }

            // 기존 스폰된 퀘스트 로우 파괴
            for (int i = 0; i < m_spawnedQuestRows.Count; i++)
            {
                if (m_spawnedQuestRows[i] != null)
                {
                    Destroy(m_spawnedQuestRows[i].gameObject);
                }
            }
            m_spawnedQuestRows.Clear();

            bool anyRewardAvailable = false;

            // 새로운 퀘스트 로우 스폰 및 바인딩
            if (def.quests != null && m_questListContent != null && m_questRowPrefab != null)
            {
                for (int i = 0; i < def.quests.Count; i++)
                {
                    var quest = def.quests[i];
                    var rowInstance = Instantiate(m_questRowPrefab, m_questListContent);
                    if (rowInstance != null)
                    {
                        rowInstance.Bind(quest, m_viewModel);
                        m_spawnedQuestRows.Add(rowInstance);

                        // 일괄 완료 가능 여부 검사
                        bool canClaim = await m_viewModel.CanClaimRewardAsync(quest.questId);
                        if (canClaim)
                        {
                            anyRewardAvailable = true;
                        }
                    }
                }
            }

            // 일괄 보상 받기 버튼 상태 갱신
            if (m_claimAllButton != null)
            {
                m_claimAllButton.interactable = anyRewardAvailable;
            }
        }

        /// <summary>
        /// [기능]: 사용자가 일괄 [보상 받기] 버튼을 클릭하였을 때 실행되는 UI Callback. 뷰모델에 일괄 수령을 요청합니다. func_ 접두사 준수.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public async void func_OnClaimAllButtonClick()
        {
            if (m_viewModel != null)
            {
                await m_viewModel.ClaimAllRewardsAsync();
            }
        }
        #endregion
    }
}
