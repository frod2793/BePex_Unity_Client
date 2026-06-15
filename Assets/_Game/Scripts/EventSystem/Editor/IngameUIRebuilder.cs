#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BePex.EventSystem.Editor
{
    /// <summary>
    /// [기능]: 인게임 씬(Inagme.unity)의 EventDetailView 계층 구조를 다중 퀘스트 목록 스크롤 뷰 형식으로 자동 개편하고 
    ///        EventQuestRowView 프리팹을 조립하여 바인딩하는 에디터 유틸리티 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    public static class IngameUIRebuilder
    {
        /// <summary>
        /// [기능]: 씬 컴포넌트를 탐색하고 레이아웃 및 직렬화 필드를 자동 재조립하는 에디터 진입점 메서드.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        [MenuItem("Tools/BePex/Rebuild Ingame UI Layout")]
        public static void func_RebuildIngameLayout()
        {
            // 1. EventQuestRowView 프리팹 제작
            string prefabPath = "Assets/_Game/Prefab/EventQuestRowView.prefab";
            GameObject rowPrefab = func_CreateQuestRowPrefab(prefabPath);
            if (rowPrefab == null)
            {
                return;
            }

            // 2. 인게임 씬 로드 상태 확인 및 EventDetailView 획득
            var detailView = Object.FindAnyObjectByType<BePex.EventSystem.Views.EventDetailView>();
            if (detailView == null)
            {
                Debug.LogError("[IngameUIRebuilder] 씬 내에서 EventDetailView를 찾을 수 없습니다. Inagme 씬을 먼저 열어주세요.");
                return;
            }

            Undo.RegisterCompleteObjectUndo(detailView.gameObject, "Rebuild Ingame UI Layout");

            // 3. 기존 자식 오브젝트 정리
            Transform descTextTrans = detailView.transform.Find("Desc_Text");
            Transform progressTextTrans = detailView.transform.Find("Progress_Text");
            Transform progressSliderTrans = detailView.transform.Find("Progress_Slider");
            Transform claimButtonTrans = detailView.transform.Find("Claim_Button");

            // Progress_Text, Progress_Slider 삭제
            if (progressTextTrans != null)
            {
                Object.DestroyImmediate(progressTextTrans.gameObject);
            }
            if (progressSliderTrans != null)
            {
                Object.DestroyImmediate(progressSliderTrans.gameObject);
            }

            // Desc_Text 크기 조절 (높이 120)
            if (descTextTrans != null)
            {
                var rect = descTextTrans as RectTransform;
                rect.sizeDelta = new Vector2(rect.sizeDelta.x, 120f);
                var le = descTextTrans.GetComponent<LayoutElement>();
                if (le == null)
                {
                    le = descTextTrans.gameObject.AddComponent<LayoutElement>();
                }
                le.preferredHeight = 120f;
            }

            // Claim_Button -> ClaimAll_Button 리네이밍 및 수령 버튼 핏팅
            Button claimAllBtn = null;
            if (claimButtonTrans != null)
            {
                claimButtonTrans.name = "ClaimAll_Button";
                claimAllBtn = claimButtonTrans.GetComponent<Button>();
                var txt = claimButtonTrans.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null)
                {
                    txt.text = "일괄 보상 받기";
                }
            }

            // 4. QuestList_ScrollView 동적 생성 및 레이아웃 조립
            Transform scrollViewTrans = detailView.transform.Find("QuestList_ScrollView");
            if (scrollViewTrans != null)
            {
                Object.DestroyImmediate(scrollViewTrans.gameObject);
            }

            GameObject scrollGo = new GameObject("QuestList_ScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            scrollGo.transform.SetParent(detailView.transform, false);
            scrollGo.transform.SetSiblingIndex(2); // Title(0) -> Desc(1) -> ScrollView(2) -> Button(3)

            // Scroll Rect 기본 설정
            var scrollRect = scrollGo.GetComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            
            // 배경 투명화
            var scrollImg = scrollGo.GetComponent<Image>();
            scrollImg.color = new Color(0, 0, 0, 0.1f);

            var scrollLE = scrollGo.AddComponent<LayoutElement>();
            scrollLE.flexibleHeight = 1f;
            scrollLE.preferredHeight = 660f;

            // Viewport 생성
            GameObject viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewportGo.transform.SetParent(scrollGo.transform, false);
            var viewRect = viewportGo.GetComponent<RectTransform>();
            viewRect.anchorMin = Vector2.zero;
            viewRect.anchorMax = Vector2.one;
            viewRect.sizeDelta = Vector2.zero;
            viewportGo.GetComponent<Mask>().showMaskGraphic = false;

            // Content 생성
            GameObject contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRect = contentGo.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.sizeDelta = new Vector2(0f, 0f);

            var contentVLG = contentGo.GetComponent<VerticalLayoutGroup>();
            contentVLG.padding = new RectOffset(10, 10, 10, 10);
            contentVLG.spacing = 10;
            contentVLG.childControlWidth = true;
            contentVLG.childControlHeight = true;
            contentVLG.childForceExpandWidth = true;
            contentVLG.childForceExpandHeight = false;

            var contentCSF = contentGo.GetComponent<ContentSizeFitter>();
            contentCSF.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewRect;
            scrollRect.content = contentRect;

            // 5. EventDetailView 직렬화 레퍼런스 바인딩 매핑
            var so = new SerializedObject(detailView);
            so.FindProperty("m_questListContent").objectReferenceValue = contentRect;
            so.FindProperty("m_questRowPrefab").objectReferenceValue = rowPrefab.GetComponent<BePex.EventSystem.Views.EventQuestRowView>();
            so.FindProperty("m_claimAllButton").objectReferenceValue = claimAllBtn;
            so.ApplyModifiedProperties();

            // 씬 저장 및 마무리
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(detailView.gameObject.scene);
            Debug.Log("[IngameUIRebuilder] 인게임 EventDetailView 다중 퀘스트 스크롤 뷰 및 바인딩 조립이 성공적으로 완료되었습니다.");
        }

        /// <summary>
        /// [기능]: 퀘스트 개별 행용 EventQuestRowView 프리팹 에셋을 생성 및 텍스트 레이캐스트 속성을 안전 조치하여 반환합니다.
        /// [작성자]: 윤승종
        /// </summary>
        private static GameObject func_CreateQuestRowPrefab(string path)
        {
            // 이미 생성되어 있는지 로드 시도
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                return existing;
            }

            // 새 프리팹 루트 조립 (높이 85px)
            GameObject root = new GameObject("EventQuestRowView", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement), typeof(BePex.EventSystem.Views.EventQuestRowView));
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(900f, 85f);

            var hlg = root.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(15, 15, 8, 8);
            hlg.spacing = 15;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            var rootLE = root.GetComponent<LayoutElement>();
            rootLE.minHeight = 85f;
            rootLE.flexibleWidth = 1f;

            // 배경용 투명 패널 추가
            var img = root.AddComponent<Image>();
            img.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);

            // 1. QuestTitle_Text
            GameObject titleGo = new GameObject("QuestTitle_Text", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            titleGo.transform.SetParent(root.transform, false);
            var titleText = titleGo.GetComponent<TextMeshProUGUI>();
            titleText.fontSize = 18;
            titleText.text = "퀘스트 제목";
            titleText.raycastTarget = false; // 클릭 방해 방지
            titleGo.GetComponent<LayoutElement>().flexibleWidth = 1f;

            // 2. Progress_Text
            GameObject progressGo = new GameObject("Progress_Text", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            progressGo.transform.SetParent(root.transform, false);
            var progressText = progressGo.GetComponent<TextMeshProUGUI>();
            progressText.fontSize = 14;
            progressText.text = "0 / 10 (0%)";
            progressText.alignment = TextAlignmentOptions.Center;
            progressText.raycastTarget = false;
            var progLE = progressGo.GetComponent<LayoutElement>();
            progLE.preferredWidth = 100f;

            // 3. Progress_Slider
            GameObject sliderGo = new GameObject("Progress_Slider", typeof(RectTransform), typeof(Slider), typeof(LayoutElement));
            sliderGo.transform.SetParent(root.transform, false);
            var slider = sliderGo.GetComponent<Slider>();
            var sliderLE = sliderGo.GetComponent<LayoutElement>();
            sliderLE.preferredWidth = 180f;
            sliderLE.preferredHeight = 15f;
            
            // 슬라이더 최소화 뼈대 조립
            slider.interactable = false;

            // 4. Reward_Text
            GameObject rewardGo = new GameObject("Reward_Text", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            rewardGo.transform.SetParent(root.transform, false);
            var rewardText = rewardGo.GetComponent<TextMeshProUGUI>();
            rewardText.fontSize = 14;
            rewardText.text = "보상 목록";
            rewardText.raycastTarget = false;
            var rewLE = rewardGo.GetComponent<LayoutElement>();
            rewLE.preferredWidth = 200f;

            // 5. Claim_Button
            GameObject buttonGo = new GameObject("Claim_Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            buttonGo.transform.SetParent(root.transform, false);
            var btnLE = buttonGo.GetComponent<LayoutElement>();
            btnLE.preferredWidth = 110f;
            btnLE.preferredHeight = 35f;
            
            var btnImg = buttonGo.GetComponent<Image>();
            btnImg.color = new Color(0.25f, 0.4f, 0.25f, 1f);

            GameObject btnTextGo = new GameObject("Text (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
            btnTextGo.transform.SetParent(buttonGo.transform, false);
            var btnText = btnTextGo.GetComponent<TextMeshProUGUI>();
            btnText.fontSize = 14;
            btnText.text = "보상 받기";
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.raycastTarget = false; // 편집 버튼 클릭 방해 방지 표준 적용
            
            // 텍스트 stretch 적용
            var btnTextRect = btnTextGo.GetComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.sizeDelta = Vector2.zero;

            // 6. EventQuestRowView 컴포넌트 필드 매핑
            var rowView = root.GetComponent<BePex.EventSystem.Views.EventQuestRowView>();
            var viewSO = new SerializedObject(rowView);
            viewSO.FindProperty("m_questTitleText").objectReferenceValue = titleText;
            viewSO.FindProperty("m_progressText").objectReferenceValue = progressText;
            viewSO.FindProperty("m_progressSlider").objectReferenceValue = slider;
            viewSO.FindProperty("m_rewardText").objectReferenceValue = rewardText;
            viewSO.FindProperty("m_claimButton").objectReferenceValue = buttonGo.GetComponent<Button>();
            viewSO.ApplyModifiedProperties();

            // 디렉토리가 없으면 자동 생성 후 프리팹 저장
            System.IO.Directory.CreateDirectory("Assets/_Game/Prefab");
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);

            Debug.Log($"[IngameUIRebuilder] {path} 프리팹 에셋 조립 완료.");
            return savedPrefab;
        }
    }
}
#endif
