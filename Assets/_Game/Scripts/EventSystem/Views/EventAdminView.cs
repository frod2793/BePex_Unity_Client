/// <summary>
/// [기능]: 이벤트 관리자 UI 씬의 모든 입력/갱신 시각 인터렉션을 제어하고 ViewModel을 중개하는 View 클래스.
/// [작성자]: 윤승종
/// </summary>

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using BePex.EventSystem.ViewModels;
using BePex.EventSystem.DTOs;
using BePex.EventSystem.Data;
using BePex.EventSystem.Utils;
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
        // 생성된 이벤트 항목 셀 프리팹들이 배치될 스크롤 뷰의 콘텐츠 영역
        [SerializeField] private RectTransform m_eventListContent;
        // 목록에 노출될 개별 이벤트 항목 셀 프리팹
        [SerializeField] private EventItemCell m_eventItemPrefab;
        // 신규 이벤트를 생성하는 버튼
        [SerializeField] private Button m_addEventButton;
        #endregion

        #region UI 참조 (Inspector) - 우측 패널
        [Header("우측 패널 (상세 편집)")]
        // 선택된 이벤트의 유무에 따라 활성/비활성화되는 상세 편집 패널 컨테이너
        [SerializeField] private GameObject m_detailPanel;
        // 이벤트 식별용 고유 ID 입력 필드
        [SerializeField] private TMP_InputField m_eventIdInput;
        // 이벤트 제목 입력 필드
        [SerializeField] private TMP_InputField m_titleInput;
        // 이벤트 상세 설명 입력 필드
        [SerializeField] private TMP_InputField m_descInput;
        // 이벤트 아이콘 스프라이트의 어드레서블 주소 입력 필드
        [SerializeField] private TMP_InputField m_iconAddressInput;
        // 이벤트 시작 일시 입력 필드 (yyyy-MM-dd)
        [SerializeField] private TMP_InputField m_startDateInput;
        // 이벤트 종료 일시 입력 필드 (yyyy-MM-dd)
        [SerializeField] private TMP_InputField m_endDateInput;

        [Header("조건 편집")]
        // 이벤트 달성 조건의 종류를 선택하는 드롭다운 (KillCount, StageClear, Attendance)
        [SerializeField] private TMP_Dropdown m_condTypeDropdown;
        // 조건 달성에 필요한 목표 수치 입력 필드
        [SerializeField] private TMP_InputField m_condTargetInput;

        [Header("보상 편집")]
        // 동적으로 생성될 보상 데이터 입력 행 프리팹들이 배치될 스크롤 뷰의 콘텐츠 영역
        [SerializeField] private RectTransform m_rewardListContent;
        // 보상 개별 항목 데이터 입력을 위한 UI 행 프리팹
        [SerializeField] private GameObject m_rewardRowPrefab;
        // 해당 이벤트에 신규 지급 보상을 한 줄 추가하는 버튼
        [SerializeField] private Button m_addRewardButton;

        [Header("신규 보상 추가 입력 영역")]
        [SerializeField] private TMP_Dropdown m_newRewardTypeDropdown;
        [SerializeField] private TMP_InputField m_newRewardAmountInput;
        [SerializeField] private TMP_InputField m_newRewardNameInput;
        [SerializeField] private TMP_InputField m_newRewardIconInput;
        #endregion

        #region UI 참조 (Inspector) - 제어 바 & 상태 메시지
        [Header("하단 제어 바")]
        // 로컬 event_table.json 파일에 작성된 데이터를 최종 덤프 저장하는 버튼
        [SerializeField] private Button m_saveLocalButton;
        // 기획 수치 유효성 검사 통과 시 Firebase 서버에 비동기 가상 배포하는 버튼
        [SerializeField] private Button m_uploadFirebaseButton;
        // 상세 편집 중인 타겟 이벤트를 목록 및 데이터에서 삭제하는 버튼
        [SerializeField] private Button m_removeEventButton;
        // 로드, 저장, 배포 작업 결과 및 에러 경고를 표시하는 상태 텍스트
        [SerializeField] private TextMeshProUGUI m_statusText;
        #endregion

        #region 내부 필드
        private EventAdminViewModel m_viewModel;
        private readonly List<GameObject> m_spawnedItems = new List<GameObject>();
        private readonly List<EventAdminRewardRowView> m_spawnedRewardRows = new List<EventAdminRewardRowView>();
        #endregion

        #region 초기화 및 바인딩
        /// <summary>
        /// [기능]: 뷰모델 의존성을 바인딩하고 이벤트를 구독합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        public void Bind(EventAdminViewModel viewModel)
        {
            m_viewModel = viewModel;

            if (m_viewModel != null)
            {
                m_viewModel.OnEventListChanged += func_OnEventListChanged;
                m_viewModel.OnEventSelected += func_OnEventSelected;
                m_viewModel.OnErrorOccurred += func_OnErrorOccurred;
                m_viewModel.OnSaveCompleted += func_OnSaveCompleted;
                m_viewModel.OnUploadCompleted += func_OnUploadCompleted;
            }

            if (m_condTypeDropdown != null)
            {
                m_condTypeDropdown.onValueChanged.RemoveAllListeners();
                m_condTypeDropdown.ClearOptions();
                var options = EnumDisplayHelper.GetDisplayNames<ConditionDefinitionSO.ConditionType>();
                m_condTypeDropdown.AddOptions(options);
            }

            if (m_newRewardTypeDropdown != null)
            {
                m_newRewardTypeDropdown.onValueChanged.RemoveAllListeners();
                m_newRewardTypeDropdown.ClearOptions();
                var options = EnumDisplayHelper.GetDisplayNames<RewardDefinitionSO.RewardType>();
                m_newRewardTypeDropdown.AddOptions(options);
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

            func_RegisterFormListeners();
            func_OnEventListChanged();
            func_OnEventSelected(string.Empty);
        }
        #endregion

        #region 유니티 생명주기
        /// <summary>
        /// [기능]: 뷰 디바인딩 해제 및 뷰모델 이벤트 연결을 취소하여 메모리 누수를 방지합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        private void OnDestroy()
        {
            if (m_viewModel != null)
            {
                m_viewModel.OnEventListChanged -= func_OnEventListChanged;
                m_viewModel.OnEventSelected -= func_OnEventSelected;
                m_viewModel.OnErrorOccurred -= func_OnErrorOccurred;
                m_viewModel.OnSaveCompleted -= func_OnSaveCompleted;
                m_viewModel.OnUploadCompleted -= func_OnUploadCompleted;
            }
        }
        #endregion

        #region 이벤트 구독 반응 UI 갱신
        /// <summary>
        /// [기능]: 뷰모델 내의 전체 이벤트 리스트가 변경되었을 때 좌측 Scroll View 항목을 갱신 렌더링합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
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

        /// <summary>
        /// [기능]: 목록 아이템 선택 클릭을 뷰모델에 전달합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        private void func_OnSelectItem(string eventId)
        {
            if (m_viewModel != null)
            {
                m_viewModel.SelectEvent(eventId);
            }
        }

        /// <summary>
        /// [기능]: 뷰모델에서 특정 이벤트의 활성화 변경 신호를 보냈을 때 우측 상세 폼 데이터를 채우고 활성화합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        private void func_OnEventSelected(string eventId)
        {
            if (m_viewModel == null)
            {
                return;
            }

            var selected = m_viewModel.GetSelectedEvent();
            if (selected == null)
            {
                if (m_detailPanel != null)
                {
                    m_detailPanel.SetActive(false);
                }
                return;
            }

            if (m_detailPanel != null)
            {
                m_detailPanel.SetActive(true);
            }

            func_UnregisterFormListeners();

            if (m_eventIdInput != null)
            {
                m_eventIdInput.text = selected.eventId;
            }
            if (m_titleInput != null)
            {
                m_titleInput.text = selected.eventTitle;
            }
            if (m_descInput != null)
            {
                m_descInput.text = selected.eventDescription;
            }
            if (m_iconAddressInput != null)
            {
                m_iconAddressInput.text = selected.eventIconAddress;
            }
            if (m_startDateInput != null)
            {
                m_startDateInput.text = selected.startDate;
            }
            if (m_endDateInput != null)
            {
                m_endDateInput.text = selected.endDate;
            }

            if (m_condTypeDropdown != null)
            {
                int condIdx = 0;
                string searchName = selected.condition.conditionType;
                if (Enum.TryParse<ConditionDefinitionSO.ConditionType>(selected.condition.conditionType, true, out var typeEnum))
                {
                    searchName = EnumDisplayHelper.GetDisplayName(typeEnum);
                }

                for (int i = 0; i < m_condTypeDropdown.options.Count; i++)
                {
                    if (m_condTypeDropdown.options[i].text.Equals(searchName, StringComparison.OrdinalIgnoreCase))
                    {
                        condIdx = i;
                        break;
                    }
                }
                m_condTypeDropdown.value = condIdx;
            }

            if (m_condTargetInput != null)
            {
                m_condTargetInput.text = selected.condition.targetValue.ToString();
            }

            func_RegisterFormListeners();
            func_RenderRewards(selected.rewards);
        }

        /// <summary>
        /// [기능]: 보상 목록을 뷰 하이어라키 내에 동적으로 인스턴싱하고 바인딩합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        private void func_RenderRewards(List<RewardDefinitionDTO> rewards)
        {
            // 기존 스폰된 모든 자식 오브젝트 소거 (메모리 누수 및 오정렬 방지)
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
                GameObject go = Instantiate(m_rewardRowPrefab, m_rewardListContent);
                var rowView = go.GetComponent<EventAdminRewardRowView>();
                if (rowView != null)
                {
                    rowView.Bind(rew, func_OnRemoveRewardRow);
                    m_spawnedRewardRows.Add(rowView);
                }
            }
        }

        /// <summary>
        /// [기능]: 개별 보상 줄에서 삭제 요청이 넘어왔을 때 이를 소거하고 보상 리스트를 재생성합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        private void func_OnRemoveRewardRow(EventAdminRewardRowView row)
        {
            if (m_viewModel == null || row == null)
            {
                return;
            }
            var selected = m_viewModel.GetSelectedEvent();
            if (selected != null)
            {
                selected.rewards.Remove(row.GetDTO());
                func_RenderRewards(selected.rewards);
            }
        }

        /// <summary>
        /// [기능]: 경고 발생 시 텍스트 컴포넌트의 컬러를 빨간색으로 변경하고 경고 내용을 갱신합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        private void func_OnErrorOccurred(string error)
        {
            if (m_statusText != null)
            {
                m_statusText.color = Color.red;
                m_statusText.text = error;
            }
        }

        /// <summary>
        /// [기능]: 로컬 세이브 파일 출력 완료 결과를 하단 상태 창에 출력합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        private void func_OnSaveCompleted(bool success)
        {
            if (m_statusText != null)
            {
                m_statusText.color = success ? Color.green : Color.red;
                m_statusText.text = success ? "[EventAdminView] 로컬 저장에 성공하였습니다." : "[EventAdminView] 로컬 저장에 실패하였습니다.";
            }
        }

        /// <summary>
        /// [기능]: Firebase 비동기 업로드 완료 결과를 하단 상태 창에 출력합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
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
        /// <summary>
        /// [기능]: 신규 이벤트 생성 버튼 클릭 핸들러.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        private void func_OnAddEventClick()
        {
            if (m_viewModel != null)
            {
                m_viewModel.AddNewEvent();
            }
        }

        /// <summary>
        /// [기능]: 이벤트 삭제 버튼 클릭 핸들러.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
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

        /// <summary>
        /// [기능]: 사용자가 지정한 보상 종류, 수량, 노출명, 아이콘 주소를 입력받아 DTO를 생성하고 목록에 주입합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 기존 하드코딩 방식에서 UI 입력 수집 동적 빌드 방식으로 개편
        /// </summary>
        private void func_OnAddRewardClick()
        {
            if (m_viewModel == null)
            {
                return;
            }
            var selected = m_viewModel.GetSelectedEvent();
            if (selected != null)
            {
                // 1) 드롭다운 한글 표시명을 DTO 저장용 영문 식별자로 변환
                string rawType = "Exp";
                if (m_newRewardTypeDropdown != null)
                {
                    string displayName = m_newRewardTypeDropdown.options[m_newRewardTypeDropdown.value].text;
                    rawType = EnumDisplayHelper.GetEnumValue<RewardDefinitionSO.RewardType>(displayName).ToString();
                }

                // 2) 보상 수량 파싱
                int amount = 10;
                if (m_newRewardAmountInput != null)
                {
                    int.TryParse(m_newRewardAmountInput.text, out amount);
                }

                // 3) 보상 노출명 및 어드레서블 아이콘 경로 획득
                string displayNameField = m_newRewardNameInput != null ? m_newRewardNameInput.text : "경험치";
                if (string.IsNullOrEmpty(displayNameField))
                {
                    displayNameField = m_newRewardTypeDropdown != null ? m_newRewardTypeDropdown.options[m_newRewardTypeDropdown.value].text : "보상";
                }
                string iconAddress = m_newRewardIconInput != null ? m_newRewardIconInput.text : "UI/Icons/Default";
                if (string.IsNullOrEmpty(iconAddress))
                {
                    iconAddress = "UI/Icons/Default";
                }

                var newRew = new RewardDefinitionDTO
                {
                    rewardType = rawType,
                    amount = amount,
                    displayName = displayNameField,
                    iconAddress = iconAddress
                };

                selected.rewards.Add(newRew);
                func_RenderRewards(selected.rewards);

                // 4) 입력 UI 텍스트 초기화 (사용자 편의)
                if (m_newRewardAmountInput != null)
                {
                    m_newRewardAmountInput.text = "10";
                }
                if (m_newRewardNameInput != null)
                {
                    m_newRewardNameInput.text = string.Empty;
                }
                if (m_newRewardIconInput != null)
                {
                    m_newRewardIconInput.text = string.Empty;
                }
            }
        }

        /// <summary>
        /// [기능]: 로컬 파일 저장 버튼 클릭 비동기 핸들러.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        private async void func_OnSaveLocalClick()
        {
            if (m_viewModel != null)
            {
                func_SubmitFormChanges();
                if (m_statusText != null)
                {
                    m_statusText.text = "저장 중...";
                }
                await m_viewModel.SaveToLocalFileAsync();
            }
        }

        /// <summary>
        /// [기능]: Firebase 서버 배포 버튼 클릭 비동기 핸들러.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        private async void func_OnUploadFirebaseClick()
        {
            if (m_viewModel != null)
            {
                func_SubmitFormChanges();
                if (m_statusText != null)
                {
                    m_statusText.text = "업로드 중...";
                }
                await m_viewModel.UploadToFirebaseAsync();
            }
        }
        #endregion

        #region 폼 입력 실시간 동기화
        /// <summary>
        /// [기능]: 우측 폼 입력 컴포넌트들의 데이터 변경 리스너를 일제히 등록합니다. 단일 행 중괄호 규칙 준수.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의 및 중괄호 보완
        /// </summary>
        private void func_RegisterFormListeners()
        {
            if (m_eventIdInput != null)
            {
                m_eventIdInput.onEndEdit.AddListener(func_OnFormInputEndEdit);
            }
            if (m_titleInput != null)
            {
                m_titleInput.onEndEdit.AddListener(func_OnFormInputEndEdit);
            }
            if (m_descInput != null)
            {
                m_descInput.onEndEdit.AddListener(func_OnFormInputEndEdit);
            }
            if (m_iconAddressInput != null)
            {
                m_iconAddressInput.onEndEdit.AddListener(func_OnFormInputEndEdit);
            }
            if (m_startDateInput != null)
            {
                m_startDateInput.onEndEdit.AddListener(func_OnFormInputEndEdit);
            }
            if (m_endDateInput != null)
            {
                m_endDateInput.onEndEdit.AddListener(func_OnFormInputEndEdit);
            }
            if (m_condTypeDropdown != null)
            {
                m_condTypeDropdown.onValueChanged.AddListener(func_OnDropdownValueChanged);
            }
            if (m_condTargetInput != null)
            {
                m_condTargetInput.onEndEdit.AddListener(func_OnFormInputEndEdit);
            }
        }

        /// <summary>
        /// [기능]: 리스너 등록을 해제하여 중복 감지를 차단합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의 및 중괄호 보완
        /// </summary>
        private void func_UnregisterFormListeners()
        {
            if (m_eventIdInput != null)
            {
                m_eventIdInput.onEndEdit.RemoveAllListeners();
            }
            if (m_titleInput != null)
            {
                m_titleInput.onEndEdit.RemoveAllListeners();
            }
            if (m_descInput != null)
            {
                m_descInput.onEndEdit.RemoveAllListeners();
            }
            if (m_iconAddressInput != null)
            {
                m_iconAddressInput.onEndEdit.RemoveAllListeners();
            }
            if (m_startDateInput != null)
            {
                m_startDateInput.onEndEdit.RemoveAllListeners();
            }
            if (m_endDateInput != null)
            {
                m_endDateInput.onEndEdit.RemoveAllListeners();
            }
            if (m_condTypeDropdown != null)
            {
                m_condTypeDropdown.onValueChanged.RemoveAllListeners();
            }
            if (m_condTargetInput != null)
            {
                m_condTargetInput.onEndEdit.RemoveAllListeners();
            }
        }

        /// <summary>
        /// [기능]: 인풋 필드 폼 수정 완료 시 바인딩.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        private void func_OnFormInputEndEdit(string val)
        {
            func_SubmitFormChanges();
        }

        /// <summary>
        /// [기능]: 드롭다운 설정 변경 시 바인딩.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        private void func_OnDropdownValueChanged(int index)
        {
            func_SubmitFormChanges();
        }

        /// <summary>
        /// [기능]: UI 상의 모든 편집 텍스트와 드롭다운 값을 수집하여 뷰모델 갱신 명령을 인가합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        private void func_SubmitFormChanges()
        {
            if (m_viewModel == null)
            {
                return;
            }
            var selected = m_viewModel.GetSelectedEvent();
            if (selected == null)
            {
                return;
            }

            var updated = new EventDefinitionDTO
            {
                eventId = m_eventIdInput != null ? m_eventIdInput.text : selected.eventId,
                eventTitle = m_titleInput != null ? m_titleInput.text : selected.eventTitle,
                eventDescription = m_descInput != null ? m_descInput.text : selected.eventDescription,
                eventIconAddress = m_iconAddressInput != null ? m_iconAddressInput.text : selected.eventIconAddress,
                startDate = m_startDateInput != null ? m_startDateInput.text : selected.startDate,
                endDate = m_endDateInput != null ? m_endDateInput.text : selected.endDate,
                condition = new ConditionDefinitionDTO
                {
                    conditionType = m_condTypeDropdown != null 
                        ? EnumDisplayHelper.GetEnumValue<ConditionDefinitionSO.ConditionType>(m_condTypeDropdown.options[m_condTypeDropdown.value].text).ToString() 
                        : selected.condition.conditionType,
                    targetValue = m_condTargetInput != null ? (int.TryParse(m_condTargetInput.text, out int target) ? target : 0) : selected.condition.targetValue
                },
                rewards = selected.rewards
            };

            m_viewModel.UpdateSelectedEvent(updated);
        }
        #endregion
    }
}
