using UnityEngine;

namespace RSUVFramework
{   
    [ExecuteAlways]
    public class RSUVTestAtlasDriver : MonoBehaviour
    {
        [SerializeField] private RSUVRendererValueWriter _writer;
        [SerializeField] private float speed = 1f;

        private void Update()
        {
            if (_writer == null)
            {
                return;
            }

            int atlasIndex = (Mathf.FloorToInt(Time.time * speed) % 3) + 1;
            _writer.SetAtlasIndex(atlasIndex);
        }
    }
}