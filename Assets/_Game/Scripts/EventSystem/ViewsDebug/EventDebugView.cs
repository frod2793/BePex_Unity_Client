using UnityEngine;
using UnityEngine.UI;
using BePex.EventSystem.ViewModelsDebug;
using TMPro;
using System.Collections;

namespace BePex.EventSystem.ViewsDebug
{
    /// <summary>
    /// [기능]: 동적 생성(OCP) 방식을 통해 이벤트 조작 및 보상 현황을 스크롤 뷰에 나열하는 시뮬레이터 전용 View (슬라이드 드로어 지원).
    /// [작성자]: 윤승종
    /// </summary>
    public class EventDebugView : MonoBehaviour
    {
        #region UI 참조 (Inspector)
        [Header("Scroll View Container")]
        [SerializeField] private RectTransform m_contentParent;
        
        [Header("Dynamic Prefabs")]
        [SerializeField] private Button m_actionButtonPrefab;
        [SerializeField] private TextMeshProUGUI m_rewardStatusTextPrefab;

        [Header("Time & Data Controls")]
        [SerializeField] private Button m_addOneDayButton;
        [SerializeField] private Button m_addSevenDaysButton;
        [SerializeField] private Button m_resetTimeButton;
        [SerializeField] private Button m_resetDataButton;

        [Header("Drawer Panel Settings (Left Sliding Panel)")]
        [SerializeField] private RectTransform m_drawerPanel;
        [SerializeField] private Button m_drawerToggleButton;
        [SerializeField] private TextMeshProUGUI m_drawerToggleText;
        [SerializeField] private float m_slideDuration = 0.25f;
        [SerializeField] private float m_drawerWidth = 450f;
        #endregion

        #region 내부 필드
        private EventDebugViewModel m_viewModel;
        private readonly System.Collections.Generic.List<GameObject> m_spawnedItems = new System.Collections.Generic.List<GameObject>();
        private bool m_isDrawerOpen = false;
        private Coroutine m_slideCoroutine;
        #endregion

        #region 유니티 생명주기
        /// <summary>
        /// [기능]: 드로어 패널의 앵커, 피벗, 자식 위치 및 토글 레이아웃을 초기 자동 설정하며, 2x2 컨트롤 그리드 및 flexibleHeight 스크롤 뷰를 구성합니다.
        /// [작성자]: 윤승종
        /// </summary>
        private void Awake()
        {
            // 캔버스 하위 직속으로 이동하여 전체 화면 스트레치 앵커 작동 보장
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null && transform.parent != canvas.transform)
            {
                transform.SetParent(canvas.transform, false);
            }

            if (m_drawerPanel == null)
            {
                m_drawerPanel = GetComponent<RectTransform>();
            }

            if (m_drawerPanel != null)
            {
                m_drawerPanel.anchorMin = new Vector2(0f, 0f);
                m_drawerPanel.anchorMax = new Vector2(0f, 1f);
                m_drawerPanel.pivot = new Vector2(0f, 0.5f);
                m_drawerPanel.sizeDelta = new Vector2(m_drawerWidth, 0f);
                m_drawerPanel.anchoredPosition = new Vector2(-m_drawerWidth, 0f);
            }

            // ControlPanel을 2x2 그리드로 구성하여 넓은 공간 활용 및 글씨 겹침 방지
            if (m_addOneDayButton != null)
            {
                var controlPanel = m_addOneDayButton.transform.parent as RectTransform;
                if (controlPanel != null)
                {
                    var horizontalLayout = controlPanel.GetComponent<HorizontalLayoutGroup>();
                    if (horizontalLayout != null)
                    {
                        DestroyImmediate(horizontalLayout);
                    }

                    var grid = controlPanel.GetComponent<GridLayoutGroup>();
                    if (grid == null)
                    {
                        grid = controlPanel.gameObject.AddComponent<GridLayoutGroup>();
                    }

                    grid.cellSize = new Vector2(195f, 40f);
                    grid.spacing = new Vector2(10f, 10f);
                    grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    grid.constraintCount = 2;
                    grid.childAlignment = TextAnchor.UpperLeft;

                    var fitter = controlPanel.GetComponent<ContentSizeFitter>();
                    if (fitter == null)
                    {
                        fitter = controlPanel.gameObject.AddComponent<ContentSizeFitter>();
                    }
                    fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                }
            }

            // ScrollView가 남는 세로 공간을 전부 채우도록 flexibleHeight 주입 (Squish 현상 복구)
            if (m_contentParent != null)
            {
                var viewport = m_contentParent.parent as RectTransform;
                if (viewport != null)
                {
                    var scrollView = viewport.parent as RectTransform;
                    if (scrollView != null)
                    {
                        var layoutElement = scrollView.gameObject.GetComponent<LayoutElement>();
                        if (layoutElement == null)
                        {
                            layoutElement = scrollView.gameObject.AddComponent<LayoutElement>();
                        }
                        layoutElement.flexibleHeight = 1f;
                        layoutElement.minHeight = 200f;
                    }
                }
            }

