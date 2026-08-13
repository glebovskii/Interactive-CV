using UnityEngine;

public sealed class CameraBillboard : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    private void LateUpdate()
    {
        Camera camera = targetCamera != null ? targetCamera : Camera.main;

        if (camera != null)
            transform.rotation = camera.transform.rotation;
    }
}
