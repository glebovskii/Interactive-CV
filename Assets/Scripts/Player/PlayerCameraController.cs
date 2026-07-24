using Unity.Cinemachine;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private CinemachineTargetGroup cinemachineTargetGroup;

    public CinemachineCamera CinemachineCamera => cinemachineCamera;
    public CinemachineTargetGroup CinemachineTargetGroup => cinemachineTargetGroup;
}
