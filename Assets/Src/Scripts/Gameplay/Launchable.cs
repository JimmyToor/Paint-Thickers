using System;
using System.Collections;
using UnityEngine;

namespace Src.Scripts.Gameplay
{
    public class Launchable : MonoBehaviour
    {
        [Serializable]
        public struct LaunchableParams
        {
            [HideInInspector]
            public Vector3 endPos;
            public AnimationCurve launchArc;
            [HideInInspector]
            public float stepScale;
            public float arcScale;
            public float buildupTime;
        }

        [HideInInspector]
        public bool isLaunched;
        public bool canLaunch;
        public AudioClip landingAudioClip;
        public GameObject landingVFX;
        public Transform VFXPosition;

        protected PlayerEvents _playerEvents;
        protected Vector3 _startPos;
        protected float _progress;
        protected LaunchableParams _launchParams;
    

        private void OnEnable()
        {
            if (TryGetComponent(out _playerEvents))
                SetupEvents();
        }

        private void SetupEvents()
        {
            _playerEvents.Land += Land;
            _playerEvents.Squid += () => canLaunch = true;
            _playerEvents.Stand += () => canLaunch = false;
        }

        private void OnDisable()
        {
            if (_playerEvents != null)
            {
                DisableEvents();
            }
        }

        private void DisableEvents()
        {
            if (_playerEvents != null)
            {
                _playerEvents.Land -= Land;
            }
        }
        
        private void FixedUpdate()
        {
            if (isLaunched)
            {
                UpdateLaunchProgress();
            }
        }

        /// <summary>
        /// Launch the object in an arc by moving it in a straight line towards the target and adjusting the
        /// y-position over time.
        /// </summary>
        protected virtual void UpdateLaunchProgress()
        {
            // Increment our progress from 0 at the start, to 1 when we arrive.
            _progress = Mathf.Min(_progress + Time.deltaTime * _launchParams.stepScale, 1.0f);

            Vector3 nextPos = Vector3.Lerp(_startPos, _launchParams.endPos, _progress);

            // Add vertical arc
            nextPos.y += _launchParams.launchArc.Evaluate(_progress) * _launchParams.arcScale;
            transform.position = nextPos;

            if (_progress >= 1) // We've landed, resume normal movement
            {
                if (_playerEvents != null)
                {
                    _playerEvents.OnLand();
                }
            }
        }

        public virtual void Launch(LaunchableParams launchParameters)
        {
            _launchParams = launchParameters;
            _startPos = transform.position;
            _progress = 0;
            canLaunch = false;

            if (_playerEvents != null)
            {
                _playerEvents.OnLaunch();
            }
            StartCoroutine(BuildUp());
        }

        // Wait while the launcher plays effects
        protected IEnumerator BuildUp()
        {
            yield return new WaitForSeconds(_launchParams.buildupTime);
            isLaunched = true;
        }

        public void Land()
        {
            isLaunched = false;
            SpawnFXAtFeet();
        }

        protected virtual void SpawnFXAtFeet()
        {
            Vector3 fxPos = VFXPosition.position;
            AudioSource.PlayClipAtPoint(landingAudioClip, fxPos);
            Instantiate(landingVFX, fxPos, Quaternion.identity);
        }

    }
}
