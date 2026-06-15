#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;
using System.Reflection;
using BePex.EventSystem.Views;

namespace BePex.EventSystem.Editor
{
    /// <summary>
    /// [기능]: 상단 재화 HUD의 미배치 UI 컴포넌트들을 씬에서 자동 복제 생성하고 뷰의 인스펙터 참조에 리플렉션으로 자동 주입해주는 에디터 자동화 유틸리티.
    /// [작성자]: 윤승종
    /// </summary>
    public static class CurrencyHUDViewSetup
    {
        /// <summary>
        /// [기능]: 씬에 배치된 CurrencyHUDView를 찾아 GoldGroup을 템플릿 삼아 시즌 포인트 및 재화 텍스트 레이아웃을 자동 증설하고 필드를 주입합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 구현 및 Undo, SetDirty 이식
        /// </summary>
        [MenuItem("Tools/BePex/Setup CurrencyHUDView Layout")]
        public static void SetupLayout()
        {
            var hudView = Object.FindFirstObjectByType<CurrencyHUDView>();
            if (hudView == null)
            {
                Debug.LogWarning("[CurrencyHUDViewSetup] 씬에 CurrencyHUDView가 존재하지 않습니다.");
                return;
            }

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Setup CurrencyHUDView Layout");
            int groupIndex = Undo.GetCurrentGroup();

            Transform container = hudView.transform;
            Transform goldGroup = container.Find("GoldGroup");
            if (goldGroup == null)
            {
                Debug.LogError("[CurrencyHUDViewSetup] 템플릿으로 사용할 GoldGroup을 찾을 수 없습니다.");
                return;
            }

            // 1. SeasonPoint UI 배치 및 셋업
            var seasonText = SetupGroup(hudView, goldGroup, "SeasonPointGroup", "[시즌]");
            // 2. Credit UI 배치 및 셋업
            var creditText = SetupGroup(hudView, goldGroup, "CreditGroup", "[재화]");

            // 3. 리플렉션을 통해 private SerializeField 필드에 할당
            var viewType = typeof(CurrencyHUDView);
            var seasonField = viewType.GetField("m_seasonPointText", BindingFlags.NonPublic | BindingFlags.Instance);
            var creditField = viewType.GetField("m_creditText", BindingFlags.NonPublic | BindingFlags.Instance);

            if (seasonField != null && seasonText != null)
            {
                seasonField.SetValue(hudView, seasonText);
            }
            if (creditField != null && creditText != null)
            {
                creditField.SetValue(hudView, creditText);
            }

            // 변경 사항 직렬화 및 저장 마킹
            EditorUtility.SetDirty(hudView);
            Undo.CollapseUndoOperations(groupIndex);

            // 씬 더티 마크
            if (!Application.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(hudView.gameObject.scene);
            }

            Debug.Log("[CurrencyHUDViewSetup] 2종 신규 재화 UI 자동 배치 및 리플렉션 주입 완료.");
        }

        /// <summary>
        /// [기능]: 템플릿을 복제하여 새로운 UI 그룹을 생성하고 텍스트 컴포넌트를 탐색해 반환하는 내부 헬퍼 메서드.
        /// [작성자]: 윤승종
        /// </summary>
        private static TextMeshProUGUI SetupGroup(CurrencyHUDView hudView, Transform template, string groupName, string prefix)
        {
            Transform existing = hudView.transform.Find(groupName);
            GameObject groupGo;

            if (existing != null)
            {
                groupGo = existing.gameObject;
            }
            else
            {
                // 복제 본체 생성
                groupGo = Object.Instantiate(template.gameObject, hudView.transform);
                groupGo.name = groupName;
                Undo.RegisterCreatedObjectUndo(groupGo, $"Create {groupName}");
            }

            // 하위 텍스트 컴포넌트 수색
            var textComp = groupGo.GetComponentInChildren<TextMeshProUGUI>();
            if (textComp != null)
            {
                // 구별을 위한 기본 텍스트 셋팅
                textComp.text = $"{prefix}: 0";
            }

            return textComp;
        }
    }
}
#endif
