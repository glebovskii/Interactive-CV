using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public sealed class PlayerNameController : MonoBehaviour
{
    private readonly UICallbackBinder uiCallbacks = new();

    private PanelRenderer panelRenderer;
    private TextField playerNameField;

    private void OnEnable()
    {
        panelRenderer = GetComponent<PanelRenderer>();
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        uiCallbacks.Clear();
        playerNameField = null;
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        uiCallbacks.Clear();

        playerNameField = root.Q<TextField>("player-name");

        if (playerNameField == null)
        {
            Debug.LogError("TextField named 'player-name' was not found.");
            return;
        }

        playerNameField.textEdition.placeholder = PlayerInfoSave.GetName();
        playerNameField.hideMobileInput = true;
        uiCallbacks.BindChange<string>(playerNameField, PlayerInfoSave.SaveName, sound => sound.PlayLinkLoad());
        AnalyticsService.NameChanged(PlayerInfoSave.GetName());
    }
}