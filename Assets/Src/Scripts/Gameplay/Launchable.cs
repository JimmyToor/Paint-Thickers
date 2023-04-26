using System;
using System.Collections;
using UnityEngine;

namespace Gameplay
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
        public Transform fxPosition;

        private PlayerEvents _playerEvents;
        private Vector3 _startPos;
        private float _progress;
        private LaunchableParams _launchParams;
    

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

        private void Update()
        {
            if (isLaunched)
            {
                // Increment our progress from 0 at the start, to 1 when we arrive.
                _progress = Mathf.Min(_progress + Time.deltaTime * _launchParams.stepScale, 1.0f);

                Vector3 nextPos = Vector3.Lerp(_startPos, _launchParams.endPos, _progress);

                // Add vertical arc
                nextPos.y += _launchParams.launchArc.Evaluate(_progress)*_launchParams.arcScale;
                transform.position = nextPos;

                if (_progress >= 1) // We've landed, resume normal movement
                {
                    if (_playerEvents != null)
                    {
                        _playerEvents.OnLand();
                    }
                }
            }
        }

        public void Launch(LaunchableParams launchParameters)
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
        private IEnumerator BuildUp()
        {
            yield return new WaitForSeconds(_launchParams.buildupTime);
            isLaunched = true;
        }

        public void Land()
        {
            isLaunched = false;
            SpawnFXAtFeet();
        }

        private void SpawnFXAtFeet()
        {
            Vector3 fxPos = fxPosition.position;
            fxPos.y = transform.position.y;
            AudioSource.PlayClipAtPoint(landingAudioClip, fxPos);
            Instantiate(landingVFX, fxPos, Quaternion.identity);
        }

    }
}
