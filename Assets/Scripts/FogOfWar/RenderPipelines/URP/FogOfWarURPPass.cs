﻿using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FoW
{
    class FogOfWarURPManager : FogOfWarPostProcessManager
    {
        Material _material;
        CommandBuffer _cmd;
        ScriptableRenderContext _context;
        RenderingData _renderingData;
        public RenderTargetIdentifier sourceTarget { get; set; }

        public bool isActive => _material != null;

        public FogOfWarURPManager()
        {
            if (_material == null)
                _material = new Material(FogOfWarUtils.FindShader("Hidden/FogOfWarURP"));
        }

        public void Setup(CommandBuffer cmd, ref ScriptableRenderContext context, ref RenderingData renderingdata)
        {
            _cmd = cmd;
            _context = context;
            _renderingData = renderingdata;

            _material.mainTexture = renderingdata.cameraData.targetTexture;
        }

        public void OnDestroy()
        {
            if (Application.isPlaying)
                Object.Destroy(_material);
            else
                Object.DestroyImmediate(_material);
            _material = null;
        }

        protected override void SetTexture(int id, Texture value) { _material.SetTexture(id, value); }
        protected override void SetVector(int id, Vector4 value) { _material.SetVector(id, value); }
        protected override void SetColor(int id, Color value) { _material.SetColor(id, value); }
        protected override void SetFloat(int id, float value) { _material.SetFloat(id, value); }
        protected override void SetMatrix(int id, Matrix4x4 value) { _material.SetMatrix(id, value); }
        protected override void SetKeyword(string keyword, bool enabled)
        {
            if (enabled)
                _material.EnableKeyword(keyword);
            else
                _material.DisableKeyword(keyword);
        }

        protected override void GetTargetSize(out int width, out int height, out int depth)
        {
            width = _renderingData.cameraData.camera.scaledPixelWidth;
            height = _renderingData.cameraData.camera.scaledPixelHeight;
            depth = _renderingData.cameraData.targetTexture.depth;
        }

        protected override void BlitToScreen()
        {
            int destination = Shader.PropertyToID("FogOfWarURP");

            _cmd.SetGlobalTexture("_MainTex", sourceTarget);
            _cmd.GetTemporaryRT(destination, _renderingData.cameraData.camera.scaledPixelWidth, _renderingData.cameraData.camera.scaledPixelHeight, 0, FilterMode.Point, _renderingData.cameraData.isHdrEnabled ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
            _cmd.Blit(sourceTarget, destination);
            _cmd.Blit(destination, sourceTarget, _material);

            _context.ExecuteCommandBuffer(_cmd);
            CommandBufferPool.Release(_cmd);
        }
    }

    public class FogOfWarURPPass : ScriptableRenderPass
    {
        FogOfWarURP _fowURP;
        FogOfWarURPManager _postProcess = null;
        ScriptableRenderer _renderer;

#if UNITY_EDITOR && UNITY_2021_2_OR_NEWER
        bool _hasForcedImmediateTexture = false;
#endif

        public FogOfWarURPPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            _postProcess = new FogOfWarURPManager();
        }

        public void Setup(ScriptableRenderer renderer)
        {
#if UNITY_EDITOR && UNITY_2021_2_OR_NEWER
            if (!_hasForcedImmediateTexture)
            {
                foreach (string guid in UnityEditor.AssetDatabase.FindAssets("t:UniversalRendererData"))
                {
                    UniversalRendererData renderdata = UnityEditor.AssetDatabase.LoadAssetAtPath<UniversalRendererData>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));

                    if (renderdata.intermediateTextureMode == IntermediateTextureMode.Always)
                        continue;

                    Debug.LogWarning("FoW: Forcing UniversalRendererData.intermediateTextureMode to Always.", renderdata);
                    renderdata.intermediateTextureMode = IntermediateTextureMode.Always;
                    UnityEditor.EditorUtility.SetDirty(renderdata);
                }

                _hasForcedImmediateTexture = true;
            }
#endif

            _renderer = renderer;

            //ConfigureInput(ScriptableRenderPassInput.Color);
            //ConfigureInput(ScriptableRenderPassInput.Depth);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!renderingData.cameraData.postProcessEnabled)
                return;

            VolumeStack stack = VolumeManager.instance.stack;
            _fowURP = stack.GetComponent<FogOfWarURP>();
            if (_fowURP == null || !_fowURP.IsActive())
                return;

#if UNITY_2022_1_OR_NEWER
            _postProcess.sourceTarget = _renderer.cameraColorTargetHandle;
#else
            _postProcess.sourceTarget = _renderer.cameraColorTarget;
#endif

            CommandBuffer cmd = CommandBufferPool.Get("FogOfWarURP");

            _postProcess.Setup(cmd, ref context, ref renderingData);
            _postProcess.team = _fowURP.team.value;
            _postProcess.camera = renderingData.cameraData.camera;
            _postProcess.pointFiltering = _fowURP.pointFiltering.value;
            _postProcess.fogFarPlane = _fowURP.fogFarPlane.value;
            _postProcess.outsideFogStrength = _fowURP.outsideFogStrength.value;
            _postProcess.fogHeightMin = _fowURP.minFogHeight.value;
            _postProcess.fogHeightMax = _fowURP.maxFogHeight.value;
            _postProcess.fogColor = _fowURP.fogColor.value;
            _postProcess.fogColorTexture = _fowURP.fogColorTexture.value;
            _postProcess.fogColorTextureScale = _fowURP.fogColorTextureScale.value;
            _postProcess.fogColorTextureHeight = _fowURP.fogColorTextureHeight.value;

            _postProcess.Render();
        }
        /*
        public void OnDestroy(CommandBuffer cmd)
        {
            _postProcess?.OnDestroy();
            _postProcess = null;
        }*/
    }
}
