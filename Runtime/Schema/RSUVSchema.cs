using System.Collections.Generic;
using UnityEngine;

namespace RSUVFramework
{
    [CreateAssetMenu(menuName = "Rendering/RSUV/Schema", fileName = "RSUVSchema")]
    public sealed class RSUVSchema : ScriptableObject
    {
        [SerializeField] private string _namingPrefix = string.Empty;
        [SerializeField] private bool _autoGenerateOnChange = true;
        [SerializeField] private List<RSUVSchemaField> _fields = new List<RSUVSchemaField>();

        public string NamingPrefix
        {
            get => _namingPrefix;
            set => _namingPrefix = value;
        }

        public bool AutoGenerateOnChange
        {
            get => _autoGenerateOnChange;
            set => _autoGenerateOnChange = value;
        }

        public List<RSUVSchemaField> Fields => _fields;

        public void ReplaceFields(IEnumerable<RSUVSchemaField> fields)
        {
            _fields = new List<RSUVSchemaField>(fields);
        }
    }
}