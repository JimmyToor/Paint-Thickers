using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Src.Scripts.Gameplay
{
    [RequireComponent(typeof(XRRig), typeof(CharacterController))]
    public class PlayerLaunchable : Launchable
    {
        private PlayerEvents _playerEvents;

        private XRRig _xrRig;
        private CharacterController _charController;
        private Action _makeLaunchable;
        private Action _makeUnlaunchable;
        
        private void Awake()
        {
            _xrRig = GetComponent<XRRig>();
            _charController = _xrRig.gameObject.GetComponent<CharacterController>();
            _makeLaunchable = () => canLaunch = true;
            _makeUnlaunchable = () => canLaunch = false;
        }
        
        private void OnEnable()
        {
            if (TryGetComponent(out _playerEvents))
                SetupEvents();
        }

        private void SetupEvents()
        {
            _playerEvents.Land += Land;
            _playerEvents.Squid += _makeLaunchable;
            _playerEvents.Stand += _makeUnlaunchable;
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

        protected override void UpdateLaunchProgress()
        {
            // Increment our progress from 0 at the start, to 1 when we arrive.
            _progress = Mathf.Min(_progress + Time.deltaTime * _launchParams.stepScale, 1.0f);

            Vector3 nextPos = Vector3.Lerp(_startPos, _launchParams.endPos, _progress);

            // Add vertical arc
            nextPos.y += (_launchParams.launchArc.Evaluate(_progress) * _launchParams.arcScale) + _charController.height;
            _xrRig.MoveCameraToWorldLocation(nextPos);

            if (!(_progress >= 1) || _playerEvents == null) return;
            
            // We've landed, resume normal movement
            isLaunched = false;
            _playerEvents.OnLand();
        }

        public override void Launch(LaunchableParams launchParameters)
        {
            _launchParams = launchParameters;
            _startPos = _xrRig.cameraGameObject.transform.position;
            _progress = 0;
            canLaunch = false;

            if (_playerEvents != null)
            {
                _playerEvents.OnLauncherActivated();
            }
            StartCoroutine(BuildUp());
        }

        protected override IEnumerator BuildUp()
        {
            yield return base.BuildUp();
            if (_playerEvents != null)
            {
                _playerEvents.OnLaunch();
            }
        }
        
        protected override void SpawnFXAtFeet()
        {
            Vector3 fxPos = VFXPosition.position;
            fxPos.y -= _charController.height;
            AudioSource.PlayClipAtPoint(landingAudioClip, fxPos);
            Instantiate(landingVFX, fxPos, Quaternion.identity);
            
        }
    }
}
