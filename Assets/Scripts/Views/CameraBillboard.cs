using UnityEngine;
using UnityEngine.UIElements;

public sealed class CameraBillboard : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    private PanelRenderer panelRenderer;


    private void Awake()
    {
        panelRenderer = GetComponent<PanelRenderer>();
    }

    //private void OnBecameVisible()
    //{
    //    panelRenderer.enabled = true;
    //}

    //private void OnBecameInvisible()
    //{
    //    panelRenderer.enabled = false;
    //}

    private void LateUpdate()
    {
        if(!panelRenderer.isVisible)
            return;

        Camera camera = targetCamera != null ? targetCamera : Camera.main;

        if (camera != null)
            transform.rotation = camera.transform.rotation;
    }
}
