using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public class PlayerUI : MonoBehaviour
{
    private PanelRenderer panelRenderer;

    private Transform camera;
    private bool isLocalPlayer;

    private void OnEnable()
    {
        panelRenderer = GetComponent<PanelRenderer>();
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }
    public void Init(CinemachineCamera cinemachineCamera, bool isLocalPlayer)
    {
        camera = cinemachineCamera.transform;
    }

    private void LateUpdate()
    {
        if (camera == null) return;

        transform.rotation = camera.rotation;
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);
    }

    private Label playerNameField;

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        playerNameField = root.Q<Label>("player-name");
        playerNameField.text = PlayerInfoSave.GetName();
    }

    public void SetVisible(bool value)
    {
        panelRenderer.enabled = value;
    }

    
}
