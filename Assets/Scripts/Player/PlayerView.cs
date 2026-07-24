using Unity.Cinemachine;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement playerMovement;

    [SerializeField] private PlayerCameraController playerCameraControllerPrefab;
    [SerializeField] private Transform cameraTarget;

    [SerializeField] private SkinnedMeshRenderer renderer;

    private PlayerCameraController playerCameraController;

    private int SpeedId = Animator.StringToHash("Speed");

    private void Awake()
    {
        playerMovement.OnSpawn += Init;
        renderer.sharedMaterial.color = PlayerInfoSave.GetColor();
    }

    public void Init()
    {
        InitPlayerCameraController();
    }

    private void InitPlayerCameraController()
    {
        playerCameraController = Instantiate<PlayerCameraController>(playerCameraControllerPrefab);
        playerCameraController.CinemachineTargetGroup.AddMember(cameraTarget, 1, 1);
    }

    private void Update()
    {
        HandleAnimator();
    }

    private void HandleAnimator()
    {
        animator.SetFloat(SpeedId, playerMovement.Velocity.sqrMagnitude);
    }

    private void OnDestroy()
    {
        playerMovement.OnSpawn -= Init;
    }
}
