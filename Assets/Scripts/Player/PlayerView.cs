using Fusion;
using System;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement playerMovement;

    [SerializeField] private PlayerCameraController playerCameraControllerPrefab;
    [SerializeField] private PlayerUI playerUIPrefab;
    [SerializeField] private Transform playerUISpawnPosition;

    [SerializeField] private Transform cameraTarget;

    [SerializeField] private SkinnedMeshRenderer renderer;


    private PlayerCameraController playerCameraController;
    private PlayerUI playerUI;

    private int SpeedId = Animator.StringToHash("Speed");

    private void Awake()
    {
        playerMovement.OnSpawn += Init;
    }

    public void Init(bool isLocalPlayer)
    {
        //if (!isLocalPlayer)
        //    return;

        InitPlayerCameraController(isLocalPlayer);
        InitPlayerUI(isLocalPlayer);
        renderer.sharedMaterial.color = playerMovement.Color;
        //playerUI.SetVisible(isLocalPlayer);
    }

    private void InitPlayerUI(bool isLocalPlayer)
    {
        playerUI = Instantiate<PlayerUI>(playerUIPrefab, new InstantiateParameters()
        {
            parent = playerUISpawnPosition,
        });
        playerUI.Init(playerCameraController.CinemachineCamera, isLocalPlayer);

    }

    private void InitPlayerCameraController(bool isLocalPlayer)
    {
        playerCameraController = Instantiate<PlayerCameraController>(playerCameraControllerPrefab);
        playerCameraController.CinemachineCamera.Priority = isLocalPlayer ? 1 : -100;
        if (isLocalPlayer)
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
