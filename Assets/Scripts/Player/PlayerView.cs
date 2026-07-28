using Fusion;
using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

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

    public bool IsLocalPlayer { get; private set; }
    public SkinnedMeshRenderer Renderer => renderer;

    private Material cachedMaterial;


    private int SpeedId = Animator.StringToHash("Speed");

    public CinemachineCamera Camera => playerCameraController.CinemachineCamera;

    private void Awake()
    {
        IsLocalPlayer = false;
        playerMovement.OnSpawn += Init;
    }

    public void Init(bool isLocalPlayer)
    {
        this.IsLocalPlayer = isLocalPlayer;

        if (isLocalPlayer)
        {
            playerTrigger.TriggerEnter += OnTriggerEnterPlayer;
            playerTrigger.TriggerExit += OnTriggerExitPlayer;
        }
        renderer.material.color = playerMovement.Color;
        cachedMaterial = renderer.material;

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
        playerUI.Init(playerCameraController.CinemachineCamera, IsLocalPlayer);

    }

    private void InitPlayerCameraController()
    {
        playerCameraController = Instantiate<PlayerCameraController>(playerCameraControllerPrefab);
        playerCameraController.CinemachineCamera.Priority = IsLocalPlayer ? 1 : -100;
        if (IsLocalPlayer)
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

        if (IsLocalPlayer)
        {
            playerTrigger.TriggerEnter -= OnTriggerEnterPlayer;
            playerTrigger.TriggerExit -= OnTriggerExitPlayer;
        }
    }

    public void AddTarget(Transform panel)
    {
        if (!IsLocalPlayer)
            return;

        playerCameraController.CinemachineTargetGroup.AddMember(panel, 10, 1);
    }

    public void RemoveTarget(Transform panel)
    {
        if (!IsLocalPlayer)
            return;

        playerCameraController.CinemachineTargetGroup.RemoveMember(panel);
    }

    public void ResetMaterial()
    {
        renderer.material = cachedMaterial;
    }
}
