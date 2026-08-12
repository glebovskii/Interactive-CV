using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
[RequireComponent(typeof(PanelUI))]
public class OpenProjectButton : MonoBehaviour
{
    private PanelRenderer panelRenderer;
    private Button openButton;
    private Tween buttonTween;
    [SerializeField] private LocalizedString localizedText;

    public bool Inited { get; private set; }

    private void OnEnable()
    {
        Inited = false;
        panelRenderer = GetComponent<PanelRenderer>();
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnUIReload(PanelRenderer panelRenderer, VisualElement rootElement)
    {
        CreateOpenButton(rootElement.Q<VisualElement>(name: "panel-root"));
    }

    public void CreateOpenButton(VisualElement root)
    {
        localizedText.StringChanged -= OnOpenButtonTextChanged;

        openButton = new Button
        {
            name = "trigger-open-link"
        };

        openButton.AddToClassList("trigger-open-link");
        root.Add(openButton);
        localizedText.StringChanged += OnOpenButtonTextChanged;
    
        Inited = true;
    }

    public void Set(Action action)
    {
        if (openButton != null)
            openButton.clicked -= action;

        openButton.clicked += action;
    }

    private void OnOpenButtonTextChanged(string value)
    {
        if (openButton != null)
            openButton.text = value;
    }

    public void ShowOpenButton(PlayerView view, bool animate = true)
    {
        if (openButton == null || openButton.style.display == DisplayStyle.Flex)
            return;

        transform.LookAt(Camera.main.transform);
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

    public void HideOpenButton(bool animate = true)
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
