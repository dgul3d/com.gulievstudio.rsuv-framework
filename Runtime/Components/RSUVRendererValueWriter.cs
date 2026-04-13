using System;
using System.Collections.Generic;
using UnityEngine;

namespace RSUVFramework
{
    [ExecuteAlways]
    public sealed class RSUVRendererValueWriter : MonoBehaviour
    {
        [SerializeField] private RSUVSchema _schema;

        [SerializeField] private List<Renderer> _renderers = new List<Renderer>();
        [SerializeField] private List<RSUVSerializedFieldValue> _fieldValues = new List<RSUVSerializedFieldValue>();

        private RSUVRuntimeState _runtimeState;

        public RSUVSchema Schema
        {
            get => _schema;
            set
            {
                _schema = value;
                RefreshSerializedFields();
            }
        }

        public IReadOnlyList<RSUVSerializedFieldValue> FieldValues => _fieldValues;
        public IReadOnlyList<Renderer> Renderers => _renderers;

        private void Awake()
        {
            RefreshSerializedFields();
        }

        private void OnDestroy()
        {
            _runtimeState = null;
        }

        private void OnEnable()
        {
            RefreshSerializedFields();
        }

        private void OnValidate()
        {
            RefreshSerializedFields();
        }

        public void RefreshSerializedFields()
        {
            if (_schema == null)
            {
                _fieldValues.Clear();
                _runtimeState = null;
                return;
            }

            if (!TryGetResolvedSchema(out RSUVResolvedSchema resolvedSchema, out string errorMessage))
            {
                _runtimeState = null;
                Debug.LogError(errorMessage, this);
                return;
            }

            SynchronizeSerializedValues(resolvedSchema);
            ApplySerializedValues();
        }

        public void RebuildFromSchemaDefaults()
        {
            if (!TryGetResolvedSchema(out RSUVResolvedSchema resolvedSchema, out string errorMessage))
            {
                _runtimeState = null;
                Debug.LogError(errorMessage, this);
                return;
            }

            _fieldValues.Clear();

            for (int i = 0; i < resolvedSchema.Fields.Count; i++)
            {
                RSUVSerializedFieldValue fieldValue = new RSUVSerializedFieldValue();
                fieldValue.ApplyResolvedDefaults(resolvedSchema.Fields[i]);
                _fieldValues.Add(fieldValue);
            }

            ApplySerializedValues();
        }

        public void SetRenderers(params Renderer[] renderers)
        {
            _renderers.Clear();

            if (renderers != null)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    Renderer renderer = renderers[i];
                    if (renderer == null || _renderers.Contains(renderer))
                    {
                        continue;
                    }

                    _renderers.Add(renderer);
                }
            }

