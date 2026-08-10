using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Prymara
{
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

        private GlobalKeyword useEdge;
        private GlobalKeyword useAberration;
        private GlobalKeyword useGrid;
        private GlobalKeyword useGridTexture;
        private GlobalKeyword useKuwahara;
        private GlobalKeyword invertAlpha;
        private GlobalKeyword useOil;

        #endregion

        [SerializeField] private PanelRenderer panelRenderer;

        private readonly UICallbackBinder uiCallbacks = new();

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

            useEdge = GlobalKeyword.Create(EdgeKeyword);
            useAberration = GlobalKeyword.Create(AberrationKeyword);
            useGrid = GlobalKeyword.Create(GridKeyword);
            useGridTexture = GlobalKeyword.Create(GridTextureKeyword);
            useKuwahara = GlobalKeyword.Create(KuwaharaKeyword);
            invertAlpha = GlobalKeyword.Create(InvertAlphaKeyword);
            useOil = GlobalKeyword.Create(OilKeyword);

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

            TabView tabView = root.Q<TabView>("shader-tab-view");

            if (tabView != null)
                tabView.selectedTabIndex = 0;

            BindKeywordToggle(root, "toggle-edge", useEdge, "edge-fields", true);
            BindKeywordToggle(root, "toggle-aberration", useAberration, "aberration-fields", true);
            BindKeywordToggle(root, "toggle-grid", useGrid, "grid-fields", false);
            BindKeywordToggle(root, "toggle-kuwahara", useKuwahara, "kuwahara-fields", true);
            BindKeywordToggle(root, "toggle-oil", useOil, "oil-fields", false);

            BindFloat(root, "slider-edge-strength", EdgeStrength, 4.36f);
            BindFloat(root, "slider-edge-thickness", Thickness, 0.04f);
            BindFloat(root, "slider-edge-threshold", Threshold, 0.4f);
            BindFloat(root, "slider-edge-power", EdgePower, 0.66f);
            BindFloat(root, "slider-edge-softness", Softness, 0.3f);
            BindFloat(root, "slider-edge-min-depth", MinEdgeDepth, 0f);

            BindFloat(root, "slider-aberration-red", OffsetRed, 0.06f);
            BindFloat(root, "slider-aberration-green", OffsetGreen, -0.04f);
            BindFloat(root, "slider-aberration-blue", OffsetBlue, 0.03f);
            BindFloat(root, "slider-aberration-frame-comparison", FrameComparison, 1f);
            BindFloat(root, "slider-aberration-min-depth", AberrationMinDepth, 0f);

            BindFloat(root, "slider-grid-size", GridSize, 1f);
            BindFloat(root, "slider-grid-min-depth", GridMinDepth, 0f);
            BindFloat(root, "slider-grid-alpha", GridAlpha, 0.07f);
            BindFloat(root, "slider-grid-softness", GridSoftness);
            BindFloat(root, "slider-grid-radius", GridRadius);

            BindInt(root, "slider-kuwahara-kernel", KernelSize, 17f);
            BindInt(root, "slider-kuwahara-sector-count", SectorCount, 3f);
            BindFloat(root, "slider-kuwahara-hardness", Hardness, 4.22f);
            BindFloat(root, "slider-kuwahara-q", VariancePower, 44f);
            BindFloat(root, "slider-kuwahara-alpha", KuwaharaAlpha, 1f);
            BindFloat(root, "slider-kuwahara-zero-crossing", ZeroCrossing, 5.8f);
            BindFloat(root, "slider-kuwahara-zeta", Zeta, 0.3f);
            BindFloat(root, "slider-kuwahara-min-depth", KuwaharaMinDepth, 0f);

            BindFloat(root, "slider-oil-radius", OilRadius, 4f);
            BindFloat(root, "slider-oil-thickness", OilThickness, 5);
            BindFloat(root, "slider-oil-min-depth", OilMinDepth, 0);

            BindGridOptions(root);
        }

        private void BindKeywordToggle(VisualElement root, string toggleName, GlobalKeyword keyword, string fieldsName, bool defaultValue = false, bool playSound = true)
        {
            Toggle toggle = root.Q<Toggle>(toggleName);
            VisualElement fields = root.Q<VisualElement>(fieldsName);

            if (toggle == null)
            {
                Debug.LogError($"Toggle '{toggleName}' not found in the UI.", this);
                return;
            }

            Shader.SetKeyword(keyword, defaultValue);
            bool initialValue = Shader.IsKeywordEnabled(keyword);

            toggle.SetValueWithoutNotify(initialValue);
            fields?.SetEnabled(initialValue);

            if (fields == null)
                Debug.LogError($"Fields '{fieldsName}' not found in the UI.", this);

            Action<UISoundController> soundAction = playSound ? sound => sound.PlayToggle() : null;

            uiCallbacks.BindChange<bool>(toggle, value =>
            {
                SetKeyword(keyword, value);
                fields?.SetEnabled(value);
            }, soundAction);
        }

        private void BindGridOptions(VisualElement root)
        {
            Toggle textureToggle = root.Q<Toggle>("toggle-grid-texture");
            Toggle invertAlphaToggle = root.Q<Toggle>("toggle-grid-invert-alpha");

            gridProceduralFields = root.Q<VisualElement>("grid-procedural-fields");
            gridTextureNote = root.Q<VisualElement>("grid-texture-note");

            if (textureToggle != null)
            {
                bool useTexture = Shader.IsKeywordEnabled(useGridTexture);

                textureToggle.SetValueWithoutNotify(useTexture);
                UpdateGridTextureMode(useTexture);

                uiCallbacks.BindChange<bool>(textureToggle, value =>
                {
                    SetKeyword(useGridTexture, value);
                    UpdateGridTextureMode(value);
                }, sound => sound.PlayToggle());
            }

            if (invertAlphaToggle != null)
            {
                bool inverted = Shader.IsKeywordEnabled(invertAlpha);

                invertAlphaToggle.SetValueWithoutNotify(inverted);

                uiCallbacks.BindChange<bool>(invertAlphaToggle,
                    value => SetKeyword(invertAlpha, value),
                    sound => sound.PlayToggle());
            }
        }

        private void UpdateGridTextureMode(bool useTexture)
        {
            if (gridProceduralFields != null)
                gridProceduralFields.style.display = useTexture ? DisplayStyle.None : DisplayStyle.Flex;

            if (gridTextureNote != null)
                gridTextureNote.style.display = useTexture ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void BindFloat(VisualElement root, string sliderName, int propertyId, float defailtValue = 0)
        {
            Slider slider = root.Q<Slider>(sliderName);

            if (slider == null)
                return;
            float currValue = Shader.GetGlobalFloat(propertyId);
            currValue = currValue == 0 ? defailtValue : currValue;
            Shader.SetGlobalFloat(propertyId, currValue);
            slider.SetValueWithoutNotify(currValue);
            uiCallbacks.BindChange<float>(slider, value => Shader.SetGlobalFloat(propertyId, value), sound => sound.PlaySliderChange());
        }

        private void BindInt(VisualElement root, string sliderName, int propertyId, float defaultValue = 0f)
        {
            SliderInt slider = root.Q<SliderInt>(sliderName);

            if (slider == null)
                return;
            float currValue = Shader.GetGlobalFloat(propertyId);
            currValue = currValue == 0 ? defaultValue : currValue;
            Shader.SetGlobalFloat(propertyId, currValue);

            slider.SetValueWithoutNotify(Mathf.RoundToInt(Shader.GetGlobalFloat(propertyId)));
            uiCallbacks.BindChange<int>(slider, value => Shader.SetGlobalFloat(propertyId, value), sound => sound.PlaySliderChange());
        }

        private void SetKeyword(GlobalKeyword keyword, bool enabled)
        {
            Shader.SetKeyword(keyword, enabled);
        }

        private void UnbindUI()
        {
            uiCallbacks.Clear();
            gridProceduralFields = null;
            gridTextureNote = null;
        }
    }
}