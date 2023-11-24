using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

// Based on 'PaintTarget.cs' from https://assetstore.unity.com/packages/tools/paintz-free-145977
// and https://github.com/SquirrelyJones/Splatoonity/blob/master/Assets/Splatoonity/Scripts/SplatManager.cs
namespace Src.Scripts.Gameplay
{
    public enum TextureSize
    {
        Texture64x64 = 64,
        Texture128x128 = 128,
        Texture256x256 = 256,
        Texture512x512 = 512,
        Texture1024x1024 = 1024,
        Texture2048x2048 = 2048,
        Texture4096x4096 = 4096
    }
    
    public class PaintTarget : MonoBehaviour
    {
        // Affects paint resolution. Higher values result in less choppy edges.
        public TextureSize paintTextureSize = TextureSize.Texture256x256;
        // Affects paint border quality. Higher values result in better visual depth.
        public TextureSize renderTextureSize = TextureSize.Texture256x256;

        public bool setupOnStart = true;
        public bool useBakedPaintMap;
        public RenderTexture paintMap;
        public ComputeShader paintComputeShader;
        public Texture2D bakedPaintMap;
        public int maxNearSplats = 16; // Maximum number of nearby targets to paint in addition to the primary target.
        
        private bool _validTarget;
        private Collider[] _colliders;
        private RenderTexture _worldPosTex;
        private RenderTexture _worldPosTexTemp;
        private RenderTexture _worldTangentTex;
        private RenderTexture _worldBinormalTex;
        private List<Paint> _paintList = new List<Paint>();
        private bool _setupComplete;
        private Renderer _paintRenderer;

        private Material _paintBlitMaterial;
        private Material _worldPosMaterial;
        private Material _worldTangentMaterial;
        private Material _worldBiNormalMaterial;
        
        private static uint _xGroupSize, _yGroupSize;
        private static int _paintKernel;
        private const int PaintMatrixBufferStride = sizeof(float) * 16;
        private const int ScaleBiasBufferStride = sizeof(float) * 4;
        private const int PaintColorBufferStride = sizeof(float) * 4;
        private const int StepsBufferStride = sizeof(int);
        private static readonly int PaintMap = Shader.PropertyToID("_PaintMap");
        private static readonly int WorldPosTex = Shader.PropertyToID("_WorldPosTex");
        private static readonly int WorldTangentTex = Shader.PropertyToID("_WorldTangentTex");
        private static readonly int WorldBinormalTex = Shader.PropertyToID("_WorldBinormalTex");
        private static readonly int SplatTexSize = Shader.PropertyToID("_SplatTexSize");
        private static readonly int ScaleBias = Shader.PropertyToID("scale_bias");
        private static readonly int PaintWorldToObject = Shader.PropertyToID("paint_world_to_object");
        private static readonly int PaintPattern = Shader.PropertyToID("paint_pattern");
        private static readonly int Paintmap = Shader.PropertyToID("paintmap");
        private static readonly int WorldPosTexCompute = Shader.PropertyToID("world_pos_tex");
        private static readonly int PaintChannelMask = Shader.PropertyToID("paint_channel_mask");
        private static readonly int NumSplats = Shader.PropertyToID("num_splats");
        private static readonly int Resolution = Shader.PropertyToID("resolution");
        private static readonly int Steps = Shader.PropertyToID("steps");

        private static GameObject _splatObject;
        private Camera _renderCamera;
        

        private void Awake()
        {
            _colliders = new Collider[maxNearSplats];
        }

        private void Start()
        {
            CheckValid();
            if (setupOnStart) SetupPaint();
            
            paintComputeShader = (ComputeShader) Resources.Load("PaintCompute");
            _paintKernel = paintComputeShader.FindKernel("BrushPaint");
            paintComputeShader.GetKernelThreadGroupSizes(_paintKernel,
                out _xGroupSize, out _yGroupSize, out _);
            
            // Paint around 100 times per second to make paint increments look smooth
            InvokeRepeating(nameof(PaintSplats),1f,0.01f);
        }

        /// <summary>
        /// Attempt to paint the target object.
        /// </summary>
        /// <param name="target">The GameObject to be painted.</param>
        /// <param name="point">The spot to paint.</param>
        /// <param name="normal">The normal of the paint.</param>
        /// <param name="brush">The brush parameters to use.</param>
        private static void Paint(GameObject target, Vector3 point, Vector3 normal, Brush brush)
        {
            target.TryGetComponent(out PaintTarget paintTarget);
            if (paintTarget != null)
            {
                paintTarget.PaintObject(point, normal , brush);
            }
        }

