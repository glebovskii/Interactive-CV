using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Prymara
{
    /// <summary>
    /// Runtime UI Toolkit controller for the Comic shader material.
    /// The component binds the controls in Shader_Comic.uxml directly to the
    /// shader properties and keywords used by ComicShaderGUI.
    /// </summary>
    [RequireComponent(typeof(PanelRenderer))]
    public sealed class ComicShaderPanelController : MonoBehaviour
    {
        #region Property names

        private static readonly int EdgeStrength = Shader.PropertyToID("_Edge_Strength");
        private static readonly int Thickness = Shader.PropertyToID("_Thickness");
        private static readonly int Threshold = Shader.PropertyToID("_Threshold");
        private static readonly int EdgePower = Shader.PropertyToID("_EdgePower");
        private static readonly int Softness = Shader.PropertyToID("_Softness");
        private static readonly int MinEdgeDepth = Shader.PropertyToID("_Min_Edge_Depth");

        private static readonly int OffsetRed = Shader.PropertyToID("_OffsetRed");
        private static readonly int OffsetGreen = Shader.PropertyToID("_OffsetGreen");
        private static readonly int OffsetBlue = Shader.PropertyToID("_OffsetBlue");
        private static readonly int FrameComparison = Shader.PropertyToID("_FrameComparison");
        private static readonly int AberrationMinDepth = Shader.PropertyToID("_AberrationMinDepth");

        private static readonly int GridSize = Shader.PropertyToID("_GridSize");
        private static readonly int GridMinDepth = Shader.PropertyToID("_MinDepth");
        private static readonly int GridAlpha = Shader.PropertyToID("_Alpha");
        private static readonly int GridSoftness = Shader.PropertyToID("_GridSoftness");
        private static readonly int GridRadius = Shader.PropertyToID("_Radius");

        private static readonly int KernelSize = Shader.PropertyToID("_Kernel_Size");
        private static readonly int SectorCount = Shader.PropertyToID("_n");
        private static readonly int Hardness = Shader.PropertyToID("_Hardness");
        private static readonly int VariancePower = Shader.PropertyToID("_Q");
        private static readonly int KuwaharaAlpha = Shader.PropertyToID("_Kuwahara_Alpha");
        private static readonly int ZeroCrossing = Shader.PropertyToID("_Zero_crossing");
        private static readonly int Zeta = Shader.PropertyToID("_Zeta");
        private static readonly int KuwaharaMinDepth = Shader.PropertyToID("_KuwaharaMinDepth");

        private static readonly int OilRadius = Shader.PropertyToID("_Oil_Radius");
        private static readonly int OilThickness = Shader.PropertyToID("_Oil_thickness");
        private static readonly int OilMinDepth = Shader.PropertyToID("_Oil_minDepth");

        #endregion

        #region Keywords

        private const string EdgeKeyword = "_USE_EDGE";
        private const string AberrationKeyword = "_USE_ABERRATION";
        private const string GridKeyword = "_USE_GRID";
        private const string GridTextureKeyword = "_USE_GRID_TEXTURE";
        private const string KuwaharaKeyword = "_USE_KUWAHARA";
        private const string InvertAlphaKeyword = "_INVERTALPHA";
        private const string OilKeyword = "_USE_OIL";

        #endregion

        [SerializeField] private PanelRenderer panelRenderer;
        [SerializeField] private Material targetMaterial;

        private readonly List<Action> unbindActions = new();

        private VisualElement gridProceduralFields;
        private VisualElement gridTextureNote;

        private void Awake()
        {
            if (panelRenderer == null)
                panelRenderer = GetComponent<PanelRenderer>();

            if (panelRenderer == null)
            {
                Debug.LogError($"{nameof(ComicShaderPanelController)} requires a PanelRenderer.", this);
                return;
            }

            panelRenderer.RegisterUIReloadCallback(OnUIReload);
        }

        private void OnDestroy()
        {
            UnbindUI();

            if (panelRenderer != null)
                panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        }

        private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
        {
            UnbindUI();

            if (targetMaterial == null)
            {
                Debug.LogError("Comic shader material is not assigned.", this);
                return;
            }

            TabView tabView = root.Q<TabView>("shader-tab-view");
            if (tabView != null)
                tabView.selectedTabIndex = 0;

            BindKeywordToggle(root, "toggle-edge", EdgeKeyword, "edge-fields");
            BindKeywordToggle(root, "toggle-aberration", AberrationKeyword, "aberration-fields");
            BindKeywordToggle(root, "toggle-grid", GridKeyword, "grid-fields");
            BindKeywordToggle(root, "toggle-kuwahara", KuwaharaKeyword, "kuwahara-fields");
            BindKeywordToggle(root, "toggle-oil", OilKeyword, "oil-fields");

            BindFloat(root, "slider-edge-strength", EdgeStrength);
            BindFloat(root, "slider-edge-thickness", Thickness);
            BindFloat(root, "slider-edge-threshold", Threshold);
            BindFloat(root, "slider-edge-power", EdgePower);
            BindFloat(root, "slider-edge-softness", Softness);
            BindFloat(root, "slider-edge-min-depth", MinEdgeDepth);

            BindFloat(root, "slider-aberration-red", OffsetRed);
            BindFloat(root, "slider-aberration-green", OffsetGreen);
            BindFloat(root, "slider-aberration-blue", OffsetBlue);
            BindFloat(root, "slider-aberration-frame-comparison", FrameComparison);
            BindFloat(root, "slider-aberration-min-depth", AberrationMinDepth);

            BindFloat(root, "slider-grid-size", GridSize);
            BindFloat(root, "slider-grid-min-depth", GridMinDepth);
            BindFloat(root, "slider-grid-alpha", GridAlpha);
            BindFloat(root, "slider-grid-softness", GridSoftness);
            BindFloat(root, "slider-grid-radius", GridRadius);

            BindInt(root, "slider-kuwahara-kernel", KernelSize);
            BindInt(root, "slider-kuwahara-sector-count", SectorCount);
            BindFloat(root, "slider-kuwahara-hardness", Hardness);
            BindFloat(root, "slider-kuwahara-q", VariancePower);
            BindFloat(root, "slider-kuwahara-alpha", KuwaharaAlpha);
            BindFloat(root, "slider-kuwahara-zero-crossing", ZeroCrossing);
            BindFloat(root, "slider-kuwahara-zeta", Zeta);
            BindFloat(root, "slider-kuwahara-min-depth", KuwaharaMinDepth);

            BindFloat(root, "slider-oil-radius", OilRadius);
            BindFloat(root, "slider-oil-thickness", OilThickness);
            BindFloat(root, "slider-oil-min-depth", OilMinDepth);

            BindGridOptions(root);
        }

        private void BindKeywordToggle(
            VisualElement root,
            string toggleName,
            string keyword,
            string fieldsName)
        {
            Toggle toggle = root.Q<Toggle>(toggleName);
            VisualElement fields = root.Q<VisualElement>(fieldsName);

            if (toggle == null)
                return;

            bool initialValue = targetMaterial.IsKeywordEnabled(keyword);
            toggle.SetValueWithoutNotify(initialValue);
            fields?.SetEnabled(initialValue);

            EventCallback<ChangeEvent<bool>> callback = evt =>
            {
                SetKeyword(keyword, evt.newValue);
                fields?.SetEnabled(evt.newValue);
            };

            toggle.RegisterValueChangedCallback(callback);
            unbindActions.Add(() => toggle.UnregisterValueChangedCallback(callback));
        }

        private void BindGridOptions(VisualElement root)
        {
            Toggle textureToggle = root.Q<Toggle>("toggle-grid-texture");
            Toggle invertAlphaToggle = root.Q<Toggle>("toggle-grid-invert-alpha");

            gridProceduralFields = root.Q<VisualElement>("grid-procedural-fields");
            gridTextureNote = root.Q<VisualElement>("grid-texture-note");

            if (textureToggle != null)
            {
                bool useTexture = targetMaterial.IsKeywordEnabled(GridTextureKeyword);
                textureToggle.SetValueWithoutNotify(useTexture);
                UpdateGridTextureMode(useTexture);

                EventCallback<ChangeEvent<bool>> textureCallback = evt =>
                {
                    SetKeyword(GridTextureKeyword, evt.newValue);
                    UpdateGridTextureMode(evt.newValue);
                };

                textureToggle.RegisterValueChangedCallback(textureCallback);
                unbindActions.Add(() => textureToggle.UnregisterValueChangedCallback(textureCallback));
            }

            if (invertAlphaToggle != null)
            {
                bool inverted = targetMaterial.IsKeywordEnabled(InvertAlphaKeyword);
                invertAlphaToggle.SetValueWithoutNotify(inverted);

                EventCallback<ChangeEvent<bool>> invertCallback = evt =>
                    SetKeyword(InvertAlphaKeyword, evt.newValue);

                invertAlphaToggle.RegisterValueChangedCallback(invertCallback);
                unbindActions.Add(() => invertAlphaToggle.UnregisterValueChangedCallback(invertCallback));
            }
        }

        private void UpdateGridTextureMode(bool useTexture)
        {
            if (gridProceduralFields != null)
            {
                gridProceduralFields.style.display = useTexture
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
            }

            if (gridTextureNote != null)
            {
                gridTextureNote.style.display = useTexture
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            }
        }

        private void BindFloat(VisualElement root, string sliderName, int propertyId)
        {
            Slider slider = root.Q<Slider>(sliderName);

            if (slider == null)
                return;

            if (!targetMaterial.HasProperty(propertyId))
            {
                DisableMissingControl(slider, propertyId);
                return;
            }

            slider.SetValueWithoutNotify(targetMaterial.GetFloat(propertyId));

            EventCallback<ChangeEvent<float>> callback = evt => targetMaterial.SetFloat(propertyId, evt.newValue);

            slider.RegisterValueChangedCallback(callback);
            unbindActions.Add(() => slider.UnregisterValueChangedCallback(callback));
        }

        private void BindInt(VisualElement root, string sliderName, int propertyId)
        {
            SliderInt slider = root.Q<SliderInt>(sliderName);

            if (slider == null)
                return;

            if (!targetMaterial.HasProperty(propertyId))
            {
                DisableMissingControl(slider, propertyId);
                return;
            }

            slider.SetValueWithoutNotify(Mathf.RoundToInt(targetMaterial.GetFloat(propertyId)));

            EventCallback<ChangeEvent<int>> callback = evt =>
                targetMaterial.SetFloat(propertyId, evt.newValue);

            slider.RegisterValueChangedCallback(callback);
            unbindActions.Add(() => slider.UnregisterValueChangedCallback(callback));
        }

        private void DisableMissingControl(VisualElement control, int propertyId)
        {
            control.SetEnabled(false);
            control.tooltip = $"The assigned material does not contain property ID {propertyId}.";
        }

        private void SetKeyword(string keyword, bool enabled)
        {
            if (enabled)
                targetMaterial.EnableKeyword(keyword);
            else
                targetMaterial.DisableKeyword(keyword);
        }

        private void UnbindUI()
        {
            for (int index = unbindActions.Count - 1; index >= 0; index--)
                unbindActions[index]?.Invoke();

            unbindActions.Clear();
            gridProceduralFields = null;
            gridTextureNote = null;
        }
    }
}
