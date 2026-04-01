using System;
using UnityEngine;

namespace RSUVFramework
{
    [Serializable]
    public sealed class RSUVSerializedFieldValue
    {
        [SerializeField] private string _fieldName;
        [SerializeField] private RSUVFieldType _fieldType;
        [SerializeField] private bool _booleanValue;
        [SerializeField] private int _integerValue;
        [SerializeField] private float _floatValue;
        [SerializeField] private Color _colorValue = Color.white;

        public string FieldName
        {
            get => _fieldName;
            set => _fieldName = value;
        }

        public RSUVFieldType FieldType
        {
            get => _fieldType;
            set => _fieldType = value;
        }

        public bool BooleanValue
        {
            get => _booleanValue;
            set => _booleanValue = value;
        }

        public int IntegerValue
        {
            get => _integerValue;
            set => _integerValue = value;
        }

        public float FloatValue
        {
            get => _floatValue;
            set => _floatValue = value;
        }

        public Color ColorValue
        {
            get => _colorValue;
            set => _colorValue = value;
        }

        public void ApplyResolvedDefaults(RSUVResolvedField resolvedField)
        {
            _fieldName = resolvedField.Name;
            _fieldType = resolvedField.FieldType;
            _booleanValue = resolvedField.DefaultBooleanValue;
            _integerValue = resolvedField.DefaultIntegerValue;
            _floatValue = resolvedField.DefaultFloatValue;
            _colorValue = resolvedField.DefaultColorValue;
        }

        public void ClampToField(RSUVResolvedField resolvedField)
        {
            _fieldName = resolvedField.Name;
            _fieldType = resolvedField.FieldType;

            switch (resolvedField.FieldType)
            {
                case RSUVFieldType.Bool:
                    break;

                case RSUVFieldType.Int:
                    _integerValue = Mathf.Clamp(_integerValue, resolvedField.MinimumIntegerValue, resolvedField.MaximumIntegerValue);
                    break;

                case RSUVFieldType.Float:
                    _floatValue = Mathf.Clamp(_floatValue, resolvedField.MinimumFloatValue, resolvedField.MaximumFloatValue);
                    break;

                case RSUVFieldType.Color:
                    _colorValue = new Color(
                        Mathf.Clamp01(_colorValue.r),
                        Mathf.Clamp01(_colorValue.g),
                        Mathf.Clamp01(_colorValue.b),
                        Mathf.Clamp01(_colorValue.a));
                    break;
            }
        }
    }
}