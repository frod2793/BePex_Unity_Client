using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using BePex.EventSystem.ViewModels;
using BePex.EventSystem.DTOs;
using BePex.EventSystem.Data;
using BePex.EventSystem.Factories;
using System;

namespace BePex.EventSystem.Views
{
    /// <summary>
    /// [기능]: 이벤트 관리자 UI 씬의 모든 입력/갱신 시각 인터렉션을 제어하고 ViewModel을 중개하는 View 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    public class EventAdminView : MonoBehaviour
    {
        #region UI 참조 (Inspector) - 좌측 패널
        [Header("좌측 패널")]
        [SerializeField] private RectTransform m_eventListContent;
        [SerializeField] private EventItemCell m_eventItemPrefab;
        [SerializeField] private Button m_addEventButton;
        #endregion

        #region UI 참조 (Inspector) - 우측 패널 (이벤트 편집 모드)
        [Header("우측 패널 (이벤트 편집 모드)")]
        [SerializeField] private GameObject m_eventDetailPanel;
        [SerializeField] private TMP_InputField m_eventIdInput;
        [SerializeField] private TMP_InputField m_titleInput;
        [SerializeField] private TMP_InputField m_descInput;
        [SerializeField] private TMP_Dropdown m_iconAddressDropdown;
        [SerializeField] private Image m_iconAddressPreview;
        [SerializeField] private TMP_InputField m_startDateInput;
        [SerializeField] private TMP_InputField m_endDateInput;

        [Header("퀘스트 목록 (이벤트 편집 모드)")]
        [SerializeField] private RectTransform m_questListContent;
        [SerializeField] private EventAdminQuestRowView m_questRowPrefab;
        [SerializeField] private Button m_addQuestButton;
        #endregion

        #region UI 참조 (Inspector) - 우측 패널 (퀘스트 편집 모드)
        [Header("우측 패널 (퀘스트 편집 모드)")]
        [SerializeField] private GameObject m_questDetailPanel;
        [SerializeField] private Button m_backToEventButton;
        [SerializeField] private TMP_InputField m_questIdInput;
        [SerializeField] private TMP_InputField m_questTitleInput;
        [SerializeField] private TMP_InputField m_questDescInput;

        [Header("조건 편집 (퀘스트 편집 모드)")]
        [SerializeField] private TMP_Dropdown m_condTypeDropdown;
        [SerializeField] private TMP_InputField m_condTargetInput;

        [Header("보상 편집 (퀘스트 편집 모드)")]
        [SerializeField] private RectTransform m_rewardListContent;
        [SerializeField] private EventAdminRewardRowView m_rewardRowPrefab;
        [SerializeField] private Button m_addRewardButton;

        [Header("신규 보상 추가 입력 영역 (퀘스트 편집 모드)")]
        [SerializeField] private TMP_Dropdown m_newRewardTypeDropdown;
        [SerializeField] private TMP_InputField m_newRewardAmountInput;
        [SerializeField] private TMP_InputField m_newRewardNameInput;
        [SerializeField] private TMP_Dropdown m_newRewardIconDropdown;
        [SerializeField] private Image m_newRewardIconPreview;

        [Header("퀘스트 상세 상단 전환 바")]
        [SerializeField] private TMP_Dropdown m_questSelectDropdown;
        [SerializeField] private Button m_addQuestInDetailButton;
        [SerializeField] private Button m_deleteQuestInDetailButton;
        #endregion

        #region UI 참조 (Inspector) - 제어 바 & 상태 메시지
        [Header("하단 제어 바")]
        [SerializeField] private Button m_saveLocalButton;
        [SerializeField] private Button m_uploadFirebaseButton;
        [SerializeField] private Button m_removeEventButton;
        [SerializeField] private TextMeshProUGUI m_statusText;
        #endregion

        #region 내부 필드
        private EventAdminViewModel m_viewModel;
        private readonly List<GameObject> m_spawnedItems = new List<GameObject>();
        private readonly List<EventAdminQuestRowView> m_spawnedQuestRows = new List<EventAdminQuestRowView>();
        private readonly List<EventAdminRewardRowView> m_spawnedRewardRows = new List<EventAdminRewardRowView>();
        #endregion

        #region 초기화 및 바인딩
        /// <summary>
        /// [기능]: 뷰모델 의존성을 바인딩하고 이벤트를 구독합니다.
        /// [작성자]: 윤승종
        /// </summary>
        public void Bind(EventAdminViewModel viewModel)
        {
            m_viewModel = viewModel;

            if (m_viewModel != null)
            {
                m_viewModel.OnEventListChanged += func_OnEventListChanged;
                m_viewModel.OnEventSelected += func_OnEventSelected;
                m_viewModel.OnQuestListChanged += func_OnQuestListChanged;
                m_viewModel.OnQuestSelected += func_OnQuestSelected;
                m_viewModel.OnErrorOccurred += func_OnErrorOccurred;
                m_viewModel.OnSaveCompleted += func_OnSaveCompleted;
                m_viewModel.OnUploadCompleted += func_OnUploadCompleted;
            }

            if (m_condTypeDropdown != null && m_viewModel != null)
            {
                m_condTypeDropdown.onValueChanged.RemoveAllListeners();
                m_condTypeDropdown.ClearOptions();
                
                var condTypes = m_viewModel.GetAvailableConditionTypes();
                var options = new List<string>();
                for (int i = 0; i < condTypes.Count; i++)
                {
                    options.Add(condTypes[i].DisplayName);
                }
                m_condTypeDropdown.AddOptions(options);
            }

            if (m_newRewardTypeDropdown != null && m_viewModel != null)
            {
                m_newRewardTypeDropdown.onValueChanged.RemoveAllListeners();
                m_newRewardTypeDropdown.ClearOptions();
                var rewardTypes = m_viewModel.GetAvailableRewardTypes();
                var options = new List<string>();
                for (int i = 0; i < rewardTypes.Count; i++)
                {
                    if (rewardTypes[i] != null)
                    {
                        options.Add(rewardTypes[i].DisplayName);
                    }
                }
                m_newRewardTypeDropdown.AddOptions(options);
            }

            if (m_iconAddressDropdown != null)
            {
                m_iconAddressDropdown.onValueChanged.RemoveAllListeners();
                m_iconAddressDropdown.ClearOptions();
                m_iconAddressDropdown.AddOptions(GetIconDropdownOptions());
            }

            if (m_newRewardIconDropdown != null)
            {
                m_newRewardIconDropdown.onValueChanged.RemoveAllListeners();
                m_newRewardIconDropdown.ClearOptions();
                m_newRewardIconDropdown.AddOptions(GetIconDropdownOptions());
                m_newRewardIconDropdown.onValueChanged.AddListener(func_OnNewRewardIconDropdownChangedWrapper);
                func_UpdateNewRewardThumbnailWrapper(m_newRewardIconDropdown.value);
            }

            if (m_addEventButton != null)
            {
                m_addEventButton.onClick.RemoveAllListeners();
                m_addEventButton.onClick.AddListener(func_OnAddEventClick);
            }

            if (m_removeEventButton != null)
            {
                m_removeEventButton.onClick.RemoveAllListeners();
                m_removeEventButton.onClick.AddListener(func_OnRemoveEventClick);
            }

            if (m_addQuestButton != null)
            {
                m_addQuestButton.onClick.RemoveAllListeners();
                m_addQuestButton.onClick.AddListener(func_OnAddQuestClick);
            }

            if (m_addQuestInDetailButton != null)
            {
                m_addQuestInDetailButton.onClick.RemoveAllListeners();
                m_addQuestInDetailButton.onClick.AddListener(func_OnAddQuestClick);
            }

            if (m_deleteQuestInDetailButton != null)
            {
                m_deleteQuestInDetailButton.onClick.RemoveAllListeners();
                m_deleteQuestInDetailButton.onClick.AddListener(func_OnDeleteQuestInDetailClick);
            }

            if (m_questSelectDropdown != null)
            {
                m_questSelectDropdown.onValueChanged.RemoveAllListeners();
                m_questSelectDropdown.onValueChanged.AddListener(func_OnQuestSelectDropdownChanged);
            }

            if (m_backToEventButton != null)
            {
                m_backToEventButton.onClick.RemoveAllListeners();
                m_backToEventButton.onClick.AddListener(func_OnBackToEventClick);
            }

            if (m_addRewardButton != null)
            {
                m_addRewardButton.onClick.RemoveAllListeners();
                m_addRewardButton.onClick.AddListener(func_OnAddRewardClick);
            }

            if (m_saveLocalButton != null)
            {
                m_saveLocalButton.onClick.RemoveAllListeners();
                m_saveLocalButton.onClick.AddListener(func_OnSaveLocalClick);
            }

            if (m_uploadFirebaseButton != null)
            {
                m_uploadFirebaseButton.onClick.RemoveAllListeners();
                m_uploadFirebaseButton.onClick.AddListener(func_OnUploadFirebaseClick);
            }

            func_RegisterEventFormListeners();
            func_RegisterQuestFormListeners();

            func_OnEventListChanged();
            func_OnEventSelected(string.Empty);
        }

        private void OnDestroy()
        {
            if (m_viewModel != null)
            {
                m_viewModel.OnEventListChanged -= func_OnEventListChanged;
                m_viewModel.OnEventSelected -= func_OnEventSelected;
                m_viewModel.OnQuestListChanged -= func_OnQuestListChanged;
                m_viewModel.OnQuestSelected -= func_OnQuestSelected;
                m_viewModel.OnErrorOccurred -= func_OnErrorOccurred;
                m_viewModel.OnSaveCompleted -= func_OnSaveCompleted;
                m_viewModel.OnUploadCompleted -= func_OnUploadCompleted;
            }

            if (m_questSelectDropdown != null)
            {
                m_questSelectDropdown.onValueChanged.RemoveAllListeners();
            }
        }
        #endregion

        #region 이벤트 구독 반응 UI 갱신 (Event Mode)
        private void func_OnEventListChanged()
        {
            for (int i = 0; i < m_spawnedItems.Count; i++)
            {
                if (m_spawnedItems[i] != null)
                {
                    Destroy(m_spawnedItems[i]);
                }
            }
            m_spawnedItems.Clear();

            if (m_viewModel == null || m_eventListContent == null || m_eventItemPrefab == null)
            {
                return;
            }

            var events = m_viewModel.GetEvents();
            for (int i = 0; i < events.Count; i++)
            {
                var ev = events[i];
                EventItemCell cell = Instantiate(m_eventItemPrefab, m_eventListContent);
                if (cell != null)
                {
                    m_spawnedItems.Add(cell.gameObject);
                    cell.Setup(ev, func_OnSelectItem);
                }
            }
        }

        private void func_OnSelectItem(string eventId)
        {
            if (m_viewModel != null)
            {
                m_viewModel.SelectEvent(eventId);
            }
        }

        private void func_OnEventSelected(string eventId)
        {
            if (m_viewModel == null)
            {
                return;
            }

            var selected = m_viewModel.GetSelectedEvent();
            if (selected == null)
            {
                if (m_eventDetailPanel != null) m_eventDetailPanel.SetActive(false);
                if (m_questDetailPanel != null) m_questDetailPanel.SetActive(false);
                return;
            }

            if (m_eventDetailPanel != null) m_eventDetailPanel.SetActive(true);
            if (m_questDetailPanel != null) m_questDetailPanel.SetActive(false);

            func_UnregisterEventFormListeners();

            if (m_eventIdInput != null) m_eventIdInput.text = selected.eventId;
            if (m_titleInput != null) m_titleInput.text = selected.eventTitle;
            if (m_descInput != null) m_descInput.text = selected.eventDescription;
            if (m_iconAddressDropdown != null)
            {
                int iconIdx = ParseIconIndex(selected.eventIconAddress);
                m_iconAddressDropdown.value = iconIdx;
                func_UpdateIconAddressThumbnailWrapper(iconIdx);
            }
            if (m_startDateInput != null) m_startDateInput.text = selected.startDate;
            if (m_endDateInput != null) m_endDateInput.text = selected.endDate;

            func_RegisterEventFormListeners();
            func_RenderQuests(selected.quests);
        }

        private void func_RenderQuests(List<QuestDefinitionDTO> quests)
        {
            if (m_questListContent != null)
            {
                for (int i = m_questListContent.childCount - 1; i >= 0; i--)
                {
                    Destroy(m_questListContent.GetChild(i).gameObject);
                }
            }
            m_spawnedQuestRows.Clear();

            if (m_questListContent == null || m_questRowPrefab == null)
            {
                return;
            }

            for (int i = 0; i < quests.Count; i++)
            {
                var q = quests[i];
                EventAdminQuestRowView rowView = Instantiate(m_questRowPrefab, m_questListContent);
                if (rowView != null)
                {
                    rowView.Bind(q, func_OnEditQuestRow, func_OnRemoveQuestRow);
                    m_spawnedQuestRows.Add(rowView);
                }
            }
        }

        private void func_OnEditQuestRow(string questId)
        {
            if (m_viewModel != null)
            {
                m_viewModel.SelectQuest(questId);
            }
        }

        private void func_OnRemoveQuestRow(string questId)
        {
            if (m_viewModel != null)
            {
                m_viewModel.RemoveQuest(questId);
            }
        }
        #endregion

        #region 이벤트 구독 반응 UI 갱신 (Quest Mode)
        private void func_OnQuestListChanged()
        {
            var selectedEvent = m_viewModel?.GetSelectedEvent();
            if (selectedEvent != null)
            {
                func_RenderQuests(selectedEvent.quests);
            }
        }

        private void func_OnQuestSelected(string questId)
        {
            if (m_viewModel == null)
            {
                return;
            }

            var selectedQuest = m_viewModel.GetSelectedQuest();
            if (selectedQuest == null)
            {
                if (m_questDetailPanel != null) m_questDetailPanel.SetActive(false);
                if (m_eventDetailPanel != null) m_eventDetailPanel.SetActive(true);
                return;
            }

            if (m_eventDetailPanel != null) m_eventDetailPanel.SetActive(false);
            if (m_questDetailPanel != null) m_questDetailPanel.SetActive(true);

            // 퀘스트 선택 드롭다운 갱신
            if (m_questSelectDropdown != null)
            {
                m_questSelectDropdown.onValueChanged.RemoveAllListeners();
                m_questSelectDropdown.ClearOptions();

                var selectedEvent = m_viewModel.GetSelectedEvent();
                if (selectedEvent != null && selectedEvent.quests != null)
                {
                    var options = new List<TMP_Dropdown.OptionData>();
                    int selectedIdx = 0;
                    for (int i = 0; i < selectedEvent.quests.Count; i++)
                    {
                        var q = selectedEvent.quests[i];
                        string display = $"{q.questId} : {q.questTitle}";
                        options.Add(new TMP_Dropdown.OptionData(display));

                        if (q.questId == selectedQuest.questId)
                        {
                            selectedIdx = i;
                        }
                    }
                    m_questSelectDropdown.AddOptions(options);
                    m_questSelectDropdown.value = selectedIdx;
                }
                m_questSelectDropdown.onValueChanged.AddListener(func_OnQuestSelectDropdownChanged);
            }

            func_UnregisterQuestFormListeners();

            if (m_questIdInput != null) m_questIdInput.text = selectedQuest.questId;
            if (m_questTitleInput != null) m_questTitleInput.text = selectedQuest.questTitle;
            if (m_questDescInput != null) m_questDescInput.text = selectedQuest.questDescription;

            if (m_condTypeDropdown != null && m_viewModel != null)
            {
                int condIdx = 0;
                string searchTypeName = selectedQuest.condition.conditionType;
                var condTypes = m_viewModel.GetAvailableConditionTypes();
                
                string searchDisplayName = string.Empty;
                for (int i = 0; i < condTypes.Count; i++)
                {
                    if (condTypes[i].TypeName.Equals(searchTypeName, StringComparison.OrdinalIgnoreCase))
                    {
                        searchDisplayName = condTypes[i].DisplayName;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(searchDisplayName))
                {
                    for (int i = 0; i < m_condTypeDropdown.options.Count; i++)
                    {
                        if (m_condTypeDropdown.options[i].text.Equals(searchDisplayName, StringComparison.OrdinalIgnoreCase))
                        {
                            condIdx = i;
                            break;
                        }
                    }
                }
                m_condTypeDropdown.value = condIdx;
            }

            if (m_condTargetInput != null)
            {
                m_condTargetInput.text = selectedQuest.condition.targetValue.ToString();
            }

            func_RegisterQuestFormListeners();
            func_RenderRewards(selectedQuest.rewards);
        }

        private void func_RenderRewards(List<RewardDefinitionDTO> rewards)
        {
            if (m_rewardListContent != null)
            {
                for (int i = m_rewardListContent.childCount - 1; i >= 0; i--)
                {
                    Destroy(m_rewardListContent.GetChild(i).gameObject);
                }
            }
            m_spawnedRewardRows.Clear();

            if (m_rewardListContent == null || m_rewardRowPrefab == null)
            {
                return;
            }

            for (int i = 0; i < rewards.Count; i++)
            {
                var rew = rewards[i];
                EventAdminRewardRowView rowView = Instantiate(m_rewardRowPrefab, m_rewardListContent);
                if (rowView != null)
                {
                    rowView.Bind(rew, func_OnRemoveRewardRow, m_viewModel.GetAvailableRewardTypes());
                    m_spawnedRewardRows.Add(rowView);
                }
            }
        }

        private void func_OnRemoveRewardRow(EventAdminRewardRowView row)
        {
            if (m_viewModel == null || row == null)
            {
                return;
            }
            var selectedQuest = m_viewModel.GetSelectedQuest();
            if (selectedQuest != null)
            {
                selectedQuest.rewards.Remove(row.GetDTO());
                func_RenderRewards(selectedQuest.rewards);
            }
        }
        #endregion

        #region 상태 메시지
        private void func_OnErrorOccurred(string error)
        {
            if (m_statusText != null)
            {
                m_statusText.color = Color.red;
                m_statusText.text = error;
            }
        }

        private void func_OnSaveCompleted(bool success)
        {
            if (m_statusText != null)
            {
                m_statusText.color = success ? Color.green : Color.red;
                m_statusText.text = success ? "[EventAdminView] 로컬 저장에 성공하였습니다." : "[EventAdminView] 로컬 저장에 실패하였습니다.";
            }
        }

        private void func_OnUploadCompleted(bool success)
        {
            if (m_statusText != null)
            {
                m_statusText.color = success ? Color.green : Color.red;
                m_statusText.text = success ? "[EventAdminView] Firebase 서버 업로드에 성공하였습니다." : "[EventAdminView] Firebase 서버 업로드에 실패하였습니다.";
            }
        }
        #endregion

        #region 사용자 입력 버튼 콜백
        private void func_OnAddEventClick()
        {
            if (m_viewModel != null)
            {
                m_viewModel.AddNewEvent();
            }
        }

        private void func_OnRemoveEventClick()
        {
            if (m_viewModel != null)
            {
                var selected = m_viewModel.GetSelectedEvent();
                if (selected != null)
                {
                    m_viewModel.RemoveEvent(selected.eventId);
                }
            }
        }

        private void func_OnAddQuestClick()
        {
            if (m_viewModel != null)
            {
                m_viewModel.AddNewQuest();
            }
        }

        private void func_OnQuestSelectDropdownChanged(int index)
        {
            if (m_viewModel == null || m_questSelectDropdown == null)
            {
                return;
            }

            var selectedEvent = m_viewModel.GetSelectedEvent();
            if (selectedEvent != null && index >= 0 && index < selectedEvent.quests.Count)
            {
                func_SubmitQuestFormChanges();
                string targetQuestId = selectedEvent.quests[index].questId;
                m_viewModel.SelectQuest(targetQuestId);
            }
        }

        private void func_OnDeleteQuestInDetailClick()
        {
            if (m_viewModel == null)
            {
                return;
            }

            var selectedQuest = m_viewModel.GetSelectedQuest();
            var selectedEvent = m_viewModel.GetSelectedEvent();
            if (selectedQuest != null && selectedEvent != null)
            {
                string targetQuestId = selectedQuest.questId;
                m_viewModel.RemoveQuest(targetQuestId);

                if (selectedEvent.quests.Count > 0)
                {
                    m_viewModel.SelectQuest(selectedEvent.quests[0].questId);
                }
                else
                {
                    func_OnBackToEventClick();
                }
            }
        }

        private void func_OnBackToEventClick()
        {
            if (m_viewModel != null)
            {
                var selectedEvent = m_viewModel.GetSelectedEvent();
                if (selectedEvent != null)
                {
                    m_viewModel.SelectEvent(selectedEvent.eventId);
                }
            }
        }

        private void func_OnAddRewardClick()
        {
            if (m_viewModel == null) return;
            
            var selectedQuest = m_viewModel.GetSelectedQuest();
            if (selectedQuest != null)
            {
                string rawType = "Exp";
                if (m_newRewardTypeDropdown != null)
                {
                    var rewardTypes = m_viewModel.GetAvailableRewardTypes();
                    if (m_newRewardTypeDropdown.value >= 0 && m_newRewardTypeDropdown.value < rewardTypes.Count)
                    {
                        if (rewardTypes[m_newRewardTypeDropdown.value] != null)
                        {
                            rawType = rewardTypes[m_newRewardTypeDropdown.value].TypeName;
                        }
                    }
                }

                int amount = 10;
                if (m_newRewardAmountInput != null)
                {
                    int.TryParse(m_newRewardAmountInput.text, out amount);
                }

                string displayNameField = m_newRewardNameInput != null ? m_newRewardNameInput.text : "경험치";
                if (string.IsNullOrEmpty(displayNameField))
                {
                    displayNameField = m_newRewardTypeDropdown != null ? m_newRewardTypeDropdown.options[m_newRewardTypeDropdown.value].text : "보상";
                }
                
                string iconAddress = "item_Sheet[item_Sheet_0]";
                if (m_newRewardIconDropdown != null)
                {
                    iconAddress = m_newRewardIconDropdown.options[m_newRewardIconDropdown.value].text;
                }

                var newRew = new RewardDefinitionDTO
                {
                    rewardType = rawType,
                    amount = amount,
                    displayName = displayNameField,
                    iconAddress = iconAddress
                };

                selectedQuest.rewards.Add(newRew);
                func_RenderRewards(selectedQuest.rewards);

                if (m_newRewardAmountInput != null) m_newRewardAmountInput.text = "10";
                if (m_newRewardNameInput != null) m_newRewardNameInput.text = string.Empty;
                if (m_newRewardIconDropdown != null)
                {
                    m_newRewardIconDropdown.value = 0;
                    func_UpdateNewRewardThumbnailWrapper(0);
                }
            }
        }

        private async void func_OnSaveLocalClick()
        {
            if (m_viewModel != null)
            {
                func_SubmitEventFormChanges();
                func_SubmitQuestFormChanges();
                if (m_statusText != null) m_statusText.text = "저장 중...";
                await m_viewModel.SaveToLocalFileAsync();
            }
        }

        private async void func_OnUploadFirebaseClick()
        {
            if (m_viewModel != null)
            {
                func_SubmitEventFormChanges();
                func_SubmitQuestFormChanges();

                // 1단계: 로컬 저장 선행
                if (m_statusText != null)
                {
                    m_statusText.color = Color.white;
                    m_statusText.text = "로컬 저장 중...";
                }

                bool saveSuccess = await m_viewModel.SaveToLocalFileAsync();
                if (saveSuccess == false)
                {
                    if (m_statusText != null)
                    {
                        m_statusText.color = Color.red;
                        m_statusText.text = "[EventAdminView] 로컬 저장 실패로 인해 서버 배포가 취소되었습니다.";
                    }
                    return;
                }

                // 2단계: 로컬 저장 성공 시 서버 업로드 개시
                if (m_statusText != null)
                {
                    m_statusText.color = Color.white;
                    m_statusText.text = "서버 배포 중...";
                }

                await m_viewModel.UploadToFirebaseAsync();
            }
        }
        #endregion

        #region 폼 입력 실시간 동기화
        private void func_RegisterEventFormListeners()
        {
            if (m_eventIdInput != null) m_eventIdInput.onEndEdit.AddListener(func_OnEventFormInputEndEdit);
            if (m_titleInput != null) m_titleInput.onEndEdit.AddListener(func_OnEventFormInputEndEdit);
            if (m_descInput != null) m_descInput.onEndEdit.AddListener(func_OnEventFormInputEndEdit);
            if (m_iconAddressDropdown != null) m_iconAddressDropdown.onValueChanged.AddListener(func_OnIconAddressDropdownChangedWrapper);
            if (m_startDateInput != null) m_startDateInput.onEndEdit.AddListener(func_OnEventFormInputEndEdit);
            if (m_endDateInput != null) m_endDateInput.onEndEdit.AddListener(func_OnEventFormInputEndEdit);
        }

        private void func_UnregisterEventFormListeners()
        {
            if (m_eventIdInput != null) m_eventIdInput.onEndEdit.RemoveAllListeners();
            if (m_titleInput != null) m_titleInput.onEndEdit.RemoveAllListeners();
            if (m_descInput != null) m_descInput.onEndEdit.RemoveAllListeners();
            if (m_iconAddressDropdown != null) m_iconAddressDropdown.onValueChanged.RemoveAllListeners();
            if (m_startDateInput != null) m_startDateInput.onEndEdit.RemoveAllListeners();
            if (m_endDateInput != null) m_endDateInput.onEndEdit.RemoveAllListeners();
        }

        private void func_RegisterQuestFormListeners()
        {
            if (m_questIdInput != null) m_questIdInput.onEndEdit.AddListener(func_OnQuestFormInputEndEdit);
            if (m_questTitleInput != null) m_questTitleInput.onEndEdit.AddListener(func_OnQuestFormInputEndEdit);
            if (m_questDescInput != null) m_questDescInput.onEndEdit.AddListener(func_OnQuestFormInputEndEdit);
            if (m_condTypeDropdown != null) m_condTypeDropdown.onValueChanged.AddListener(func_OnQuestDropdownValueChanged);
            if (m_condTargetInput != null) m_condTargetInput.onEndEdit.AddListener(func_OnQuestFormInputEndEdit);
        }

        private void func_UnregisterQuestFormListeners()
        {
            if (m_questIdInput != null) m_questIdInput.onEndEdit.RemoveAllListeners();
            if (m_questTitleInput != null) m_questTitleInput.onEndEdit.RemoveAllListeners();
            if (m_questDescInput != null) m_questDescInput.onEndEdit.RemoveAllListeners();
            if (m_condTypeDropdown != null) m_condTypeDropdown.onValueChanged.RemoveAllListeners();
            if (m_condTargetInput != null) m_condTargetInput.onEndEdit.RemoveAllListeners();
        }

        private void func_OnEventFormInputEndEdit(string val)
        {
            func_SubmitEventFormChanges();
        }

        private void func_OnQuestFormInputEndEdit(string val)
        {
            func_SubmitQuestFormChanges();
        }

        private void func_OnQuestDropdownValueChanged(int index)
        {
            func_SubmitQuestFormChanges();
        }

        private void func_SubmitEventFormChanges()
        {
            if (m_viewModel == null) return;
            var selected = m_viewModel.GetSelectedEvent();
            if (selected == null) return;

            var updated = new EventDefinitionDTO
            {
                eventId = m_eventIdInput != null ? m_eventIdInput.text : selected.eventId,
                eventTitle = m_titleInput != null ? m_titleInput.text : selected.eventTitle,
                eventDescription = m_descInput != null ? m_descInput.text : selected.eventDescription,
                eventIconAddress = m_iconAddressDropdown != null 
                    ? m_iconAddressDropdown.options[m_iconAddressDropdown.value].text 
                    : selected.eventIconAddress,
                startDate = m_startDateInput != null ? m_startDateInput.text : selected.startDate,
                endDate = m_endDateInput != null ? m_endDateInput.text : selected.endDate,
                quests = selected.quests
            };

            m_viewModel.UpdateSelectedEvent(updated);
        }

        private void func_SubmitQuestFormChanges()
        {
            if (m_viewModel == null) return;
            var selectedQuest = m_viewModel.GetSelectedQuest();
            if (selectedQuest == null) return;

            var updated = new QuestDefinitionDTO
            {
                questId = m_questIdInput != null ? m_questIdInput.text : selectedQuest.questId,
                questTitle = m_questTitleInput != null ? m_questTitleInput.text : selectedQuest.questTitle,
                questDescription = m_questDescInput != null ? m_questDescInput.text : selectedQuest.questDescription,
                condition = new ConditionDefinitionDTO
                {
                    conditionType = (m_condTypeDropdown != null && m_viewModel != null)
                        ? (m_condTypeDropdown.value >= 0 && m_condTypeDropdown.value < m_viewModel.GetAvailableConditionTypes().Count 
                            ? m_viewModel.GetAvailableConditionTypes()[m_condTypeDropdown.value].TypeName 
                            : selectedQuest.condition.conditionType)
                        : selectedQuest.condition.conditionType,
                    targetValue = m_condTargetInput != null ? (int.TryParse(m_condTargetInput.text, out int target) ? target : 0) : selectedQuest.condition.targetValue
                },
                rewards = selectedQuest.rewards
            };

            m_viewModel.UpdateSelectedQuest(updated);
        }
        #endregion

        #region 헬퍼 메서드 및 썸네일 제어
        private List<TMP_Dropdown.OptionData> GetIconDropdownOptions()
        {
            var options = new List<TMP_Dropdown.OptionData>();
            for (int i = 0; i <= 20; i++)
            {
                options.Add(new TMP_Dropdown.OptionData($"item_Sheet[item_Sheet_{i}]"));
            }
            return options;
        }

        private int ParseIconIndex(string address)
        {
            if (string.IsNullOrEmpty(address)) return 0;
            try
            {
                int openBracket = address.IndexOf('[');
                int closeBracket = address.IndexOf(']');
                if (openBracket != -1 && closeBracket != -1 && closeBracket > openBracket)
                {
                    string subAsset = address.Substring(openBracket + 1, closeBracket - openBracket - 1);
                    string indexStr = subAsset.Replace("item_Sheet_", "");
                    if (int.TryParse(indexStr, out int idx)) return idx;
                }
                else
                {
                    string indexStr = address.Replace("item_Sheet_", "");
                    if (int.TryParse(indexStr, out int idx)) return idx;
                }
            }
            catch { /* Ignore */ }
            return 0;
        }

        private async Awaitable UpdateThumbnailAsync(int index, Image targetImage)
        {
            if (targetImage != null)
            {
                Sprite sprite = await ItemSpriteMapper.GetItemSpriteAsync(index);
                if (targetImage != null)
                {
                    targetImage.sprite = sprite;
                }
            }
        }

        private async void func_OnIconAddressDropdownChangedWrapper(int index)
        {
            await UpdateThumbnailAsync(index, m_iconAddressPreview);
            func_SubmitEventFormChanges();
        }

        private async void func_OnNewRewardIconDropdownChangedWrapper(int index)
        {
            await UpdateThumbnailAsync(index, m_newRewardIconPreview);
        }

        private async void func_UpdateNewRewardThumbnailWrapper(int index)
        {
            await UpdateThumbnailAsync(index, m_newRewardIconPreview);
        }

        private async void func_UpdateIconAddressThumbnailWrapper(int index)
        {
            await UpdateThumbnailAsync(index, m_iconAddressPreview);
        }
        #endregion
    }
}