        /// <summary>
        /// Paints all objects in a sphere around the passed <paramref name="point"/>.
        /// Useful for painting multiple objects at once with a seemingly contiguous paint splat.
        /// </summary>
        /// <param name="point">The spot to paint.</param>
        /// <param name="brush">The brush parameters to use.</param>
        /// <param name="normal">The normal of the paint.</param>
        public void PaintSphere(Vector3 point, Vector3 normal, Brush brush)
        {
            // SphereCast has issues with colliders inside the sphere at the start of the cast,
            // so OverlapSphere is used instead
            int size = Physics.OverlapSphereNonAlloc(point, brush.splatScale/2, _colliders, LayerMask.GetMask("Terrain"));
            for (var i = 0; i < size; i++)
            {
                if (_colliders[i].CompareTag("Terrain"))
                {
                    Paint(_colliders[i].gameObject, point, normal, brush);
                }
            }
        }

        /// <summary>
        /// Shoots a ray that paints the first object it hits.
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="brush"></param>
        /// <param name="multi">Allows resulting splat to spread across multiple objects.</param>
        public static void PaintRaycast(Ray ray, Brush brush, bool multi = true)
        {
            if (!Physics.Raycast(ray, out RaycastHit hit, 10000)) return;
            
            PaintTarget paintTarget = hit.collider.gameObject.GetComponent<PaintTarget>();

            if (!paintTarget) return;

            if (!multi)
            {
                paintTarget.PaintObject(hit.point, hit.normal, brush);
                return;
            }
            
            paintTarget.PaintSphere(hit.point, hit.normal, brush);
        }
    
        /// <summary>
        /// Place paint at the passed Vector3 position.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="normal"></param>
        /// <param name="brush"></param>
        private void PaintObject(Vector3 point, Vector3 normal, Brush brush)
        {
            if (!_validTarget) return;

            if (_splatObject == null)
            {
                _splatObject = new GameObject();
                _splatObject.name = "splatObject";
                _splatObject.hideFlags = HideFlags.HideInHierarchy;
            }

            Transform splatTransform = _splatObject.transform;
            splatTransform.position = point;

            Vector3 leftVec = Vector3.Cross(normal, Vector3.up);
            if (leftVec.magnitude > 0.001f)
                splatTransform.rotation = Quaternion.LookRotation(leftVec, normal);
            else
                splatTransform.rotation = Quaternion.identity;

            float randScale = Random.Range(brush.splatRandomScaleMin, brush.splatRandomScaleMax);
            splatTransform.RotateAround(point, normal, brush.splatRotation);
            splatTransform.RotateAround(point, normal, Random.Range(-brush.splatRandomRotation, brush.splatRandomRotation));
            splatTransform.localScale = new Vector3(randScale, randScale, randScale) * brush.splatScale;

            Paint newPaint = new Paint
            {
                paintMatrix = splatTransform.worldToLocalMatrix,
                channelMask = brush.GetMask(),
                scaleBias = brush.GetTile(),
                brush = brush,
                stepsRemaining = brush.steps // Don't want to change the brush's step amount when painting
            };

            PaintSplat(newPaint);
        }

        public static void ClearAllPaint()
        {
            PaintTarget[] targets = FindObjectsOfType<PaintTarget>();

            foreach (PaintTarget target in targets)
            {
                if (!target._validTarget) continue;
                target.ClearPaint();
            }
        }

        /// <summary>
        /// Creates and configures a camera to be able to render out the required textures
        /// </summary>
        private void SetupCamera()
        {
            // Need to use an existing camera with Target Eye set to None because the stereoTargetEye property
            // does not actually change when modified via script
            GameObject cam = GameObject.Find("PaintCamera");
            if (cam == null)
            {
                Debug.LogError("PaintCamera was not found!");
                return;
            }
            cam.TryGetComponent(out _renderCamera);
            if (_renderCamera == null)
            {
                Debug.LogError("PaintCamera has no Camera component!");
                return;
            }
            _renderCamera.clearFlags = CameraClearFlags.SolidColor;
            _renderCamera.backgroundColor = new Color(0, 0, 0, 0);
            _renderCamera.orthographic = true;
            _renderCamera.nearClipPlane = 0.01f;
            _renderCamera.farClipPlane = 1.0f;
            _renderCamera.orthographicSize = 1.0f;
            _renderCamera.aspect = 1.0f;
            _renderCamera.useOcclusionCulling = false;
            _renderCamera.enabled = false;
            _renderCamera.cullingMask = 1 << LayerMask.NameToLayer("Nothing");
        }

