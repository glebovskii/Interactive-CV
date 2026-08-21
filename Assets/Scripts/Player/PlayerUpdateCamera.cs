using Fusion;
using Unity.Cinemachine;
using UnityEngine;

public sealed class PlayerUpdateCamera : NetworkBehaviour
{
    private CinemachineBrain brain;

    public override void Spawned()
    {
        if (!HasStateAuthority)
        {
            enabled = false;
            return;
        }

        brain = Camera.main.GetComponent<CinemachineBrain>();
    }

    public override void Render()
    {
        brain.ManualUpdate();
    }
}