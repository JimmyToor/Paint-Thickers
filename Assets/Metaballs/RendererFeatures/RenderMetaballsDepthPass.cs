using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Metaballs.RendererFeatures
{
    class RenderMetaballsDepthPass : ScriptableRenderPass
    {
        const string MetaballDepthRTId = "_MetaballDepthRT";
        int _metaballDepthRTId;
        public Material WriteDepthMaterial;

        RenderTargetIdentifier _metaballDepthRT;
        RenderStateBlock _renderStateBlock;
        RenderQueueType _renderQueueType;
        FilteringSettings _filteringSettings;
        ProfilingSampler _profilingSampler;
        List<ShaderTagId> _shaderTagIdList = new List<ShaderTagId>();

        public RenderMetaballsDepthPass(string profilerTag, RenderPassEvent renderPassEvent,
            string[] shaderTags, RenderQueueType renderQueueType, int layerMask)
        {
            profilingSampler = new ProfilingSampler(nameof(RenderObjectsPass));

            _profilingSampler = new ProfilingSampler(profilerTag);
            this.renderPassEvent = renderPassEvent;
            this._renderQueueType = renderQueueType;
            RenderQueueRange renderQueueRange = (renderQueueType == RenderQueueType.Transparent)
                ? RenderQueueRange.transparent
                : RenderQueueRange.opaque;
            _filteringSettings = new FilteringSettings(renderQueueRange, layerMask);

            if (shaderTags != null && shaderTags.Length > 0)
            {
                foreach (var passName in shaderTags)
                    _shaderTagIdList.Add(new ShaderTagId(passName));
            }
            else
            {
                _shaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
                _shaderTagIdList.Add(new ShaderTagId("UniversalForward"));
                _shaderTagIdList.Add(new ShaderTagId("UniversalForwardOnly"));
                _shaderTagIdList.Add(new ShaderTagId("LightweightForward"));
            }
             
            _renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor blitTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;

            _metaballDepthRTId = Shader.PropertyToID(MetaballDepthRTId);
            cmd.GetTemporaryRT(_metaballDepthRTId, blitTargetDescriptor);
            _metaballDepthRT = new RenderTargetIdentifier(_metaballDepthRTId);
            ConfigureTarget(_metaballDepthRT);
            ConfigureClear(ClearFlag.All, Color.clear);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            SortingCriteria sortingCriteria = (_renderQueueType == RenderQueueType.Transparent)
                ? SortingCriteria.CommonTransparent
                : renderingData.cameraData.defaultOpaqueSortFlags;

            DrawingSettings drawingSettings =
                CreateDrawingSettings(_shaderTagIdList, ref renderingData, sortingCriteria);

            // NOTE: Do NOT mix ProfilingScope with named CommandBuffers i.e. CommandBufferPool.Get("name").
            // Currently there's an issue which results in mismatched markers.
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, _profilingSampler))
            {
                //Write Depth
                drawingSettings.overrideMaterial = WriteDepthMaterial;
                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref _filteringSettings,
                    ref _renderStateBlock);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}