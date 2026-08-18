using Fusion;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerUpdateCamera : NetworkBehaviour
{
    [SerializeField] private CinemachineBrain brain;

    public override void Render()
    {
        if (HasStateAuthority)
            brain.ManualUpdate();
    }
}
