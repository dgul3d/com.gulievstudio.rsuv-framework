using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace RSUVFramework.Editor
{
    public static class RSUVHlslGenerator
    {
        private const string CORE_INCLUDE_PATH = "Packages/com.gulievstudio.rsuv-framework/ShaderLibrary/RSUVCore.hlsl";
        private const string DEFAULT_OUTPUT_DIRECTORY = "Assets/RSUVFramework/Generated";

        public static string GenerateToDisk(RSUVSchema schema, bool refreshAssetDatabase = true)
        {
            if (!RSUVSchemaUtility.TryResolve(schema, out RSUVResolvedSchema resolvedSchema, out string errorMessage))
            {
                throw new InvalidOperationException(errorMessage);
            }

            string outputDirectory = string.IsNullOrWhiteSpace(schema.GeneratedHlslBindingsDirectory)
                ? DEFAULT_OUTPUT_DIRECTORY
                : schema.GeneratedHlslBindingsDirectory.Replace('\\', '/');

            string fileName = $"{RSUVSchemaUtility.SanitizeIdentifier(schema.name)}.generated.hlsl";
            string assetPath = $"{outputDirectory.TrimEnd('/')}/{fileName}";
            string absolutePath = Path.GetFullPath(assetPath);
            string directoryPath = Path.GetDirectoryName(absolutePath);

            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string generatedText = BuildHlsl(resolvedSchema);
            WriteIfChanged(absolutePath, generatedText);

            if (refreshAssetDatabase)
            {
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }

            return assetPath;
        }

        public static string BuildHlsl(RSUVResolvedSchema resolvedSchema)
        {
            string prefix = RSUVSchemaUtility.SanitizeIdentifier(resolvedSchema.ShaderSymbolPrefix);
            string includeGuard = $"{prefix.ToUpperInvariant()}_GENERATED_INCLUDED";
            StringBuilder builder = new StringBuilder(4096);

            builder.AppendLine($"#ifndef {includeGuard}");
            builder.AppendLine($"#define {includeGuard}");
            builder.AppendLine();
            builder.AppendLine($"#include \"{CORE_INCLUDE_PATH}\"");
            builder.AppendLine();

            for (int i = 0; i < resolvedSchema.Fields.Count; i++)
            {
                RSUVResolvedField field = resolvedSchema.Fields[i];
                string constantPrefix = $"{prefix}_{field.Identifier}";
                builder.AppendLine($"static const uint {constantPrefix}_OFFSET = {field.BitOffset}u;");
                builder.AppendLine($"static const uint {constantPrefix}_BITS = {field.BitCount}u;");

                if (field.FieldType == RSUVFieldType.Int)
                {
                    builder.AppendLine($"static const int {constantPrefix}_MIN_INT = {field.MinimumIntegerValue};");
                    builder.AppendLine($"static const int {constantPrefix}_MAX_INT = {field.MaximumIntegerValue};");
                }

                if (field.FieldType == RSUVFieldType.Float)
                {
                    builder.AppendLine($"static const float {constantPrefix}_MIN_FLOAT = {FormatFloat(field.MinimumFloatValue)};");
                    builder.AppendLine($"static const float {constantPrefix}_MAX_FLOAT = {FormatFloat(field.MaximumFloatValue)};");
                }

                builder.AppendLine();
                builder.AppendLine($"uint {prefix}_Get{field.Identifier}RawFromData(uint data)");
                builder.AppendLine("{");
                builder.AppendLine($"    return RSUV_GetBits(data, {constantPrefix}_OFFSET, {constantPrefix}_BITS);");
                builder.AppendLine("}");
                builder.AppendLine();
                builder.AppendLine($"uint {prefix}_Get{field.Identifier}Raw()");
                builder.AppendLine("{");
                builder.AppendLine($"    return {prefix}_Get{field.Identifier}RawFromData(RSUV_GetData());");
                builder.AppendLine("}");

                AppendSemanticFunctions(builder, prefix, field, constantPrefix);
                builder.AppendLine();
            }

            builder.AppendLine($"#endif // {includeGuard}");
            return builder.ToString();
        }

        private static void AppendSemanticFunctions(StringBuilder builder, string prefix, RSUVResolvedField field, string constantPrefix)
        {
            switch (field.FieldType)
            {
                case RSUVFieldType.Bool:
                    builder.AppendLine();
                    builder.AppendLine($"bool {prefix}_Get{field.Identifier}FromData(uint data)");
                    builder.AppendLine("{");
                    builder.AppendLine($"    return RSUV_DecodeBool(data, {constantPrefix}_OFFSET);");
                    builder.AppendLine("}");
                    builder.AppendLine();
                    builder.AppendLine($"bool {prefix}_Get{field.Identifier}()");
                    builder.AppendLine("{");
                    builder.AppendLine($"    return {prefix}_Get{field.Identifier}FromData(RSUV_GetData());");
                    builder.AppendLine("}");
                    builder.AppendLine();
                    builder.AppendLine($"float {prefix}_Get{field.Identifier}AsFloat()");
                    builder.AppendLine("{");
                    builder.AppendLine($"    return {prefix}_Get{field.Identifier}() ? 1.0f : 0.0f;");
                    builder.AppendLine("}");
                    builder.AppendLine();
                    builder.AppendLine($"void {prefix}_Get{field.Identifier}_float(out float Value)");
                    builder.AppendLine("{");
                    builder.AppendLine($"    Value = {prefix}_Get{field.Identifier}AsFloat();");
                    builder.AppendLine("}");
                    builder.AppendLine();
                    builder.AppendLine($"void {prefix}_Get{field.Identifier}_half(out half Value)");
                    builder.AppendLine("{");
                    builder.AppendLine($"    Value = {prefix}_Get{field.Identifier}() ? (half)1.0h : (half)0.0h;");
                    builder.AppendLine("}");
                    break;

                case RSUVFieldType.Float:
                    builder.AppendLine();
                    builder.AppendLine($"float {prefix}_Get{field.Identifier}FromData(uint data)");
                    builder.AppendLine("{");
                    builder.AppendLine($"    return RSUV_DecodeFloat(data, {constantPrefix}_OFFSET, {constantPrefix}_BITS, {constantPrefix}_MIN_FLOAT, {constantPrefix}_MAX_FLOAT);");
                    builder.AppendLine("}");
                    builder.AppendLine();
                    builder.AppendLine($"float {prefix}_Get{field.Identifier}()");
                    builder.AppendLine("{");
                    builder.AppendLine($"    return {prefix}_Get{field.Identifier}FromData(RSUV_GetData());");
                    builder.AppendLine("}");
                    builder.AppendLine();
                    builder.AppendLine($"void {prefix}_Get{field.Identifier}_float(out float Value)");
                    builder.AppendLine("{");
                    builder.AppendLine($"    Value = {prefix}_Get{field.Identifier}();");
                    builder.AppendLine("}");
                    builder.AppendLine();
                    builder.AppendLine($"void {prefix}_Get{field.Identifier}_half(out half Value)");
                    builder.AppendLine("{");
                    builder.AppendLine($"    Value = (half){prefix}_Get{field.Identifier}();");
                    builder.AppendLine("}");
                    break;

                case RSUVFieldType.Int:
                    builder.AppendLine();
                    builder.AppendLine($"int {prefix}_Get{field.Identifier}FromData(uint data)");
                    builder.AppendLine("{");
                    builder.AppendLine($"    return RSUV_DecodeInt(data, {constantPrefix}_OFFSET, {constantPrefix}_BITS, {constantPrefix}_MIN_INT);");
                    builder.AppendLine("}");
                    builder.AppendLine();
                    builder.AppendLine($"int {prefix}_Get{field.Identifier}()");
                    builder.AppendLine("{");
                    builder.AppendLine($"    return {prefix}_Get{field.Identifier}FromData(RSUV_GetData());");
                    builder.AppendLine("}");
                    builder.AppendLine();
                    builder.AppendLine($"void {prefix}_Get{field.Identifier}_float(out float Value)");
                    builder.AppendLine("{");
                    builder.AppendLine($"    Value = (float){prefix}_Get{field.Identifier}();");
                    builder.AppendLine("}");
                    builder.AppendLine();
                    builder.AppendLine($"void {prefix}_Get{field.Identifier}_half(out half Value)");
                    builder.AppendLine("{");
                    builder.AppendLine($"    Value = (half){prefix}_Get{field.Identifier}();");
                    builder.AppendLine("}");
                    break;

                case RSUVFieldType.Color:
                    builder.AppendLine();
                    builder.AppendLine($"float4 {prefix}_Get{field.Identifier}FromData(uint data)");
                    builder.AppendLine("{");
                    builder.AppendLine($"    return RSUV_DecodeColor(data, {constantPrefix}_OFFSET, {constantPrefix}_BITS);");
                    builder.AppendLine("}");
                    builder.AppendLine();
                    builder.AppendLine($"float4 {prefix}_Get{field.Identifier}()");
                    builder.AppendLine("{");
                    builder.AppendLine($"    return {prefix}_Get{field.Identifier}FromData(RSUV_GetData());");
                    builder.AppendLine("}");
                    builder.AppendLine();
                    builder.AppendLine($"void {prefix}_Get{field.Identifier}_float(out float4 Value)");
                    builder.AppendLine("{");
                    builder.AppendLine($"    Value = {prefix}_Get{field.Identifier}();");
                    builder.AppendLine("}");
                    builder.AppendLine();
                    builder.AppendLine($"void {prefix}_Get{field.Identifier}_half(out half4 Value)");
                    builder.AppendLine("{");
                    builder.AppendLine($"    Value = (half4){prefix}_Get{field.Identifier}();");
                    builder.AppendLine("}");
                    break;
            }
        }

        private static string FormatFloat(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                return "0.0f";
            }

            return value.ToString("0.0###############", System.Globalization.CultureInfo.InvariantCulture) + "f";
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