            // 토글 버튼은 VerticalLayoutGroup의 영향을 받지 않도록 LayoutElement 배치 제외 설정
            if (m_drawerToggleButton != null)
            {
                var layoutElement = m_drawerToggleButton.gameObject.GetComponent<LayoutElement>();
                if (layoutElement == null)
                {
                    layoutElement = m_drawerToggleButton.gameObject.AddComponent<LayoutElement>();
                }
                layoutElement.ignoreLayout = true;

                var btnRect = m_drawerToggleButton.GetComponent<RectTransform>();
                if (btnRect != null)
                {
                    btnRect.anchorMin = new Vector2(1f, 0.5f);
                    btnRect.anchorMax = new Vector2(1f, 0.5f);
                    btnRect.pivot = new Vector2(0f, 0.5f);
                    btnRect.anchoredPosition = new Vector2(0f, 0f);
                }

                m_drawerToggleButton.onClick.RemoveAllListeners();
                m_drawerToggleButton.onClick.AddListener(func_ToggleDrawer);
            }

            // 레이아웃 그룹 여백 패딩 및 간격 조정 (너비 확장에 맞춰 가독성 극대화)
            var layout = GetComponent<VerticalLayoutGroup>();
            if (layout != null)
            {
                layout.padding = new RectOffset(20, 20, 30, 30);
                layout.spacing = 15f;
            }

            UpdateToggleText();
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 디버그 뷰모델을 주입받고 컨트롤 버튼 바인딩 및 초기 동적 리스트업을 수행합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: OCP 기반 동적 UI 렌더링으로 개편
        /// </summary>
        public void Bind(EventDebugViewModel viewModel)
        {
            m_viewModel = viewModel;

            if (m_addOneDayButton != null)
            {
                m_addOneDayButton.onClick.RemoveAllListeners();
                m_addOneDayButton.onClick.AddListener(func_OnAddOneDayClick);
            }

            if (m_addSevenDaysButton != null)
            {
                m_addSevenDaysButton.onClick.RemoveAllListeners();
                m_addSevenDaysButton.onClick.AddListener(func_OnAddSevenDaysClick);
            }

            if (m_resetTimeButton != null)
            {
                m_resetTimeButton.onClick.RemoveAllListeners();
                m_resetTimeButton.onClick.AddListener(func_OnResetTimeClick);
            }

            if (m_resetDataButton != null)
            {
                m_resetDataButton.onClick.RemoveAllListeners();
                m_resetDataButton.onClick.AddListener(func_OnResetDataClick);
            }

            RefreshDynamicUI();
        }

        /// <summary>
        /// [기능]: +1일 시간 경과 버튼 이벤트 핸들러입니다.
        /// [작성자]: 윤승종
        /// </summary>
        public void func_OnAddOneDayClick()
        {
            if (m_viewModel != null)
            {
                m_viewModel.SimulateTimeOffset(1);
                RefreshDynamicUI();
            }
        }

        /// <summary>
        /// [기능]: +7일 시간 경과 버튼 이벤트 핸들러입니다.
        /// [작성자]: 윤승종
        /// </summary>
        public void func_OnAddSevenDaysClick()
        {
            if (m_viewModel != null)
            {
                m_viewModel.SimulateTimeOffset(7);
                RefreshDynamicUI();
            }
        }

        /// <summary>
        /// [기능]: 가상 시간 리셋 버튼 이벤트 핸들러입니다.
        /// [작성자]: 윤승종
        /// </summary>
        public void func_OnResetTimeClick()
        {
            if (m_viewModel != null)
            {
                m_viewModel.ResetTimeOffset();
                RefreshDynamicUI();
            }
        }

        /// <summary>
        /// [기능]: 전체 데이터 리셋 버튼 이벤트 핸들러입니다.
        /// [작성자]: 윤승종
        /// </summary>
        public async void func_OnResetDataClick()
        {
            if (m_viewModel != null)
            {
                await m_viewModel.ResetAllDataAsync();
                RefreshDynamicUI();
            }
        }

        /// <summary>
        /// [기능]: 슬라이드 드로어 열기/닫기 토글 처리를 실행합니다.
        /// [작성자]: 윤승종
        /// </summary>
        public void func_ToggleDrawer()
        {
            m_isDrawerOpen = !m_isDrawerOpen;

            if (m_slideCoroutine != null)
            {
                StopCoroutine(m_slideCoroutine);
            }

            float targetX = m_isDrawerOpen ? 0f : -m_drawerWidth;
            m_slideCoroutine = StartCoroutine(SlideDrawer(new Vector2(targetX, 0f)));
            UpdateToggleText();
        }
        #endregion

