using System;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public sealed class PlayerEnterController : MonoBehaviour
{
    private PanelRenderer panelRenderer;
    private VisualElement menuContent;
    private VisualElement findingRoomOverlay;
    private VisualElement findingRoomSpinner;
    private Button enterButton;
    private IVisualElementScheduledItem spinnerAnimation;

    private bool isFindingRoom;
    private float spinnerAngle;

    private UISoundController soundController;

    private void OnEnable()
    {
        panelRenderer = GetComponent<PanelRenderer>();
        panelRenderer.RegisterUIReloadCallback(OnUIReload);

        ServiceLocator.TryGet(out soundController);
    }

    private void OnDisable()
    {
        if (panelRenderer != null)
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);

        UnregisterUI();
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        UnregisterUI();

        menuContent = root.Q<VisualElement>("menu-content");
        findingRoomOverlay = root.Q<VisualElement>("finding-room-overlay");
        findingRoomSpinner = root.Q<VisualElement>("finding-room-spinner");
        enterButton = root.Q<Button>("enter-button");

        if (menuContent == null || findingRoomOverlay == null || findingRoomSpinner == null || enterButton == null)
        {
            Debug.LogError("Player menu UI elements were not found.");
            return;
        }

        enterButton.RegisterCallback<ClickEvent>(OnEnterButtonClicked);

        if (isFindingRoom)
            ShowFindingRoom();
        else
            HideFindingRoom();
    }

    private void OnEnterButtonClicked(ClickEvent evt)
    {
        evt.StopPropagation();

        if (isFindingRoom)
            return;

        Enter();
    }

    private async void Enter()
    {
        if (!ServiceLocator.TryGet(out NetworkSessionService networkSessionService))
        {
            Debug.LogError("NOT FOUND NETWORK SESSION CONTROLLER");
            return;
        }

        ServiceLocator.TryGet(out soundController);
        soundController?.PlayButtonClick();
        ShowFindingRoom();

        try
        {
            await networkSessionService.JoinDefaultRoomAsync();
            Debug.Log("JOIN ROOM CALLED");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
        finally
        {
            HideFindingRoom();
        }
    }

    private void ShowFindingRoom()
    {
        isFindingRoom = true;

        if (menuContent == null || findingRoomOverlay == null || findingRoomSpinner == null)
            return;

        menuContent.SetEnabled(false);
        findingRoomOverlay.style.display = DisplayStyle.Flex;
        findingRoomOverlay.Focus();

        spinnerAnimation?.Pause();
        spinnerAnimation = findingRoomSpinner.schedule.Execute(() =>
        {
            spinnerAngle = (spinnerAngle + 8f) % 360f;
            findingRoomSpinner.style.rotate = new StyleRotate(new Rotate(Angle.Degrees(spinnerAngle)));
        }).Every(16);
    }

    private void HideFindingRoom()
    {
        isFindingRoom = false;
        spinnerAnimation?.Pause();
        spinnerAnimation = null;

        if (menuContent != null)
            menuContent.SetEnabled(true);

        if (findingRoomOverlay != null)
            findingRoomOverlay.style.display = DisplayStyle.None;
    }

    private void UnregisterUI()
    {
        spinnerAnimation?.Pause();
        spinnerAnimation = null;

        if (enterButton != null)
            enterButton.UnregisterCallback<ClickEvent>(OnEnterButtonClicked);

        menuContent = null;
        findingRoomOverlay = null;
        findingRoomSpinner = null;
        enterButton = null;
    }
}