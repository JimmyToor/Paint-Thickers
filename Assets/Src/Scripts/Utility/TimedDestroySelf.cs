using System.Collections;
using UnityEngine;

// Handy for disabling VFX when they're done playing
namespace Src.Scripts.Utility
{
    public class TimedDestroySelf : MonoBehaviour
    {

        public float lifetime;
        void OnEnable()
        {
            StartCoroutine(nameof(DestroyObject));
        }

        private IEnumerator DestroyObject()
        {
            yield return new WaitForSeconds(lifetime);
            Destroy(this);
        }

    }
}
