using UnityEngine;
using UnityEngine.InputSystem;

namespace Src.Scripts.Utility
{
    public class PaintShooter : MonoBehaviour
    {
        public Brush brush;
    
        private void Update()
        {
            if (Mouse.current.leftButton.isPressed)
            {
                PaintCursor();
            }
        }

        public void PaintCursor()
        {
            if (Camera.main == null)
            {
                Debug.Log("Warning: No Main Camera tagged");
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            PaintRaycast(ray);
        }
    
        private void PaintRaycast(Ray ray)
        {
            if (!Physics.Raycast(ray, out var hit, 10000)) return;
        
            PaintTarget paintTarget = hit.collider.gameObject.GetComponent<PaintTarget>();
        
            if (!paintTarget) return;
            PaintTarget.PaintObject(paintTarget, hit.point, hit.normal, brush);
            Debug.Log("Painted " + paintTarget.name);
        }
    }
}

