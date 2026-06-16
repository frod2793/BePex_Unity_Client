using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using BePex.EventSystem.Data;

namespace BePex.EventSystem.Editor
{
    /// <summary>
    /// [기능]: 이벤트 조건(Condition) 및 보상(Reward)을 자동으로 등록하고, 관련 C# 클래스 보일러플레이트 파일을 생성하는 유니티 에디터 창.
    /// [작성자]: 윤승종
    /// </summary>
    public class EventExtensionWindow : EditorWindow
    {
        #region 지원 타입 정의
        public enum ExtensionType
        {
            Condition,
            Reward
        }
        #endregion

        #region 내부 필드 (Private Fields)
        private ExtensionType m_extensionType = ExtensionType.Condition;
        private string m_typeName = string.Empty;
        private string m_displayName = string.Empty;
        #endregion

        #region 초기화 (Initialization)
        /// <summary>
        /// [기능]: 에디터 상단 메뉴에 'Tools > BePex > 이벤트 시스템 확장 도구'를 추가하여 윈도우를 엽니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        [MenuItem("Tools/BePex/이벤트 시스템 확장 도구")]
        public static void ShowWindow()
        {
            GetWindow<EventExtensionWindow>("이벤트 확장 도구");
        }
        #endregion

        #region 유니티 생명주기 (Unity Lifecycle)
        /// <summary>
        /// [기능]: 에디터 윈도우의 GUI를 렌더링합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        private void OnGUI()
        {
            GUILayout.Label("이벤트 시스템 확장 툴", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 라벨과 인풋 필드가 겹치지 않도록 기본 가로 너비 비율 조정
            EditorGUIUtility.labelWidth = 220f;

            string[] options = new string[] { "이벤트 타입", "보상 타입" };
            m_extensionType = (ExtensionType)EditorGUILayout.Popup("확장 대상", (int)m_extensionType, options);
            m_typeName = EditorGUILayout.TextField("식별자 영문명 (예: GuildEvent)", m_typeName);
            m_displayName = EditorGUILayout.TextField("표시명 한글명 (예: 길드 이벤트)", m_displayName);

            EditorGUILayout.Space();

            if (GUILayout.Button("확장 파일 생성 및 등록"))
            {
                func_ExecuteExtension();
            }
        }
        #endregion

        #region 내부 메서드 (Private Methods)
        /// <summary>
        /// [기능]: 사용자가 입력한 필드 값을 검증한 뒤, 해당 SO의 Enum 파일 주입 및 C# 템플릿 파일 생성을 수행합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Type Object 패턴에 맞춰 조건/보상 생성 프로세스 분리
        /// </summary>
        private void func_ExecuteExtension()
        {
            if (string.IsNullOrEmpty(m_typeName) || string.IsNullOrEmpty(m_displayName))
            {
                EditorUtility.DisplayDialog("경고", "영문명과 한글 표시명을 모두 입력해주세요.", "확인");
                return;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(m_typeName, "^[A-Z][a-zA-Z0-9_]*$"))
            {
                EditorUtility.DisplayDialog("경고", "식별자 영문명은 영문 대문자로 시작하는 영숫자 형식이어야 합니다.", "확인");
                return;
            }

            if (m_extensionType == ExtensionType.Condition)
            {
                if (func_CreateConditionTypeAsset(m_typeName, m_displayName))
                {
                    func_CreateConditionClass(m_typeName, m_displayName);
                }
            }
            else
            {
                if (func_CreateRewardTypeAsset(m_typeName, m_displayName))
                {
                    func_CreateRewardClass(m_typeName, m_displayName);
                }
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// [기능]: 신규 조건 타입 SO 에셋을 생성하고 레지스트리 파일에 동적으로 등록합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        private bool func_CreateConditionTypeAsset(string typeName, string displayName)
        {
            string folderPath = "Assets/_Game/Data/ConditionTypes";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Game/Data"))
                {
                    if (!AssetDatabase.IsValidFolder("Assets/_Game"))
                    {
                        AssetDatabase.CreateFolder("Assets", "_Game");
                    }
                    AssetDatabase.CreateFolder("Assets/_Game", "Data");
                }
                AssetDatabase.CreateFolder("Assets/_Game/Data", "ConditionTypes");
            }

            string assetPath = $"{folderPath}/{typeName}.asset";
            ConditionTypeSO existAsset = AssetDatabase.LoadAssetAtPath<ConditionTypeSO>(assetPath);

            if (existAsset != null)
            {
                Debug.LogWarning($"[EventExtensionWindow] 이미 {typeName}.asset 에셋이 존재합니다. 레지스트리 등록 및 클래스 생성만 수행합니다.");
            }
            else
            {
                existAsset = CreateInstance<ConditionTypeSO>();

                var typeField = typeof(ConditionTypeSO).GetField("m_typeName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var displayField = typeof(ConditionTypeSO).GetField("m_displayName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (typeField != null)
                {
                    typeField.SetValue(existAsset, typeName);
                }
                if (displayField != null)
                {
                    displayField.SetValue(existAsset, displayName);
                }

                AssetDatabase.CreateAsset(existAsset, assetPath);
                Debug.Log($"[EventExtensionWindow] 신규 조건 타입 에셋 생성 완료: {assetPath}");
            }

            string registryPath = "Assets/_Game/Data/ConditionTypeRegistry.asset";
            ConditionTypeRegistrySO registry = AssetDatabase.LoadAssetAtPath<ConditionTypeRegistrySO>(registryPath);

            if (registry == null)
            {
                registry = CreateInstance<ConditionTypeRegistrySO>();
                AssetDatabase.CreateAsset(registry, registryPath);
                Debug.Log($"[EventExtensionWindow] 신규 ConditionTypeRegistry 에셋 생성 완료: {registryPath}");
            }

            registry.Register(existAsset);
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();

            Debug.Log($"[EventExtensionWindow] ConditionTypeRegistry에 '{typeName}' 에셋이 등록되었습니다.");
            return true;
        }

        /// <summary>
        /// [기능]: 신규 보상 타입 SO 에셋을 생성하고 레지스트리 파일에 동적으로 등록합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        private bool func_CreateRewardTypeAsset(string typeName, string displayName)
        {
            string folderPath = "Assets/_Game/Data/RewardTypes";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Game/Data"))
                {
                    if (!AssetDatabase.IsValidFolder("Assets/_Game"))
                    {
                        AssetDatabase.CreateFolder("Assets", "_Game");
                    }
                    AssetDatabase.CreateFolder("Assets/_Game", "Data");
                }
                AssetDatabase.CreateFolder("Assets/_Game/Data", "RewardTypes");
            }

            string assetPath = $"{folderPath}/{typeName}.asset";
            RewardTypeSO existAsset = AssetDatabase.LoadAssetAtPath<RewardTypeSO>(assetPath);

            if (existAsset != null)
            {
                Debug.LogWarning($"[EventExtensionWindow] 이미 {typeName}.asset 에셋이 존재합니다. 레지스트리 등록 및 클래스 생성만 수행합니다.");
            }
            else
            {
                existAsset = CreateInstance<RewardTypeSO>();

                var typeField = typeof(RewardTypeSO).GetField("m_typeName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var displayField = typeof(RewardTypeSO).GetField("m_displayName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (typeField != null)
                {
                    typeField.SetValue(existAsset, typeName);
                }
                if (displayField != null)
                {
                    displayField.SetValue(existAsset, displayName);
                }

                AssetDatabase.CreateAsset(existAsset, assetPath);
                Debug.Log($"[EventExtensionWindow] 신규 보상 타입 에셋 생성 완료: {assetPath}");
            }

            string registryPath = "Assets/_Game/Data/RewardTypeRegistry.asset";
            RewardTypeRegistrySO registry = AssetDatabase.LoadAssetAtPath<RewardTypeRegistrySO>(registryPath);

            if (registry == null)
            {
                registry = CreateInstance<RewardTypeRegistrySO>();
                AssetDatabase.CreateAsset(registry, registryPath);
                Debug.Log($"[EventExtensionWindow] 신규 RewardTypeRegistry 에셋 생성 완료: {registryPath}");
            }

            registry.Register(existAsset);
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();

            Debug.Log($"[EventExtensionWindow] RewardTypeRegistry에 '{typeName}' 에셋이 등록되었습니다.");
            return true;
        }

        /// <summary>
        /// [기능]: 새로운 QuestCondition C# 클래스 소스 코드를 생성합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Type Object 패턴에 맞춰 속성 인자가 이넘 대신 문자열 상수로 지정하도록 변경
        /// </summary>
        private void func_CreateConditionClass(string typeName, string displayName)
        {
            string folderPath = Path.Combine(Application.dataPath, "_Game/Scripts/EventSystem/Conditions");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string className = $"{typeName}QuestCondition";
            string filePath = Path.Combine(folderPath, $"{className}.cs");

            if (File.Exists(filePath))
            {
                Debug.LogWarning($"[EventExtensionWindow] 이미 파일이 존재하여 생성을 스킵합니다: {filePath}");
                return;
            }

            string code = $@"using UnityEngine;
using BePex.EventSystem.Interfaces;
using BePex.EventSystem.Data;

namespace BePex.EventSystem.Conditions
{{
    /// <summary>
    /// [기능]: {displayName}을 이벤트 완료 조건으로 달성하였는지 판정하는 Strategy 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    [QuestCondition(""{typeName}"")]
    public class {className} : BaseQuestCondition
    {{
        #region 초기화
        /// <summary>
        /// [기능]: 부모 생성자를 경유해 목표 수치, 세이브장치, 시간 제공자, 이벤트 ID 및 퀘스트 ID를 매핑받습니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public {className}(int targetValue, ISaveSystem saveSystem, ITimeProvider timeProvider, string eventId, string questId)
            : base(targetValue, saveSystem, timeProvider, eventId, questId)
        {{
        }}
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 조건 달성 가능 여부를 확인합니다. 기본적으로 참을 반환하며 필요 시 재정의합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 가상 메서드 재정의
        /// </summary>
        public override bool CanAddProgress(Models.EventProgressModel progress)
        {{
            return true;
        }}
        #endregion
    }}
}}
";
            File.WriteAllText(filePath, code);
            Debug.Log($"[EventExtensionWindow] 신규 조건 클래스가 성공적으로 생성되었습니다: {filePath}");
        }

        /// <summary>
        /// [기능]: 새로운 QuestReward C# 클래스 소스 코드를 생성합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: BaseQuestReward 및 [QuestReward] 어트리뷰트 구조로 변경
        /// </summary>
        private void func_CreateRewardClass(string typeName, string displayName)
        {
            string folderPath = Path.Combine(Application.dataPath, "_Game/Scripts/EventSystem/Rewards");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string className = $"{typeName}QuestReward";
            string filePath = Path.Combine(folderPath, $"{className}.cs");

            if (File.Exists(filePath))
            {
                Debug.LogWarning($"[EventExtensionWindow] 이미 파일이 존재하여 생성을 스킵합니다: {filePath}");
                return;
            }

            string code = $@"using BePex.EventSystem.Models;
using BePex.EventSystem.Data;

namespace BePex.EventSystem.Rewards
{{
    /// <summary>
    /// [기능]: 플레이어 자산에 이벤트 완료 보상으로 {displayName}을 부여해 주는 Strategy 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    [QuestReward(""{typeName}"")]
    public class {className} : BaseQuestReward
    {{
        #region 초기화
        /// <summary>
        /// [기능]: 부모 생성자를 경유해 지급할 보상 수량 및 표시 이름을 주입받습니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public {className}(int amount, string displayName)
            : base(amount, displayName)
        {{
        }}
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 플레이어의 누적 자산에 {displayName} 보상 수량을 더해 지급합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최신 AddCurrency API 활용 가이드 보완
        /// </summary>
        public override void Grant(PlayerRewardModel playerReward)
        {{
            if (playerReward != null)
            {{
                playerReward.AddCurrency(""{typeName}"", m_amount);
            }}
        }}
        #endregion
    }}
}}
";
            File.WriteAllText(filePath, code);
            Debug.Log($"[EventExtensionWindow] 신규 보상 클래스가 성공적으로 생성되었습니다: {filePath}");
        }
        #endregion
    }
}
