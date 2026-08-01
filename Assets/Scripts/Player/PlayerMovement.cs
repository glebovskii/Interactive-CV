using Fusion;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputReader))]
public sealed class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)]
    private float moveSpeed = 4f;

    [SerializeField, Min(0f)]
    private float rotationSpeed = 12f;

    [SerializeField, Min(0f)]
    private float stoppingDistance = 0.15f;

    [SerializeField]
    private float groundedVerticalVelocity = -2f;

    [Header("Mouse Movement")]
    [SerializeField] private LayerMask walkableLayer;
    [SerializeField] private LayerMask uiBlockLayer;

    [SerializeField, Min(0f)]
    private float maximumRayDistance = 1000f;

    [Header("References")]
    [SerializeField]
    private CharacterController characterController;

    private Camera playerCamera;

    private PlayerInputReader inputReader;

    private Vector3 pointerTarget;
    private Vector3 currentMoveDirection;

    private float verticalVelocity;
    private bool hasPointerTarget;

    public event Action<bool> OnSpawn;

    [Networked] public Vector3 Velocity { get; private set; }
    [Networked] public Color Color { get; private set; }

    private int combinedRaycastMask;

    public override void Spawned()
    {
        bool isLocallyControlled = HasStateAuthority;

        if (isLocallyControlled)
        {
            var name = PlayerInfoSave.GetName();
            Color = PlayerInfoSave.GetColor();
        }
        inputReader = GetComponent<PlayerInputReader>();

        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        if (playerCamera == null && isLocallyControlled)
            playerCamera = Camera.main;

        inputReader.SetInputEnabled(isLocallyControlled);

        hasPointerTarget = false;
        currentMoveDirection = Vector3.zero;
        Velocity = Vector3.zero;

        combinedRaycastMask = walkableLayer.value | uiBlockLayer.value;

        OnSpawn?.Invoke(isLocallyControlled);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        if (!characterController.enabled)
            return;

        UpdateGravity();
        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector2 keyboardInput = inputReader.ReadMovement();

        if (keyboardInput.sqrMagnitude > 0.001f)
        {
            hasPointerTarget = false;
            HandleKeyboardMovement(keyboardInput);
            return;
        }

        if (inputReader.TryReadPointerPosition(out Vector2 pointerPosition))
        {
            TryUpdatePointerTarget(pointerPosition);
        }

        if (inputReader.TryConsumePointerRelease(out bool wasClick))
        {
            if (!wasClick)
            {
                hasPointerTarget = false;
                MoveCharacter(Vector3.zero);
                return;
            }
        }

        if (hasPointerTarget)
        {
            HandlePointerMovement();
            return;
        }

        MoveCharacter(Vector3.zero);
    }

    private void HandleKeyboardMovement(Vector2 input)
    {
        Vector3 direction = new(input.x, 0f, input.y);

        direction = Vector3.ClampMagnitude(direction, 1f);

        MoveCharacter(direction);
    }

    private void HandlePointerMovement()
    {
        Vector3 toTarget = pointerTarget - transform.position;

        toTarget.y = 0f;

        float stoppingDistanceSquared = stoppingDistance * stoppingDistance;

        if (toTarget.sqrMagnitude <= stoppingDistanceSquared)
        {
            hasPointerTarget = false;
            MoveCharacter(Vector3.zero);
            return;
        }

        Vector3 direction = toTarget.normalized;

        MoveCharacter(direction);
    }

    private bool TryUpdatePointerTarget(Vector2 screenPosition)
    {
        if (playerCamera == null)
            return false;

        Ray ray = playerCamera.ScreenPointToRay(screenPosition);

        if (!Physics.Raycast(ray, out var hit, maximumRayDistance, combinedRaycastMask))
        {
            return false;
        }

        int hitLayerMask = 1 << hit.collider.gameObject.layer;
        if ((uiBlockLayer.value & hitLayerMask) != 0)
            return false;

        pointerTarget = hit.point;
        hasPointerTarget = true;

        return true;
    }

    private void MoveCharacter(Vector3 horizontalDirection)
    {
        currentMoveDirection = horizontalDirection;
        currentMoveDirection.y = 0f;

        if (currentMoveDirection.sqrMagnitude > 1f)
            currentMoveDirection.Normalize();

        Vector3 velocity = currentMoveDirection * moveSpeed;

        velocity.y = verticalVelocity;

        Vector3 previousPosition = transform.position;

        characterController.Move(velocity * Runner.DeltaTime);

        Velocity = (transform.position - previousPosition) / Runner.DeltaTime;

        HandleRotation(currentMoveDirection);
    }

    private void HandleRotation(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

        float interpolation = 1f - Mathf.Exp(-rotationSpeed * Runner.DeltaTime);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, interpolation);
    }

    private void UpdateGravity()
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundedVerticalVelocity;
        }
        else
        {
            verticalVelocity += Physics.gravity.y * Runner.DeltaTime;
        }
    }

    public void StopMovement()
    {
        hasPointerTarget = false;
        currentMoveDirection = Vector3.zero;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        inputReader?.SetInputEnabled(false);
    }
}