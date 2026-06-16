using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BePex.EventSystem.Views;

namespace BePex.EventSystem.Editor
{
    /// <summary>
    /// [기능]: Ingame 씬의 CurrencyHUDView UI 오브젝트 하위에 SeasonPoint 및 Credit 텍스트를 복제/배치하고 
    ///         컴포넌트 직렬화 참조를 자동으로 바인딩해 주는 에디터 헬퍼 유틸리티.
    /// [작성자]: 윤승종
    /// </summary>
    public static class CurrencyHUDLayoutSetup
    {
        /// <summary>
        /// [기능]: Ingame 씬을 열고 HUD UI 오브젝트 및 텍스트를 정렬/바인딩한 후 저장합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        [MenuItem("BePex/UI/Align Currency HUD")]
        public static void AlignHUD()
        {
            const string scenePath = "Assets/_Game/Scenes/Inagme.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            
            var hudView = Object.FindAnyObjectByType<CurrencyHUDView>();
            if (hudView == null)
            {
                Debug.LogError("[CurrencyHUDLayoutSetup] 씬에서 CurrencyHUDView를 찾을 수 없습니다.");
                return;
            }

            var serializedObject = new SerializedObject(hudView);
            var goldTextProp = serializedObject.FindProperty("m_goldText");
            var expTextProp = serializedObject.FindProperty("m_expText");
            var ticketTextProp = serializedObject.FindProperty("m_ticketText");
            var seasonPointTextProp = serializedObject.FindProperty("m_seasonPointText");
            var creditTextProp = serializedObject.FindProperty("m_creditText");

            // 기존 티켓 텍스트가 있을 경우 이를 원본으로 복제하여 생성
            var ticketText = ticketTextProp.objectReferenceValue as TextMeshProUGUI;
            if (ticketText == null)
            {
                Debug.LogError("[CurrencyHUDLayoutSetup] 기준이 되는 m_ticketText 바인딩이 누락되어 복제에 실패했습니다.");
                return;
            }

            var hudParent = hudView.transform;

            // 1. SeasonPoint UI 오브젝트 생성 및 바인딩
            // 이미 존재할 경우 갱신만 수행하고 없으면 신규 생성
            TextMeshProUGUI seasonPointText = null;
            var seasonPointChild = hudParent.Find("SeasonPointText");
            if (seasonPointChild != null)
            {
                seasonPointText = seasonPointChild.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                var copy = Object.Instantiate(ticketText.gameObject, hudParent);
                copy.name = "SeasonPointText";
                seasonPointText = copy.GetComponent<TextMeshProUGUI>();
                seasonPointText.text = "0";
            }
            seasonPointTextProp.objectReferenceValue = seasonPointText;

            // 2. Credit UI 오브젝트 생성 및 바인딩
            TextMeshProUGUI creditText = null;
            var creditChild = hudParent.Find("CreditText");
            if (creditChild != null)
            {
                creditText = creditChild.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                var copy = Object.Instantiate(ticketText.gameObject, hudParent);
                copy.name = "CreditText";
                creditText = copy.GetComponent<TextMeshProUGUI>();
                creditText.text = "0";
            }
            creditTextProp.objectReferenceValue = creditText;

            serializedObject.ApplyModifiedProperties();

            // Layout 컴포넌트 정렬 (Horizontal Layout Group이 있으면 자식들이 자동 정렬됨)
            var layoutGroup = hudView.GetComponent<HorizontalLayoutGroup>();
            if (layoutGroup != null)
            {
                layoutGroup.enabled = false;
                layoutGroup.enabled = true; // 강제 갱신
                Canvas.ForceUpdateCanvases();
            }

            EditorUtility.SetDirty(hudView);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[CurrencyHUDLayoutSetup] CurrencyHUD UI 배치 및 텍스트 자동 바인딩이 완수되었습니다.");
        }
    }
}
