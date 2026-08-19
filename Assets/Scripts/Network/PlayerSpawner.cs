using Fusion;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private NetworkObject playerPrefab;

    private PlayerSpawnPoints spawnPoints;

    public event Action<NetworkObject> OnPlayerSpawned;

    private void Awake()
    {
        ServiceLocator.RegisterOrReplace(this);
    }

    void IPlayerJoined.PlayerJoined(PlayerRef player)
    {
        if (player != Runner.LocalPlayer)
            return;

        StartCoroutine(SpawnRoutine(player));
        //spawnPoints ??= FindAnyObjectByType<PlayerSpawnPoints>();

        //Vector3 position = Vector3.up;
        //Quaternion rotation = Quaternion.identity;

        //if (spawnPoints != null)
        //{
        //    spawnPoints.GetSpawnPose(player, out position, out rotation);
        //}

        //NetworkObject playerObject = Runner.Spawn(playerPrefab, position, rotation, player);

        //if (playerObject == null)
        //{
        //    Debug.LogError($"Failed to spawn player object for {player}.");
        //    return;
        //}

        //Runner.SetPlayerObject(player, playerObject);
        //playerObject.GetComponent<PlayerMovement>().Init(this);
        //OnPlayerSpawned?.Invoke(playerObject);
    }

    private IEnumerator SpawnRoutine(PlayerRef player)
    {
        while (spawnPoints == null)
        {
            spawnPoints ??= FindAnyObjectByType<PlayerSpawnPoints>();
            yield return null;
        }

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
            yield break;
        }

        Runner.SetPlayerObject(player, playerObject);
        playerObject.GetComponent<PlayerMovement>().Init(this);
        OnPlayerSpawned?.Invoke(playerObject);
    }

    public Vector3 FindClosestSpawnPoint(Vector3 position)
    {
        spawnPoints ??= FindAnyObjectByType<PlayerSpawnPoints>();
        if (spawnPoints != null)
        {
            return spawnPoints.FindClosestSpawnPoint(position);
        }
        return Vector3.zero;
    }
}