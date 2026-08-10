using Fusion;
using UnityEngine;

public sealed class PlayerSpawnPoints : MonoBehaviour
{
    [SerializeField]
    private Transform[] spawnPoints;

    public void GetSpawnPose(PlayerRef player, out Vector3 position, out Quaternion rotation)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            int index = Mathf.Abs(player.RawEncoded);
            position = new Vector3(index % 4 * 2f, 1f, index / 4 * 2f);
            rotation = Quaternion.identity;
            return;
        }

        int spawnIndex = Mathf.Abs(player.RawEncoded) % spawnPoints.Length;

        Transform spawnPoint = spawnPoints[spawnIndex];

        position = spawnPoint.position;
        rotation = spawnPoint.rotation;
    }

    public Vector3 FindClosestSpawnPoint(Vector3 position)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            return Vector3.zero;
        }
        return spawnPoints[0].position;
    }
}