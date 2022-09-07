using UnityEngine;

namespace Gameplay
{
    public class PaintHandler : MonoBehaviour
    {
        public ParticlePainter partPainter;
        public Renderer paintRenderer;
        [HideInInspector] public int paintChannel;

        private Material[] _paintMats;

        private Brush _brush;
        // Start is called before the first frame update
        void Start()
        {
            if (partPainter != null || TryGetComponent(out partPainter))
            {
                _brush = partPainter.brush;
            }

            if (paintRenderer == null)
            {
                if (TryGetComponent(out paintRenderer))
                {
                    _paintMats = paintRenderer.materials;
                }
            }
        }

        public void UpdateColorChannel(int newChannel)
        {
            paintChannel = newChannel;
            Color newColor = GameManager.Instance.GetTeamColor(paintChannel);
            if (_paintMats != null && _paintMats.Length > 0)
            {
                foreach (var mat in _paintMats)
                {
                    mat.color = newColor;
                }
            }

            if (_brush != null)
            {
                _brush.splatChannel = paintChannel;
            }
        }
    }
}
