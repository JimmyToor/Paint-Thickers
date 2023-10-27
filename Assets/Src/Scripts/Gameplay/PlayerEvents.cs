using System;
using Src.Scripts.Utility;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Src.Scripts.Gameplay
{
    public class PlayerEvents : MonoBehaviour
    {
        public InputActionAsset inputs;
        public PauseHandler pauseHandler;
        public event Action Land;
        public event Action Squid;
        public event Action Stand;
        public event Action<Vector3> Move;
        public event Action Launch;
        public event Action LauncherActivated;
        public event Action<float> TakeHit;
        
        private InputAction _leftHandMove;
        private InputAction _leftHandTurn;
        private InputAction _leftHandSquid;
        private InputAction _rightHandMove;
        private InputAction _rightHandTurn;
        private InputAction _rightHandSquid;
    
        void Awake()
        {
            _leftHandMove = inputs.FindAction("XRI LeftHand/Move");
            _leftHandTurn = inputs.FindAction("XRI LeftHand/Turn");
            _leftHandSquid = inputs.FindAction("XRI LeftHand/Squid");
            _rightHandMove = inputs.FindAction("XRI RightHand/Move");
            _rightHandTurn = inputs.FindAction("XRI RightHand/Turn");
            _rightHandSquid = inputs.FindAction("XRI RightHand/Squid");
        }

        private void OnEnable()
        {
            _leftHandSquid.performed += OnSquid;
            _rightHandSquid.performed += OnSquid;
            _leftHandSquid.canceled += OnStand;
            _rightHandSquid.canceled += OnStand;
            _leftHandMove.performed +=  OnMove;
            _rightHandMove.performed +=  OnMove;
        }

        private void OnDisable() 
        {
            _leftHandSquid.performed -= OnSquid;
            _rightHandSquid.performed -= OnSquid;
            _leftHandSquid.canceled -= OnStand;
            _rightHandSquid.canceled -= OnStand;
            _leftHandMove.performed -=  OnMove;
            _rightHandMove.performed -=  OnMove;
        }

        public void OnSquid(InputAction.CallbackContext ctx = default)
        {
            Squid?.Invoke();
        }

        public void OnStand(InputAction.CallbackContext ctx = default)
        {
            Stand?.Invoke();
        }

        public void OnMove(InputAction.CallbackContext ctx = default)
        {
            Move?.Invoke(new Vector3(ctx.ReadValue<Vector2>().x, 0f, ctx.ReadValue<Vector2>().y));
        }

        public void OnLaunch()
        {   
            Launch?.Invoke();
        }
        
        public void OnLauncherActivated()
        {   
            LauncherActivated?.Invoke();
        }

        public void OnLand()
        {
            Land?.Invoke();
        }

        public void OnTakeHit(float damage)
        {
            TakeHit?.Invoke(damage);
        }

        public void OnDeath()
        {
            Stand?.Invoke();
        }
        
        public void DisableStand()
        {
            _leftHandSquid.canceled -= OnStand;
            _rightHandSquid.canceled -= OnStand;
        }
        
        public void EnableStand()
        {
            _leftHandSquid.canceled += OnStand;
            _rightHandSquid.canceled += OnStand;
            // Stand the player up if they're not holding the button
            if (!_leftHandSquid.triggered && !_rightHandSquid.triggered)
            {
                OnStand();
            }
        }
        
        public void DisableInputMovement()
        {
            _leftHandMove.Disable();
            _leftHandTurn.Disable();
            _rightHandMove.Disable();
            _rightHandTurn.Disable();
        }

        public void EnableInputMovement()
        {
            _leftHandMove.Enable();
            _leftHandTurn.Enable();
            _rightHandMove.Enable();
            _rightHandTurn.Enable();
        }
        
        public void DisableSquid()
        {
            _leftHandSquid.Disable();
            _rightHandSquid.Disable();
        }
        
        public void EnableSquid()
        {
            _leftHandSquid.Enable();
            _rightHandSquid.Enable();
        }
    }
}
