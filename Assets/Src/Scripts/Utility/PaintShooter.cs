using Paintz_Free.Scripts;
using Src.Scripts.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Src.Scripts.Utility
{
    public class PaintShooter : MonoBehaviour
    {
        public Brush brush;

        private Camera _cam;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
        }

        private void Update()
        {
            if (Mouse.current.leftButton.isPressed)
            {
                PaintCursor();
            }
        }

        private void PaintCursor()
        {
            Ray ray = _cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            PaintTarget.PaintRaycast(ray, brush);
        }
    
        private void PaintRaycast(Ray ray)
        {
            if (!Physics.Raycast(ray, out var hit, 10000)) return;
        
            PaintTarget paintTarget = hit.collider.gameObject.GetComponent<PaintTarget>();
        
            if (!paintTarget) return;
            paintTarget.PaintObject(hit.point, hit.normal, brush);
            Debug.Log("Painted " + paintTarget.name);
        }
    }
}

