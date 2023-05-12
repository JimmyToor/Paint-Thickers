using System.Collections;
using UnityEngine;

namespace Src.Scripts.Utility
{
    public class TimedDisable : MonoBehaviour
    {
        public float lifetime;
        void OnEnable()
        {
            StartCoroutine(nameof(DisableObject));
        }

        private IEnumerator DisableObject()
        {
            yield return new WaitForSeconds(lifetime);
            gameObject.SetActive(false);
        }
    }
}
