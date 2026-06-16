#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace BePex.EventSystem.Editor
{
    /// <summary>
    /// [기능]: 어드민 씬과 인게임 씬을 독립 standalone 바이너리로 분리 빌드하고, 세이브 경로(persistentDataPath) 및 어드레서블 자산 호환을 자동 제어하는 빌드 자동화 스크립트.
    /// [작성자]: 윤승종
    /// </summary>
    public static class EditorBuildScript
    {
        private const string COMPANY_NAME = "BePex";
        private const string PRODUCT_NAME = "BePexEventClient"; // 세이브 폴더 공유를 위해 동일하게 설정

        /// <summary>
        /// [기능]: 어드민 씬(EventAdminScene)만 포함하는 독립 실행형 standalone 빌드를 수행합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        [MenuItem("BePex/Build/Build Admin Standalone")]
        public static void func_BuildAdminOnly()
        {
            SetupSharedPlayerSettings();
            string[] scenes = { "Assets/_Game/Scenes/EventAdminScene.unity" };
            string buildPath = GetBuildPath("Admin");
            
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = EditorUserBuildSettings.activeBuildTarget,
                options = BuildOptions.None
            };

            BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log($"[EditorBuildScript] 어드민 단독 빌드 완료: {buildPath}");
        }

        /// <summary>
        /// [기능]: 어드레서블 에셋을 선제 자동 리빌드한 후 인게임 씬(SampleScene)만 포함하는 독립 빌드를 수행합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        [MenuItem("BePex/Build/Build Ingame Standalone")]
        public static void func_BuildIngameOnly()
        {
            SetupSharedPlayerSettings();
            
            // 1. 인게임 빌드 전 어드레서블 자산 자동 리빌드 연동
            RebuildAddressables();

            string[] scenes = { "Assets/_Game/Scenes/SampleScene.unity" };
            string buildPath = GetBuildPath("Ingame");
            
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = EditorUserBuildSettings.activeBuildTarget,
                options = BuildOptions.None
            };

            BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log($"[EditorBuildScript] 인게임 단독 빌드 완료: {buildPath}");
        }

        /// <summary>
        /// [기능]: 어드민 씬 빌드와 인게임 씬 빌드를 순차적으로 모두 실행하여 일괄 바이너리를 추출합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        [MenuItem("BePex/Build/Build All Standalone")]
        public static void func_BuildAll()
        {
            func_BuildAdminOnly();
            func_BuildIngameOnly();
            Debug.Log("[EditorBuildScript] 전체 빌드 프로세스가 정상적으로 종료되었습니다.");
        }

        /// <summary>
        /// [기능]: 로컬 세이브 데이터 파일 디렉토리가 호환되도록 회사 및 제품명을 세팅합니다.
        /// [작성자]: 윤승종
        /// </summary>
        private static void SetupSharedPlayerSettings()
        {
            PlayerSettings.companyName = COMPANY_NAME;
            PlayerSettings.productName = PRODUCT_NAME; // 세이브 파일 디렉토리 호환 보장
        }

        /// <summary>
        /// [기능]: 리플렉션을 통해 어드레서블 빌드 스크립트를 찾아 실행하여 에셋 자산을 최신 상태로 번들링합니다.
        /// [작성자]: 윤승종
        /// </summary>
        private static void RebuildAddressables()
        {
            var settingsType = System.Type.GetType("UnityEditor.AddressableAssets.Settings.AddressableAssetSettings, Unity.Addressables.Editor");
            var defaultObjectType = System.Type.GetType("UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject, Unity.Addressables.Editor");
            
            if (settingsType != null && defaultObjectType != null)
            {
                var settingsProperty = defaultObjectType.GetProperty("Settings", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                var settings = settingsProperty?.GetValue(null);
                var buildMethod = settingsType.GetMethod("BuildPlayerContent", new System.Type[] { settingsType });
                
                if (buildMethod != null && settings != null)
                {
                    buildMethod.Invoke(null, new object[] { settings });
                    Debug.Log("[EditorBuildScript] Addressables 빌드 자산이 강제 갱신 및 내장되었습니다.");
                }
            }
        }

        /// <summary>
        /// [기능]: 빌드 타겟 플랫폼에 따라 하위 디렉토리 및 확장자를 맵핑하여 빌드 대상 절대 경로를 반환합니다.
        /// [작성자]: 윤승종
        /// </summary>
        private static string GetBuildPath(string subDir)
        {
            string extension = "";
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            if (target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64)
            {
                extension = ".exe";
            }
            else if (target == BuildTarget.StandaloneOSX)
            {
                extension = ".app";
            }

            return Path.Combine("Builds", subDir, $"{subDir}{extension}");
        }
    }
}
#endif
