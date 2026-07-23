using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public sealed class PlayerEnterController : MonoBehaviour
{
    private PanelRenderer panelRenderer;
    private Button enterButton;

    private void OnEnable()
    {
        panelRenderer = GetComponent<PanelRenderer>();
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnDisable()
    {
        if (panelRenderer != null)
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);

        UnregisterButton();
    }

    private void OnUIReload(
        PanelRenderer renderer,
        VisualElement root,
        int version)
    {
        UnregisterButton();

        enterButton = root.Q<Button>("enter-button");

        if (enterButton == null)
        {
            Debug.LogError("Button 'enter-button' was not found.");
            return;
        }

        enterButton.RegisterCallback<ClickEvent>(OnEnterButtonClicked);
    }

    private void OnEnterButtonClicked(ClickEvent evt)
    {
        Debug.Log("Enter button pressed.");
        Enter();
    }

    private void Enter()
    {
        if (ServiceLocator.TryGet(out NetworkSessionService networkSessionService))
        {
            networkSessionService.JoinDefaultRoomAsync();
            Debug.Log("JOIN ROOM CALLED");
        }
        else
        {
            Debug.LogError("NOT FOUND NETWORK SESSION CONTROLLER");
        }

    }

    private void UnregisterButton()
    {
        if (enterButton == null)
            return;

        enterButton.UnregisterCallback<ClickEvent>(OnEnterButtonClicked);
        enterButton = null;
    }
}