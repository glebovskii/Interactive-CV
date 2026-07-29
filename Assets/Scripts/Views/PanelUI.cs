using Unity.Cinemachine;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UIElements;

public sealed class PanelUI : MonoBehaviour
{
    [SerializeField] private PanelRenderer panelRenderer;
    [SerializeField] private PanelRevealAnimation panelRevealAnimation;

    [Header("Camera tilt")]
    [SerializeField, Range(-45f, 45f)]
    private float maximumXTilt = 6f;

    [SerializeField, Range(-90f, 90f)]
    private float maximumYTilt = 18f;

    [SerializeField, Min(0f)]
    private float rotationSmoothness = 6f;

    [Tooltip("Enable this if the visible front of the panel points along local -Z.")]
    [SerializeField]
    private bool frontFacesNegativeZ;

    private Transform cameraTransform;
    private Quaternion baseRotation;

    private void Awake()
    {
        baseRotation = transform.rotation;
    }

    public void Show(CinemachineCamera camera)
    {
        if (camera == null)
        {
            Debug.LogError("Cannot show PanelUI: CinemachineCamera is null.", this);
            return;
        }

        cameraTransform = camera.transform;

        // Store the panel's normal resting orientation.
        baseRotation = transform.rotation;

        panelRevealAnimation.Show();
    }

    public void Hide()
    {
        cameraTransform = null;
        panelRevealAnimation.Hide();
        transform.rotation = baseRotation;
    }

    private void LateUpdate()
    {

        //if (cameraTransform == null)
        //{
        //    Debug.LogError("CAMERA IS NULL");
        //    return;
        //}
        //    transform.rotation = cameraTransform.rotation;


        if (cameraTransform == null)
            return;

        RotateSlightlyTowardsCamera();
    }

    private void RotateSlightlyTowardsCamera()
    {
        Vector3 directionToCamera =
            cameraTransform.position - transform.position;

        if (directionToCamera.sqrMagnitude < 0.0001f)
            return;

        directionToCamera.Normalize();

        if (frontFacesNegativeZ)
            directionToCamera = -directionToCamera;

        // Convert camera direction into the panel's original local space.
        Vector3 localDirection =
            Quaternion.Inverse(baseRotation) * directionToCamera;

        float desiredYRotation =
            Mathf.Atan2(
                localDirection.x,
                localDirection.z) *
            Mathf.Rad2Deg;

        float horizontalDistance = new Vector2(
            localDirection.x,
            localDirection.z).magnitude;

        float desiredXRotation =
            -Mathf.Atan2(
                localDirection.y,
                horizontalDistance) *
            Mathf.Rad2Deg;

        float xTilt = Mathf.Clamp(
            desiredXRotation,
            -maximumXTilt,
            maximumXTilt);

        float yTilt = Mathf.Clamp(
            desiredYRotation,
            -maximumYTilt,
            maximumYTilt);

        Quaternion targetRotation =
            baseRotation *
            Quaternion.Euler(xTilt, yTilt, 0f);

        float interpolation =
            1f - Mathf.Exp(
                -rotationSmoothness * Time.deltaTime);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            interpolation);
    }
}