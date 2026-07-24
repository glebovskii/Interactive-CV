using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerInputReader : MonoBehaviour
{
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference clickAction;

    private bool inputEnabled;

    private bool clickBuffered;
    private Vector2 bufferedClickPosition;

    private void Update()
    {
        if (!CanRead(clickAction))
            return;

        if (!clickAction.action.WasPressedThisFrame())
            return;

        if (clickAction.action.activeControl?.device is not Pointer pointer)
            return;

        bufferedClickPosition = pointer.position.ReadValue();
        clickBuffered = true;
    }

    public void SetInputEnabled(bool value)
    {
        if (inputEnabled == value)
            return;

        inputEnabled = value;

        SetActionEnabled(moveAction, value);
        SetActionEnabled(clickAction, value);

        if (!value)
        {
            clickBuffered = false;
            bufferedClickPosition = Vector2.zero;
        }
    }

    public Vector2 ReadMovement()
    {
        if (!CanRead(moveAction))
            return Vector2.zero;

        return moveAction.action.ReadValue<Vector2>();// Vector2.ClampMagnitude(moveAction.action.ReadValue<Vector2>(), 1f);
    }

    public bool CheckIsClicked(out Vector2 screenPosition)
    {
        screenPosition = bufferedClickPosition;

        if (!inputEnabled || !clickBuffered)
            return false;

        clickBuffered = false;
        return true;
    }

    private bool CanRead(InputActionReference actionReference)
    {
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

    private void OnDisable()
    {
        SetInputEnabled(false);
    }
}