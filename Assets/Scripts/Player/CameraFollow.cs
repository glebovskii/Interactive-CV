using Fusion;
using UnityEngine;

public class CameraFollow : NetworkBehaviour
{
    private Transform target;
    [SerializeField] private float distance = 10f;
    [SerializeField] private Vector3 rotation = new Vector3(42f, 50f, 0f);

    private PlayerSpawner spawner;

    private void Awake()
    {
        spawner = ServiceLocator.Get<PlayerSpawner>();
        spawner.OnPlayerSpawned += Set;
    }

    private void Set(NetworkObject player)
    {
        if (!player.HasStateAuthority)
            return;

        target = player.transform;
    }

    public override void Render()
    {
        if (target == null) return;
        Quaternion cameraRotation = Quaternion.Euler(rotation);

        transform.rotation = cameraRotation;
        transform.position = target.position - cameraRotation * Vector3.forward * distance;
    }

    //public override void FixedUpdateNetwork()
    //{
    //    if (target == null) return;
    //    Quaternion cameraRotation = Quaternion.Euler(rotation);

    //    transform.rotation = cameraRotation;
    //    transform.position = target.position - cameraRotation * Vector3.forward * distance;
    //}
}