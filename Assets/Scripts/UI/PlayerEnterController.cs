using System;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public sealed class PlayerEnterController : MonoBehaviour
{
    private readonly UICallbackBinder uiCallbacks = new();

    private PanelRenderer panelRenderer;
    private VisualElement menuContent;
    private VisualElement findingRoomOverlay;
    private VisualElement findingRoomSpinner;
    private Button enterButton;
    private IVisualElementScheduledItem spinnerAnimation;

    private bool isFindingRoom;
    private float spinnerAngle;

    private void OnEnable()
    {
        panelRenderer = GetComponent<PanelRenderer>();
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        UnregisterUI();
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        UnregisterUI();
        HideFindingRoom();

        menuContent = root.Q<VisualElement>("menu-content");
        findingRoomOverlay = root.Q<VisualElement>("finding-room-overlay");
        findingRoomSpinner = root.Q<VisualElement>("finding-room-spinner");
        enterButton = root.Q<Button>("enter-button");

        if (menuContent == null || findingRoomOverlay == null || findingRoomSpinner == null || enterButton == null)
        {
            Debug.LogError("Player menu UI elements were not found.");
            return;
        }

        uiCallbacks.Bind<ClickEvent>(enterButton, OnEnterButtonClicked, sound => sound.PlayButtonClick());

        if (isFindingRoom)
            ShowFindingRoom();
        else
            HideFindingRoom();
    }

    private void OnEnterButtonClicked(ClickEvent evt)
    {
        evt.StopPropagation();

        if (!isFindingRoom)
            Enter();
    }

    private async void Enter()
    {
        if (!ServiceLocator.TryGet(out NetworkSessionService networkSessionService))
        {
            Debug.LogError("NOT FOUND NETWORK SESSION CONTROLLER");
            return;
        }

        ShowFindingRoom();

        try
        {
            await networkSessionService.JoinDefaultRoomAsync();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
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

        menuContent?.SetEnabled(true);

        if (findingRoomOverlay != null)
            findingRoomOverlay.style.display = DisplayStyle.None;
    }

    private void UnregisterUI()
    {
        uiCallbacks.Clear();

        spinnerAnimation?.Pause();
        spinnerAnimation = null;

        menuContent = null;
        findingRoomOverlay = null;
        findingRoomSpinner = null;
        enterButton = null;
    }
}