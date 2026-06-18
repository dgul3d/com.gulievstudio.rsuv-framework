using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RSUVFramework
{
    [ExecuteAlways]
    public sealed class RSUVRendererValueWriter : MonoBehaviour
    {
        [SerializeField] private RSUVSchema _schema;

        [SerializeField] private List<Renderer> _renderers = new List<Renderer>();
        [SerializeField] private List<RSUVSerializedFieldValue> _fieldValues = new List<RSUVSerializedFieldValue>();

        private bool _hasPendingSerializedChanges;
        private Dictionary<string, RSUVSerializedFieldValue> _fieldValuesByName = new Dictionary<string, RSUVSerializedFieldValue>(StringComparer.OrdinalIgnoreCase);
        private RSUVResolvedSchema _resolvedSchema;
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


        public void RefreshSerializedFields()
        {
            if (_schema == null)
            {
                _fieldValues.Clear();
                _fieldValuesByName.Clear();
                _hasPendingSerializedChanges = false;
                _resolvedSchema = null;
                _runtimeState = null;
                return;
            }

            if (!TryGetResolvedSchema(out RSUVResolvedSchema resolvedSchema, out string errorMessage))
            {
                _fieldValuesByName.Clear();
                _hasPendingSerializedChanges = false;
                _resolvedSchema = null;
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
                _fieldValuesByName.Clear();
                _hasPendingSerializedChanges = false;
                _resolvedSchema = null;
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

            RebuildFieldValueLookup();
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
            SetBooleanValue(fieldName, value, applyImmediately);
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
            SetIntegerValue(fieldName, value, applyImmediately);
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
            SetFloatValue(fieldName, value, applyImmediately);
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
            SetColorValue(fieldName, value, applyImmediately);
        }

        public void SetColor(RSUVFieldKey<Color> field, Color value)
        {
            SetColor(field.FieldName, value);
        }

        public uint GetPackedValue()
        {
            EnsureState();

            if (_hasPendingSerializedChanges)
            {
                ApplySerializedValues();
            }

            return _runtimeState.PackedValue;
        }

        public bool TryGetResolvedSchema(out RSUVResolvedSchema resolvedSchema, out string errorMessage)
        {
            if (_resolvedSchema != null)
            {
                resolvedSchema = _resolvedSchema;
                errorMessage = string.Empty;
                return true;
            }

            bool isResolved = RSUVSchemaUtility.TryResolve(_schema, out resolvedSchema, out errorMessage);
            if (isResolved)
            {
                _resolvedSchema = resolvedSchema;
            }

            return isResolved;
        }

        public void ApplySerializedValues()
        {
            if (!TryGetResolvedSchema(out RSUVResolvedSchema resolvedSchema, out string errorMessage))
            {
                _fieldValuesByName.Clear();
                _hasPendingSerializedChanges = false;
                _resolvedSchema = null;
                _runtimeState = null;
                Debug.LogError(errorMessage, this);
                return;
            }

            if (_fieldValues.Count != resolvedSchema.Fields.Count)
            {
                SynchronizeSerializedValues(resolvedSchema);
            }

            EnsureRuntimeState(resolvedSchema);
            _runtimeState.ClearPackedValue();

            for (int i = 0; i < resolvedSchema.Fields.Count; i++)
            {
                RSUVResolvedField field = resolvedSchema.Fields[i];
                RSUVSerializedFieldValue fieldValue = _fieldValues[i];
                fieldValue.ClampToField(field);
                ApplyFieldValue(field, fieldValue);
            }

            _hasPendingSerializedChanges = false;
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
            if (_fieldValuesByName.TryGetValue(fieldName, out RSUVSerializedFieldValue fieldValue))
            {
                return fieldValue;
            }

            throw new InvalidOperationException($"Serialized field '{fieldName}' does not exist.");
        }

        private void SetBooleanValue(string fieldName, bool value, bool applyImmediately)
        {
            EnsureState();

            RSUVResolvedField field = GetResolvedField(fieldName, RSUVFieldType.Bool);
            RSUVSerializedFieldValue fieldValue = GetSerializedValue(fieldName);
            fieldValue.BooleanValue = value;

            ApplyFieldChange(field, fieldValue, applyImmediately);
        }

        private void SetIntegerValue(string fieldName, int value, bool applyImmediately)
        {
            EnsureState();

            RSUVResolvedField field = GetResolvedField(fieldName, RSUVFieldType.Int);
            RSUVSerializedFieldValue fieldValue = GetSerializedValue(fieldName);
            fieldValue.IntegerValue = value;

            ApplyFieldChange(field, fieldValue, applyImmediately);
        }

        private void SetFloatValue(string fieldName, float value, bool applyImmediately)
        {
            EnsureState();

            RSUVResolvedField field = GetResolvedField(fieldName, RSUVFieldType.Float);
            RSUVSerializedFieldValue fieldValue = GetSerializedValue(fieldName);
            fieldValue.FloatValue = value;

            ApplyFieldChange(field, fieldValue, applyImmediately);
        }

        private void SetColorValue(string fieldName, Color value, bool applyImmediately)
        {
            EnsureState();

            RSUVResolvedField field = GetResolvedField(fieldName, RSUVFieldType.Color);
            RSUVSerializedFieldValue fieldValue = GetSerializedValue(fieldName);
            fieldValue.ColorValue = value;

            ApplyFieldChange(field, fieldValue, applyImmediately);
        }

        private void ApplyFieldChange(RSUVResolvedField field, RSUVSerializedFieldValue fieldValue, bool applyImmediately)
        {
            if (!applyImmediately)
            {
                _hasPendingSerializedChanges = true;
                return;
            }

            if (_hasPendingSerializedChanges)
            {
                ApplySerializedValues();
                return;
            }

            fieldValue.ClampToField(field);
            ApplyFieldValue(field, fieldValue);
            ApplyPackedValue();
        }

        private RSUVResolvedField GetResolvedField(string fieldName, RSUVFieldType expectedFieldType)
        {
            if (_resolvedSchema == null || !_resolvedSchema.TryGetField(fieldName, out RSUVResolvedField field))
            {
                throw new InvalidOperationException($"Field '{fieldName}' does not exist in schema.");
            }

            if (field.FieldType != expectedFieldType)
            {
                throw new InvalidOperationException($"Field '{fieldName}' is not of type {expectedFieldType}.");
            }

            return field;
        }

        private void EnsureRuntimeState(RSUVResolvedSchema resolvedSchema)
        {
            if (_runtimeState != null && ReferenceEquals(_runtimeState.ResolvedSchema, resolvedSchema))
            {
                return;
            }

            _runtimeState = new RSUVRuntimeState(resolvedSchema, false);
        }

        private void RebuildFieldValueLookup()
        {
            _fieldValuesByName.Clear();

            for (int i = 0; i < _fieldValues.Count; i++)
            {
                RSUVSerializedFieldValue fieldValue = _fieldValues[i];
                if (fieldValue == null || string.IsNullOrWhiteSpace(fieldValue.FieldName))
                {
                    continue;
                }

                _fieldValuesByName[fieldValue.FieldName] = fieldValue;
            }
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
            RebuildFieldValueLookup();
        }
    }
}