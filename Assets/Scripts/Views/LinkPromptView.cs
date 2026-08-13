using DG.Tweening;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

public sealed class LinkPromptView : PanelRendererBehaviour
{
    [SerializeField] private ExternalLink link;
    [SerializeField] private string buttonName = "link";
    [SerializeField] private string fillName = "fill";
    [SerializeField, Min(0f)] private float idleDistance = 3f;
    [SerializeField, Min(0.1f)] private float halfCycleSeconds = 0.9f;
    [SerializeField] private LocalizedString openLinkText;

    private readonly UIElementAnimator buttonAnimator = new();

    private VisualElement orb;
    private VisualElement fill;
    private Button openButton;
    private Tween idleTween;
    private Vector3 basePosition;
    private float normalizedFill;
    private bool openButtonVisible;

    protected override void OnUIReload(VisualElement root)
    {
        StopIdle();
        buttonAnimator.Stop();

        if (openButton != null)
            openButton.clicked -= OpenLink;

        openLinkText.StringChanged -= OnOpenLinkTextChanged;

        orb = root.Q<VisualElement>(buttonName);
        fill = root.Q<VisualElement>(fillName);

        if (fill != null)
            UpdateFillVisual();

        if (orb == null)
        {
            Debug.LogError($"VisualElement named '{buttonName}' was not found.", this);
            return;
        }

        basePosition = orb.resolvedStyle.translate;
        CreateOpenButton();

        openButtonVisible = normalizedFill >= 1f;

        if (openButtonVisible)
            buttonAnimator.Show(openButton, false);
        else
            buttonAnimator.Hide(openButton, false);

        if (isActiveAndEnabled)
            StartIdle();
    }

    private void OnEnable()
    {
        if (orb != null)
            StartIdle();
    }

    private void OnDisable()
    {
        StopIdle();
        buttonAnimator.Stop();
    }

    public void SetFill(float value)
    {
        normalizedFill = Mathf.Clamp01(value);

        if (fill != null)
            UpdateFillVisual();

        bool shouldShowButton = normalizedFill >= 1f;

        if (openButton == null || shouldShowButton == openButtonVisible)
            return;

        openButtonVisible = shouldShowButton;

        if (openButtonVisible)
            buttonAnimator.Show(openButton);
        else
            buttonAnimator.Hide(openButton);
    }

    private void UpdateFillVisual()
    {
        fill.style.scale = new Scale(new Vector3(normalizedFill, normalizedFill, 1f));
    }

    private void CreateOpenButton()
    {
        openButton = new Button
        {
            name = "trigger-open-link",
            text = openLinkText.GetLocalizedString()
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

    private void OpenLink()
    {
        link?.Open();
    }

    private void StartIdle()
    {
        if (orb == null)
            return;

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

    protected override void OnDestroy()
    {
        StopIdle();
        buttonAnimator.Stop();
        openLinkText.StringChanged -= OnOpenLinkTextChanged;

        if (openButton != null)
            openButton.clicked -= OpenLink;

        base.OnDestroy();
    }
}
