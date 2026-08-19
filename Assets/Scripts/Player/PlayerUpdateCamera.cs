using Fusion;
using System;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerUpdateCamera : NetworkBehaviour
{
    CinemachineBrain brain;
    private NetworkObject player;

    public override void Spawned()
    {
        if (!HasStateAuthority)
        {
            enabled = false;
            return;
        }

        brain = Camera.main.GetComponent<CinemachineBrain>();

        var spawner = ServiceLocator.Get<PlayerSpawner>();
        spawner.OnPlayerSpawned += OnPlayerJoined;
    }

    private void OnPlayerJoined(NetworkObject player)
    {
        if (player.HasStateAuthority)
            this.player = player;
    }

    public override void Render()
    {
        if (player.HasStateAuthority)
            brain.ManualUpdate();
    }
}
