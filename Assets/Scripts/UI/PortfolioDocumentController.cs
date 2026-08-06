using System;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public sealed class PortfolioDocumentController : MonoBehaviour
{
    [Header("Section templates in display order")]
    [SerializeField] private VisualTreeAsset[] contentSections;
    [SerializeField] private VisualTreeAsset[] socialSections;

    [Header("Initial state")]
    [SerializeField] private bool visibleOnEnable;

    private PanelRenderer panelRenderer;
    private VisualElement root;
    private Button closeButton;

    public event Action<VisualElement> DocumentBuilt;

    public VisualElement Root => root;

    public bool IsVisible =>
        root != null && root.style.display.value != DisplayStyle.None;

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
        UnregisterCloseButton();
        root = null;
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement newRoot, int version)
    {
        UnregisterCloseButton();
        root = newRoot;

        BuildTemplates(root.Q<VisualElement>("content-sections"), contentSections);
        BuildTemplates(root.Q<VisualElement>("social-sections"), socialSections);

        closeButton = root.Q<Button>("close-button");
        if (closeButton != null)
            closeButton.clicked += Hide;

        SetVisible(visibleOnEnable);
        DocumentBuilt?.Invoke(root);
    }

    private static void BuildTemplates(
        VisualElement container,
        VisualTreeAsset[] templates)
    {
        if (container == null)
        {
            Debug.LogError("Portfolio UXML is missing a required template container.");
            return;
        }

        container.Clear();

        if (templates == null)
            return;

        foreach (VisualTreeAsset template in templates)
        {
            if (template == null)
                continue;

            template.CloneTree(container);
        }
    }

    public void Show() => SetVisible(true);
    public void Hide() => SetVisible(false);
    public void Toggle() => SetVisible(!IsVisible);

    public void SetVisible(bool visible)
    {
        visibleOnEnable = visible;

        if (root == null)
            return;

        root.style.display = visible
            ? DisplayStyle.Flex
            : DisplayStyle.None;
    }

    private void UnregisterCloseButton()
    {
        if (closeButton != null)
            closeButton.clicked -= Hide;

        closeButton = null;
    }
}
