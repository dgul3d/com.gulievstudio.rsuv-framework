using UnityEngine;
using RSUVFramework;
using static RSUVFramework.Generated.TestRSUVSchemaApi;

namespace RSUVFramework
{   
    [ExecuteAlways]
    public class RSUVTestDriver : MonoBehaviour
    {
        [SerializeField] private RSUVRendererValueWriter _writer;
        [SerializeField] private Color color1;
        [SerializeField] private Color color2;
        [SerializeField] private float speed = 1f;

        private void Update()
        {
            if (_writer == null)
            {
                return;
            }

            float blend = Mathf.PingPong(Time.time * speed, 1f);
            _writer.SetMyCol(Color.Lerp(color1, color2, blend));
        }
    }
    }