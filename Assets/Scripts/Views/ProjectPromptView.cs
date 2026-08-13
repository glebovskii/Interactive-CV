using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

public sealed class ProjectPromptView : PanelRendererBehaviour
{
    private const string RootName = "panel-root";
    private const string ButtonName = "trigger-open-link";

    [SerializeField] private LocalizedString localizedText;

    private readonly UIElementAnimator animator = new();
    private Button openButton;

    public event Action Clicked;

    protected override void OnUIReload(VisualElement root)
    {
        if (openButton != null)
            openButton.clicked -= OnClicked;

        localizedText.StringChanged -= OnLocalizedTextChanged;

        VisualElement buttonRoot = root.Q<VisualElement>(RootName);

        if (buttonRoot == null)
        {
            Debug.LogError($"VisualElement named '{RootName}' was not found.", this);
            return;
        }

        openButton = new Button
        {
            name = ButtonName,
            text = localizedText.GetLocalizedString()
        };

        openButton.AddToClassList(ButtonName);
        openButton.clicked += OnClicked;
        buttonRoot.Add(openButton);
        localizedText.StringChanged += OnLocalizedTextChanged;
        Hide(false);
    }

    public void Show(bool animate = true)
    {
        if (Camera.main != null)
            transform.LookAt(Camera.main.transform);

        animator.Show(openButton, animate);
    }

    public void Hide(bool animate = true)
    {
        animator.Hide(openButton, animate);
    }

    private void OnClicked()
    {
        Clicked?.Invoke();
    }

    private void OnLocalizedTextChanged(string value)
    {
        if (openButton != null)
            openButton.text = value;
    }

    private void OnDisable()
    {
        animator.Stop();
    }

    protected override void OnDestroy()
    {
        animator.Stop();
        localizedText.StringChanged -= OnLocalizedTextChanged;

        if (openButton != null)
            openButton.clicked -= OnClicked;

        base.OnDestroy();
    }
}
