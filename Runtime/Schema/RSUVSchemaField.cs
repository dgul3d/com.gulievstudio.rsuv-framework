using System;
using UnityEngine;

namespace RSUVFramework
{
    [Serializable]
    public sealed class RSUVSchemaField
    {
        [SerializeField] private string _name = "Field";
        [SerializeField] private RSUVFieldType _fieldType = RSUVFieldType.Int;
        [SerializeField] private int _bitCount = 1;

        [SerializeField] private int _minimumIntegerValue;
        [SerializeField] private int _maximumIntegerValue = 1;
        [SerializeField] private int _defaultIntegerValue;

        [SerializeField] private float _minimumFloatValue;
        [SerializeField] private float _maximumFloatValue = 1f;
        [SerializeField] private float _defaultFloatValue;

        [SerializeField] private Color _defaultColorValue = Color.white;

        [SerializeField] private bool _defaultBooleanValue;

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public RSUVFieldType FieldType
        {
            get => _fieldType;
            set => _fieldType = value;
        }

        public int BitCount
        {
            get => _bitCount;
            set => _bitCount = value;
        }

        public int MinimumIntegerValue
        {
            get => _minimumIntegerValue;
            set => _minimumIntegerValue = value;
        }

        public int MaximumIntegerValue
        {
            get => _maximumIntegerValue;
            set => _maximumIntegerValue = value;
        }

        public float MinimumFloatValue
        {
            get => _minimumFloatValue;
            set => _minimumFloatValue = value;
        }

        public float MaximumFloatValue
        {
            get => _maximumFloatValue;
            set => _maximumFloatValue = value;
        }

        public int DefaultIntegerValue
        {
            get => _defaultIntegerValue;
            set => _defaultIntegerValue = value;
        }

        public float DefaultFloatValue
        {
            get => _defaultFloatValue;
            set => _defaultFloatValue = value;
        }

        public Color DefaultColorValue
        {
            get => _defaultColorValue;
            set => _defaultColorValue = value;
        }

        public bool DefaultBooleanValue
        {
            get => _defaultBooleanValue;
            set => _defaultBooleanValue = value;
        }
    }
}