        private void CheckValid()
        {
            if (!TryGetComponent(out _paintRenderer))
            {
                Debug.LogError("No renderer on " + name, gameObject);
                return;
            }
            
            if (!TryGetComponent(out MeshCollider _))
            {
                Debug.LogError("No MeshCollider on " + name, gameObject);
                return;
            }

            _validTarget = true;
        }

        private void SetupPaint()
        {
            SetupCamera();
            CreateMaterials();
            CreateTextures();

            RenderTextures();
            _setupComplete = true;
        }

        private void CreateMaterials()
        {
            _paintBlitMaterial = new Material(Shader.Find("Hidden/PaintBlit"));
            _worldPosMaterial = new Material(Shader.Find("Hidden/PaintPos"));
            _worldTangentMaterial = new Material(Shader.Find("Hidden/PaintTangent"));
            _worldBiNormalMaterial = new Material(Shader.Find("Hidden/PaintBinormal"));
        }

        private void CreateTextures()
        {
            paintMap = new RenderTexture((int)paintTextureSize, (int)paintTextureSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                enableRandomWrite = true
            };
            paintMap.Create();

            if (useBakedPaintMap)
                Graphics.Blit(bakedPaintMap, paintMap);
            

            _worldPosTex = new RenderTexture((int)renderTextureSize, (int)renderTextureSize, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear)
            {
                enableRandomWrite = true
            };
            _worldPosTex.Create();
            
            _worldPosTexTemp = new RenderTexture((int)renderTextureSize, (int)renderTextureSize, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear)
            {
                enableRandomWrite = true
            };
            _worldPosTexTemp.Create();
            _worldTangentTex = new RenderTexture((int)renderTextureSize, (int)renderTextureSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            _worldTangentTex.Create();
            _worldBinormalTex = new RenderTexture((int)renderTextureSize, (int)renderTextureSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            _worldBinormalTex.Create();

            foreach (Material mat in _paintRenderer.materials)
            {
                mat.SetTexture(PaintMap, paintMap);
                mat.SetTexture(WorldPosTex, _worldPosTex);
                mat.SetTexture(WorldTangentTex, _worldTangentTex);
                mat.SetTexture(WorldBinormalTex, _worldBinormalTex);
                mat.SetVector(SplatTexSize, new Vector4((int)paintTextureSize, (int)paintTextureSize, 0, 0));
            }
        }

        private void RenderTextures()
        {
            transform.hasChanged = false;

            CommandBuffer cb = new CommandBuffer();

            // basically unwraps the object and renders that on the camera, which is rendered onto worldPosTex
            cb.SetRenderTarget(_worldPosTex);
            cb.ClearRenderTarget(true, true, new Color(0, 0, 0, 0));
            for (int i = 0; i < _paintRenderer.materials.Length; i++)
            {
                cb.DrawRenderer(_paintRenderer, _worldPosMaterial, i);
            }

            cb.SetRenderTarget(_worldTangentTex);
            cb.ClearRenderTarget(true, true, new Color(0, 0, 0, 0));
            for (int i = 0; i < _paintRenderer.materials.Length; i++)
            {
                cb.DrawRenderer(_paintRenderer, _worldTangentMaterial, i);
            }

            cb.SetRenderTarget(_worldBinormalTex);
            cb.ClearRenderTarget(true, true, new Color(0, 0, 0, 0));
            for (int i = 0; i < _paintRenderer.materials.Length; i++)
            {
                cb.DrawRenderer(_paintRenderer, _worldBiNormalMaterial, i);
            }

            // Only have to render the camera once!
            _renderCamera.AddCommandBuffer(CameraEvent.AfterEverything, cb);
            _renderCamera.Render();
            _renderCamera.RemoveAllCommandBuffers();

            // Bleed the world position out 2 pixels
            _paintBlitMaterial.SetVector(SplatTexSize, new Vector2((int)paintTextureSize,
                (int)paintTextureSize));
            Graphics.Blit(_worldPosTex, _worldPosTexTemp, _paintBlitMaterial, 2);
            Graphics.Blit(_worldPosTexTemp, _worldPosTex, _paintBlitMaterial, 2);
        }

        public void ClearPaint()
        {
            if (_setupComplete)
            {
                Graphics.Blit(paintMap, paintMap, _paintBlitMaterial, 1);
            }
        }
        
        /// <summary>
        /// Returns the current splat texture as a Texture2D
        /// </summary>
        /// <returns></returns>
        public Texture2D CreateBakedPaintMap()
        {        
            RenderTexture.active = paintMap;
            Texture2D bakedTexture = new Texture2D(paintMap.width, paintMap.height, TextureFormat.ARGB32, false,true);
            bakedTexture.ReadPixels(new Rect(0, 0, paintMap.width, paintMap.height), 0, 0);
            bakedTexture.Apply();
            RenderTexture.active = null;
            return bakedTexture;
        }

        public void PaintSplat(Paint paint)
        {
            _paintList.Add(paint);
        }

        private void PaintSplats()
        {
            if (!_validTarget) return;

            if (_paintList.Count <= 0) return;
            
            if (!_setupComplete) SetupPaint();

            if (transform.hasChanged) RenderTextures();

            Matrix4x4[] paintMatrixArray = new Matrix4x4[10];
            Vector4[] paintScaleBiasArray = new Vector4[10];
            Vector4[] paintChannelMaskArray = new Vector4[10];
            int[] stepsArray = new int[10];

            // Render up to 10 splats per frame of the same texture!
            int numSplats = 0;
            Texture2D paintPattern = _paintList[0].brush.paintPattern;

            for (int s=0; s < _paintList.Count; s++)
            {
                if (numSplats >= 10) break;
                if (_paintList[s].brush.paintPattern != paintPattern)
                {
                    continue;
                }
                
                paintMatrixArray[numSplats] = _paintList[s].paintMatrix;
                paintScaleBiasArray[numSplats] = _paintList[s].scaleBias;
                paintChannelMaskArray[numSplats] = _paintList[s].channelMask;
                stepsArray[numSplats] = _paintList[s].stepsRemaining;
                numSplats++;

                if (--_paintList[s].stepsRemaining < 0)
                {
                    _paintList.RemoveAt(s);
                }
            }

            ComputeBuffer paintWorldToObjectBuffer = new ComputeBuffer(paintMatrixArray.Length, PaintMatrixBufferStride); 
            ComputeBuffer paintScaleBiasBuffer = new ComputeBuffer(paintScaleBiasArray.Length, ScaleBiasBufferStride);
            ComputeBuffer paintChannelMaskBuffer = new ComputeBuffer(paintChannelMaskArray.Length, PaintColorBufferStride);
            ComputeBuffer stepsBuffer = new ComputeBuffer(stepsArray.Length, StepsBufferStride);
            paintWorldToObjectBuffer.SetData(paintMatrixArray);
            paintScaleBiasBuffer.SetData(paintScaleBiasArray);
            paintChannelMaskBuffer.SetData(paintChannelMaskArray);
            stepsBuffer.SetData(stepsArray);
            
            paintComputeShader.SetTexture(_paintKernel, WorldPosTexCompute, _worldPosTex);
            paintComputeShader.SetTexture(_paintKernel, Paintmap, paintMap);
            paintComputeShader.SetTexture(_paintKernel,PaintPattern, paintPattern);
            paintComputeShader.SetBuffer(_paintKernel, PaintWorldToObject, paintWorldToObjectBuffer);
            paintComputeShader.SetBuffer(_paintKernel, ScaleBias, paintScaleBiasBuffer);
            paintComputeShader.SetBuffer(_paintKernel, PaintChannelMask, paintChannelMaskBuffer);
            paintComputeShader.SetBuffer(_paintKernel, Steps, stepsBuffer);
            paintComputeShader.SetFloat(NumSplats, numSplats);
            paintComputeShader.SetFloats(Resolution,paintMap.width, paintMap.height);
            paintComputeShader.Dispatch(_paintKernel,
                Mathf.CeilToInt(paintMap.width / (float) _xGroupSize),
                Mathf.CeilToInt(paintMap.height / (float) _yGroupSize), 1);
            
            paintWorldToObjectBuffer.Dispose();
            paintChannelMaskBuffer.Dispose();
            paintScaleBiasBuffer.Dispose();
            stepsBuffer.Dispose();
        }
    }
}