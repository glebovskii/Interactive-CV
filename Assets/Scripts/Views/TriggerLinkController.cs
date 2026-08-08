using DG.Tweening;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public sealed class TriggerLinkController : MonoBehaviour
{
    [SerializeField] private PanelRenderer panelRenderer;
    [SerializeField] private Link link;
    [SerializeField] private string buttonName = "link";
    [SerializeField] private string fillName = "fill";
    [SerializeField, Min(0f)] private float idleDistance = 3f;
    [SerializeField, Min(0.1f)] private float halfCycleSeconds = 0.9f;
    [SerializeField] private LocalizedString openLinkText;

    private VisualElement orb;
    private VisualElement fill;
    private Button openButton;
    private Tween idleTween;
    private Tween buttonTween;
    private Vector3 basePosition;
    private float normalizedFill;

    private void Awake()
    {
        if (panelRenderer == null)
            panelRenderer = GetComponent<PanelRenderer>();

        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnEnable()
    {
        if (orb != null)
            StartIdle();
    }

    private void OnDisable()
    {
        StopIdle();
        StopButtonTween();
    }

    private void Update()
    {
        transform.rotation = Camera.main.transform.rotation;
    }

    private void OnDestroy()
    {
        StopIdle();
        StopButtonTween();

        openLinkText.StringChanged -= OnOpenLinkTextChanged;

        if (openButton != null)
            openButton.clicked -= OpenLink;

        if (panelRenderer != null)
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);
    }

    public void SetFill(float value)
    {
        normalizedFill = Mathf.Clamp01(value);

        if (fill != null)
            fill.style.scale = new Scale(new Vector3(normalizedFill, normalizedFill, 1f));

        if (normalizedFill >= 1f)
            ShowOpenButton();
        else
            HideOpenButton();
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        StopIdle();
        StopButtonTween();

        orb = root.Q<VisualElement>(buttonName);
        fill = root.Q<VisualElement>(fillName);

        if (fill != null)
            fill.style.scale = new Scale(new Vector3(normalizedFill, normalizedFill, 1f));

        if (orb == null)
            return;

        basePosition = orb.resolvedStyle.translate;

        CreateOpenButton();

        if (normalizedFill >= 1f)
            ShowOpenButton(false);
        else
            HideOpenButton(false);

        if (isActiveAndEnabled)
            StartIdle();
    }

    private void CreateOpenButton()
    {
        if (openButton != null)
            openButton.clicked -= OpenLink;

        openLinkText.StringChanged -= OnOpenLinkTextChanged;

        openButton = new Button
        {
            name = "trigger-open-link"
        };

        openButton.AddToClassList("trigger-open-link");
        openButton.clicked += OpenLink;
        orb.Add(openButton);

        openLinkText.StringChanged += OnOpenLinkTextChanged;
    }

    private void OnOpenLinkTextChanged(string value)
    {
        if (openButton != null)
            openButton.text = value;
    }

    private void ShowOpenButton(bool animate = true)
    {
        if (openButton == null || openButton.style.display == DisplayStyle.Flex)
            return;

        StopButtonTween();

        openButton.style.display = DisplayStyle.Flex;
        openButton.style.opacity = animate ? 0f : 1f;
        openButton.style.scale = animate ? new Scale(new Vector3(0.8f, 0.8f, 1f)) : new Scale(Vector3.one);

        if (!animate)
            return;

        float progress = 0f;

        buttonTween = DOTween.To(() => progress, value =>
        {
            progress = value;
            openButton.style.opacity = progress;

            float scale = Mathf.Lerp(0.8f, 1f, EaseOutBack(progress));
            openButton.style.scale = new Scale(new Vector3(scale, scale, 1f));
        }, 1f, 0.25f).SetUpdate(true);
    }

    private void HideOpenButton(bool animate = true)
    {
        if (openButton == null || openButton.style.display == DisplayStyle.None)
            return;

        StopButtonTween();

        if (!animate)
        {
            openButton.style.display = DisplayStyle.None;
            return;
        }

        float progress = 1f;

        buttonTween = DOTween.To(() => progress, value =>
        {
            progress = value;
            openButton.style.opacity = progress;
            openButton.style.scale = new Scale(new Vector3(progress, progress, 1f));
        }, 0f, 0.15f).SetUpdate(true).OnComplete(() =>
        {
            if (openButton != null)
                openButton.style.display = DisplayStyle.None;
        });
    }

    private void OpenLink()
    {
        link.OpenLink();
    }

    private void StartIdle()
    {
        StopIdle();

        orb.style.translate = basePosition + Vector3.down * idleDistance;

        idleTween = DOVirtual.Float(-idleDistance, idleDistance, halfCycleSeconds,
                y => orb.style.translate = basePosition + Vector3.up * y)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);
    }

    private void StopIdle()
    {
        idleTween?.Kill();
        idleTween = null;

        if (orb != null)
            orb.style.translate = basePosition;
    }

    private void StopButtonTween()
    {
        buttonTween?.Kill();
        buttonTween = null;
    }

    private static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }
}