using Metaballs.RendererFeatures;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal;

public class RenderMetaballsScreenSpace : ScriptableRendererFeature
{
    public string PassTag = "RenderMetaballsScreenSpace";
    public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;

    public RenderObjects.FilterSettings FilterSettings = new RenderObjects.FilterSettings();

    public Material BlitMaterial;
    public Material WriteDepthMaterial;

    RenderMetaballsDepthPass _renderMetaballsDepthPass;
    RenderMetaballsScreenSpacePass _scriptableMetaballsScreenSpacePass;

    [Range(1, 15)]
    public int BlurPasses = 1;

    [Range(0f, 1f)]
    public float BlurDistance = 0.5f;

    /// <inheritdoc/>
    public override void Create()
    {
        _renderMetaballsDepthPass = new RenderMetaballsDepthPass(PassTag, Event, FilterSettings.PassNames,
            FilterSettings.RenderQueueType, FilterSettings.LayerMask)
        {
            WriteDepthMaterial = WriteDepthMaterial
        };

        _scriptableMetaballsScreenSpacePass = new RenderMetaballsScreenSpacePass(PassTag, Event,
            FilterSettings.PassNames, FilterSettings.RenderQueueType, FilterSettings.LayerMask)
        {
            BlitMaterial = BlitMaterial,
            BlurPasses = BlurPasses,
            BlurDistance = BlurDistance
        };
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_renderMetaballsDepthPass);
        renderer.EnqueuePass(_scriptableMetaballsScreenSpacePass);
    }
}