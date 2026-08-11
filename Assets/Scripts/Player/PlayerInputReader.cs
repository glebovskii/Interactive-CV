using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerInputReader : MonoBehaviour
{
    [Header("Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference clickAction;

    [Header("Click Detection")]
    [SerializeField, Min(0f)]
    private float maximumClickDuration = 0.2f;

    [SerializeField, Min(0f)]
    private float maximumClickMovement = 10f;

    private bool inputEnabled;

    private bool pointerHeld;
    private bool pointerReleaseBuffered;
    private bool bufferedReleaseWasClick;
    private bool hasPointerPosition;

    private float pointerPressTime;

    private Vector2 pointerPressPosition;
    private Vector2 pointerPosition;
    public bool IsPointerOverUI { get; private set; }
    private void Update()
    {
        if (!CanRead(clickAction))
            return;

        InputAction action = clickAction.action;
        Pointer pointer = action.activeControl?.device as Pointer ?? Pointer.current;

        if (pointer == null)
            return;

        pointerPosition = pointer.position.ReadValue();
        hasPointerPosition = true;

        IsPointerOverUI = UIInputBlocker.IsPointerOverUI(pointerPosition);

        if (IsPointerOverUI)
        {
            CancelPointerInput();
            return;
        }

        if (action.WasPressedThisFrame())
        {
            pointerHeld = true;
            pointerPressTime = Time.unscaledTime;
            pointerPressPosition = pointerPosition;
        }

        if (action.WasReleasedThisFrame())
        {
            float pressDuration = Time.unscaledTime - pointerPressTime;
            float pointerMovement = Vector2.Distance(pointerPressPosition, pointerPosition);

            bufferedReleaseWasClick =
                pressDuration <= maximumClickDuration &&
                pointerMovement <= maximumClickMovement;

            pointerHeld = false;
            pointerReleaseBuffered = true;
        }
    }

    private void CancelPointerInput()
    {
        pointerHeld = false;
        pointerReleaseBuffered = false;
        bufferedReleaseWasClick = false;
    }

    public void SetInputEnabled(bool value)
    {
        if (inputEnabled == value)
            return;

        inputEnabled = value;

        SetActionEnabled(moveAction, value);
        SetActionEnabled(clickAction, value);

        if (!value)
            ResetPointerInput();
    }

    public Vector2 ReadMovement()
    {
        if (!CanRead(moveAction))
            return Vector2.zero;

        return Vector2.ClampMagnitude(moveAction.action.ReadValue<Vector2>(), 1f);
    }

    public bool TryReadPointerPosition(out Vector2 screenPosition)
    {
        screenPosition = pointerPosition;

        if (!inputEnabled || !hasPointerPosition)
            return false;

        //if (EventSystem.current.IsPointerOverGameObject())
        //    return false;

        return pointerHeld || pointerReleaseBuffered;
    }

    public bool TryConsumePointerRelease(out bool wasClick)
    {
        wasClick = false;

        if (!inputEnabled || !pointerReleaseBuffered)
            return false;

        wasClick = bufferedReleaseWasClick;

        pointerReleaseBuffered = false;
        bufferedReleaseWasClick = false;

        return true;
    }

    public bool IsPointerHeld()
    {
        return inputEnabled && pointerHeld;
    }

    private bool CanRead(InputActionReference actionReference)
    {
        //if(EventSystem.current.IsPointerOverGameObject(-1))
        //    return false;
        return inputEnabled &&
               actionReference != null &&
               actionReference.action != null &&
               actionReference.action.enabled;
    }

    private static void SetActionEnabled(InputActionReference actionReference, bool enabled)
    {
        if (actionReference?.action == null)
            return;

        if (enabled)
            actionReference.action.Enable();
        else
            actionReference.action.Disable();
    }

    private void ResetPointerInput()
    {
        pointerHeld = false;
        pointerReleaseBuffered = false;
        bufferedReleaseWasClick = false;
        hasPointerPosition = false;

        pointerPressTime = 0f;
        pointerPressPosition = Vector2.zero;
        pointerPosition = Vector2.zero;
    }

    private void OnDisable()
    {
        SetInputEnabled(false);
    }
}