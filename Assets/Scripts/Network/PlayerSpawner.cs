using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private NetworkObject playerPrefab;

    private PlayerSpawnPoints spawnPoints;

    void IPlayerJoined.PlayerJoined(PlayerRef player)
    {
        if (player != Runner.LocalPlayer)
            return;

        spawnPoints ??= FindAnyObjectByType<PlayerSpawnPoints>();

        Vector3 position = Vector3.up;
        Quaternion rotation = Quaternion.identity;

        if (spawnPoints != null)
        {
            spawnPoints.GetSpawnPose(player, out position, out rotation);
        }

        NetworkObject playerObject = Runner.Spawn(playerPrefab, position, rotation, player);

        if (playerObject == null)
        {
            Debug.LogError($"Failed to spawn player object for {player}.");
            return;
        }

        Runner.SetPlayerObject(player, playerObject);
    }
}