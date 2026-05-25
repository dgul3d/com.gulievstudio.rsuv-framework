using System;
using System.Collections.Generic;

namespace RSUVFramework
{
    public sealed class RSUVResolvedSchema
    {
        private readonly Dictionary<string, RSUVResolvedField> _fieldsByName;

        public RSUVResolvedSchema(string schemaName, string namingPrefix, IReadOnlyList<RSUVResolvedField> fields, int usedBitCount)
        {
            SchemaName = schemaName;
            NamingPrefix = namingPrefix;
            Fields = fields;
            UsedBitCount = usedBitCount;
            _fieldsByName = new Dictionary<string, RSUVResolvedField>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < fields.Count; i++)
            {
                _fieldsByName[fields[i].Name] = fields[i];
            }
        }

        public string SchemaName { get; }

    public string NamingPrefix { get; }

        public IReadOnlyList<RSUVResolvedField> Fields { get; }

        public int UsedBitCount { get; }

        public int FreeBitCount => 32 - UsedBitCount;

        public bool TryGetField(string fieldName, out RSUVResolvedField field)
        {
            return _fieldsByName.TryGetValue(fieldName, out field);
        }
    }
}