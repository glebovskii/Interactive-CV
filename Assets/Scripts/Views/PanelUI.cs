using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class PanelUI : MonoBehaviour
{
    [SerializeField] private PanelRenderer panelRenderer;
    [SerializeField] private PanelRevealAnimation panelRevealAnimation;
    [SerializeField] private PanelTiltSettings tiltSettings;

    [Header("Camera tilt")]
    [SerializeField, Range(-45f, 45f)] private float maximumXTilt = 6f;
    [SerializeField, Range(-90f, 90f)] private float maximumYTilt = 18f;
    [SerializeField, Min(0f)] private float rotationSmoothness = 6f;

    [Tooltip("Enable this if the visible front of the panel points along local -Z.")]
    [SerializeField] private bool frontFacesNegativeZ;

    private Transform cameraTransform;
    private Quaternion baseLocalRotation;
    private Vector3 baseLocalEulerAngles;

    public float MaximumXTilt => maximumXTilt;
    public float MaximumYTilt => maximumYTilt;
    public PanelTiltSettings TiltSettings => tiltSettings;

    private void Awake()
    {
        //ApplyTiltSettings();
    }

    public void ApplyTiltSettings()
    {
        if (tiltSettings == null)
            return;

        transform.localPosition = tiltSettings.localPosition;
        transform.localRotation = Quaternion.Euler(tiltSettings.localEulerAngles);
        transform.localScale = tiltSettings.localScale;

        maximumXTilt = tiltSettings.maximumXTilt;
        maximumYTilt = tiltSettings.maximumYTilt;

        CacheBaseTransform();
    }

    public void Show(CinemachineCamera camera)
    {
        if (camera == null)
        {
            Debug.LogError("Cannot show PanelUI: CinemachineCamera is null.", this);
            return;
        }

        cameraTransform = camera.transform;
        CacheBaseTransform();
        panelRevealAnimation.Show();
    }

    public void Hide(bool playSound = true)
    {
        cameraTransform = null;
        panelRevealAnimation.Hide(playSound);
        transform.localRotation = baseLocalRotation;
    }

    private void LateUpdate()
    {
        if (cameraTransform != null)
            RotateSlightlyTowardsCamera();
    }

    private void RotateSlightlyTowardsCamera()
    {
        Vector3 directionToCamera = cameraTransform.position - transform.position;

        if (directionToCamera.sqrMagnitude < 0.0001f)
            return;

        directionToCamera.Normalize();

        if (frontFacesNegativeZ)
            directionToCamera = -directionToCamera;

        Quaternion baseWorldRotation = transform.parent != null
            ? transform.parent.rotation * baseLocalRotation
            : baseLocalRotation;

        Vector3 localDirection = Quaternion.Inverse(baseWorldRotation) * directionToCamera;

        float desiredYRotation = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
        float horizontalDistance = new Vector2(localDirection.x, localDirection.z).magnitude;
        float desiredXRotation = -Mathf.Atan2(localDirection.y, horizontalDistance) * Mathf.Rad2Deg;

        float xTilt = Mathf.Clamp(desiredXRotation, -maximumXTilt, maximumXTilt);
        float yTilt = Mathf.Clamp(desiredYRotation, -maximumYTilt, maximumYTilt);

        Quaternion targetRotation = Quaternion.Euler(
            baseLocalEulerAngles.x + xTilt,
            baseLocalEulerAngles.y + yTilt,
            baseLocalEulerAngles.z);

        float interpolation = 1f - Mathf.Exp(-rotationSmoothness * Time.deltaTime);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, interpolation);
    }

    private void CacheBaseTransform()
    {
        baseLocalRotation = transform.localRotation;
        baseLocalEulerAngles = transform.localEulerAngles;
    }
}