using Fusion;
using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;

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

    [SerializeField]
    private float groundedVerticalVelocity = -2f;

    [Header("Physics")]
    [SerializeField] private LayerMask walkableLayer;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private Transform rotationTransform;

    private CharacterController characterController;
    private PlayerInputReader inputReader;

    private float verticalVelocity;

    private RaycastHit[] hits;
    private Camera camera;

    private NavMeshAgent agent;

    private Vector3 moveDirection;

    public event Action OnSpawn;
    public Vector3 Velocity { get; private set; }

    private bool isNavMeshMove = false;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        inputReader = GetComponent<PlayerInputReader>();
        agent = GetComponentInChildren<NavMeshAgent>();
        camera = Camera.main;
        hits = new RaycastHit[1];

        agent.speed = moveSpeed;
        agent.updateRotation = true;
    }

    public override void Spawned()
    {
        bool isLocallyControlled = HasStateAuthority;

        inputReader.SetInputEnabled(isLocallyControlled);
        characterController.enabled = isLocallyControlled;

        OnSpawn?.Invoke();
    }

    private void HandleMovement()
    {
        if (inputReader.CheckIsClicked(out var clickPos))
        {
            HandleNavMeshMovement(clickPos);
        }
        else
        {
            HandleInputMovement();
            HandleRotation();
        }
    }

    private void HandleInputMovement()
    {
        Vector2 moveInput = inputReader.ReadMovement();
        moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        if (moveDirection.sqrMagnitude <= 0)
            return;
        agent.isStopped = true;
        isNavMeshMove = false;

        Vector3 velocity = moveDirection * moveSpeed;
        velocity.y = verticalVelocity;

        characterController.enabled = true;
        characterController.Move(velocity * Runner.DeltaTime);

    }

    private void HandleNavMeshMovement(Vector2 clickPos)
    {
        var ray = camera.ScreenPointToRay(clickPos);
        if (Physics.RaycastNonAlloc(ray, hits, 1000f, walkableLayer.value) > 0)
        {
            MoveTo(hits[0]);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        UpdateGravity();
        HandleMovement();
        Velocity = isNavMeshMove ? agent.velocity : characterController.velocity;
    }

    private void HandleRotation()
    {
        RotateTowards();
    }

    private void MoveTo(RaycastHit hit)
    {
        agent.isStopped = false;
        agent.SetDestination(hit.point);
        characterController.enabled = false;
        isNavMeshMove = true;
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

    private void RotateTowards()
    {
        if (moveDirection.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);

        float interpolation = 1f - Mathf.Exp(-rotationSpeed * Runner.DeltaTime);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, interpolation);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        inputReader.SetInputEnabled(false);
    }
}