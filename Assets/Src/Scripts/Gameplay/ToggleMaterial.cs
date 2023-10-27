using UnityEngine;

namespace Src.Scripts.Gameplay
{
    /// <summary>
    /// Switches between material A and B.
    /// </summary>
    public class ToggleMaterial : MonoBehaviour
    {
        [Tooltip("The initial material.")]
        public Material materialA;
        [Tooltip("The material that will be enabled on toggle.")]
        public Material materialB;

        private Renderer _renderer;
        
        // Start is called before the first frame update
        void Start()
        {
            TryGetComponent(out _renderer);
        }
        

        [ContextMenu("Toggle Material")]
        public void Toggle()
        {
            _renderer.sharedMaterial = _renderer.sharedMaterial == materialA ? materialB : materialA;
        }
    }
}
