using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using BePex.EventSystem.Views;
using TMPro;

namespace BePex.EventSystem.Editor
{
    /// <summary>
    /// [기능]: 에디터 상에서 RewardPopupView의 레이아웃을 효율적이고 아름다운 텍스트 통폐합 구조로 자동 재구성 및 바인딩해 주는 헬퍼 스크립트.
    /// [작성자]: 윤승종
    /// </summary>
    public static class RewardPopupViewSetup
    {
        /// <summary>
        /// [기능]: 씬 내의 RewardPopupView를 찾아 기존 5개 개별 재화 텍스트들을 일괄 정리하고,
        ///        획득 보상 목록 및 누적 보유 재화 2개 컴포넌트로 압축 레이아웃을 자동 재구성합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최적화된 팝업 레이아웃 에디터 셋업 메뉴 추가
        /// </summary>
        [MenuItem("Tools/BePex/Setup RewardPopupView Layout")]
        public static void SetupLayout()
        {
            var popupView = Object.FindAnyObjectByType<RewardPopupView>();
            if (popupView == null)
            {
                Debug.LogError("[RewardPopupViewSetup] 씬에서 RewardPopupView를 찾을 수 없습니다.");
                return;
            }

            var viewType = typeof(RewardPopupView);
            var rootField = viewType.GetField("m_popupRoot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (rootField == null)
            {
                Debug.LogError("[RewardPopupViewSetup] m_popupRoot 필드를 찾을 수 없습니다.");
                return;
            }

            var popupRootGo = (GameObject)rootField.GetValue(popupView);
            if (popupRootGo == null)
            {
                // 없으면 자식 중 Popup_Root 검색
                var t = popupView.transform.Find("Popup_Root");
                if (t != null)
                {
                    popupRootGo = t.gameObject;
                    Undo.RecordObject(popupView, "Assign m_popupRoot");
                    rootField.SetValue(popupView, popupRootGo);
                }
            }

            if (popupRootGo == null)
            {
                Debug.LogError("[RewardPopupViewSetup] Popup_Root 오브젝트를 식별할 수 없습니다.");
                return;
            }

            Transform rootTransform = popupRootGo.transform;

            // 1. 기존의 개별 재화 텍스트 오브젝트 정리 (파괴 처리)
            string[] targetsToDestroy = {
                "ExpText", "TicketText", "PointText", "SeasonPointText", "CreditText",
                "Exp_Text", "Ticket_Text", "Point_Text", "SeasonPoint_Text", "Credit_Text",
                "expText", "ticketText", "pointText", "seasonPointText", "creditText"
            };

            for (int i = 0; i < targetsToDestroy.Length; i++)
            {
                var child = rootTransform.Find(targetsToDestroy[i]);
                if (child != null)
                {
                    Undo.DestroyObjectImmediate(child.gameObject);
                }
            }

            // 2. 획득 보상 목록 텍스트 (EarnedRewardsText) 구성
            TextMeshProUGUI earnedTMP = null;
            var earnedTransform = rootTransform.Find("EarnedRewardsText");
            if (earnedTransform == null)
            {
                var go = new GameObject("EarnedRewardsText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                go.transform.SetParent(rootTransform, false);
                earnedTMP = go.GetComponent<TextMeshProUGUI>();
                Undo.RegisterCreatedObjectUndo(go, "Create EarnedRewardsText");
            }
            else
            {
                earnedTMP = earnedTransform.GetComponent<TextMeshProUGUI>();
            }

            if (earnedTMP != null)
            {
                Undo.RecordObject(earnedTMP, "Configure EarnedRewardsText Style");
                earnedTMP.text = "★ 이번에 획득한 보상:\n- 로딩 중...";
                earnedTMP.fontSize = 18f;
                earnedTMP.color = Color.black; // 기존 황금색에서 검은색으로 변경
                earnedTMP.alignment = TextAlignmentOptions.Left;
                EditorUtility.SetDirty(earnedTMP);
            }

            // 3. 누적 보유 재화 텍스트 (CumulativeRewardsText) 구성
            TextMeshProUGUI cumulativeTMP = null;
            var cumulativeTransform = rootTransform.Find("CumulativeRewardsText");
            if (cumulativeTransform == null)
            {
                var go = new GameObject("CumulativeRewardsText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                go.transform.SetParent(rootTransform, false);
                cumulativeTMP = go.GetComponent<TextMeshProUGUI>();
                Undo.RegisterCreatedObjectUndo(go, "Create CumulativeRewardsText");
            }
            else
            {
                cumulativeTMP = cumulativeTransform.GetComponent<TextMeshProUGUI>();
            }

            if (cumulativeTMP != null)
            {
                Undo.RecordObject(cumulativeTMP, "Configure CumulativeRewardsText Style");
                cumulativeTMP.text = "★ 누적 보유 재화:\n- 로딩 중...";
                cumulativeTMP.fontSize = 16f;
                cumulativeTMP.color = Color.black; // 기존 흰색에서 검은색으로 변경
                cumulativeTMP.alignment = TextAlignmentOptions.Left;
                EditorUtility.SetDirty(cumulativeTMP);
            }

            // 4. 타이틀 텍스트 (TitleText)가 있는지 검사 및 폰트 개선
            var titleTransform = rootTransform.Find("TitleText") ?? rootTransform.Find("Title_Text");
            if (titleTransform != null)
            {
                var titleTMP = titleTransform.GetComponent<TextMeshProUGUI>();
                if (titleTMP != null)
                {
                    Undo.RecordObject(titleTMP, "Configure Title Text Style");
                    titleTMP.text = "★ 보상 획득 완료 ★";
                    titleTMP.fontSize = 22f;
                    titleTMP.color = Color.black; // 기존 시안색에서 검은색으로 변경
                    titleTMP.alignment = TextAlignmentOptions.Center;
                    EditorUtility.SetDirty(titleTMP);
                }
            }

            // 5. 닫기 버튼을 계층 구조의 가장 아래로 이동 (VerticalLayoutGroup에서 맨 아래 배치되도록 순서 보정)
            var closeTransform = rootTransform.Find("Close_Button") ?? rootTransform.Find("CloseButton");
            if (closeTransform != null)
            {
                closeTransform.SetAsLastSibling();
            }

            // 6. RewardPopupView 컴포넌트에 새 필드 주입
            Undo.RecordObject(popupView, "Assign RewardPopupView TMP fields");
            
            var earnedField = viewType.GetField("m_earnedRewardsText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (earnedField != null)
            {
                earnedField.SetValue(popupView, earnedTMP);
            }

            var cumulativeField = viewType.GetField("m_cumulativeRewardsText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (cumulativeField != null)
            {
                cumulativeField.SetValue(popupView, cumulativeTMP);
            }

            EditorUtility.SetDirty(popupView);

            Debug.Log("[RewardPopupViewSetup] 보상 팝업 레이아웃 재구성 및 변수 자동 바인딩이 성공적으로 완료되었습니다.");
        }
    }
}
