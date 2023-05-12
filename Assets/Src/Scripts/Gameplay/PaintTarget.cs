using System.Collections.Generic;
using Paintz_Free.Scripts;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

// Based on 'PaintTarget.cs' from https://assetstore.unity.com/packages/tools/paintz-free-145977
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
        [Tooltip("Affects paint resolution. Higher values result in less choppy edges.")]
        public TextureSize paintTextureSize = TextureSize.Texture256x256;
        [Tooltip("Affects paint border quality. Higher values result in better visual depth.")]
        public TextureSize renderTextureSize = TextureSize.Texture256x256;

        public bool setupOnStart = true;
        public bool paintAllSplats;
        public bool useBaked;
        
        public ComputeShader paintComputeShader;

        public Texture2D splatTexPick;
        public Texture2D bakedTex;

        private bool _bPickDirty = true;
        private bool _validTarget;
        private bool _bHasMeshCollider;

        private RenderTexture _paintMap;
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

        private Renderer _r;
        private RenderTexture _rt;
        private MeshCollider _mc;
        
        private ComputeBuffer _paintComputeBuffer;
        private static uint _xGroupSize, _yGroupSize;
        private static int _paintKernel;
        private static int _paintMatrixBufferStride;
        private static int _scaleBiasBufferStride;
        private static int _paintColorBufferStride;

        private static GameObject _splatObject;
        private Camera _renderCamera;
        private static readonly int PaintMap = Shader.PropertyToID("_PaintMap");
        private static readonly int WorldPosTex = Shader.PropertyToID("_WorldPosTex");
        private static readonly int WorldTangentTex = Shader.PropertyToID("_WorldTangentTex");
        private static readonly int WorldBinormalTex = Shader.PropertyToID("_WorldBinormalTex");
        private static readonly int SplatTexSize = Shader.PropertyToID("_SplatTexSize");

        // Components may have changed, so make sure we have what we need
        private bool SetComponents()
        {
            _paintRenderer = GetComponent<Renderer>();
            if (!_paintRenderer) return false;

            _mc = GetComponent<MeshCollider>();
            if (!_mc) return false;

            _r = GetComponent<Renderer>();
            if (!_r) return false;

            _rt = (RenderTexture)_r.sharedMaterial.GetTexture(PaintMap);
            return _rt;
        }

        private static Color GetPixelColor(PaintTarget paintTarget, RaycastHit hit)
        {
            if (!paintTarget._validTarget) return Color.clear;

            if (!paintTarget.SetComponents()) return Color.clear;

            UpdatePickColors(paintTarget);
            Texture2D tc = paintTarget.splatTexPick;
            if (!tc)
            {
                Debug.Log("no tc");
                return Color.clear;
            }
            int x = (int)(hit.lightmapCoord.x * tc.width);
            int y = (int)(hit.lightmapCoord.y * tc.height);
            
            return tc.GetPixel(x,y);
            
        }

        public static int RayChannel(RaycastHit hit)
        {
            if (!hit.collider || !hit.transform) return -1;
            PaintTarget paintTarget = hit.collider.gameObject.GetComponent<PaintTarget>();
            if (!paintTarget)
            {
                return -1;
            }
            Color pc = GetPixelColor(paintTarget, hit);
            int l = -1;
            if (pc.r > .5) l = 0;
            if (pc.g > .5) l = 1;
            if (pc.b > .5) l = 2;
            if (pc.a > .5) l = 3;
            return l;
        }

        /// <summary>
        /// Attempt to paint the target object.
        /// </summary>
        /// <param name="target">The GameObject to be painted.</param>
        /// <param name="point">The spot to paint.</param>
        /// <param name="normal">The normal of the paint.</param>
        /// <param name="brush">The brush parameters to use.</param>
        public static void Paint(GameObject target, Vector3 point, Vector3 normal, Brush brush)
        {
            PaintTarget paintTarget;
            target.TryGetComponent(out paintTarget);
            if (paintTarget != null)
            {
                PaintObject(paintTarget, point, normal , brush);
            }
        }

        /// <summary>
        /// Paints all objects in a sphere around the passed <paramref name="point"/>.
        /// Useful for painting multiple objects at once with a seemingly contiguous paint splat.
        /// </summary>
        /// <param name="point">The spot to paint.</param>
        /// <param name="brush">The brush parameters to use.</param>
        /// <param name="normal">The normal of the paint.</param>
        public static void PaintSphere(Vector3 point, Vector3 normal, Brush brush)
        {
            // SphereCast has issues with colliders inside the sphere at the start of the cast,
            // so OverlapSphere is used instead
             Collider[] colliders = Physics.OverlapSphere(point, brush.splatScale,
                 LayerMask.GetMask("Terrain"));
             
             foreach (var collider in colliders)
             {
                 Paint(collider.gameObject, point, normal, brush);
             }
        }

        public static void PaintRaycast(Ray ray, Brush brush, bool multi = true)
        {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10000))
            {
                if (multi)
                {
                    RaycastHit[] hits = Physics.SphereCastAll(hit.point, brush.splatScale, ray.direction);
                    for (int h=0; h < hits.Length; h++)
                    {
                        PaintTarget paintTarget = hits[h].collider.gameObject.GetComponent<PaintTarget>();
                        if (paintTarget != null)
                        {
                            PaintObject(paintTarget, hit.point, hits[h].normal, brush);
                        }
                    }
                }
                else
                {
                    PaintTarget paintTarget = hit.collider.gameObject.GetComponent<PaintTarget>();
                    if (!paintTarget) return;
                    PaintObject(paintTarget, hit.point, hit.normal, brush);
                }
            }
        }
    

        public static void PaintObject(PaintTarget target, Vector3 point, Vector3 normal, Brush brush)
        {
            if (!target) return;
            if (!target._validTarget) return;

            if (_splatObject == null)
            {
                _splatObject = new GameObject();
                _splatObject.name = "splatObject";
                //splatObject.hideFlags = HideFlags.HideInHierarchy;
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

            Paint newPaint = new Paint();
            newPaint.paintMatrix = splatTransform.worldToLocalMatrix;
            newPaint.channelMask = brush.getMask();
            newPaint.scaleBias = brush.getTile();
            newPaint.brush = brush;

            target.PaintSplat(newPaint);
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

        private static void UpdatePickColors(PaintTarget paintTarget)
        {
            if (!paintTarget._validTarget) return;
            if (!paintTarget._bPickDirty) return;
            if (!paintTarget._bHasMeshCollider) return;

            if (!paintTarget.splatTexPick)
            {
                paintTarget.splatTexPick = new Texture2D((int)paintTarget.paintTextureSize, (int)paintTarget.paintTextureSize, TextureFormat.ARGB32, false);
            }

            // Rect rectReadPicture = new Rect(0, 0, rt.width, rt.height);
            // RenderTexture.active = rt;
            // paintTarget.splatTexPick.ReadPixels(rectReadPicture, 0, 0);
            // paintTarget.splatTexPick.Apply();
            
            RenderTexture.active = paintTarget._paintMap;
            paintTarget.splatTexPick = new Texture2D(paintTarget._paintMap.width, paintTarget._paintMap.height, TextureFormat.ARGB32, false,true);
            paintTarget.splatTexPick.ReadPixels(new Rect(0, 0, paintTarget._paintMap.width, paintTarget._paintMap.height), 0, 0);
            paintTarget.splatTexPick.Apply();

            RenderTexture.active = null;
            paintTarget._bPickDirty = false;
        }

        private void CreateCamera()
        {
            GameObject cam = GameObject.Find("PaintCamera");
            if (cam != null)
            {
                _renderCamera = cam.GetComponent<Camera>();
                return;
            }
            GameObject rtCameraObject = new GameObject();
            rtCameraObject.name = "PaintCamera";
            rtCameraObject.transform.position = Vector3.zero;
            rtCameraObject.transform.rotation = Quaternion.identity;
            rtCameraObject.transform.localScale = Vector3.one;
            rtCameraObject.hideFlags = HideFlags.HideInHierarchy;
            _renderCamera = rtCameraObject.AddComponent<Camera>();
            _renderCamera.stereoTargetEye = StereoTargetEyeMask.None;
            _renderCamera.clearFlags = CameraClearFlags.SolidColor;
            _renderCamera.backgroundColor = new Color(0, 0, 0, 0);
            _renderCamera.orthographic = true;
            _renderCamera.nearClipPlane = 0.0f;
            _renderCamera.farClipPlane = 1.0f;
            _renderCamera.orthographicSize = 1.0f;
            _renderCamera.aspect = 1.0f;
            _renderCamera.useOcclusionCulling = false;
            _renderCamera.enabled = false;
            _renderCamera.cullingMask = 1 << LayerMask.NameToLayer("Nothing");
        }

        void CheckValid()
        {
            _paintRenderer = GetComponent<Renderer>();
            if (!_paintRenderer) return;

            MeshCollider mc = GetComponent<MeshCollider>();
            if (mc != null) _bHasMeshCollider = true;

            _validTarget = true;
        }

        private void Start()
        {
            CheckValid();
            if (setupOnStart) SetupPaint();

            paintComputeShader = (ComputeShader) Resources.Load("PaintCompute");
            _paintKernel = paintComputeShader.FindKernel("BrushPaint");
            paintComputeShader.GetKernelThreadGroupSizes(_paintKernel,
                out _xGroupSize, out _yGroupSize, out _);
            _paintMatrixBufferStride = sizeof(float) * 16;
            _scaleBiasBufferStride = sizeof(float) * 4;
            _paintColorBufferStride = sizeof(float) * 4;
        }

        private void SetupPaint()
        {

            CreateCamera();
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
            _paintMap = new RenderTexture((int)paintTextureSize, (int)paintTextureSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                enableRandomWrite = true
            };
            _paintMap.Create();

            RenderTexture.active = _paintMap;
            if (useBaked)
                Graphics.Blit(bakedTex, _paintMap);
            RenderTexture.active = null;

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

            splatTexPick = new Texture2D((int)paintTextureSize, (int)paintTextureSize, TextureFormat.ARGB32, false);
            foreach (Material mat in _paintRenderer.materials)
            {
                mat.SetTexture(PaintMap, _paintMap);
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
            _paintBlitMaterial.SetVector(SplatTexSize, new Vector2((int)paintTextureSize, (int)paintTextureSize));
            Graphics.Blit(_worldPosTex, _worldPosTexTemp, _paintBlitMaterial, 2);
            Graphics.Blit(_worldPosTexTemp, _worldPosTex, _paintBlitMaterial, 2);
        }

        public void ClearPaint()
        {
            if (_setupComplete)
            {
                Graphics.Blit(_paintMap, _paintMap, _paintBlitMaterial, 1);
            }
        }

        // Returns the current splat texture as a Texture2D
        public Texture2D CreateBakedTex()
        {        
            RenderTexture.active = _paintMap;
            Texture2D bakedTexture = new Texture2D(_paintMap.width, _paintMap.height, TextureFormat.ARGB32, false,true);
            bakedTexture.ReadPixels(new Rect(0, 0, _paintMap.width, _paintMap.height), 0, 0);
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

            if (_paintList.Count > 0)
            {
                _bPickDirty = true;

                if (!_setupComplete) SetupPaint();

                if (transform.hasChanged) RenderTextures();

                Matrix4x4[] paintMatrixArray = new Matrix4x4[10];
                Vector4[] paintScaleBiasArray = new Vector4[10];
                Vector4[] paintChannelMaskArray = new Vector4[10];

                // Render up to 10 splats per frame of the same texture!
                int numSplats = 0;
                Texture2D paintPattern = _paintList[0].brush.paintPattern;

                for (int s=0; s < _paintList.Count;)
                {
                    if (numSplats >= 10) break;
                    if (_paintList[s].brush.paintPattern == paintPattern)
                    {
                        paintMatrixArray[numSplats] = _paintList[s].paintMatrix;
                        paintScaleBiasArray[numSplats] = _paintList[s].scaleBias;
                        paintChannelMaskArray[numSplats] = _paintList[s].channelMask;
                        numSplats++;
                        _paintList.RemoveAt(s);
                    }
                    else
                    {
                        //different texture..skip for now
                        s++;
                    }
                }

                ComputeBuffer paintWorldToObjectBuffer = new ComputeBuffer(paintMatrixArray.Length, _paintMatrixBufferStride);
                ComputeBuffer paintScaleBiasBuffer = new ComputeBuffer(paintScaleBiasArray.Length, _scaleBiasBufferStride);
                ComputeBuffer paintChannelMaskBuffer = new ComputeBuffer(paintChannelMaskArray.Length, _paintColorBufferStride);
                paintWorldToObjectBuffer.SetData(paintMatrixArray);
                paintScaleBiasBuffer.SetData(paintScaleBiasArray);
                paintChannelMaskBuffer.SetData(paintChannelMaskArray);
            
                paintComputeShader.SetTexture(_paintKernel, "world_pos_tex", _worldPosTex);
                paintComputeShader.SetTexture(_paintKernel, "paintmap", _paintMap);
                paintComputeShader.SetTexture(_paintKernel,"paint_pattern", paintPattern);
                paintComputeShader.SetBuffer(_paintKernel, "paint_world_to_object", paintWorldToObjectBuffer);
                paintComputeShader.SetBuffer(_paintKernel, "scale_bias", paintScaleBiasBuffer);
                paintComputeShader.SetBuffer(_paintKernel, "paint_channel_mask", paintChannelMaskBuffer);
                paintComputeShader.SetFloat("num_splats", numSplats);
                paintComputeShader.SetFloats("resolution",_paintMap.width, _paintMap.height);
                paintComputeShader.Dispatch(_paintKernel,
                    Mathf.CeilToInt(_paintMap.width / (float) _xGroupSize),
                    Mathf.CeilToInt(_paintMap.height / (float) _yGroupSize), 1);
            
                paintWorldToObjectBuffer.Dispose();
                paintChannelMaskBuffer.Dispose();
                paintScaleBiasBuffer.Dispose();
            }
        }

        private void Update()
        {
            if (paintAllSplats)
            {
                while(_paintList.Count > 0)
                {
                    PaintSplats();
                }
            }
            else
                PaintSplats();
        }
    }
}