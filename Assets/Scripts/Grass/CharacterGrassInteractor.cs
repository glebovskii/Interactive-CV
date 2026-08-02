using System;
using UnityEngine;

public sealed class CharacterGrassInteractor : MonoBehaviour
{
    [SerializeField, Min(0f)] private float minimumMoveDistance = 0.01f;
    [SerializeField, Min(0.1f)] private float teleportThreshold = 2f;

    public event Action<Vector3> OnWalk;

    private Vector3 previousPosition;

    private void OnEnable()
    {
        previousPosition = transform.position;
    }

    private void LateUpdate()
    {
        Vector3 currentPosition = transform.position;
        Vector3 movement = currentPosition - previousPosition;

        previousPosition = currentPosition;

        float squaredDistance = movement.sqrMagnitude;

        if (squaredDistance < minimumMoveDistance * minimumMoveDistance ||
            squaredDistance > teleportThreshold * teleportThreshold)
        {
            return;
        }

        OnWalk?.Invoke(currentPosition);
    }
}