        #region 내부 렌더링 및 애니메이션
        /// <summary>
        /// [기능]: 드로어 패널의 anchoredPosition을 코루틴을 통해 부드럽게 보간 이동시킵니다.
        /// [작성자]: 윤승종
        /// </summary>
        private IEnumerator SlideDrawer(Vector2 targetPosition)
        {
            if (m_drawerPanel == null) yield break;

            Vector2 startPosition = m_drawerPanel.anchoredPosition;
            float elapsedTime = 0f;

            while (elapsedTime < m_slideDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsedTime / m_slideDuration);
                m_drawerPanel.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
                yield return null;
            }

            m_drawerPanel.anchoredPosition = targetPosition;
            m_slideCoroutine = null;
        }

        /// <summary>
        /// [기능]: 현재 드로어 개폐 상태에 맞춰 토글 버튼 텍스트를 기호로 변경합니다.
        /// [작성자]: 윤승종
        /// </summary>
        private void UpdateToggleText()
        {
            if (m_drawerToggleText != null)
            {
                m_drawerToggleText.text = m_isDrawerOpen ? "<" : ">";
            }
        }

        /// <summary>
        /// [기능]: 현재 생성된 모든 동적 UI를 지우고, 뷰모델의 최신 상태를 받아와 2열 그리드 구조로 보기 쉽게 재배치합니다.
        /// [작성자]: 윤승종
        /// </summary>
        private void RefreshDynamicUI()
        {
            if (m_contentParent == null || m_viewModel == null) return;

            // 1. 기존 생성물 클리어
            for (int i = 0; i < m_spawnedItems.Count; i++)
            {
                if (m_spawnedItems[i] != null) Destroy(m_spawnedItems[i]);
            }
            m_spawnedItems.Clear();

            // 2. 리워드 현황 동적 렌더링 (2열 Grid 배치)
            if (m_rewardStatusTextPrefab != null)
            {
                var titleText = Instantiate(m_rewardStatusTextPrefab, m_contentParent);
                titleText.text = "--- [보유 재화 현황] ---";
                titleText.alignment = TextAlignmentOptions.Center;
                m_spawnedItems.Add(titleText.gameObject);

                // Grid Container 생성
                var gridGo = new GameObject("StatusGrid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
                gridGo.transform.SetParent(m_contentParent, false);
                m_spawnedItems.Add(gridGo);

                var grid = gridGo.GetComponent<GridLayoutGroup>();
                grid.cellSize = new Vector2(195f, 35f);
                grid.spacing = new Vector2(10f, 8f);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 2;
                grid.childAlignment = TextAnchor.UpperLeft;

                var fitter = gridGo.GetComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

                var statusDict = m_viewModel.GetRewardStatus();
                foreach (var kvp in statusDict)
                {
                    var txt = Instantiate(m_rewardStatusTextPrefab, gridGo.transform);
                    txt.text = $"• {kvp.Key}: <color=#FFD700>{kvp.Value}</color>";
                    txt.alignment = TextAlignmentOptions.Left;
                }
            }

            // 3. 액션 트리거 동적 렌더링 (2열 Grid 배치)
            if (m_actionButtonPrefab != null)
            {
                if (m_rewardStatusTextPrefab != null)
                {
                    // 구분을 위한 간격용 투명 텍스트 배치
                    var spaceText = Instantiate(m_rewardStatusTextPrefab, m_contentParent);
                    spaceText.text = "";
                    m_spawnedItems.Add(spaceText.gameObject);

                    var titleText = Instantiate(m_rewardStatusTextPrefab, m_contentParent);
                    titleText.text = "--- [행동 모의 트리거] ---";
                    titleText.alignment = TextAlignmentOptions.Center;
                    m_spawnedItems.Add(titleText.gameObject);
                }

                // Grid Container 생성
                var gridGo = new GameObject("ActionGrid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
                gridGo.transform.SetParent(m_contentParent, false);
                m_spawnedItems.Add(gridGo);

                var grid = gridGo.GetComponent<GridLayoutGroup>();
                grid.cellSize = new Vector2(195f, 50f);
                grid.spacing = new Vector2(10f, 10f);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 2;
                grid.childAlignment = TextAnchor.UpperLeft;

                var fitter = gridGo.GetComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

                string[] actionTypes = m_viewModel.GetAvailableActionTypes();
                for (int i = 0; i < actionTypes.Length; i++)
                {
                    string actionType = actionTypes[i];
                    var btn = Instantiate(m_actionButtonPrefab, gridGo.transform);
                    
                    var btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
                    if (btnText != null) 
                    {
                        btnText.text = $"{actionType}";
                        btnText.fontSize = 14f;
                    }

                    btn.onClick.AddListener(async () => 
                    {
                        await m_viewModel.SimulateActionAsync(actionType);
                        RefreshDynamicUI();
                    });

                    m_spawnedItems.Add(btn.gameObject);
                }
            }
        }
        #endregion
    }
}

