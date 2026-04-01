using System;
using System.IO;
using System.Text;
using UnityEditor;

namespace RSUVFramework.Editor
{
    public static class RSUVCSharpGenerator
    {
        private const string GENERATED_NAMESPACE = "RSUVFramework.Generated";
        private const string DEFAULT_OUTPUT_DIRECTORY = "Assets/RSUVFramework/Generated";

        public static string GenerateToDisk(RSUVSchema schema, bool refreshAssetDatabase = true)
        {
            if (!RSUVSchemaUtility.TryResolve(schema, out RSUVResolvedSchema resolvedSchema, out string errorMessage))
            {
                throw new InvalidOperationException(errorMessage);
            }

            string outputDirectory = string.IsNullOrWhiteSpace(schema.GeneratedBindingsDirectory)
                ? DEFAULT_OUTPUT_DIRECTORY
                : schema.GeneratedBindingsDirectory.Replace('\\', '/');

            string fileName = $"{RSUVSchemaUtility.SanitizeIdentifier(schema.name)}.generated.cs";
            string assetPath = $"{outputDirectory.TrimEnd('/')}/{fileName}";
            string absolutePath = Path.GetFullPath(assetPath);
            string directoryPath = Path.GetDirectoryName(absolutePath);

            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string generatedText = BuildCSharp(resolvedSchema);
            WriteIfChanged(absolutePath, generatedText);

            if (refreshAssetDatabase)
            {
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }

            return assetPath;
        }

        public static string BuildCSharp(RSUVResolvedSchema resolvedSchema)
        {
            string className = $"{RSUVSchemaUtility.SanitizeIdentifier(resolvedSchema.SchemaName)}Api";
            StringBuilder builder = new StringBuilder(4096);

            builder.AppendLine("using UnityEngine;");
            builder.AppendLine();
            builder.AppendLine($"namespace {GENERATED_NAMESPACE}");
            builder.AppendLine("{");
            builder.AppendLine($"    public static class {className}");
            builder.AppendLine("    {");

            for (int i = 0; i < resolvedSchema.Fields.Count; i++)
            {
                RSUVResolvedField field = resolvedSchema.Fields[i];
                string valueType = GetValueTypeName(field.FieldType);
                builder.AppendLine($"        public static readonly RSUVFieldKey<{valueType}> {field.Identifier} = new RSUVFieldKey<{valueType}>(\"{EscapeString(field.Name)}\");");
            }

            if (resolvedSchema.Fields.Count > 0)
            {
                builder.AppendLine();
            }

            for (int i = 0; i < resolvedSchema.Fields.Count; i++)
            {
                RSUVResolvedField field = resolvedSchema.Fields[i];
                string valueType = GetValueTypeName(field.FieldType);
                string setterName = GetSetterName(field.FieldType);
                builder.AppendLine($"        public static void Set{field.Identifier}(this RSUVRendererValueWriter writer, {valueType} value)");
                builder.AppendLine("        {");
                builder.AppendLine($"            writer.{setterName}({field.Identifier}, value);");
                builder.AppendLine("        }");
                builder.AppendLine();
            }

            builder.AppendLine("    }");
            builder.AppendLine("}");
            return builder.ToString();
        }

        private static string GetSetterName(RSUVFieldType fieldType)
        {
            switch (fieldType)
            {
                case RSUVFieldType.Bool:
                    return "SetBool";

                case RSUVFieldType.Int:
                    return "SetInt";

                case RSUVFieldType.Float:
                    return "SetFloat";

                case RSUVFieldType.Color:
                    return "SetColor";

                default:
                    throw new ArgumentOutOfRangeException(nameof(fieldType), fieldType, null);
            }
        }

        private static string GetValueTypeName(RSUVFieldType fieldType)
        {
            switch (fieldType)
            {
                case RSUVFieldType.Bool:
                    return "bool";

                case RSUVFieldType.Int:
                    return "int";

                case RSUVFieldType.Float:
                    return "float";

                case RSUVFieldType.Color:
                    return "Color";

                default:
                    throw new ArgumentOutOfRangeException(nameof(fieldType), fieldType, null);
            }
        }

        private static string EscapeString(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private static void WriteIfChanged(string absolutePath, string content)
        {
            if (File.Exists(absolutePath))
            {
                string existingContent = File.ReadAllText(absolutePath);
                if (string.Equals(existingContent, content, StringComparison.Ordinal))
                {
                    return;
                }
            }

            File.WriteAllText(absolutePath, content, Encoding.UTF8);
        }
    }
}