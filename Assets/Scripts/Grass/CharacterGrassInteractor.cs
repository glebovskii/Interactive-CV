using Fusion;
using System;
using UnityEngine;

public sealed class CharacterGrassInteractor : NetworkBehaviour
{
    [SerializeField, Min(0f)] private float minimumMoveDistance = 0.01f;
    [SerializeField, Min(0.1f)] private float teleportThreshold = 2f;

    public event Action<Vector3> OnWalk;

    private GrassInteractionController grassController;
    private Vector3 previousPosition;
    private float minimumDistanceSquared;
    private float teleportThresholdSquared;

    public override void Spawned()
    {
        grassController = ServiceLocator.Get<GrassInteractionController>();
        grassController.Register(this);

        previousPosition = transform.position;
        minimumDistanceSquared = minimumMoveDistance * minimumMoveDistance;
        teleportThresholdSquared = teleportThreshold * teleportThreshold;
    }

    public override void Render()
    {
        Vector3 currentPosition = transform.position;
        float distanceSquared = (currentPosition - previousPosition).sqrMagnitude;
        previousPosition = currentPosition;

        if (distanceSquared < minimumDistanceSquared || distanceSquared > teleportThresholdSquared)
            return;

        OnWalk?.Invoke(currentPosition);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        grassController.Unregister(this);
    }
}