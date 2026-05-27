using System;
using UnityEngine;

namespace RSUVFramework
{
    public sealed class RSUVRuntimeState
    {
        private readonly RSUVResolvedSchema _resolvedSchema;

        private uint _packedValue;

        public RSUVRuntimeState(RSUVSchema schema)
        {
            if (!RSUVSchemaUtility.TryResolve(schema, out _resolvedSchema, out string errorMessage))
            {
                throw new InvalidOperationException(errorMessage);
            }

            ResetToDefaults();
        }

        public RSUVRuntimeState(RSUVResolvedSchema resolvedSchema)
        {
            _resolvedSchema = resolvedSchema ?? throw new ArgumentNullException(nameof(resolvedSchema));
            ResetToDefaults();
        }

        public RSUVRuntimeState(RSUVResolvedSchema resolvedSchema, bool initializeWithDefaults)
        {
            _resolvedSchema = resolvedSchema ?? throw new ArgumentNullException(nameof(resolvedSchema));

            if (initializeWithDefaults)
            {
                ResetToDefaults();
            }
            else
            {
                _packedValue = 0u;
            }
        }

        public uint PackedValue => _packedValue;

        public RSUVResolvedSchema ResolvedSchema => _resolvedSchema;

        public void ResetToDefaults()
        {
            _packedValue = 0u;

            for (int i = 0; i < _resolvedSchema.Fields.Count; i++)
            {
                RSUVResolvedField field = _resolvedSchema.Fields[i];
                switch (field.FieldType)
                {
                    case RSUVFieldType.Bool:
                        SetBool(field.Name, field.DefaultBooleanValue);
                        break;

                    case RSUVFieldType.Float:
                        SetFloat(field.Name, field.DefaultFloatValue);
                        break;

                    case RSUVFieldType.Int:
                        SetInt(field.Name, field.DefaultIntegerValue);
                        break;

                    case RSUVFieldType.Color:
                        SetColor(field.Name, field.DefaultColorValue);
                        break;
                }
            }
        }

        public void ClearPackedValue()
        {
            _packedValue = 0u;
        }

        public void SetBool(string fieldName, bool value)
        {
            RSUVResolvedField field = GetField(fieldName, RSUVFieldType.Bool);
            _packedValue = RSUVBitUtility.EncodeBits(_packedValue, value ? 1u : 0u, field.BitOffset, field.BitCount);
        }

        public void SetInt(string fieldName, int value)
        {
            RSUVResolvedField field = GetField(fieldName, RSUVFieldType.Int);
            uint rawValue = RSUVBitUtility.QuantizeInteger(value, field.MinimumIntegerValue, field.MaximumIntegerValue);

            _packedValue = RSUVBitUtility.EncodeBits(_packedValue, rawValue, field.BitOffset, field.BitCount);
        }

        public void SetNormalized(string fieldName, float value)
        {
            SetFloat(fieldName, value);
        }

        public void SetFloat(string fieldName, float value)
        {
            RSUVResolvedField field = GetField(fieldName, RSUVFieldType.Float);
            uint rawValue = RSUVBitUtility.QuantizeRange(value, field.MinimumFloatValue, field.MaximumFloatValue, field.BitCount);
            _packedValue = RSUVBitUtility.EncodeBits(_packedValue, rawValue, field.BitOffset, field.BitCount);
        }

        public void SetColor(string fieldName, Color value)
        {
            RSUVResolvedField field = GetField(fieldName, RSUVFieldType.Color);
            uint rawValue = RSUVBitUtility.QuantizeColor(value, field.BitCount);
            _packedValue = RSUVBitUtility.EncodeBits(_packedValue, rawValue, field.BitOffset, field.BitCount);
        }

        private RSUVResolvedField GetField(string fieldName)
        {
            if (!_resolvedSchema.TryGetField(fieldName, out RSUVResolvedField field))
            {
                throw new InvalidOperationException($"Field '{fieldName}' does not exist in schema '{_resolvedSchema.SchemaName}'.");
            }

            return field;
        }

        private RSUVResolvedField GetField(string fieldName, RSUVFieldType expectedFieldType)
        {
            RSUVResolvedField field = GetField(fieldName);
            if (field.FieldType != expectedFieldType)
            {
                throw new InvalidOperationException($"Field '{fieldName}' is not of type {expectedFieldType}.");
            }

            return field;
        }
    }
}