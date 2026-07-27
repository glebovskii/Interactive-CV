using System;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class PanelLinkButton : MonoBehaviour
{
    private const string ButtonName = "button-link";

    [SerializeField] private PanelRenderer panelRenderer;

    [SerializeField] private string link;

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

    private void OnUIReload(
        PanelRenderer renderer,
        VisualElement root,
        int version)
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

    private void OpenLink()
    {
        if (!IsValidWebLink(link))
        {
            Debug.LogError(
                $"Invalid link assigned to {nameof(PanelLinkButton)}: '{link}'",
                this);

            return;
        }

        Application.OpenURL(link);
    }

    private void UnregisterButton()
    {
        if (linkButton == null)
            return;

        Debug.LogError("UNREGISTER");

        linkButton.clicked -= OpenLink;
        linkButton = null;
    }

    private static bool IsValidWebLink(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (!Uri.TryCreate(
                value,
                UriKind.Absolute,
                out Uri uri))
        {
            return false;
        }

        return uri.Scheme == Uri.UriSchemeHttp ||
               uri.Scheme == Uri.UriSchemeHttps;
    }
}