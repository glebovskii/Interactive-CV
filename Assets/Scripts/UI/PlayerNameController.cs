using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public class PlayerNameController : MonoBehaviour
{
    private PanelRenderer panelRenderer;
    private TextField textField;

    private void OnEnable()
    {
        panelRenderer = GetComponent<PanelRenderer>();
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);

        textField?.UnregisterCallback<ChangeEvent<string>>(OnInputFinished);
    }

    private TextField playerNameField;

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        playerNameField?.UnregisterCallback<ChangeEvent<string>>(OnInputFinished);

        playerNameField = root.Q<TextField>("player-name");
        playerNameField.textEdition.placeholder = PlayerInfoSave.GetName();
        playerNameField.RegisterCallback<ChangeEvent<string>>(OnInputFinished);
    }

    private void OnInputFinished(ChangeEvent<string> evt)
    {
        PlayerInfoSave.SaveName(evt.newValue);
    }
}
