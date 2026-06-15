#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;

namespace BePex.EventSystem.Editor
{
    /// <summary>
    /// [기능]: QuestRowView 프리팹 내 텍스트 컴포넌트의 레이캐스트 차단 및 앵커 스트레칭 교정을 처리하는 에디터 스크립트.
    /// [작성자]: 윤승종
    /// </summary>
    public static class QuestRowViewLayoutFixer
    {
        /// <summary>
        /// [기능]: 프리팹 에셋을 로드하여 편집하고 저장하는 에디터 진입점 메서드.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        [MenuItem("Tools/BePex/Fix QuestRowView Layout")]
        public static void func_FixQuestRowLayout()
        {
            string prefabPath = "Assets/_Game/Prefab/QuestRowView.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            if (prefab == null)
            {
                Debug.LogError("[QuestRowViewLayoutFixer] QuestRowView.prefab 프리팹 에셋을 찾을 수 없습니다.");
                return;
            }

            GameObject tempInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (tempInstance == null)
            {
                return;
            }

            Transform fixBtn = tempInstance.transform.Find("Fix_Button");
            Transform delBtn = tempInstance.transform.Find("Delete_Button");

            func_NormalizeButtonTextLayout(fixBtn);
            func_NormalizeButtonTextLayout(delBtn);

            PrefabUtility.SaveAsPrefabAsset(tempInstance, prefabPath);
            Object.DestroyImmediate(tempInstance);
            
            Debug.Log("[QuestRowViewLayoutFixer] QuestRowView 프리팹의 텍스트 레이캐스트(Raycast Target) 및 오버플로우 교정 작업이 성공적으로 완료되었습니다.");
        }

        /// <summary>
        /// [기능]: 버튼 하위의 텍스트 RaycastTarget을 비활성화하고 RectTransform 오프셋을 부모 크기에 핏팅합니다.
        /// [작성자]: 윤승종
        /// </summary>
        private static void func_NormalizeButtonTextLayout(Transform buttonTransform)
        {
            if (buttonTransform != null)
            {
                Transform textTransform = buttonTransform.Find("Text (TMP)");
                if (textTransform != null)
                {
                    TextMeshProUGUI tmp = textTransform.GetComponent<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        tmp.raycastTarget = false;
                    }

                    RectTransform rect = textTransform as RectTransform;
                    if (rect != null)
                    {
                        rect.anchorMin = Vector2.zero;
                        rect.anchorMax = Vector2.one;
                        rect.offsetMin = Vector2.zero;
                        rect.offsetMax = Vector2.zero;
                        rect.sizeDelta = Vector2.zero;
                    }
                }
            }
        }
    }
}
#endif
