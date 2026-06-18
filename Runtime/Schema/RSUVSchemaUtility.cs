using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RSUVFramework
{
    public static class RSUVSchemaUtility
    {
        public static bool TryResolve(RSUVSchema schema, out RSUVResolvedSchema resolvedSchema, out string errorMessage)
        {
            resolvedSchema = null;
            errorMessage = string.Empty;

            if (schema == null)
            {
                errorMessage = "Schema is null.";
                return false;
            }

            List<string> errors = GetValidationErrors(schema);
            if (errors.Count > 0)
            {
                errorMessage = string.Join("\n", errors);
                return false;
            }

            string prefix = GetNamingPrefix(schema);
            List<RSUVResolvedField> resolvedFields = new List<RSUVResolvedField>(schema.Fields.Count);
            int nextBitOffset = 0;

            for (int i = 0; i < schema.Fields.Count; i++)
            {
                RSUVSchemaField field = schema.Fields[i];
                if ((nextBitOffset + field.BitCount) > 32)
                {
                    errorMessage = $"Unable to place field '{field.Name}' within 32 bits.";
                    return false;
                }

                resolvedFields.Add(CreateResolvedField(field, nextBitOffset));
                nextBitOffset += field.BitCount;
            }

            resolvedSchema = new RSUVResolvedSchema(schema.name, prefix, resolvedFields, nextBitOffset);
            return true;
        }

        public static string GetNamingPrefix(RSUVSchema schema)
        {
            if (schema == null)
            {
                return SanitizeIdentifier(string.Empty);
            }

            return SanitizeIdentifier(string.IsNullOrWhiteSpace(schema.NamingPrefix) ? schema.name : schema.NamingPrefix);
        }

        public static List<string> GetValidationErrors(RSUVSchema schema)
        {
            List<string> errors = new List<string>();
            if (schema == null)
            {
                errors.Add("Schema is null.");
                return errors;
            }

            if (schema.Fields == null || schema.Fields.Count == 0)
            {
                errors.Add("Schema does not contain any fields.");
                return errors;
            }

            HashSet<string> names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> identifiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int totalBitCount = 0;

            for (int i = 0; i < schema.Fields.Count; i++)
            {
                RSUVSchemaField field = schema.Fields[i];
                string fieldLabel = string.IsNullOrWhiteSpace(field.Name) ? $"Field {i}" : field.Name;

                if (string.IsNullOrWhiteSpace(field.Name))
                {
                    errors.Add($"Field at index {i} requires a name.");
                }

                if (!names.Add(fieldLabel))
                {
                    errors.Add($"Field name '{fieldLabel}' is duplicated.");
                }

                string identifier = SanitizeIdentifier(fieldLabel);
                if (!identifiers.Add(identifier))
                {
                    errors.Add($"Field name '{fieldLabel}' collides with another sanitized identifier.");
                }

                if (field.BitCount <= 0 || field.BitCount > 32)
                {
                    errors.Add($"Field '{fieldLabel}' uses invalid bit count {field.BitCount}.");
                    continue;
                }

                totalBitCount += field.BitCount;
                if (totalBitCount > 32)
                {
                    errors.Add($"Field '{fieldLabel}' exceeds the 32-bit budget.");
                }

                ValidateFieldCapacity(field, fieldLabel, errors);
            }

#if UNITY_EDITOR
            ValidateUniqueNamingPrefix(schema, errors);
#endif

            return errors;
        }

        public static string SanitizeIdentifier(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "RSUVIdentifier";
            }

            StringBuilder builder = new StringBuilder(value.Length);
            bool writeUnderscore = false;

            for (int i = 0; i < value.Length; i++)
            {
                char character = value[i];
                if (char.IsLetterOrDigit(character))
                {
                    if (writeUnderscore && builder.Length > 0 && builder[builder.Length - 1] != '_')
                    {
                        builder.Append('_');
                    }

                    builder.Append(character);
                    writeUnderscore = false;
                    continue;
                }

                writeUnderscore = builder.Length > 0;
            }

            if (builder.Length == 0)
            {
                return "RSUVIdentifier";
            }

            if (char.IsDigit(builder[0]))
            {
                builder.Insert(0, '_');
            }

            return builder.ToString();
        }

        private static RSUVResolvedField CreateResolvedField(RSUVSchemaField field, int bitOffset)
        {
            return new RSUVResolvedField(
                field.Name,
                SanitizeIdentifier(field.Name),
                field.FieldType,
                bitOffset,
                field.BitCount,
                GetMaxRawValue(field.BitCount),
                GetMinimumIntegerValue(field),
                GetMaximumIntegerValue(field),
                GetMinimumFloatValue(field),
                GetMaximumFloatValue(field),
                field.DefaultIntegerValue,
                GetDefaultFloatValue(field),
                field.DefaultColorValue,
                field.DefaultBooleanValue);
        }

        private static uint GetMaxRawValue(int bitCount)
        {
            return bitCount >= 32 ? uint.MaxValue : ((1u << bitCount) - 1u);
        }

        private static void ValidateFieldCapacity(RSUVSchemaField field, string fieldLabel, List<string> errors)
        {
            uint maxRawValue = GetMaxRawValue(field.BitCount);
            switch (field.FieldType)
            {
                case RSUVFieldType.Bool:
                    if (field.BitCount != 1)
                    {
                        errors.Add($"Bool field '{fieldLabel}' must use exactly 1 bit.");
                    }

                    break;

                case RSUVFieldType.Int:
                    int minimumIntegerValue = GetMinimumIntegerValue(field);
                    int maximumIntegerValue = GetMaximumIntegerValue(field);

                    if (maximumIntegerValue < minimumIntegerValue)
                    {
                        errors.Add($"Integer field '{fieldLabel}' has an invalid min/max range.");
                    }
                    else if ((maximumIntegerValue - minimumIntegerValue) > maxRawValue)
                    {
                        errors.Add($"Integer field '{fieldLabel}' range exceeds its bit capacity.");
                    }

                    if (field.DefaultIntegerValue < minimumIntegerValue || field.DefaultIntegerValue > maximumIntegerValue)
                    {
                        errors.Add($"Integer field '{fieldLabel}' default value is outside its min/max range.");
                    }

                    break;

                case RSUVFieldType.Float:
                    float minimumFloatValue = GetMinimumFloatValue(field);
                    float maximumFloatValue = GetMaximumFloatValue(field);

                    if (maximumFloatValue <= minimumFloatValue)
                    {
                        errors.Add($"Range float field '{fieldLabel}' has an invalid min/max range.");
                    }

                    float defaultFloatValue = GetDefaultFloatValue(field);
                    if (defaultFloatValue < minimumFloatValue || defaultFloatValue > maximumFloatValue)
                    {
                        errors.Add($"Float field '{fieldLabel}' default value is outside its min/max range.");
                    }

                    break;

                case RSUVFieldType.Color:
                    if (field.BitCount < 4 || (field.BitCount % 4) != 0)
                    {
                        errors.Add($"Color field '{fieldLabel}' must use a bit count divisible by 4 and at least 4 bits.");
                    }

                    Color defaultColor = field.DefaultColorValue;
                    if (!IsUnitRange(defaultColor.r) || !IsUnitRange(defaultColor.g) || !IsUnitRange(defaultColor.b) || !IsUnitRange(defaultColor.a))
                    {
                        errors.Add($"Color field '{fieldLabel}' default color must stay within the 0..1 range for all channels.");
                    }

                    break;

                default:
                    errors.Add($"Field '{fieldLabel}' uses unsupported type '{field.FieldType}'.");
                    break;
            }
        }

        private static int GetMinimumIntegerValue(RSUVSchemaField field)
        {
            return field.MinimumIntegerValue;
        }

        private static int GetMaximumIntegerValue(RSUVSchemaField field)
        {
            return field.MaximumIntegerValue;
        }

        private static float GetMinimumFloatValue(RSUVSchemaField field)
        {
            return field.MinimumFloatValue;
        }

#if UNITY_EDITOR
        private static void ValidateUniqueNamingPrefix(RSUVSchema schema, List<string> errors)
        {
            if (EditorApplication.isUpdating || EditorApplication.isCompiling)
            {
                return;
            }

            string namingPrefix = GetNamingPrefix(schema);
            string schemaPath = AssetDatabase.GetAssetPath(schema);
            string[] schemaGuids = AssetDatabase.FindAssets("t:RSUVSchema");

            for (int i = 0; i < schemaGuids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(schemaGuids[i]);
                if (string.Equals(assetPath, schemaPath, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                RSUVSchema otherSchema = AssetDatabase.LoadAssetAtPath<RSUVSchema>(assetPath);
                if (otherSchema == null)
                {
                    continue;
                }

                string otherNamingPrefix = GetNamingPrefix(otherSchema);
                if (!string.Equals(namingPrefix, otherNamingPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                errors.Add($"Naming prefix '{namingPrefix}' is already used by schema '{otherSchema.name}'.");
                return;
            }
        }
#endif

        private static float GetMaximumFloatValue(RSUVSchemaField field)
        {
            return field.MaximumFloatValue;
        }

        private static float GetDefaultFloatValue(RSUVSchemaField field)
        {
            return field.DefaultFloatValue;
        }

        private static bool IsUnitRange(float value)
        {
            return value >= 0f && value <= 1f;
        }
    }
}