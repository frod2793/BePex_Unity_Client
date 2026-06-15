using System;
using UnityEngine;
using UnityEngine.UI;
using BePex.EventSystem.DTOs;
using BePex.EventSystem.ViewModels;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BePex.EventSystem.Views
{
    /// <summary>
    /// [기능]: 이벤트 리스트 뷰 내부의 개별 항목 셀을 DTO 데이터를 활용해 렌더링하고 사용자 입력을 뷰모델에 전달하는 View 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    public class EventItemCell : MonoBehaviour
    {
        #region UI 참조 (Inspector)
        [SerializeField] private TextMeshProUGUI m_titleText;
        [SerializeField] private Image m_iconImage;
        [SerializeField] private Button m_selectButton;
        [SerializeField] private Image m_backgroundImage;
        #endregion

        #region 내부 필드
        private EventDefinitionDTO m_definition;
        private EventListViewModel m_viewModel;
        private Action<string> m_onSelectAction;
        private AsyncOperationHandle<Sprite> m_spriteLoadHandle;
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 이벤트 DTO 데이터 및 뷰모델 인스턴스를 주입받아 UI 텍스트를 구성하고, 어드레서블 주소로 스프라이트를 비동기 로드 및 바인딩합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// <summary>
        /// [기능]: 이벤트 DTO 데이터 및 뷰모델 인스턴스를 주입받아 UI 텍스트를 구성하고, 어드레서블 주소로 스프라이트를 비동기 로드 및 바인딩합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 획득 상태별 시각 효과 적용(UpdateCellAppearanceAsync 호출)
        /// </summary>
        public void Setup(EventDefinitionDTO definition, EventListViewModel viewModel)
        {
            m_definition = definition;
            m_viewModel = viewModel;
            m_onSelectAction = null;

            if (m_titleText != null)
            {
                m_titleText.text = m_definition.eventTitle;
            }

            if (m_iconImage != null && !string.IsNullOrEmpty(m_definition.eventIconAddress))
            {
                LoadSpriteAsync(m_definition.eventIconAddress, m_iconImage);
            }

            if (m_selectButton != null)
            {
                m_selectButton.onClick.RemoveAllListeners();
                m_selectButton.onClick.AddListener(func_OnSelectCell);
            }

            UpdateCellAppearanceAsync();
        }

        /// <summary>
        /// [기능]: 기획 데이터 및 선택 액션 콜백을 주입받아 셀을 초기화합니다. (어드민 등 재사용 목적)
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 액션 기반의 Setup 오버로드 추가
        /// </summary>
        public void Setup(EventDefinitionDTO definition, Action<string> onSelect)
        {
            m_definition = definition;
            m_viewModel = null;
            m_onSelectAction = onSelect;

            if (m_titleText != null)
            {
                m_titleText.text = m_definition.eventTitle;
            }

            if (m_iconImage != null && !string.IsNullOrEmpty(m_definition.eventIconAddress))
            {
                LoadSpriteAsync(m_definition.eventIconAddress, m_iconImage);
            }

            if (m_selectButton != null)
            {
                m_selectButton.onClick.RemoveAllListeners();
                m_selectButton.onClick.AddListener(func_OnActionSelectTriggered);
            }
        }

        /// <summary>
        /// [기능]: 사용자가 셀 버튼을 클릭하였을 때 실행되는 UI Callback 메서드. 뷰모델에 해당 이벤트 선택 사실을 통지합니다. func_ 접두사 준수.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public void func_OnSelectCell()
        {
            if (m_viewModel != null && m_definition != null)
            {
                m_viewModel.SelectEvent(m_definition.eventId);
            }
        }

        /// <summary>
        /// [기능]: 액션 콜백 기반 리스너 트리거 시 호출되어 안전하게 이벤트를 발송하는 UI Callback 메서드. func_ 접두사 준수.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 람다 분리 및 정식 메서드 추출
        /// </summary>
        public void func_OnActionSelectTriggered()
        {
            if (m_onSelectAction != null && m_definition != null)
            {
                m_onSelectAction.Invoke(m_definition.eventId);
            }
        }
        #endregion

        #region 유니티 생명주기
        /// <summary>
        /// [기능]: 객체가 파괴될 때 비동기 로딩 중이거나 참조 중인 어드레서블 리소스를 해제합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        private void OnDestroy()
        {
            ReleaseSprite();
        }
        #endregion

        #region 내부 메서드
        /// <summary>
        /// [기능]: 지정된 어드레서블 주소값으로 스프라이트 애셋을 로드하여 이미지 컴포넌트에 할당합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: LoadResourceLocationsAsync 사전 체크를 도입하여 존재하지 않는 키 로드 시 발생하는 어드레서블 내부 콘솔 에러 로그 원천 방지
        /// </summary>
        private async void LoadSpriteAsync(string address, Image targetImage)
        {
            ReleaseSprite();

            if (string.IsNullOrEmpty(address) || address == "UI/Icons/Default")
            {
                address = "item_Sheet[item_Sheet_0]";
            }

            try
            {
                // 1단계: 해당 어드레서블 주소가 카탈로그에 존재하는지 검증 (콘솔 에러 강제 출력 방지)
                var locationsHandle = Addressables.LoadResourceLocationsAsync(address, typeof(Sprite));
                var locations = await locationsHandle.Task;
                if (this == null)
                {
                    if (locationsHandle.IsValid())
                    {
                        Addressables.Release(locationsHandle);
                    }
                    return;
                }
                bool exists = locations != null && locations.Count > 0;
                Addressables.Release(locationsHandle);

                if (!exists)
                {
                    // 요청한 특정 아이콘이 없는 경우 기본 이미지로 2차 폴백 시도
                    if (address != "item_Sheet[item_Sheet_0]")
                    {
                        address = "item_Sheet[item_Sheet_0]";
                        locationsHandle = Addressables.LoadResourceLocationsAsync(address, typeof(Sprite));
                        locations = await locationsHandle.Task;
                        if (this == null)
                        {
                            if (locationsHandle.IsValid())
                            {
                                Addressables.Release(locationsHandle);
                            }
                            return;
                        }
                        exists = locations != null && locations.Count > 0;
                        Addressables.Release(locationsHandle);
                    }

                    if (!exists)
                    {
                        Debug.LogWarning($"[EventItemCell] 존재하지 않는 어드레서블 주소입니다. 주소: {address}");
                        if (targetImage != null)
                        {
                            targetImage.sprite = null;
                        }
                        return;
                    }
                }

                // 2단계: 주소가 확인되었으므로 안전하게 실제 에셋 로드 시도
                m_spriteLoadHandle = Addressables.LoadAssetAsync<Sprite>(address);
                Sprite sprite = await m_spriteLoadHandle.Task;
                if (this == null)
                {
                    return;
                }
                if (targetImage != null && sprite != null)
                {
                    targetImage.sprite = sprite;
                }
            }
            catch (Exception ex)
            {
                if (this == null)
                {
                    return;
                }
                Debug.LogWarning($"[EventItemCell] 아이콘 스프라이트를 로드하지 못했습니다. 주소: {address}, 에러: {ex.Message}");
                if (targetImage != null)
                {
                    targetImage.sprite = null;
                }
            }
        }

        /// <summary>
        /// [기능]: 해당 이벤트의 획득 및 수령 완료 상태를 비동기로 조회해 배경색 및 텍스트 색상을 상태별로 업데이트합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 비동기 await 이후 GameObject 파괴로 인한 MissingReferenceException 방지 널 가드 추가
        /// </summary>
        private async void UpdateCellAppearanceAsync()
        {
            if (m_viewModel == null || m_definition == null)
            {
                return;
            }

            string eventId = m_definition.eventId;
            bool isClaimed = await m_viewModel.IsRewardClaimedAsync(eventId);
            if (this == null)
            {
                return;
            }
            bool canClaim = await m_viewModel.CanClaimRewardAsync(eventId);
            if (this == null)
            {
                return;
            }

            Image bgImage = m_backgroundImage;
            if (bgImage == null)
            {
                bgImage = GetComponent<Image>();
            }

            if (bgImage != null)
            {
                if (isClaimed)
                {
                    bgImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                }
                else if (canClaim)
                {
                    bgImage.color = new Color(1.0f, 0.9f, 0.4f, 1.0f);
                }
                else
                {
                    bgImage.color = Color.white;
                }
            }

            if (m_titleText != null)
            {
                if (isClaimed)
                {
                    m_titleText.color = new Color(0.3f, 0.3f, 0.3f, 0.6f);
                }
                else
                {
                    m_titleText.color = Color.black;
                }
            }
        }

        /// <summary>
        /// [기능]: 현재 생성 및 활성화되어 있는 어드레서블 로드 핸들을 메모리 상에서 해제합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        private void ReleaseSprite()
        {
            if (m_spriteLoadHandle.IsValid())
            {
                Addressables.Release(m_spriteLoadHandle);
            }
        }
        #endregion
    }
}
