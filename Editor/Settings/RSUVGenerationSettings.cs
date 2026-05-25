using UnityEngine;

namespace RSUVFramework.Editor
{
    [CreateAssetMenu(menuName = "Rendering/RSUV/Generation Settings", fileName = "RSUVGenerationSettings")]
    public sealed class RSUVGenerationSettings : ScriptableObject
    {
        public const string DEFAULT_HLSL_BINDINGS_DIRECTORY = "Assets/Art/Shaders/Include";
        public const string DEFAULT_CSHARP_BINDINGS_DIRECTORY = "Assets/Scripts/Generated";

        [SerializeField] private string _hlslBindingsDirectory = DEFAULT_HLSL_BINDINGS_DIRECTORY;
        [SerializeField] private string _cSharpBindingsDirectory = DEFAULT_CSHARP_BINDINGS_DIRECTORY;

        public string HlslBindingsDirectory
        {
            get => _hlslBindingsDirectory;
            set => _hlslBindingsDirectory = value;
        }

        public string CSharpBindingsDirectory
        {
            get => _cSharpBindingsDirectory;
            set => _cSharpBindingsDirectory = value;
        }
    }
}