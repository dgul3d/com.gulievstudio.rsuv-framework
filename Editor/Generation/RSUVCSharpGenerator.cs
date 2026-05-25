using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;

namespace RSUVFramework.Editor
{
    public static class RSUVCSharpGenerator
    {
        private const string GENERATED_NAMESPACE = "RSUVFramework";

        public static string GenerateToDisk(RSUVSchema schema, bool refreshAssetDatabase = true)
        {
            if (!RSUVSchemaUtility.TryResolve(schema, out RSUVResolvedSchema resolvedSchema, out string errorMessage))
            {
                throw new InvalidOperationException(errorMessage);
            }

            string outputDirectory = RSUVBindingsGenerationUtility.NormalizeOutputDirectory(schema.GeneratedBindingsDirectory);
            List<RSUVResolvedSchema> resolvedSchemas = RSUVBindingsGenerationUtility.GetResolvedSchemasForDirectory(outputDirectory);

            string fileName = RSUVBindingsGenerationUtility.CSHARP_FILE_NAME;
            string assetPath = $"{outputDirectory.TrimEnd('/')}/{fileName}";
            string absolutePath = Path.GetFullPath(assetPath);
            string directoryPath = Path.GetDirectoryName(absolutePath);

            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string generatedText = BuildCSharp(resolvedSchemas);
            WriteIfChanged(absolutePath, generatedText);

            if (refreshAssetDatabase)
            {
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }

            return assetPath;
        }

        public static string BuildCSharp(IReadOnlyList<RSUVResolvedSchema> resolvedSchemas)
        {
            StringBuilder builder = new StringBuilder(4096);

            builder.AppendLine("using UnityEngine;");
            builder.AppendLine();
            builder.AppendLine($"namespace {GENERATED_NAMESPACE}");
            builder.AppendLine("{");
            builder.AppendLine($"    public static class {RSUVBindingsGenerationUtility.CSHARP_CLASS_NAME}");
            builder.AppendLine("    {");

            for (int schemaIndex = 0; schemaIndex < resolvedSchemas.Count; schemaIndex++)
            {
                AppendSchemaKeys(builder, resolvedSchemas[schemaIndex]);
            }

            if (resolvedSchemas.Count > 0)
            {
                builder.AppendLine();
            }

            for (int schemaIndex = 0; schemaIndex < resolvedSchemas.Count; schemaIndex++)
            {
                AppendSchemaSetters(builder, resolvedSchemas[schemaIndex]);
            }

            builder.AppendLine("    }");
            builder.AppendLine("}");
            return builder.ToString();
        }

        private static void AppendSchemaKeys(StringBuilder builder, RSUVResolvedSchema resolvedSchema)
        {
            string prefix = RSUVSchemaUtility.SanitizeIdentifier(resolvedSchema.NamingPrefix);

            for (int i = 0; i < resolvedSchema.Fields.Count; i++)
            {
                RSUVResolvedField field = resolvedSchema.Fields[i];
                string valueType = GetValueTypeName(field.FieldType);
                string memberName = $"{prefix}_{field.Identifier}";
                builder.AppendLine($"        public static readonly RSUVFieldKey<{valueType}> {memberName} = new RSUVFieldKey<{valueType}>(\"{EscapeString(field.Name)}\");");
            }
        }

        private static void AppendSchemaSetters(StringBuilder builder, RSUVResolvedSchema resolvedSchema)
        {
            string prefix = RSUVSchemaUtility.SanitizeIdentifier(resolvedSchema.NamingPrefix);

            for (int i = 0; i < resolvedSchema.Fields.Count; i++)
            {
                RSUVResolvedField field = resolvedSchema.Fields[i];
                string valueType = GetValueTypeName(field.FieldType);
                string setterName = GetSetterName(field.FieldType);
                string memberName = $"{prefix}_{field.Identifier}";
                builder.AppendLine($"        public static void Set{memberName}(this RSUVRendererValueWriter writer, {valueType} value)");
                builder.AppendLine("        {");
                builder.AppendLine($"            writer.{setterName}({memberName}, value);");
                builder.AppendLine("        }");
                builder.AppendLine();
            }
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