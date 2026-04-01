using System;

namespace RSUVFramework
{
    public readonly struct RSUVFieldKey<TValue>
    {
        public RSUVFieldKey(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                throw new ArgumentException("Field name cannot be null or whitespace.", nameof(fieldName));
            }

            FieldName = fieldName;
        }

        public string FieldName { get; }

        public override string ToString()
        {
            return FieldName;
        }
    }
}