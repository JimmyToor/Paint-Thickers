using Paintz_Free.Scripts;
using UnityEngine;

namespace Src.Scripts.Gameplay
{
    /// <summary>
    /// This should be used to manage the team channel of painter scripts and set the color to match
    /// </summary>
    public class PaintColorManager : MonoBehaviour
    {
        public ParticlePainter partPainter;
        public Renderer paintRenderer;
        public int PaintChannel { get; set; }

        private Material[] _paintMats;

        private Brush _brush;
        // Start is called before the first frame update
        void Start()
        {
            if (partPainter != null || TryGetComponent(out partPainter))
            {
                _brush = partPainter.brush;
            }

            if (paintRenderer != null || TryGetComponent(out paintRenderer))
            {
                _paintMats = paintRenderer.materials;
            }
        }

        public void UpdateColorChannel(int newChannel)
        {
            PaintChannel = newChannel;
            Color newColor = GameManager.Instance.GetTeamColor(PaintChannel);
            if (_paintMats != null && _paintMats.Length > 0)
            {
                foreach (var mat in _paintMats)
                {
                    mat.color = newColor;
                }
            }

            if (_brush != null)
            {
                _brush.splatChannel = PaintChannel;
            }
        }
    }
}
