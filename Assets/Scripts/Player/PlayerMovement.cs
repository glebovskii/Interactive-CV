using Fusion;
using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditorInternal.ReorderableList;

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
    [SerializeField, Min(0f)]
    private float movingTurnSpeed = 540f;

    [SerializeField, Min(0f)]
    private float slowTurnSpeed = 900f;
    [SerializeField] private CharacterController characterController;
    private PlayerInputReader inputReader;

    private float verticalVelocity;

    private RaycastHit[] hits;
    private Camera camera;

    private NavMeshAgent agent;

    private Vector3 moveDirection;

    public event Action OnSpawn;
    public Vector3 Velocity { get; private set; }

    private bool isNavMeshMove = false;

    public override void Spawned()
    {
        bool isLocallyControlled = HasStateAuthority;

        inputReader = GetComponent<PlayerInputReader>();
        agent = GetComponentInChildren<NavMeshAgent>();

        inputReader.SetInputEnabled(isLocallyControlled);
        characterController.enabled = isLocallyControlled;

        agent.enabled = isLocallyControlled;
        camera = Camera.main;
        hits = new RaycastHit[1];

        //agent.speed = moveSpeed;
        agent.updateRotation = false;
        if (NavMesh.SamplePosition(transform.position, out var hit, float.MaxValue, NavMesh.AllAreas))
        {
            agent.transform.position = hit.position;
        }

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
            HandleInputRotation();
        }
    }

    private void HandleInputMovement()
    {
        Vector2 moveInput = inputReader.ReadMovement();
        moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        if (moveDirection.sqrMagnitude <= 0)
        {
            characterController.Move(Vector3.zero);
            return;
        }
        agent.isStopped = true;
        isNavMeshMove = false;

        Vector3 velocity = moveDirection.normalized;
        velocity.y = verticalVelocity;

        characterController.Move(velocity * moveSpeed * Runner.DeltaTime);

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
        Velocity = isNavMeshMove ? agent.velocity.normalized : characterController.velocity.normalized;
        HandleAgentRotation();
    }

    private void HandleInputRotation()
    {
        if (moveDirection.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);

        float interpolation = 1f - Mathf.Exp(-rotationSpeed * Runner.DeltaTime);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, interpolation);
    }

    private void HandleAgentRotation()
    {
        if (!agent.isOnNavMesh || !isNavMeshMove)
            return;

        Vector3 direction = agent.desiredVelocity;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction.normalized, Vector3.up);

        float speedRatio = agent.speed > 0f
            ? Mathf.Clamp01(agent.velocity.magnitude / agent.speed)
            : 0f;

        float currentTurnSpeed = Mathf.Lerp(slowTurnSpeed, movingTurnSpeed, speedRatio);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, currentTurnSpeed * Time.deltaTime);
    }

    private void MoveTo(RaycastHit hit)
    {
        agent.isStopped = false;
        agent.angularSpeed = movingTurnSpeed;
        isNavMeshMove = true;
        agent.SetDestination(hit.point);
    }

    private void UpdateGravity()
    {
        if (isNavMeshMove)
            return;

        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundedVerticalVelocity;
        }
        else
        {
            verticalVelocity += Physics.gravity.y * Runner.DeltaTime;
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        inputReader.SetInputEnabled(false);
    }
}