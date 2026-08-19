using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public sealed class GrassShaderPanelController : MonoBehaviour
{
    private const string UpdateStaticsMember = "updateStatics";

    private const float DefaultScale = 2166f;
    private const int DefaultShellCount = 117;
    private const float DefaultShellLength = 0.11f;
    private const float DefaultDistanceAttenuation = 1f;
    private const float DefaultCameraDistanceThreshold = 35f;
    private const float DefaultDensity = 3066f;
    private const float DefaultNoiseMin = 0f;
    private const float DefaultNoiseMax = 1f;
    private const float DefaultThickness = 10f;
    private const float DefaultCurvature = 10f;
    private const float DefaultDisplacementStrength = 1f;
    private const float DefaultOcclusionAttenuation = 1.3f;
    private const float DefaultOcclusionBias = 0f;
    private const float DefaultWindStrength = 0.01f;
    private const float DefaultWindFrequency = 2.92f;
    private const float DefaultWindHeightAttenuation = 2.09f;
    private const float DefaultTurbulenceStrength = 0.16f;

    private static readonly Color DefaultShellColor = new(107f / 255f, 149f / 255f, 9f / 255f, 1f);
    private static readonly Color DefaultBaseColor = new(45f / 255f, 184f / 255f, 144f / 255f, 1f);
    private static readonly Vector3 DefaultWindDirection = new(0.03f, 0f, 1f);

    [SerializeField] private PanelRenderer panelRenderer;
    [SerializeField] private MonoBehaviour simpleShell;

    private readonly UICallbackBinder uiCallbacks = new();
    private readonly Dictionary<string, FieldInfo> fields = new();
    private readonly Dictionary<string, PropertyInfo> properties = new();
    private readonly HashSet<string> missingMembers = new();

    private readonly LocalizedString shellTabText = new("ui", "grass.tab_shell");
    private readonly LocalizedString appearanceTabText = new("ui", "grass.tab_appearance");
    private readonly LocalizedString windTabText = new("ui", "grass.tab_wind");

    private Tab shellTab;
    private Tab appearanceTab;
    private Tab windTab;

    private VisualElement root;
    private VisualElement panelRoot;
    private bool isUIOpen;

    private void Awake()
    {
        if (panelRenderer == null)
            panelRenderer = GetComponent<PanelRenderer>();

        CacheMembers();

        shellTabText.StringChanged += OnShellTabTextChanged;
        appearanceTabText.StringChanged += OnAppearanceTabTextChanged;
        windTabText.StringChanged += OnWindTabTextChanged;

        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnEnable()
    {
        if (panelRoot != null)
            RefreshOpenState();
    }

    private void OnDisable()
    {
        SetUIOpen(false);
    }

    private void OnDestroy()
    {
        SetUIOpen(false);
        uiCallbacks.Clear();

        shellTabText.StringChanged -= OnShellTabTextChanged;
        appearanceTabText.StringChanged -= OnAppearanceTabTextChanged;
        windTabText.StringChanged -= OnWindTabTextChanged;

        if (panelRenderer != null)
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);
    }

    public void SetUIOpen(bool open)
    {
        isUIOpen = open;
        SetMember(UpdateStaticsMember, open);
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement newRoot, int version)
    {
        uiCallbacks.Clear();

        root = newRoot;
        panelRoot = root.Q<VisualElement>("panel-root") ?? root;

        TabView tabView = root.Q<TabView>("shader-tab-view");
        shellTab = root.Q<Tab>("tab-shell");
        appearanceTab = root.Q<Tab>("tab-appearance");
        windTab = root.Q<Tab>("tab-wind");

        if (tabView != null)
            tabView.selectedTabIndex = 0;

        shellTabText.RefreshString();
        appearanceTabText.RefreshString();
        windTabText.RefreshString();

        BindFloat("slider-scale", "scale", DefaultScale);
        BindInt("slider-shell-count", "shellCount", DefaultShellCount);
        BindFloat("slider-shell-length", "shellLength", DefaultShellLength);
        BindFloat("slider-distance-attenuation", "distanceAttenuation", DefaultDistanceAttenuation);
        BindFloat("slider-draw-distance", "cameraDistanceThreshold", DefaultCameraDistanceThreshold);
        BindFloat("slider-density", "density", DefaultDensity);
        BindFloat("slider-noise-min", "noiseMin", DefaultNoiseMin);
        BindFloat("slider-noise-max", "noiseMax", DefaultNoiseMax);
        BindFloat("slider-thickness", "thickness", DefaultThickness);
        BindFloat("slider-curvature", "curvature", DefaultCurvature);
        BindFloat("slider-displacement-strength", "displacementStrength", DefaultDisplacementStrength);

        BindColor("shell-color", "shellColor", DefaultShellColor);
        BindColor("base-color", "baseColor", DefaultBaseColor);

        BindFloat("slider-occlusion-attenuation", "occlusionAttenuation", DefaultOcclusionAttenuation);
        BindFloat("slider-occlusion-bias", "occlusionBias", DefaultOcclusionBias);

        BindVector3("wind-direction", "windDirection", DefaultWindDirection);
        BindFloat("slider-wind-strength", "windStrength", DefaultWindStrength);
        BindFloat("slider-wind-frequency", "windFrequency", DefaultWindFrequency);
        BindFloat("slider-wind-height-attenuation", "windHeightAttenuation", DefaultWindHeightAttenuation);
        BindFloat("slider-turbulence-strength", "turbulenceStrength", DefaultTurbulenceStrength);

        Button resetButton = root.Q<Button>("button-reset");
        if (resetButton != null)
            uiCallbacks.BindClick(resetButton, ResetToDefaults, sound => sound.PlayButtonClick());

        uiCallbacks.Bind<GeometryChangedEvent>(panelRoot, _ => RefreshOpenState(), null);
        RefreshOpenState();
    }

    private void OnShellTabTextChanged(string value)
    {
        if (shellTab != null)
            shellTab.label = value;
    }

    private void OnAppearanceTabTextChanged(string value)
    {
        if (appearanceTab != null)
            appearanceTab.label = value;
    }

    private void OnWindTabTextChanged(string value)
    {
        if (windTab != null)
            windTab.label = value;
    }

    private void BindFloat(string sliderName, string memberName, float fallback)
    {
        Slider slider = root.Q<Slider>(sliderName);
        if (slider == null)
            return;

        slider.SetValueWithoutNotify(GetMember(memberName, fallback));
        uiCallbacks.BindChange<float>(slider, value => SetMember(memberName, value), sound => sound.PlaySliderChange());
    }

    private void BindInt(string sliderName, string memberName, int fallback)
    {
        SliderInt slider = root.Q<SliderInt>(sliderName);
        if (slider == null)
            return;

        slider.SetValueWithoutNotify(GetMember(memberName, fallback));
        uiCallbacks.BindChange<int>(slider, value => SetMember(memberName, value), sound => sound.PlaySliderChange());
    }

    private void BindVector3(string prefix, string memberName, Vector3 fallback)
    {
        Slider x = root.Q<Slider>($"slider-{prefix}-x");
        Slider y = root.Q<Slider>($"slider-{prefix}-y");
        Slider z = root.Q<Slider>($"slider-{prefix}-z");

        if (x == null || y == null || z == null)
            return;

        Vector3 value = GetMember(memberName, fallback);

        x.SetValueWithoutNotify(value.x);
        y.SetValueWithoutNotify(value.y);
        z.SetValueWithoutNotify(value.z);

        void Apply()
        {
            SetMember(memberName, new Vector3(x.value, y.value, z.value));
        }

        uiCallbacks.BindChange<float>(x, _ => Apply(), sound => sound.PlaySliderChange());
        uiCallbacks.BindChange<float>(y, _ => Apply(), sound => sound.PlaySliderChange());
        uiCallbacks.BindChange<float>(z, _ => Apply(), sound => sound.PlaySliderChange());
    }

    private void BindColor(string prefix, string memberName, Color fallback)
    {
        Slider r = root.Q<Slider>($"slider-{prefix}-r");
        Slider g = root.Q<Slider>($"slider-{prefix}-g");
        Slider b = root.Q<Slider>($"slider-{prefix}-b");
        VisualElement preview = root.Q<VisualElement>($"{prefix}-preview");

        if (r == null || g == null || b == null)
            return;

        Color value = GetMember(memberName, fallback);

        r.SetValueWithoutNotify(value.r);
        g.SetValueWithoutNotify(value.g);
        b.SetValueWithoutNotify(value.b);
        SetColorPreview(preview, value);

        void Apply()
        {
            Color color = new(r.value, g.value, b.value, value.a);
            SetMember(memberName, color);
            SetColorPreview(preview, color);
        }

        uiCallbacks.BindChange<float>(r, _ => Apply(), sound => sound.PlaySliderChange());
        uiCallbacks.BindChange<float>(g, _ => Apply(), sound => sound.PlaySliderChange());
        uiCallbacks.BindChange<float>(b, _ => Apply(), sound => sound.PlaySliderChange());
    }

    private void ResetToDefaults()
    {
        SetMember("scale", DefaultScale);
        SetMember("shellCount", DefaultShellCount);
        SetMember("shellLength", DefaultShellLength);
        SetMember("distanceAttenuation", DefaultDistanceAttenuation);
        SetMember("cameraDistanceThreshold", DefaultCameraDistanceThreshold);
        SetMember("density", DefaultDensity);
        SetMember("noiseMin", DefaultNoiseMin);
        SetMember("noiseMax", DefaultNoiseMax);
        SetMember("thickness", DefaultThickness);
        SetMember("curvature", DefaultCurvature);
        SetMember("displacementStrength", DefaultDisplacementStrength);
        SetMember("shellColor", DefaultShellColor);
        SetMember("baseColor", DefaultBaseColor);
        SetMember("occlusionAttenuation", DefaultOcclusionAttenuation);
        SetMember("occlusionBias", DefaultOcclusionBias);
        SetMember("windDirection", DefaultWindDirection);
        SetMember("windStrength", DefaultWindStrength);
        SetMember("windFrequency", DefaultWindFrequency);
        SetMember("windHeightAttenuation", DefaultWindHeightAttenuation);
        SetMember("turbulenceStrength", DefaultTurbulenceStrength);

        SyncUI();
    }

    private void SyncUI()
    {
        SetSlider("slider-scale", GetMember("scale", DefaultScale));
        SetSliderInt("slider-shell-count", GetMember("shellCount", DefaultShellCount));
        SetSlider("slider-shell-length", GetMember("shellLength", DefaultShellLength));
        SetSlider("slider-distance-attenuation", GetMember("distanceAttenuation", DefaultDistanceAttenuation));
        SetSlider("slider-draw-distance", GetMember("cameraDistanceThreshold", DefaultCameraDistanceThreshold));
        SetSlider("slider-density", GetMember("density", DefaultDensity));
        SetSlider("slider-noise-min", GetMember("noiseMin", DefaultNoiseMin));
        SetSlider("slider-noise-max", GetMember("noiseMax", DefaultNoiseMax));
        SetSlider("slider-thickness", GetMember("thickness", DefaultThickness));
        SetSlider("slider-curvature", GetMember("curvature", DefaultCurvature));
        SetSlider("slider-displacement-strength", GetMember("displacementStrength", DefaultDisplacementStrength));

        SyncColor("shell-color", GetMember("shellColor", DefaultShellColor));
        SyncColor("base-color", GetMember("baseColor", DefaultBaseColor));

        SetSlider("slider-occlusion-attenuation", GetMember("occlusionAttenuation", DefaultOcclusionAttenuation));
        SetSlider("slider-occlusion-bias", GetMember("occlusionBias", DefaultOcclusionBias));

        Vector3 windDirection = GetMember("windDirection", DefaultWindDirection);
        SetSlider("slider-wind-direction-x", windDirection.x);
        SetSlider("slider-wind-direction-y", windDirection.y);
        SetSlider("slider-wind-direction-z", windDirection.z);

        SetSlider("slider-wind-strength", GetMember("windStrength", DefaultWindStrength));
        SetSlider("slider-wind-frequency", GetMember("windFrequency", DefaultWindFrequency));
        SetSlider("slider-wind-height-attenuation", GetMember("windHeightAttenuation", DefaultWindHeightAttenuation));
        SetSlider("slider-turbulence-strength", GetMember("turbulenceStrength", DefaultTurbulenceStrength));
    }

    private void SyncColor(string prefix, Color value)
    {
        SetSlider($"slider-{prefix}-r", value.r);
        SetSlider($"slider-{prefix}-g", value.g);
        SetSlider($"slider-{prefix}-b", value.b);
        SetColorPreview(root.Q<VisualElement>($"{prefix}-preview"), value);
    }

    private void SetSlider(string name, float value)
    {
        root.Q<Slider>(name)?.SetValueWithoutNotify(value);
    }

    private void SetSliderInt(string name, int value)
    {
        root.Q<SliderInt>(name)?.SetValueWithoutNotify(value);
    }

    private static void SetColorPreview(VisualElement preview, Color color)
    {
        if (preview != null)
            preview.style.backgroundColor = color;
    }

    private void RefreshOpenState()
    {
        if (panelRoot == null)
            return;

        bool open = panelRoot.resolvedStyle.display != DisplayStyle.None &&
                    panelRoot.resolvedStyle.visibility == Visibility.Visible &&
                    panelRoot.resolvedStyle.opacity > 0.001f;

        SetUIOpen(open);
    }

    private void CacheMembers()
    {
        fields.Clear();
        properties.Clear();

        if (simpleShell == null)
            return;

        Type type = simpleShell.GetType();

        while (type != null && type != typeof(MonoBehaviour))
        {
            foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                fields.TryAdd(Normalize(field.Name), field);

            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                properties.TryAdd(Normalize(property.Name), property);

            type = type.BaseType;
        }
    }

    private T GetMember<T>(string memberName, T fallback)
    {
        if (simpleShell == null)
            return fallback;

        string key = Normalize(memberName);

        try
        {
            if (fields.TryGetValue(key, out FieldInfo field))
                return ConvertValue(field.GetValue(simpleShell), fallback);

            if (properties.TryGetValue(key, out PropertyInfo property) && property.CanRead)
                return ConvertValue(property.GetValue(simpleShell), fallback);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
        }

        LogMissingMember(memberName);
        return fallback;
    }

    private void SetMember<T>(string memberName, T value)
    {
        if (simpleShell == null)
            return;

        string key = Normalize(memberName);

        try
        {
            if (fields.TryGetValue(key, out FieldInfo field))
            {
                field.SetValue(simpleShell, ConvertTo(value, field.FieldType));
                return;
            }

            if (properties.TryGetValue(key, out PropertyInfo property) && property.CanWrite)
            {
                property.SetValue(simpleShell, ConvertTo(value, property.PropertyType));
                return;
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
            return;
        }

        LogMissingMember(memberName);
    }

    private static T ConvertValue<T>(object value, T fallback)
    {
        if (value is T typed)
            return typed;

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return fallback;
        }
    }

    private static object ConvertTo<T>(T value, Type targetType)
    {
        if (value is null)
            return null;

        Type valueType = value.GetType();

        if (targetType.IsAssignableFrom(valueType))
            return value;

        if (targetType == typeof(float))
            return Convert.ToSingle(value);

        if (targetType == typeof(int))
            return Convert.ToInt32(value);

        if (targetType == typeof(bool))
            return Convert.ToBoolean(value);

        return Convert.ChangeType(value, targetType);
    }

    private void LogMissingMember(string memberName)
    {
        if (!missingMembers.Add(memberName))
            return;

        Debug.LogError($"SimpleShell member '{memberName}' was not found on '{simpleShell.GetType().Name}'.", this);
    }

    private static string Normalize(string value)
    {
        return value.Replace("_", string.Empty).Replace(" ", string.Empty).ToLowerInvariant();
    }
}