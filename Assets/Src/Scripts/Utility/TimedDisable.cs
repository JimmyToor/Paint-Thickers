using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Src.Scripts.Utility
{
    public class TimedDisable : MonoBehaviour
    {
        public float lifetime;
        [Tooltip("Number of times to disable the object after being enabled. -1 = Infinite.")]
        public float disableAmount = 1;

        private WaitForSeconds _disableDelay;
        
        private void Awake()
        {
            _disableDelay = new WaitForSeconds(lifetime);
        }

        void OnEnable()
        {
            if (disableAmount == 0) return;
            
            disableAmount--;
            StartCoroutine(nameof(DisableObject));
        }

        private IEnumerator DisableObject()
        {
            yield return _disableDelay;
            gameObject.SetActive(false);
        }
    }
}
