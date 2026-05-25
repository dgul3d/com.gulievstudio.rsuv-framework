using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            string outputDirectory = RSUVBindingsGenerationUtility.NormalizeOutputDirectory(
                RSUVGenerationSettingsUtility.GetCSharpBindingsDirectory(),
                RSUVGenerationSettings.DEFAULT_CSHARP_BINDINGS_DIRECTORY);

            List<RSUVResolvedSchema> resolvedSchemas = RSUVBindingsGenerationUtility.GetResolvedSchemas();

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
            List<GeneratedBindingEntry> bindingEntries = BuildBindingEntries(resolvedSchemas);
            StringBuilder builder = new StringBuilder(4096);

            builder.AppendLine("using UnityEngine;");
            builder.AppendLine();
            builder.AppendLine($"namespace {GENERATED_NAMESPACE}");
            builder.AppendLine("{");
            builder.AppendLine($"    public static class {RSUVBindingsGenerationUtility.CSHARP_CLASS_NAME}");
            builder.AppendLine("    {");

            for (int i = 0; i < bindingEntries.Count; i++)
            {
                AppendBindingKey(builder, bindingEntries[i]);
            }

            if (bindingEntries.Count > 0)
            {
                builder.AppendLine();
            }

            for (int i = 0; i < bindingEntries.Count; i++)
            {
                AppendBindingSetter(builder, bindingEntries[i]);
            }

            builder.AppendLine("    }");
            builder.AppendLine("}");
            return builder.ToString();
        }

        private static void AppendBindingKey(StringBuilder builder, GeneratedBindingEntry bindingEntry)
        {
            string valueType = GetValueTypeName(bindingEntry.FieldType);
            builder.AppendLine($"        public static readonly RSUVFieldKey<{valueType}> {bindingEntry.MemberName} = new RSUVFieldKey<{valueType}>(\"{EscapeString(bindingEntry.FieldName)}\");");
        }

        private static void AppendBindingSetter(StringBuilder builder, GeneratedBindingEntry bindingEntry)
        {
            string valueType = GetValueTypeName(bindingEntry.FieldType);
            string setterName = GetSetterName(bindingEntry.FieldType);
            builder.AppendLine($"        public static void Set{bindingEntry.MemberName}(this RSUVRendererValueWriter writer, {valueType} value)");
            builder.AppendLine("        {");
            builder.AppendLine($"            writer.{setterName}({bindingEntry.MemberName}, value);");
            builder.AppendLine("        }");
            builder.AppendLine();
        }

        private static List<GeneratedBindingEntry> BuildBindingEntries(IReadOnlyList<RSUVResolvedSchema> resolvedSchemas)
        {
            List<FieldOccurrence> fieldOccurrences = new List<FieldOccurrence>();

            for (int schemaIndex = 0; schemaIndex < resolvedSchemas.Count; schemaIndex++)
            {
                RSUVResolvedSchema resolvedSchema = resolvedSchemas[schemaIndex];
                string prefix = RSUVSchemaUtility.SanitizeIdentifier(resolvedSchema.NamingPrefix);

                for (int fieldIndex = 0; fieldIndex < resolvedSchema.Fields.Count; fieldIndex++)
                {
                    RSUVResolvedField field = resolvedSchema.Fields[fieldIndex];
                    fieldOccurrences.Add(new FieldOccurrence(prefix, field));
                }
            }

            Dictionary<string, List<FieldOccurrence>> occurrencesByIdentifier = fieldOccurrences
                .GroupBy(occurrence => occurrence.Field.Identifier, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);

            List<GeneratedBindingEntry> bindingEntries = new List<GeneratedBindingEntry>(fieldOccurrences.Count);

            for (int i = 0; i < fieldOccurrences.Count; i++)
            {
                FieldOccurrence occurrence = fieldOccurrences[i];
                List<FieldOccurrence> sharedOccurrences = occurrencesByIdentifier[occurrence.Field.Identifier];
                bool canShareName = CanShareBindingName(sharedOccurrences);
                string memberName = canShareName
                    ? occurrence.Field.Identifier
                    : $"{occurrence.Prefix}_{occurrence.Field.Identifier}";

                if (bindingEntries.Exists(entry => string.Equals(entry.MemberName, memberName, StringComparison.Ordinal)))
                {
                    continue;
                }

                bindingEntries.Add(new GeneratedBindingEntry(memberName, occurrence.Field.Name, occurrence.Field.FieldType));
            }

            bindingEntries.Sort((left, right) => string.Compare(left.MemberName, right.MemberName, StringComparison.Ordinal));
            return bindingEntries;
        }

        private static bool CanShareBindingName(List<FieldOccurrence> sharedOccurrences)
        {
            if (sharedOccurrences.Count <= 1)
            {
                return true;
            }

            string fieldName = sharedOccurrences[0].Field.Name;
            RSUVFieldType fieldType = sharedOccurrences[0].Field.FieldType;

            for (int i = 1; i < sharedOccurrences.Count; i++)
            {
                if (!string.Equals(sharedOccurrences[i].Field.Name, fieldName, StringComparison.Ordinal) || sharedOccurrences[i].Field.FieldType != fieldType)
                {
                    return false;
                }
            }

            return true;
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

        private readonly struct FieldOccurrence
        {
            public FieldOccurrence(string prefix, RSUVResolvedField field)
            {
                Prefix = prefix;
                Field = field;
            }

            public string Prefix { get; }

            public RSUVResolvedField Field { get; }
        }

        private readonly struct GeneratedBindingEntry
        {
            public GeneratedBindingEntry(string memberName, string fieldName, RSUVFieldType fieldType)
            {
                MemberName = memberName;
                FieldName = fieldName;
                FieldType = fieldType;
            }

            public string MemberName { get; }

            public string FieldName { get; }

            public RSUVFieldType FieldType { get; }
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