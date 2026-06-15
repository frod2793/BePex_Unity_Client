using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using BePex.EventSystem.ViewsDebug;
using TMPro;

namespace BePex.EventSystem.Editor
{
    /// <summary>
    /// [기능]: 에디터 상에서 EventDebugView를 슬라이드 드로어 형태로 자동 셋업하기 위한 헬퍼 스크립트.
    /// [작성자]: 윤승종
    /// </summary>
    public static class EventDebugViewSetup
    {
        /// <summary>
        /// [기능]: 씬 내의 EventDebugView를 찾아 좌측 드로어 앵커 및 토글 버튼을 생성/바인딩 해줍니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 드로어 셋업 에디터 메뉴 추가
        /// </summary>
        [MenuItem("Tools/BePex/Setup EventDebugView Drawer")]
        public static void SetupDrawer()
        {
            var debugView = Object.FindAnyObjectByType<EventDebugView>();
            if (debugView == null)
            {
                Debug.LogError("[EventDebugViewSetup] 씬에서 EventDebugView를 찾을 수 없습니다.");
                return;
            }

            // 1. 가로 너비 및 세로 스트레치 앵커 조정
            var rect = debugView.GetComponent<RectTransform>();
            if (rect != null)
            {
                Undo.RecordObject(rect, "Configure EventDebugView Size");
                rect.sizeDelta = new Vector2(450f, 0f);
                EditorUtility.SetDirty(rect);
            }

            // 2. 토글 버튼 자동 생성 및 할당
            var toggleBtn = debugView.transform.Find("DrawerToggleButton");
            if (toggleBtn == null)
            {
                var btnGo = new GameObject("DrawerToggleButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                btnGo.transform.SetParent(debugView.transform, false);
                
                var btnImage = btnGo.GetComponent<Image>();
                if (btnImage != null)
                {
                    btnImage.color = new Color(0.12f, 0.12f, 0.12f, 0.95f);
                }

                var button = btnGo.GetComponent<Button>();

                // 텍스트 생성
                var textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                textGo.transform.SetParent(btnGo.transform, false);
                var tmp = textGo.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = ">";
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.fontSize = 24f;
                    tmp.color = Color.white;
                }

                var textRect = textGo.GetComponent<RectTransform>();
                if (textRect != null)
                {
                    textRect.anchorMin = Vector2.zero;
                    textRect.anchorMax = Vector2.one;
                    textRect.sizeDelta = Vector2.zero;
                }

                // 버튼 RectTransform 설정
                var btnRect = btnGo.GetComponent<RectTransform>();
                if (btnRect != null)
                {
                    btnRect.sizeDelta = new Vector2(50f, 80f);
                }

                Undo.RegisterCreatedObjectUndo(btnGo, "Create Drawer Toggle Button");
                
                // EventDebugView 내부 private 직렬화 필드 리플렉션을 통해 값 자동 할당
                var viewType = typeof(EventDebugView);
                var btnField = viewType.GetField("m_drawerToggleButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (btnField != null)
                {
                    Undo.RecordObject(debugView, "Assign Toggle Button");
                    btnField.SetValue(debugView, button);
                }

                var textField = viewType.GetField("m_drawerToggleText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (textField != null)
                {
                    Undo.RecordObject(debugView, "Assign Toggle Text");
                    textField.SetValue(debugView, tmp);
                }

                var drawerField = viewType.GetField("m_drawerPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (drawerField != null)
                {
                    Undo.RecordObject(debugView, "Assign Drawer Panel");
                    drawerField.SetValue(debugView, rect);
                }
                
                EditorUtility.SetDirty(debugView);
                Debug.Log("[EventDebugViewSetup] 드로어 토글 버튼이 성공적으로 생성 및 할당되었습니다.");
            }
            else
            {
                Debug.Log("[EventDebugViewSetup] 이미 토글 버튼이 존재하여 구성을 스킵합니다.");
            }
        }
    }
}
