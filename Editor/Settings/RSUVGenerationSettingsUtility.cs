using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RSUVFramework.Editor
{
    internal static class RSUVGenerationSettingsUtility
    {
        private const string DEFAULT_ASSET_PATH = "Assets/Settings/RSUVGenerationSettings.asset";

        public static bool TryGetSettings(out RSUVGenerationSettings settings)
        {
            settings = AssetDatabase.LoadAssetAtPath<RSUVGenerationSettings>(DEFAULT_ASSET_PATH);
            if (settings != null)
            {
                return true;
            }

            string[] guids = AssetDatabase.FindAssets("t:RSUVGenerationSettings");
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                settings = AssetDatabase.LoadAssetAtPath<RSUVGenerationSettings>(assetPath);
                if (settings != null)
                {
                    return true;
                }
            }

            settings = null;
            return false;
        }

        public static string GetHlslBindingsDirectory()
        {
            if (!TryGetSettings(out RSUVGenerationSettings settings))
            {
                return RSUVGenerationSettings.DEFAULT_HLSL_BINDINGS_DIRECTORY;
            }

            return string.IsNullOrWhiteSpace(settings.HlslBindingsDirectory)
                ? RSUVGenerationSettings.DEFAULT_HLSL_BINDINGS_DIRECTORY
                : settings.HlslBindingsDirectory;
        }

        public static string GetCSharpBindingsDirectory()
        {
            if (!TryGetSettings(out RSUVGenerationSettings settings))
            {
                return RSUVGenerationSettings.DEFAULT_CSHARP_BINDINGS_DIRECTORY;
            }

            return string.IsNullOrWhiteSpace(settings.CSharpBindingsDirectory)
                ? RSUVGenerationSettings.DEFAULT_CSHARP_BINDINGS_DIRECTORY
                : settings.CSharpBindingsDirectory;
        }

        [MenuItem("Tools/Graphics/RSUV/Create Generation Settings Asset")]
        public static void CreateOrSelectSettingsAsset()
        {
            if (TryGetSettings(out RSUVGenerationSettings existingSettings))
            {
                Selection.activeObject = existingSettings;
                EditorGUIUtility.PingObject(existingSettings);
                return;
            }

            string directoryPath = Path.GetDirectoryName(DEFAULT_ASSET_PATH);
            if (!string.IsNullOrWhiteSpace(directoryPath) && !AssetDatabase.IsValidFolder(directoryPath))
            {
                CreateFolderRecursively(directoryPath);
            }

            RSUVGenerationSettings settings = ScriptableObject.CreateInstance<RSUVGenerationSettings>();
            AssetDatabase.CreateAsset(settings, DEFAULT_ASSET_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = settings;
            EditorGUIUtility.PingObject(settings);
        }

        private static void CreateFolderRecursively(string folderPath)
        {
            string normalizedFolderPath = folderPath.Replace('\\', '/');
            string[] segments = normalizedFolderPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string currentPath = segments[0];

            for (int i = 1; i < segments.Length; i++)
            {
                string nextPath = $"{currentPath}/{segments[i]}";
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, segments[i]);
                }

                currentPath = nextPath;
            }
        }
    }
}