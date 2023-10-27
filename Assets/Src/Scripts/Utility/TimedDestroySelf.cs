using System.Collections;
using UnityEngine;

// Handy for destroying VFX when they're done playing
namespace Src.Scripts.Utility
{
    public class TimedDestroySelf : MonoBehaviour
    {

        public float lifetime;
        
        private WaitForSeconds _destroyDelay;
        
        private void Awake()
        {
            _destroyDelay = new WaitForSeconds(lifetime);
        }

        void OnEnable()
        {
            StartCoroutine(nameof(DisableObject));
        }

        private IEnumerator DisableObject()
        {
            yield return _destroyDelay;
            Destroy(gameObject);
        }
        
    }
}
