using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public class PlayerUI : MonoBehaviour
{
    private PanelRenderer panelRenderer;

    private Transform camera;
    private string playerName;

    private void OnEnable()
    {
        panelRenderer = GetComponent<PanelRenderer>();
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }
    public void Init(CinemachineCamera cinemachineCamera, bool isLocalPlayer, string name)
    {
        camera = cinemachineCamera.transform;
        playerName = name;
    }

    private void LateUpdate()
    {
        if (camera == null)
        {
            Debug.LogError("CAMERA IS NULL");
            return;
        }
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
        playerNameField.text = playerName;
    }

    public void SetVisible(bool value)
    {
        gameObject.SetActive(value);
    }

    
}
