using System.Collections.Generic;
using UnityEngine;

namespace RSUVFramework
{
    [CreateAssetMenu(menuName = "Rendering/RSUV/Schema", fileName = "RSUVSchema")]
    public sealed class RSUVSchema : ScriptableObject
    {
        private const string DEFAULT_GENERATED_BINDINGS_DIRECTORY = "Assets/RSUVFramework/Generated";

        [SerializeField] private string _shaderSymbolPrefix = "RSUVSchema";
        [SerializeField] private string _generatedCSharpBindingsDirectory = DEFAULT_GENERATED_BINDINGS_DIRECTORY;
        [SerializeField] private string _generatedHlslBindingsDirectory = DEFAULT_GENERATED_BINDINGS_DIRECTORY;
        [SerializeField, HideInInspector] private string _generatedBindingsDirectory = DEFAULT_GENERATED_BINDINGS_DIRECTORY;
        [SerializeField] private bool _autoGenerateOnChange = true;
        [SerializeField] private List<RSUVSchemaField> _fields = new List<RSUVSchemaField>();

        public string ShaderSymbolPrefix
        {
            get => _shaderSymbolPrefix;
            set => _shaderSymbolPrefix = value;
        }

        public string GeneratedCSharpBindingsDirectory
        {
            get => ResolveGeneratedDirectory(_generatedCSharpBindingsDirectory);
            set => _generatedCSharpBindingsDirectory = value;
        }

        public string GeneratedHlslBindingsDirectory
        {
            get => ResolveGeneratedDirectory(_generatedHlslBindingsDirectory);
            set => _generatedHlslBindingsDirectory = value;
        }

        public bool AutoGenerateOnChange
        {
            get => _autoGenerateOnChange;
            set => _autoGenerateOnChange = value;
        }

        public List<RSUVSchemaField> Fields => _fields;

        public string GeneratedBindingsDirectory
        {
            get => GeneratedCSharpBindingsDirectory;
            set
            {
                _generatedCSharpBindingsDirectory = value;
                _generatedHlslBindingsDirectory = value;
            }
        }

        public void ReplaceFields(IEnumerable<RSUVSchemaField> fields)
        {
            _fields = new List<RSUVSchemaField>(fields);
        }

        private string ResolveGeneratedDirectory(string configuredDirectory)
        {
            if (!string.IsNullOrWhiteSpace(configuredDirectory))
            {
                return configuredDirectory;
            }

            if (!string.IsNullOrWhiteSpace(_generatedBindingsDirectory))
            {
                return _generatedBindingsDirectory;
            }

            return DEFAULT_GENERATED_BINDINGS_DIRECTORY;
        }
    }
}