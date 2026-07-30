using UnityEngine;
using UnityEngine.UIElements;

public class PanelLinkButton : Link
{
    private const string ButtonName = "button-link";

    [SerializeField] private PanelRenderer panelRenderer;

    private Button linkButton;

    private void Awake()
    {
        if (panelRenderer == null)
        {
            Debug.LogError(
                $"{nameof(PanelLinkButton)} requires a PanelRenderer.",
                this);

            return;
        }

        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnDestroy()
    {
        UnregisterButton();

        if (panelRenderer != null)
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        // Remove the callback from the previous visual tree.
        UnregisterButton();

        linkButton = root.Q<Button>(name:ButtonName);

        if (linkButton == null)
        {
            Debug.LogError(
                $"Button named '{ButtonName}' was not found.",
                this);

            return;
        }

        linkButton.clicked += OpenLink;
    }

    private void UnregisterButton()
    {
        if (linkButton == null)
            return;

        linkButton.clicked -= OpenLink;
        linkButton = null;
    }
}