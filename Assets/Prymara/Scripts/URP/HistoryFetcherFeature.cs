#if PRYMARA_URP
#if UNITY_6000_0_OR_NEWER
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace Prymara
{
    public class HistoryFetcherFeature : ScriptableRendererFeature
    {
        [SerializeField] HistoryFetcherSettings settings;
        HistoryFetcherPass m_ScriptablePass;

        /// <inheritdoc/>
        public override void Create()
        {
            m_ScriptablePass = new HistoryFetcherPass(settings);

            // Configures where the render pass should be injected.
            m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;

            // You can request URP color texture and depth buffer as inputs by uncommenting the line below,
            // URP will ensure copies of these resources are available for sampling before executing the render pass.
            // Only uncomment it if necessary, it will have a performance impact, especially on mobiles and other TBDR GPUs where it will break render passes.
            //m_ScriptablePass.ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth);

            // You can request URP to render to an intermediate texture by uncommenting the line below.
            // Use this option for passes that do not support rendering directly to the backbuffer.
            // Only uncomment it if necessary, it will have a performance impact, especially on mobiles and other TBDR GPUs where it will break render passes.
            //m_ScriptablePass.requiresIntermediateTexture = true;
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_ScriptablePass);
        }

        // Use this class to pass around settings from the feature to the pass
        [Serializable]
        public class HistoryFetcherSettings
        {
            public Material material;
        }

        class HistoryFetcherPass : ScriptableRenderPass
        {
            readonly HistoryFetcherSettings settings;
            private static Material material;
            private static readonly int PrevFrameTexID = Shader.PropertyToID("_PrevFrameTex");
            public HistoryFetcherPass(HistoryFetcherSettings settings)
            {
                this.settings = settings;
                material = settings.material;
            }

            // This class stores the data needed by the RenderGraph pass.
            // It is passed as a parameter to the delegate function that executes the RenderGraph pass.
            private class PassData
            {
                public RTHandle prevRT;
            }

            // This static method is passed as the RenderFunc delegate to the RenderGraph render pass.
            // It is used to execute draw commands.
            static void ExecutePass(PassData data, RasterGraphContext context)
            {
                if (data == null)
                    return;
                if (data.prevRT == null)
                    return;
                if (data.prevRT.rt == null)
                    return;
                material.SetTexture(PrevFrameTexID, data.prevRT.rt);
            }

            // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
            // FrameData is a context container through which URP resources can be accessed and managed.
            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                const string passName = "Render Custom Pass";

                // This adds a raster render pass to the graph, specifying the name and the data type that will be passed to the ExecutePass function.
                using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
                {
                    UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                    if (cameraData.cameraType == CameraType.Game)
                    {
                        if (cameraData != null)
                        {
                            if (cameraData.historyManager != null)
                            {
                                cameraData.historyManager.RequestAccess<RawColorHistory>();
                                var history = cameraData.historyManager.GetHistoryForRead<RawColorHistory>();
                                if (history != null)
                                {
                                    RTHandle prevFrameRT = history?.GetPreviousTexture();
                                    if (prevFrameRT != null)
                                    {
                                        passData.prevRT = prevFrameRT;
                                    }
                                }
                            }
                        }
                    }
                    UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                    builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
                }
            }
        }
    }
}
#else
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Prymara._2022
{
    public class HistoryFetcherFeature : ScriptableRendererFeature
    {
        [SerializeField]
        private HistoryFetcherSettings settings;

        private HistoryFetcherPass pass;

        public override void Create()
        {
            pass = new HistoryFetcherPass(settings)
            {
                renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
            };
        }

        public override void AddRenderPasses(
            ScriptableRenderer renderer,
            ref RenderingData renderingData)
        {
            if (!ShouldRenderCamera(renderingData.cameraData))
                return;

            renderer.EnqueuePass(pass);
        }

        public override void SetupRenderPasses(
            ScriptableRenderer renderer,
            in RenderingData renderingData)
        {
            if (!ShouldRenderCamera(renderingData.cameraData))
                return;

            // Makes URP provide a sampleable intermediate color texture when needed.
            pass.ConfigureInput(ScriptableRenderPassInput.Color);
            pass.SetCameraTarget(renderer.cameraColorTargetHandle);
        }

        private static bool ShouldRenderCamera(CameraData cameraData)
        {
            // Most important condition: do not execute for Inspector preview cameras.
            if (cameraData.cameraType != CameraType.Game)
                return false;

            // A single temporal history texture should not be shared with overlay cameras.
            if (cameraData.renderType == CameraRenderType.Overlay)
                return false;

            return true;
        }

        protected override void Dispose(bool disposing)
        {
            pass?.Dispose();
            pass = null;
        }

        [Serializable]
        public class HistoryFetcherSettings
        {
            public Material material;
        }

        private sealed class HistoryFetcherPass : ScriptableRenderPass
        {
            private readonly HistoryFetcherSettings settings;
            private readonly ProfilingSampler profilingSampler =
                new("HistoryFetcherPass");

            private RTHandle currentColor;
            private RTHandle previousFrame;
            private RTHandle temporaryFrame;

            private bool historyValid;
            private int previousCameraId = int.MinValue;

            private static readonly int PrevFrameTexID =
                Shader.PropertyToID("_PrevFrameTex");

            public HistoryFetcherPass(HistoryFetcherSettings settings)
            {
                this.settings = settings;
            }

            public void SetCameraTarget(RTHandle cameraColorTarget)
            {
                currentColor = cameraColorTarget;
            }

            public override void OnCameraSetup(
                CommandBuffer cmd,
                ref RenderingData renderingData)
            {
                if (currentColor == null)
                    return;

                RenderTextureDescriptor descriptor =
                    renderingData.cameraData.cameraTargetDescriptor;

                // History textures are color-only and should be sampleable.
                descriptor.depthBufferBits = 0;
                descriptor.msaaSamples = 1;
                descriptor.bindMS = false;
                descriptor.useMipMap = false;
                descriptor.autoGenerateMips = false;

                bool previousFrameReallocated =
                    RenderingUtils.ReAllocateIfNeeded(
                        ref previousFrame,
                        descriptor,
                        FilterMode.Bilinear,
                        TextureWrapMode.Clamp,
                        name: "_PrymaraPreviousFrame");

                RenderingUtils.ReAllocateIfNeeded(
                    ref temporaryFrame,
                    descriptor,
                    FilterMode.Bilinear,
                    TextureWrapMode.Clamp,
                    name: "_PrymaraTemporaryFrame");

                int currentCameraId =
                    renderingData.cameraData.camera.GetInstanceID();

                if (previousFrameReallocated ||
                    previousCameraId != currentCameraId)
                {
                    historyValid = false;
                    previousCameraId = currentCameraId;
                }

                ConfigureTarget(currentColor);
            }

            public override void Execute(
                ScriptableRenderContext context,
                ref RenderingData renderingData)
            {
                if (settings?.material == null)
                    return;

                if (currentColor == null ||
                    previousFrame == null ||
                    temporaryFrame == null)
                {
                    return;
                }

                // These handles are allocated by ReAllocateIfNeeded and should
                // therefore have concrete RenderTextures.
                if (previousFrame.rt == null || temporaryFrame.rt == null)
                    return;

                CommandBuffer cmd =
                    CommandBufferPool.Get("HistoryFetcherPass");

                using (new ProfilingScope(cmd, profilingSampler))
                {
                    // Preserve the current unmodified frame.
                    Blitter.BlitCameraTexture(
                        cmd,
                        currentColor,
                        temporaryFrame);

                    // On the first frame, use the current frame as its own history.
                    // This prevents undefined initial texture contents.
                    if (!historyValid)
                    {
                        Blitter.BlitCameraTexture(
                            cmd,
                            temporaryFrame,
                            previousFrame);
                    }

                    settings.material.SetTexture(
                        PrevFrameTexID,
                        previousFrame);

                    // Important:
                    // Read from temporaryFrame and write to currentColor.
                    // Do not read and write currentColor simultaneously.
                    Blitter.BlitCameraTexture(
                        cmd,
                        temporaryFrame,
                        currentColor,
                        settings.material,
                        0);

                    // Store the original current frame for the next frame.
                    Blitter.BlitCameraTexture(
                        cmd,
                        temporaryFrame,
                        previousFrame);
                }

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);

                historyValid = true;
            }

            public void Dispose()
            {
                previousFrame?.Release();
                temporaryFrame?.Release();

                previousFrame = null;
                temporaryFrame = null;
                currentColor = null;

                historyValid = false;
                previousCameraId = int.MinValue;
            }
        }
    }
}
#endif
#endif