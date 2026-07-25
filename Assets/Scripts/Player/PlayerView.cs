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

    [SerializeField] private PlayerTrigger playerTrigger;


    private PlayerCameraController playerCameraController;
    private PlayerUI playerUI;

    private bool isLocalPlayer;

    private int SpeedId = Animator.StringToHash("Speed");

    private void Awake()
    {
        playerMovement.OnSpawn += Init;
    }

    public void Init(bool isLocalPlayer)
    {
        this.isLocalPlayer = isLocalPlayer;

        if (isLocalPlayer)
        {
            playerTrigger.TriggerEnter += OnTriggerEnterPlayer;
            playerTrigger.TriggerExit += OnTriggerExitPlayer;
            renderer.sharedMaterial.color = playerMovement.Color;
        }

        InitPlayerCameraController();
        InitPlayerUI();
        playerUI.SetVisible(isLocalPlayer);
    }

    private void OnTriggerExitPlayer(PlayerView playerView)
    {
        playerView.playerUI.SetVisible(false);
    }

    private void OnTriggerEnterPlayer(PlayerView playerView)
    {
        playerView.playerUI.SetVisible(true);
    }

    private void InitPlayerUI()
    {
        playerUI = Instantiate<PlayerUI>(playerUIPrefab, new InstantiateParameters()
        {
            parent = playerUISpawnPosition,
        });
        playerUI.Init(playerCameraController.CinemachineCamera, isLocalPlayer);

    }

    private void InitPlayerCameraController()
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

        if (isLocalPlayer)
        {
            playerTrigger.TriggerEnter -= OnTriggerEnterPlayer;
            playerTrigger.TriggerExit -= OnTriggerExitPlayer;
        }
    }
}
