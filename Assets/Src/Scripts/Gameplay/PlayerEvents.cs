using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Src.Scripts.Gameplay
{
    [RequireComponent(typeof(Player))]
    public class PlayerEvents : MonoBehaviour
    {
        public InputActionAsset inputs;
        public event Action Land;
        public event Action Squid;
        public event Action Stand;
        public event Action<Vector3> Move;
        public event Action Launch;
        public event Action<float> TakeHit;
    
        void Start()
        {
            inputs.FindAction("Squid").performed += OnSquid;
            inputs.FindAction("Squid").canceled += OnStand;
            inputs.FindAction("Move").performed +=  OnMove;
        }

        private void OnDisable() 
        {
            inputs.FindAction("Squid").performed -= OnSquid;
            inputs.FindAction("Squid").canceled -= OnStand;
            inputs.FindAction("Move").performed -= OnMove;
        }

        public void OnSquid(InputAction.CallbackContext ctx)
        {
            Squid?.Invoke();
        }

        public void OnStand(InputAction.CallbackContext ctx)
        {
            Stand?.Invoke();
        }

        public void OnMove(InputAction.CallbackContext ctx)
        {
            Move?.Invoke(new Vector3(ctx.ReadValue<Vector2>().x, 0f, ctx.ReadValue<Vector2>().y));
        }

        public void OnLaunch()
        {   
            Stand?.Invoke();
            Launch?.Invoke();
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
    }
}
