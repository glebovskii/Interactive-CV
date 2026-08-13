using Unity.Cinemachine;
using UnityEngine;

public sealed class WorldSpacePanelTilt : MonoBehaviour
{
    [SerializeField, Range(-45f, 45f)] private float maximumXTilt = 6f;
    [SerializeField, Range(-90f, 90f)] private float maximumYTilt = 18f;
    [SerializeField, Min(0f)] private float rotationSmoothness = 6f;
    [SerializeField] private bool frontFacesNegativeZ;

    private Transform cameraTransform;
    private Quaternion baseLocalRotation;
    private Vector3 baseLocalEulerAngles;

    private void Awake()
    {
        baseLocalRotation = transform.localRotation;
        baseLocalEulerAngles = transform.localEulerAngles;
    }

    public void Follow(CinemachineCamera camera)
    {
        if (camera == null)
            return;

        cameraTransform = camera.transform;
    }

    public void StopFollowing()
    {
        cameraTransform = null;
        transform.localRotation = baseLocalRotation;
    }

    private void LateUpdate()
    {
        if (cameraTransform == null)
            return;

        Vector3 directionToCamera = cameraTransform.position - transform.position;

        if (directionToCamera.sqrMagnitude < 0.0001f)
            return;

        directionToCamera.Normalize();

        if (frontFacesNegativeZ)
            directionToCamera = -directionToCamera;

        Quaternion baseWorldRotation = transform.parent != null ? transform.parent.rotation * baseLocalRotation : baseLocalRotation;
        Vector3 localDirection = Quaternion.Inverse(baseWorldRotation) * directionToCamera;

        float desiredYRotation = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
        float horizontalDistance = new Vector2(localDirection.x, localDirection.z).magnitude;
        float desiredXRotation = -Mathf.Atan2(localDirection.y, horizontalDistance) * Mathf.Rad2Deg;

        float xTilt = Mathf.Clamp(desiredXRotation, -maximumXTilt, maximumXTilt);
        float yTilt = Mathf.Clamp(desiredYRotation, -maximumYTilt, maximumYTilt);
        Quaternion targetRotation = Quaternion.Euler(baseLocalEulerAngles.x + xTilt, baseLocalEulerAngles.y + yTilt, baseLocalEulerAngles.z);

        float interpolation = 1f - Mathf.Exp(-rotationSmoothness * Time.deltaTime);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, interpolation);
    }
}
