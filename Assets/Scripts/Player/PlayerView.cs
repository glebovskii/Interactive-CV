using Fusion;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerView : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement playerMovement;

    [SerializeField] private PlayerCameraController playerCameraControllerPrefab;
    [SerializeField] private PlayerUI playerUIPrefab;
    [SerializeField] private Transform playerUISpawnPosition;

    [SerializeField] private Transform cameraTarget;

    [SerializeField] private SkinnedMeshRenderer renderer;

    [SerializeField] private PlayerTrigger playerTrigger;
    [SerializeField] private PlayerDissolveController dissolveController;

    [SerializeField] private float memberWeight = 1f;
    [SerializeField] private float memberRadius = 1f;

    public PlayerDissolveController DissolveController => dissolveController;

    [Networked] public Color Color { get; private set; }
    [Networked] public string Name { get; private set; }

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
    }

    public override void Spawned()
    {
        this.IsLocalPlayer = HasStateAuthority;

        cachedMaterial = renderer.material;

        if (IsLocalPlayer)
        {
            Name = PlayerInfoSave.GetName();
            Color = PlayerInfoSave.GetColor();
            playerTrigger.TriggerEnter += OnTriggerEnterPlayer;
            playerTrigger.TriggerExit += OnTriggerExitPlayer;
        }

        renderer.material.color = Color;
        InitPlayerCameraController();
        InitPlayerUI();
        playerUI.SetVisible(IsLocalPlayer);
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
        playerUI.Init(playerCameraController.CinemachineCamera, IsLocalPlayer, Name);

    }

    private void InitPlayerCameraController()
    {
        playerCameraController = Instantiate<PlayerCameraController>(playerCameraControllerPrefab);
        playerCameraController.CinemachineCamera.Priority = IsLocalPlayer ? 1 : -100;
        if (IsLocalPlayer)
            playerCameraController.CinemachineTargetGroup.AddMember(cameraTarget, memberWeight, memberRadius);
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

        playerCameraController.CinemachineTargetGroup.AddMember(panel, 3, 1);
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

    public void SetDissolveMaterial()
    {
        dissolveController.SetDissolveMaterial(Renderer);
    }
}
