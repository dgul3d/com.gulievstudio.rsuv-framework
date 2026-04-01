using UnityEditor;

namespace RSUVFramework.Editor
{
    public sealed class RSUVSchemaGenerationWatcher : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            ProcessAssets(importedAssets);
            ProcessAssets(movedAssets);
        }

        private static void ProcessAssets(string[] assetPaths)
        {
            for (int i = 0; i < assetPaths.Length; i++)
            {
                string assetPath = assetPaths[i];
                RSUVSchema schema = AssetDatabase.LoadAssetAtPath<RSUVSchema>(assetPath);
                if (schema == null || !schema.AutoGenerateOnChange)
                {
                    continue;
                }

                try
                {
                    RSUVHlslGenerator.GenerateToDisk(schema, false);
                    RSUVCSharpGenerator.GenerateToDisk(schema);
                }
                catch
                {
                }
            }
        }
    }
}