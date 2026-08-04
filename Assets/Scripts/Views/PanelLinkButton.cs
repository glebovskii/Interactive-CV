using UnityEngine;
using UnityEngine.UIElements;

public sealed class PanelLinkButton : Link
{
    private const string ButtonName = "button-link";

    [SerializeField] private PanelRenderer panelRenderer;

    private readonly UICallbackBinder uiCallbacks = new();

    private Button linkButton;

    private void Awake()
    {
        if (panelRenderer == null)
            panelRenderer = GetComponent<PanelRenderer>();

        if (panelRenderer == null)
        {
            Debug.LogError($"{nameof(PanelLinkButton)} requires a PanelRenderer.", this);
            return;
        }

        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnDestroy()
    {
        uiCallbacks.Clear();

        if (panelRenderer != null)
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        uiCallbacks.Clear();

        linkButton = root.Q<Button>(ButtonName);

        if (linkButton == null)
        {
            Debug.LogError($"Button named '{ButtonName}' was not found.", this);
            return;
        }

        uiCallbacks.BindClick(linkButton, OpenLinkWithoutSound, sound => sound.PlayButtonClick());
    }
}