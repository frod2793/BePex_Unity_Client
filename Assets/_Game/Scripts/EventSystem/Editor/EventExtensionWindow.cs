using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

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
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        private void func_ExecuteExtension()
        {
            if (string.IsNullOrEmpty(m_typeName) || string.IsNullOrEmpty(m_displayName))
            {
                EditorUtility.DisplayDialog("경고", "영문명과 한글 표시명을 모두 입력해주세요.", "확인");
                return;
            }

            if (!Regex.IsMatch(m_typeName, "^[A-Z][a-zA-Z0-9_]*$"))
            {
                EditorUtility.DisplayDialog("경고", "식별자 영문명은 영문 대문자로 시작하는 영숫자 형식이어야 합니다.", "확인");
                return;
            }

            if (m_extensionType == ExtensionType.Condition)
            {
                if (func_TryRegisterEnum("ConditionDefinitionSO", "ConditionType", m_typeName, m_displayName))
                {
                    func_CreateConditionClass(m_typeName, m_displayName);
                }
            }
            else
            {
                if (func_TryRegisterEnum("RewardDefinitionSO", "RewardType", m_typeName, m_displayName))
                {
                    func_CreateRewardClass(m_typeName, m_displayName);
                }
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// [기능]: 대상 ScriptableObject 스크립트 파일을 찾아 지정한 Enum에 새 원소를 삽입합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        private bool func_TryRegisterEnum(string filename, string enumName, string typeName, string displayName)
        {
            string[] guids = AssetDatabase.FindAssets($"{filename} t:MonoScript");
            if (guids.Length == 0)
            {
                Debug.LogError($"[EventExtensionWindow] {filename}.cs 파일을 프로젝트 내에서 찾을 수 없습니다.");
                EditorUtility.DisplayDialog("오류", $"{filename}.cs 파일을 찾을 수 없습니다.", "확인");
                return false;
            }

            string filePath = AssetDatabase.GUIDToAssetPath(guids[0]);
            string fullPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, filePath);

            if (!File.Exists(fullPath))
            {
                Debug.LogError($"[EventExtensionWindow] 파일 경로를 찾을 수 없습니다: {fullPath}");
                return false;
            }

            string fileContent = File.ReadAllText(fullPath);
            string enumPattern = $@"public enum {enumName}\s*\{{([\s\S]*?)\}}";
            Match match = Regex.Match(fileContent, enumPattern);

            if (!match.Success)
            {
                Debug.LogError($"[EventExtensionWindow] {filename}.cs 파일에서 {enumName} 정의를 찾을 수 없습니다.");
                return false;
            }

            string enumBody = match.Groups[1].Value;
            string trimmedBody = enumBody.TrimEnd();

            if (trimmedBody.Contains(typeName))
            {
                Debug.LogWarning($"[EventExtensionWindow] 이미 {typeName} 원소가 {enumName} 에 존재합니다. 클래스 생성만 시도합니다.");
                return true;
            }

            string separator = string.Empty;
            if (!trimmedBody.EndsWith(","))
            {
                separator = ",";
            }

            string newElement = $"{separator}\n            [EventDisplayName(\"{displayName}\")]\n            {typeName}\n        ";
            int insertPos = enumBody.LastIndexOf(trimmedBody) + trimmedBody.Length;
            string newEnumBody = enumBody.Insert(insertPos, newElement);

            string originalEnumBlock = match.Value;
            string newEnumBlock = $"public enum {enumName}\n        {{{newEnumBody}}}";
            
            // 본문 치환 후 파일 저장
            fileContent = fileContent.Replace(originalEnumBlock, newEnumBlock);
            File.WriteAllText(fullPath, fileContent);

            Debug.Log($"[EventExtensionWindow] {filename}.cs의 {enumName}에 '{typeName}({displayName})'이(가) 추가되었습니다.");
            return true;
        }

        /// <summary>
        /// [기능]: 새로운 Condition C# 클래스 소스 코드를 생성합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        private void func_CreateConditionClass(string typeName, string displayName)
        {
            string folderPath = Path.Combine(Application.dataPath, "_Game/Scripts/EventSystem/Conditions");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string className = $"{typeName}Condition";
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
    [EventCondition(ConditionDefinitionSO.ConditionType.{typeName})]
    public class {className} : BaseEventCondition
    {{
        #region 초기화
        /// <summary>
        /// [기능]: 부모 생성자를 경유해 목표 수치, 세이브장치 및 해당 이벤트 ID를 매핑받습니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public {className}(int targetValue, ISaveSystem saveSystem, string eventId)
            : base(targetValue, saveSystem, eventId)
        {{
        }}
        #endregion
    }}
}}
";
            File.WriteAllText(filePath, code);
            Debug.Log($"[EventExtensionWindow] 신규 조건 클래스가 성공적으로 생성되었습니다: {filePath}");
        }

        /// <summary>
        /// [기능]: 새로운 Reward C# 클래스 소스 코드를 생성합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        private void func_CreateRewardClass(string typeName, string displayName)
        {
            string folderPath = Path.Combine(Application.dataPath, "_Game/Scripts/EventSystem/Rewards");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string className = $"{typeName}Reward";
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
    [EventReward(RewardDefinitionSO.RewardType.{typeName})]
    public class {className} : BaseEventReward
    {{
        #region 초기화
        /// <summary>
        /// [기능]: 부모 생성자를 경유해 지급할 보상 수량 및 표시 이름을 주입받습니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-15
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
        /// [수정 날짜]: 2026-06-15
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public override void Grant(PlayerRewardModel playerReward)
        {{
            if (playerReward != null)
            {{
                // TODO: 플레이어 자산 모델에 {displayName} 지급하는 비즈니스 로직 작성 필요
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
