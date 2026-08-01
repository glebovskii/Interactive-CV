using System;
using UnityEngine;

public sealed class CharacterGrassInteractor : MonoBehaviour
{
    [Header("Ground Detection")]
    [SerializeField] private LayerMask grassLayer;
    [SerializeField, Min(0.01f)] private float maxHeight = 2f;
    [SerializeField, Min(0f)] private float rayOriginOffset = 0.25f;

    [Header("Movement")]
    [SerializeField, Min(0f)] private float minimumMoveDistance = 0.001f;
    [SerializeField, Min(0.1f)] private float teleportThreshold = 2f;

    public event Action<Vector4> OnWalk;

    private readonly RaycastHit[] hits = new RaycastHit[4];

    private Vector3 previousPosition;
    private bool hasPreviousPosition;

    private void OnEnable()
    {
        previousPosition = transform.position;
        hasPreviousPosition = true;
    }

    private void LateUpdate()
    {
        Draw();
    }

    private void Draw()
    {
        Vector3 currentPosition = transform.position;

        if (!hasPreviousPosition)
        {
            previousPosition = currentPosition;
            hasPreviousPosition = true;
            return;
        }

        Vector3 movement = currentPosition - previousPosition;
        previousPosition = currentPosition;

        movement = Vector3.ProjectOnPlane(movement, transform.up);

        float movementDistance = movement.magnitude;

        if (movementDistance < minimumMoveDistance)
            return;

        // Prevent teleports and large network corrections from drawing tracks.
        if (movementDistance > teleportThreshold)
            return;

        Vector3 movementDirection = movement / movementDistance;
        Vector3 origin = currentPosition + transform.up * rayOriginOffset;

        int hitCount = Physics.RaycastNonAlloc(
            origin,
            -transform.up,
            hits,
            maxHeight,
            grassLayer,
            QueryTriggerInteraction.Ignore);

        if (hitCount == 0)
            return;

        RaycastHit closestHit = default;
        float closestDistance = float.MaxValue;
        bool foundMeshCollider = false;

        // RaycastNonAlloc results are not guaranteed to be sorted.
        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit currentHit = hits[i];

            if (currentHit.collider is not MeshCollider)
                continue;

            if (currentHit.distance >= closestDistance)
                continue;

            closestHit = currentHit;
            closestDistance = currentHit.distance;
            foundMeshCollider = true;
        }

        if (!foundMeshCollider)
            return;

        Vector2 uv = closestHit.textureCoord;

        OnWalk?.Invoke(new Vector4(
            uv.x,
            uv.y,
            movementDirection.x,
            movementDirection.z));
    }
}