using System;
using System.Collections;
using UnityEngine;

namespace Src.Scripts.Utility
{
    public class TimedDisable : MonoBehaviour
    {
        public float lifetime;

        private WaitForSeconds _disableDelay;
        
        private void Start()
        {
            _disableDelay = new WaitForSeconds(lifetime);
        }

        void OnEnable()
        {
            StartCoroutine(nameof(DisableObject));
        }

        private IEnumerator DisableObject()
        {
            yield return _disableDelay;
            gameObject.SetActive(false);
        }
    }
}
