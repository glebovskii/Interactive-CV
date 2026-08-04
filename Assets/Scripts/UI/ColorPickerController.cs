using System;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public sealed class ColorPickerController : MonoBehaviour
{
    private readonly UICallbackBinder uiCallbacks = new();

    private PanelRenderer panelRenderer;
    private HsvColorWheel colorWheel;
    private VisualElement preview;
    private VisualElement wheelContainer;

    public event Action<Color> ColorChanged;
    public event Action ColorPicked;

    private void Awake()
    {
        panelRenderer = GetComponent<PanelRenderer>();
    }

    private void OnEnable()
    {
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        CleanupUI();
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        CleanupUI();

        wheelContainer = root.Q<VisualElement>("wheel-container");
        preview = root.Q<VisualElement>("color-preview");

        if (wheelContainer == null)
        {
            Debug.LogError("Color picker requires an element named 'wheel-container'.");
            return;
        }

        colorWheel = new HsvColorWheel();
        colorWheel.SetValueWithoutNotify(Color.white);
        colorWheel.style.width = Length.Percent(100);
        colorWheel.style.height = Length.Percent(100);

        wheelContainer.Add(colorWheel);

        uiCallbacks.BindChange<Color>(colorWheel, OnColorChanged, sound => sound.PlaySliderChange());
        colorWheel.OnColorPicked += OnColorPicked;

        UpdatePreview();
    }

    private void OnColorChanged(Color color)
    {
        UpdatePreview();
        ColorChanged?.Invoke(color);
    }

    private void OnColorPicked()
    {
        ColorPicked?.Invoke();
    }

    private void UpdatePreview()
    {
        if (colorWheel != null && preview != null)
            preview.style.backgroundColor = colorWheel.value;
    }

    private void CleanupUI()
    {
        uiCallbacks.Clear();

        if (colorWheel != null)
        {
            colorWheel.OnColorPicked -= OnColorPicked;
            colorWheel.RemoveFromHierarchy();
            colorWheel.Dispose();
            colorWheel = null;
        }

        preview = null;
        wheelContainer = null;
    }
}