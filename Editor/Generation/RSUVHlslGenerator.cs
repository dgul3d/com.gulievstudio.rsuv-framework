using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace RSUVFramework.Editor
{
    public static class RSUVHlslGenerator
    {
        private const string CORE_INCLUDE_PATH = "Packages/com.gulievstudio.rsuv-framework/ShaderLibrary/RSUVCore.hlsl";
        private const string INCLUDE_GUARD = "RSUV_BINDINGS_INCLUDED";

        public static string GenerateToDisk(RSUVSchema schema, bool refreshAssetDatabase = true)
        {
            if (!RSUVSchemaUtility.TryResolve(schema, out RSUVResolvedSchema resolvedSchema, out string errorMessage))
            {
                throw new InvalidOperationException(errorMessage);
            }

            string outputDirectory = RSUVBindingsGenerationUtility.NormalizeOutputDirectory(
                RSUVGenerationSettingsUtility.GetHlslBindingsDirectory(),
                RSUVGenerationSettings.DEFAULT_HLSL_BINDINGS_DIRECTORY);

            List<RSUVResolvedSchema> resolvedSchemas = RSUVBindingsGenerationUtility.GetResolvedSchemas();

            string fileName = RSUVBindingsGenerationUtility.HLSL_FILE_NAME;
            string assetPath = $"{outputDirectory.TrimEnd('/')}/{fileName}";
            string absolutePath = Path.GetFullPath(assetPath);
            string directoryPath = Path.GetDirectoryName(absolutePath);

            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string generatedText = BuildHlsl(resolvedSchemas);
            WriteIfChanged(absolutePath, generatedText);

            if (refreshAssetDatabase)
            {
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }

            return assetPath;
        }

        public static string BuildHlsl(IReadOnlyList<RSUVResolvedSchema> resolvedSchemas)
        {
            StringBuilder builder = new StringBuilder(4096);

            builder.AppendLine($"#ifndef {INCLUDE_GUARD}");
            builder.AppendLine($"#define {INCLUDE_GUARD}");
            builder.AppendLine();
            builder.AppendLine($"#include \"{CORE_INCLUDE_PATH}\"");
            builder.AppendLine();

            for (int schemaIndex = 0; schemaIndex < resolvedSchemas.Count; schemaIndex++)
            {
                AppendSchema(builder, resolvedSchemas[schemaIndex]);

                if (schemaIndex < (resolvedSchemas.Count - 1))
                {
                    builder.AppendLine();
                }
            }

            builder.AppendLine($"#endif // {INCLUDE_GUARD}");
            return builder.ToString();
        }

        private static void AppendSchema(StringBuilder builder, RSUVResolvedSchema resolvedSchema)
        {
            string prefix = RSUVSchemaUtility.SanitizeIdentifier(resolvedSchema.NamingPrefix);

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
                    builder.AppendLine($"bool {prefix}_Get{field.Identifier}() //CUSTOM NODE READY");
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
                    builder.AppendLine($"float {prefix}_Get{field.Identifier}() //CUSTOM NODE READY");
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
                    builder.AppendLine($"int {prefix}_Get{field.Identifier}() //CUSTOM NODE READY");
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
                    builder.AppendLine($"float4 {prefix}_Get{field.Identifier}() //CUSTOM NODE READY");
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