            ApplyPackedValue();
        }

        public void AddRenderers(params Renderer[] renderers)
        {
            if (renderers == null)
            {
                return;
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || _renderers.Contains(renderer))
                {
                    continue;
                }

                _renderers.Add(renderer);
            }

            ApplyPackedValue();
        }

        public void SetBool(string fieldName, bool value)
        {
            SetBool(fieldName, value, true);
        }

        public void SetBool(string fieldName, bool value, bool applyImmediately)
        {
            SetValue(fieldName, fieldValue => fieldValue.BooleanValue = value, applyImmediately);
        }

        public void SetBool(RSUVFieldKey<bool> field, bool value)
        {
            SetBool(field.FieldName, value);
        }

        public void SetInt(string fieldName, int value)
        {
            SetInt(fieldName, value, true);
        }

        public void SetInt(string fieldName, int value, bool applyImmediately)
        {
            SetValue(fieldName, fieldValue => fieldValue.IntegerValue = value, applyImmediately);
        }

        public void SetInt(RSUVFieldKey<int> field, int value)
        {
            SetInt(field.FieldName, value);
        }

        public void SetNormalized(string fieldName, float value)
        {
            SetFloat(fieldName, value);
        }

        public void SetNormalized(RSUVFieldKey<float> field, float value)
        {
            SetFloat(field.FieldName, value);
        }

        public void SetFloat(string fieldName, float value)
        {
            SetFloat(fieldName, value, true);
        }

        public void SetFloat(string fieldName, float value, bool applyImmediately)
        {
            SetValue(fieldName, fieldValue => fieldValue.FloatValue = value, applyImmediately);
        }

        public void SetFloat(RSUVFieldKey<float> field, float value)
        {
            SetFloat(field.FieldName, value);
        }

        public void SetColor(string fieldName, Color value)
        {
            SetColor(fieldName, value, true);
        }

        public void SetColor(string fieldName, Color value, bool applyImmediately)
        {
            SetValue(fieldName, fieldValue => fieldValue.ColorValue = value, applyImmediately);
        }

        public void SetColor(RSUVFieldKey<Color> field, Color value)
        {
            SetColor(field.FieldName, value);
        }

        public uint GetPackedValue()
        {
            EnsureState();

            if (_runtimeState == null && _schema != null)
            {
                RefreshSerializedFields();
            }

            return _runtimeState.PackedValue;
        }

        public bool TryGetResolvedSchema(out RSUVResolvedSchema resolvedSchema, out string errorMessage)
        {
            return RSUVSchemaUtility.TryResolve(_schema, out resolvedSchema, out errorMessage);
        }

        public void ApplySerializedValues()
        {
            if (!TryGetResolvedSchema(out RSUVResolvedSchema resolvedSchema, out string errorMessage))
            {
                _runtimeState = null;
                Debug.LogError(errorMessage, this);
                return;
            }

            _runtimeState = new RSUVRuntimeState(resolvedSchema);

            for (int i = 0; i < resolvedSchema.Fields.Count; i++)
            {
                RSUVResolvedField field = resolvedSchema.Fields[i];
                RSUVSerializedFieldValue fieldValue = GetSerializedValue(field.Name);
                fieldValue.ClampToField(field);
                ApplyFieldValue(field, fieldValue);
            }

            ApplyPackedValue();
        }

        private void ApplyPackedValue()
        {
            if (_runtimeState == null)
            {
                return;
            }

            uint packedValue = _runtimeState.PackedValue;

            for (int i = 0; i < _renderers.Count; i++)
            {
                Renderer renderer = _renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
                {
                    skinnedMeshRenderer.SetShaderUserValue(packedValue);
                    continue;
                }

                if (renderer is MeshRenderer meshRenderer)
                {
                    meshRenderer.SetShaderUserValue(packedValue);
                }
            }
        }

        private void ApplyFieldValue(RSUVResolvedField field, RSUVSerializedFieldValue fieldValue)
        {
            switch (field.FieldType)
            {
                case RSUVFieldType.Bool:
                    _runtimeState.SetBool(field.Name, fieldValue.BooleanValue);
                    break;

                case RSUVFieldType.Int:
                    _runtimeState.SetInt(field.Name, fieldValue.IntegerValue);
                    break;

                case RSUVFieldType.Float:
                    _runtimeState.SetFloat(field.Name, fieldValue.FloatValue);
                    break;

                case RSUVFieldType.Color:
                    _runtimeState.SetColor(field.Name, fieldValue.ColorValue);
                    break;
            }
        }

        private void EnsureState()
        {
            if (_runtimeState == null)
            {
                RefreshSerializedFields();
            }

            if (_runtimeState == null)
            {
                throw new InvalidOperationException("RSUV runtime state is not available.");
            }
        }

        private RSUVSerializedFieldValue GetSerializedValue(string fieldName)
        {
            for (int i = 0; i < _fieldValues.Count; i++)
            {
                if (string.Equals(_fieldValues[i].FieldName, fieldName, StringComparison.OrdinalIgnoreCase))
                {
                    return _fieldValues[i];
                }
            }

            throw new InvalidOperationException($"Serialized field '{fieldName}' does not exist.");
        }

        private void SetValue(string fieldName, Action<RSUVSerializedFieldValue> applyValue)
        {
            SetValue(fieldName, applyValue, true);
        }

        private void SetValue(string fieldName, Action<RSUVSerializedFieldValue> applyValue, bool applyImmediately)
        {
            EnsureState();

            RSUVSerializedFieldValue fieldValue = GetSerializedValue(fieldName);
            applyValue(fieldValue);

            if (applyImmediately)
                ApplySerializedValues();
        }

        private void SynchronizeSerializedValues(RSUVResolvedSchema resolvedSchema)
        {
            Dictionary<string, RSUVSerializedFieldValue> existingValues = new Dictionary<string, RSUVSerializedFieldValue>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < _fieldValues.Count; i++)
            {
                RSUVSerializedFieldValue fieldValue = _fieldValues[i];
                if (fieldValue == null || string.IsNullOrWhiteSpace(fieldValue.FieldName))
                {
                    continue;
                }

                existingValues[fieldValue.FieldName] = fieldValue;
            }

            List<RSUVSerializedFieldValue> synchronizedValues = new List<RSUVSerializedFieldValue>(resolvedSchema.Fields.Count);

            for (int i = 0; i < resolvedSchema.Fields.Count; i++)
            {
                RSUVResolvedField resolvedField = resolvedSchema.Fields[i];
                if (!existingValues.TryGetValue(resolvedField.Name, out RSUVSerializedFieldValue fieldValue) || fieldValue.FieldType != resolvedField.FieldType)
                {
                    fieldValue = new RSUVSerializedFieldValue();
                    fieldValue.ApplyResolvedDefaults(resolvedField);
                }
                else
                {
                    fieldValue.ClampToField(resolvedField);
                }

                synchronizedValues.Add(fieldValue);
            }

            _fieldValues = synchronizedValues;
        }
    }
}