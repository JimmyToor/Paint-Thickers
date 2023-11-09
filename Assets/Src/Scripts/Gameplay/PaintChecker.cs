using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace Src.Scripts.Gameplay
{
    /// <summary>
    /// Non-blocking paint channel checking directly below the check origin.
    /// </summary>
    public class PaintChecker : MonoBehaviour
    {
        [Tooltip("Continually perform checks.")]
        public bool keepUpdated;
        [Tooltip("Get the material color below.")]
        public bool getColor = true;
        [Tooltip("The origin point of the check.")]
        public Transform checkOrigin;
        [Tooltip("The check will go down relative to this transform.")]
        public Transform referenceTransform;
        [Tooltip("Only these layers can be checked for paint.")]
        public LayerMask validLayers;
        [HideInInspector]
        public Vector3 hitPosition;
        
        /// <summary>
        /// The current paint channel of the surface below checkOrigin.
        /// </summary>
        [HideInInspector]
        public int currChannel = -1;
        
        /// <summary>
        /// The current normal of the surface below checkOrigin.
        /// <remarks>Zero vector indicates no normal</remarks>
        /// </summary>
        [HideInInspector] 
        public Vector3 currNormal = Vector3.zero;
        public float paintCheckDistance;
        
        private AsyncGPUReadbackRequest _request;
        private ComputeShader _getPixelComputeShader;
        private ComputeBuffer _pixelColorBuffer;
        private float[] _paintPixelArray;
        private RaycastHit _hit;
        private bool _checkReady = true;
        private GameObject _lastHitObject;
        private PaintTarget _paintTarget;
        
        private const int GetPixelBufferStride = sizeof(int)*4;

        private static int _getPixelKernel;

        private static readonly string GetPixelComputePath = "GetPixelCompute";
        private static readonly string GetPixelKernelName = "GetPixel";
        private static readonly int PixelTexture = Shader.PropertyToID("pixel_texture");
        private static readonly int PixelCoordinateX = Shader.PropertyToID("pixel_coordinateX");
        private static readonly int PixelCoordinateY = Shader.PropertyToID("pixel_coordinateY");
        private static readonly int PixelColor = Shader.PropertyToID("pixel_color");

        private void FixedUpdate()
        {
            CheckBelow();
        }

        private void CheckBelow()
        {
            if (!keepUpdated || !_checkReady || !_request.done) return;
            
            if (Physics.Raycast(checkOrigin.position, -referenceTransform.up,
                    out _hit, paintCheckDistance,validLayers)
                    && _hit.transform != null)
            {
                hitPosition = _hit.point;
                if (getColor)
                {
                    RayChannel(_hit);
                }

                currNormal = _hit.normal;
            }
            else
            {
                currChannel = -1;
                currNormal = Vector3.zero;
            }
        }
        
        void Start()
        {
            _getPixelComputeShader = (ComputeShader) Resources.Load(GetPixelComputePath);
            _getPixelKernel = _getPixelComputeShader.FindKernel(GetPixelKernelName);
            _pixelColorBuffer = new ComputeBuffer(4,GetPixelBufferStride);
        }
        
        /// <summary>
        /// Checks the channel of the point hit by the passed Raycast <paramref name="hit"/>.
        /// </summary>
        /// <remarks>Object hit by ray must have collider, transform, and PaintTarget components.</remarks>
        /// <returns>Return true if the object hit can be checked, false otherwise.</returns>
        public void RayChannel(RaycastHit hit)
        {
            if (!hit.collider || !hit.transform)
            {
                currChannel = -1;
                return;
            }

            if (_hit.transform.gameObject != _lastHitObject)
            {   // Cache the object
                _lastHitObject = _hit.transform.gameObject;
                _paintTarget = _lastHitObject.GetComponent<PaintTarget>();
            }
            
            if (!_paintTarget || !_paintTarget.paintMap)
            {
                currChannel = -1;
                return;
            }
            StartCoroutine(GetPixelColor(_paintTarget, hit));
        }
        
        private IEnumerator GetPixelColor(PaintTarget paintTarget, RaycastHit hit)
        {
            _checkReady = false;
            int x = (int)(hit.lightmapCoord.x * paintTarget.paintMap.width);
            int y = (int)(hit.lightmapCoord.y * paintTarget.paintMap.height);
            
            _getPixelComputeShader.SetBuffer(_getPixelKernel, PixelColor, _pixelColorBuffer);
            _getPixelComputeShader.SetTexture(_getPixelKernel, PixelTexture, paintTarget.paintMap);
            _getPixelComputeShader.SetInt(PixelCoordinateX, x);
            _getPixelComputeShader.SetInt(PixelCoordinateY, y);
            _getPixelComputeShader.Dispatch(_getPixelKernel,1,1,1);
            
            _request = AsyncGPUReadback.Request(_pixelColorBuffer);
            while (!_request.done)
            {
                yield return null;
            }
            _checkReady = true;
            
            if (_request.hasError) yield break;
            
            currChannel = -1;
            var color = _request.GetData<float>().ToArray();
            var pixelColor = new Color(color[0], color[1], color[2], color[3]);
            if (pixelColor.r > .5) currChannel = 0;
            if (pixelColor.g > .5) currChannel = 1;
            if (pixelColor.b > .5) currChannel = 2;
            if (pixelColor.a > .5) currChannel = 3;
        }

        private void OnDestroy()
        {
            _pixelColorBuffer?.Release();
        }
    }
}
