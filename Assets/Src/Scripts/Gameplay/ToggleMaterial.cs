using UnityEngine;
using UnityEngine.Serialization;

namespace Src.Scripts.Utility
{
    /// <summary>
    /// Switches between material A and B.
    /// </summary>
    public class ToggleMaterial : MonoBehaviour
    {
        [FormerlySerializedAs("MaterialA")] [Tooltip("The initial material.")]
        public Material materialA;
        [FormerlySerializedAs("MaterialB")] [Tooltip("The material that will be enabled on toggle.")]
        public Material materialB;

        private Renderer _renderer;
        
        // Start is called before the first frame update
        void Start()
        {
            TryGetComponent(out _renderer);
        }
        

        public void Toggle()
        {
            _renderer.material = _renderer.material == materialA ? materialB : materialA;
        }
    }
}
