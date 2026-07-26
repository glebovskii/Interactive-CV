using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

public class PanelUI : MonoBehaviour
{
    [SerializeField] private PanelRenderer panelRenderer;

    private Transform camera;

    public void Hide()
    {
        camera = null;
        panelRenderer.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        //if (camera == null)
        //{
        //    Debug.LogError("CAMERA IS NULL");
        //    return;
        //}
        //transform.rotation = camera.rotation;
    }

    public void Show(CinemachineCamera camera)
    {
        this.camera = camera.transform;
        panelRenderer.gameObject.SetActive(true);
    }
}
