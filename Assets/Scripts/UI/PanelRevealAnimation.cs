using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class PanelRevealAnimation : MonoBehaviour
{
    private const string RootName = "panel-root";
    private const string StaticOverlayName = "static-overlay";

    [SerializeField]
    private PanelRenderer panelRenderer;

    [Header("Appear")]
    [SerializeField, Min(0f)]
    private float lineWidthDuration = 0.08f;

    [SerializeField, Min(0f)]
    private float verticalExpandDuration = 0.22f;

    [Header("Disappear")]
    [SerializeField, Min(0f)]
    private float verticalCollapseDuration = 0.16f;

    [SerializeField, Min(0f)]
    private float lineCollapseDuration = 0.07f;

    [Header("Shape")]
    [SerializeField, Range(0.001f, 0.1f)]
    private float lineHeight = 0.015f;

    [SerializeField, Range(0.001f, 1f)]
    private float initialLineWidth = 0.08f;

    [Header("Static")]
    [SerializeField, Range(0f, 1f)]
    private float appearStaticOpacity = 0.65f;

    [SerializeField, Range(0f, 1f)]
    private float disappearStaticOpacity = 0.35f;

    [SerializeField]
    private bool ignoreTimeScale = true;

    private VisualElement panelRoot;
    private VisualElement staticOverlay;

    private Sequence animationSequence;

    private float currentScaleX = 1f;
    private float currentScaleY = 1f;
    private float currentStaticOpacity;

    private bool isInitialized;

    private void Awake()
    {
        if (panelRenderer == null)
            panelRenderer = GetComponent<PanelRenderer>();

        if (panelRenderer == null)
        {
            Debug.LogError(
                $"{nameof(PanelRevealAnimation)} requires a PanelRenderer.",
                this);

            return;
        }

        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnDestroy()
    {
        animationSequence?.Kill();

        if (panelRenderer != null)
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);
    }

    private void OnUIReload(
        PanelRenderer renderer,
        VisualElement root,
        int version)
    {
        animationSequence?.Kill();

        panelRoot = root.Q<VisualElement>(RootName);
        staticOverlay = root.Q<VisualElement>(StaticOverlayName);

        if (panelRoot == null)
        {
            Debug.LogError(
                $"VisualElement named '{RootName}' was not found.",
                this);

            return;
        }

        panelRoot.style.transformOrigin = new TransformOrigin(
            Length.Percent(50f),
            Length.Percent(50f),
            0f);

        SetScale(1f, 1f);
        SetStaticOpacity(0f);

        isInitialized = true;

        Hide();
    }

    public void Show()
    {
        if (!isInitialized)
            return;

        animationSequence?.Kill();

        panelRoot.style.display = DisplayStyle.Flex;
        panelRoot.style.opacity = 1f;

        SetScale(initialLineWidth, lineHeight);
        SetStaticOpacity(appearStaticOpacity);

        animationSequence = DOTween.Sequence()
            .SetUpdate(ignoreTimeScale);

        // Small central line expands horizontally.
        animationSequence.Append(
            DOTween.To(
                    () => currentScaleX,
                    value => SetScale(value, currentScaleY),
                    1f,
                    lineWidthDuration)
                .SetEase(Ease.OutQuad));

        // The horizontal line opens into the complete panel.
        animationSequence.Append(
            DOTween.To(
                    () => currentScaleY,
                    value => SetScale(currentScaleX, value),
                    1f,
                    verticalExpandDuration)
                .SetEase(Ease.OutCubic));

        // Static disappears while the panel opens.
        animationSequence.Join(
            DOTween.To(
                    () => currentStaticOpacity,
                    SetStaticOpacity,
                    0f,
                    verticalExpandDuration)
                .SetEase(Ease.OutQuad));

        animationSequence.OnComplete(() =>
        {
            SetScale(1f, 1f);
            SetStaticOpacity(0f);
        });
    }

    public void Hide()
    {
        if (!isInitialized)
            return;

        animationSequence?.Kill();

        SetScale(1f, 1f);
        SetStaticOpacity(0f);

        animationSequence = DOTween.Sequence()
            .SetUpdate(ignoreTimeScale);

        // Introduce a brief static flash while collapsing vertically.
        animationSequence.Append(
            DOTween.To(
                    () => currentScaleY,
                    value => SetScale(currentScaleX, value),
                    lineHeight,
                    verticalCollapseDuration)
                .SetEase(Ease.InCubic));

        animationSequence.Join(
            DOTween.To(
                    () => currentStaticOpacity,
                    SetStaticOpacity,
                    disappearStaticOpacity,
                    verticalCollapseDuration * 0.6f)
                .SetEase(Ease.OutQuad));

        // Collapse the remaining horizontal line.
        animationSequence.Append(
            DOTween.To(
                    () => currentScaleX,
                    value => SetScale(value, currentScaleY),
                    0f,
                    lineCollapseDuration)
                .SetEase(Ease.InQuad));

        animationSequence.Join(
            DOTween.To(
                    () => currentStaticOpacity,
                    SetStaticOpacity,
                    0f,
                    lineCollapseDuration));

        animationSequence.OnComplete(() =>
        {
            panelRoot.style.display = DisplayStyle.None;

            SetScale(1f, 1f);
            SetStaticOpacity(0f);
        });
    }

    private void SetScale(float x, float y)
    {
        currentScaleX = x;
        currentScaleY = y;

        if (panelRoot == null)
            return;

        panelRoot.style.scale = new Scale(
            new Vector3(x, y, 1f));
    }

    private void SetStaticOpacity(float opacity)
    {
        currentStaticOpacity = opacity;

        if (staticOverlay != null)
            staticOverlay.style.opacity = opacity;
    }
}