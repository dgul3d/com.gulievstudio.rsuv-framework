using System;
using System.Collections.Generic;
using UnityEditor;

namespace RSUVFramework.Editor
{
    internal static class RSUVBindingsGenerationUtility
    {
        public const string DEFAULT_OUTPUT_DIRECTORY = "Assets/RSUVFramework/Generated";
        public const string CSHARP_FILE_NAME = "RSUVBindings.cs";
        public const string HLSL_FILE_NAME = "RSUVBindings.hlsl";
        public const string CSHARP_CLASS_NAME = "RSUVBindings";

        public static string NormalizeOutputDirectory(string outputDirectory)
        {
            string normalizedDirectory = string.IsNullOrWhiteSpace(outputDirectory)
                ? DEFAULT_OUTPUT_DIRECTORY
                : outputDirectory.Replace('\\', '/');

            return normalizedDirectory.TrimEnd('/');
        }

        public static List<RSUVResolvedSchema> GetResolvedSchemasForDirectory(string outputDirectory)
        {
            string normalizedDirectory = NormalizeOutputDirectory(outputDirectory);
            List<(string AssetPath, RSUVResolvedSchema Schema)> schemas = new List<(string AssetPath, RSUVResolvedSchema Schema)>();
            string[] schemaGuids = AssetDatabase.FindAssets("t:RSUVSchema");

            for (int i = 0; i < schemaGuids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(schemaGuids[i]);
                RSUVSchema schema = AssetDatabase.LoadAssetAtPath<RSUVSchema>(assetPath);
                if (schema == null)
                {
                    continue;
                }

                string schemaOutputDirectory = NormalizeOutputDirectory(schema.GeneratedBindingsDirectory);
                if (!string.Equals(schemaOutputDirectory, normalizedDirectory, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!RSUVSchemaUtility.TryResolve(schema, out RSUVResolvedSchema resolvedSchema, out string errorMessage))
                {
                    throw new InvalidOperationException($"Schema '{schema.name}' cannot be added to shared bindings: {errorMessage}");
                }

                schemas.Add((assetPath, resolvedSchema));
            }

            schemas.Sort((left, right) => string.Compare(left.AssetPath, right.AssetPath, StringComparison.OrdinalIgnoreCase));

            List<RSUVResolvedSchema> resolvedSchemas = new List<RSUVResolvedSchema>(schemas.Count);
            for (int i = 0; i < schemas.Count; i++)
            {
                resolvedSchemas.Add(schemas[i].Schema);
            }

            return resolvedSchemas;
        }
    }
}