using System;
using Unity.Mathematics;
using UnityEngine;

namespace Src.Scripts.Utility
{
    [RequireComponent(typeof(Renderer))]
    public class ScrollingTexture : MonoBehaviour
    {
        public float xSpeed = 1;
        public float ySpeed = 1;
        public bool scroll;

        private Vector2 _offset;
        private Material _mat;
        private void Awake()
        {
            TryGetComponent(out Renderer renderer);
            _mat = renderer.material;
            _offset = _mat.mainTextureOffset;
        }

        void Update()
        {
            if (!scroll)
            {
                return;
            }

            _offset.x = Time.time * xSpeed;
            _offset.y = Time.time * ySpeed;
            _mat.mainTextureOffset = _offset;
        }
    }
}
