using System;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class HsvColorWheel : VisualElement, INotifyValueChanged<Color>, IDisposable
{
    private const int DefaultTextureResolution = 256;
    private const float MarkerSize = 14f;

    private readonly VisualElement marker;

    private Texture2D wheelTexture;

    private Color currentValue = Color.red;

    private float hue;
    private float saturation = 1f;
    private float brightness = 1f;
    private float alpha = 1f;

    private int activePointerId = -1;

    public event Action OnColorPicked;

    public HsvColorWheel() : this(DefaultTextureResolution)
    {
    }

    public HsvColorWheel(int textureResolution)
    {
        focusable = true;

        style.position = Position.Relative;
        style.width = 200f;
        style.height = 200f;

        GenerateWheelTexture(textureResolution);

        marker = CreateMarker();
        Add(marker);

        RegisterCallback<PointerDownEvent>(OnPointerDown);
        RegisterCallback<PointerMoveEvent>(OnPointerMove);
        RegisterCallback<PointerUpEvent>(OnPointerUp);
        RegisterCallback<PointerCancelEvent>(OnPointerCancel);
        RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    }

    public Color value
    {
        get => currentValue;
        set
        {
            if (ApproximatelyEqual(currentValue, value))
                return;

            Color previousValue = currentValue;

            SetValueWithoutNotify(value);
            SendColorChangedEvent(previousValue, currentValue);
        }
    }

    public void SetValueWithoutNotify(Color newValue)
    {
        currentValue = newValue;
        alpha = newValue.a;

        Color.RGBToHSV(
            newValue,
            out hue,
            out saturation,
            out brightness);

        UpdateMarker();
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        if (activePointerId != -1)
            return;

        activePointerId = evt.pointerId;
        this.CapturePointer(activePointerId);

        UpdateFromPointer(evt.localPosition);
        evt.StopPropagation();
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (evt.pointerId != activePointerId)
            return;

        if (!this.HasPointerCapture(activePointerId))
            return;

        UpdateFromPointer(evt.localPosition);
        evt.StopPropagation();
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (evt.pointerId != activePointerId)
            return;

        ReleaseActivePointer();
        SendColorPickedEvent();
        evt.StopPropagation();
    }

    private void OnPointerCancel(PointerCancelEvent evt)
    {
        if (evt.pointerId != activePointerId)
            return;

        ReleaseActivePointer();
    }

    private void ReleaseActivePointer()
    {
        if (activePointerId == -1)
            return;

        if (this.HasPointerCapture(activePointerId))
            this.ReleasePointer(activePointerId);

        activePointerId = -1;
    }

    private void UpdateFromPointer(Vector2 localPosition)
    {
        Rect rect = contentRect;

        float radius = Mathf.Min(rect.width, rect.height) * 0.5f;

        if (radius <= 0f)
            return;

        Vector2 direction = localPosition - rect.center;

        float newSaturation =
            Mathf.Clamp01(direction.magnitude / radius);

        float angle =
            Mathf.Atan2(-direction.y, direction.x);

        float newHue = Mathf.Repeat(
            angle / (Mathf.PI * 2f),
            1f);

        Color newColor = Color.HSVToRGB(
            newHue,
            newSaturation,
            brightness);

        if (ApproximatelyEqual(currentValue, newColor))
            return;

        Color previousValue = currentValue;

        hue = newHue;
        saturation = newSaturation;
        currentValue = newColor;

        UpdateMarker();
        SendColorChangedEvent(previousValue, currentValue);
    }

    private static bool ApproximatelyEqual(Color a, Color b)
    {
        const float tolerance = 0.0001f;

        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance &&
               Mathf.Abs(a.a - b.a) < tolerance;
    }

    private void UpdateMarker()
    {
        if (marker == null)
            return;

        Rect rect = contentRect;

        float radius = Mathf.Min(rect.width, rect.height) * 0.5f;

        if (radius <= 0f)
            return;

        float angle = hue * Mathf.PI * 2f;
        float distance = saturation * radius;

        Vector2 direction = new(
            Mathf.Cos(angle),
            -Mathf.Sin(angle));

        Vector2 markerPosition =
            rect.center + direction * distance;

        marker.style.left = markerPosition.x - MarkerSize * 0.5f;
        marker.style.top = markerPosition.y - MarkerSize * 0.5f;

        marker.style.backgroundColor = currentValue;
    }

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        UpdateMarker();
    }

    private void SendColorChangedEvent(Color previousValue, Color newValue)
    {
        using ChangeEvent<Color> changeEvent = ChangeEvent<Color>.GetPooled(previousValue, newValue);

        changeEvent.target = this;
        SendEvent(changeEvent);
    }

    private void SendColorPickedEvent()
    {
        OnColorPicked?.Invoke();
    }

    private void GenerateWheelTexture(int resolution)
    {
        resolution = Mathf.Max(32, resolution);

        wheelTexture = new Texture2D(
            resolution,
            resolution,
            TextureFormat.RGBA32,
            false)
        {
            name = "UI Toolkit HSV Wheel",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };

        Color32[] pixels = new Color32[resolution * resolution];

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float normalizedX =
                    ((x + 0.5f) / resolution) * 2f - 1f;

                float normalizedY =
                    ((y + 0.5f) / resolution) * 2f - 1f;

                Vector2 position = new(
                    normalizedX,
                    normalizedY);

                float distance = position.magnitude;
                int pixelIndex = y * resolution + x;

                if (distance > 1f)
                {
                    pixels[pixelIndex] = new Color32(0, 0, 0, 0);
                    continue;
                }

                float pixelHue = Mathf.Repeat(
                    Mathf.Atan2(normalizedY, normalizedX) /
                    (Mathf.PI * 2f),
                    1f);

                pixels[pixelIndex] = Color.HSVToRGB(
                    pixelHue,
                    distance,
                    1f);
            }
        }

        wheelTexture.SetPixels32(pixels);
        wheelTexture.Apply(
            updateMipmaps: false,
            makeNoLongerReadable: true);

        style.backgroundImage =
            new StyleBackground(wheelTexture);
    }

    private static VisualElement CreateMarker()
    {
        VisualElement element = new()
        {
            name = "color-wheel-marker",
            pickingMode = PickingMode.Ignore
        };

        element.style.position = Position.Absolute;

        element.style.width = MarkerSize;
        element.style.height = MarkerSize;

        element.style.borderTopLeftRadius = MarkerSize;
        element.style.borderTopRightRadius = MarkerSize;
        element.style.borderBottomLeftRadius = MarkerSize;
        element.style.borderBottomRightRadius = MarkerSize;

        element.style.borderLeftWidth = 2f;
        element.style.borderRightWidth = 2f;
        element.style.borderTopWidth = 2f;
        element.style.borderBottomWidth = 2f;

        element.style.borderLeftColor = Color.white;
        element.style.borderRightColor = Color.white;
        element.style.borderTopColor = Color.white;
        element.style.borderBottomColor = Color.white;

        return element;
    }

    public void Dispose()
    {
        ReleaseActivePointer();

        if (wheelTexture != null)
        {
            if (Application.isPlaying)
                UnityEngine.Object.Destroy(wheelTexture);
            else
                UnityEngine.Object.DestroyImmediate(wheelTexture);

            wheelTexture = null;
        }
    }
}