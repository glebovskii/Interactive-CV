using Fusion;
using System.Collections.Generic;
using UnityEngine;

public sealed class GrassInteractionController : MonoBehaviour
{
    [Header("Map")]
    [SerializeField, Min(64)] private int size = 2048;
    [SerializeField] private Shader drawShader;
    [SerializeField] private bool showDrawMap = true;

    [Header("Brush")]
    [SerializeField, Range(1f, 256f)] private float brushRadiusPixels = 24f;
    [SerializeField, Range(0f, 1f)] private float brushStrength = 1f;
    [SerializeField] private bool updateTrackPropertiesInRuntime = true;

    [SerializeField] private Transform tessellationTarget;

    private static readonly int PlayerPositionWSID = Shader.PropertyToID("_PlayerPositionWS");

    private static readonly int PlayerPositionValidID = Shader.PropertyToID("_PlayerPositionValid");

    private List<CharacterGrassInteractor> characters = new();
    private List<MeshRenderer> layerRenderers = new();

    private MaterialPropertyBlock propertyBlock;

    private RenderTexture interactionMap;
    private Material drawMaterial;
    private PlayerSpawner playerSpawner;

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
        EnsureInitialized();

        playerSpawner = ServiceLocator.Get<PlayerSpawner>();

        if (playerSpawner != null)
            playerSpawner.OnPlayerSpawned += OnPlayerSpawned;
    }

    private void Update()
    {
        if (updateTrackPropertiesInRuntime)
            ApplyBrushProperties();
    }

    private void OnPlayerSpawned(NetworkObject player)
    {
        if (!player.TryGetComponent(out CharacterGrassInteractor interactor))
            return;

        if (!characters.Contains(interactor))
        {
            characters.Add(interactor);
            interactor.OnWalk += Blit;
        }

        if (player.HasInputAuthority)
            tessellationTarget = player.transform;
    }

    private void LateUpdate()
    {
        if (tessellationTarget == null)
        {
            Shader.SetGlobalFloat(PlayerPositionValidID, 0f);
            return;
        }

        Vector3 position = tessellationTarget.position;

        Shader.SetGlobalVector(
            PlayerPositionWSID,
            new Vector4(position.x, position.y, position.z, 1f));

        Shader.SetGlobalFloat(PlayerPositionValidID, 1f);
    }

    public void SetLayers(GameObject[] shells)
    {
        ReleaseMapResources();

        layerRenderers.Clear();

        if (shells != null)
        {
            foreach (GameObject shell in shells)
            {
                if (shell != null && shell.TryGetComponent(out MeshRenderer renderer))
                    layerRenderers.Add(renderer);
            }
        }

        CreateInteractionMap();
        ApplyInteractionMap();
    }

    private void CreateInteractionMap()
    {
        if (drawShader == null)
        {
            Debug.LogError($"{nameof(GrassInteractionController)}: Draw shader is not assigned.", this);
            return;
        }

        drawMaterial = new Material(drawShader)
        {
            name = "Grass Interaction Draw Material"
        };

        interactionMap = new RenderTexture(
            size,
            size,
            0,
            RenderTextureFormat.ARGBHalf,
            RenderTextureReadWrite.Linear)
        {
            name = "Grass Interaction Map",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            useMipMap = false,
            autoGenerateMips = false
        };

        interactionMap.Create();
        ClearInteractionMap();
        ApplyBrushProperties();
    }

    private void ClearInteractionMap()
    {
        if (interactionMap == null)
            return;

        RenderTexture previous = RenderTexture.active;

        RenderTexture.active = interactionMap;
        GL.Clear(false, true, Color.clear);

        RenderTexture.active = previous;
    }

    private void ApplyBrushProperties()
    {
        if (drawMaterial == null)
            return;

        drawMaterial.SetFloat(SizeID, Mathf.Max(1f, brushRadiusPixels));
        drawMaterial.SetFloat(BrushStrengthID, brushStrength);
    }

    private void ApplyInteractionMap()
    {
        if (interactionMap == null)
            return;

        EnsureInitialized();

        Shader.SetGlobalTexture(InteractionMapID, interactionMap);

        foreach (MeshRenderer renderer in layerRenderers)
        {
            if (renderer == null)
                continue;

            propertyBlock.Clear();
            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetTexture(InteractionMapID, interactionMap);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }

    private void EnsureInitialized()
    {
        propertyBlock ??= new MaterialPropertyBlock();
    }


    private void Blit(Vector4 coordinate)
    {
        if (drawMaterial == null || interactionMap == null || !interactionMap.IsCreated())
            return;

        // xy = texture UV, zw = signed world-space XZ movement direction.
        drawMaterial.SetVector(CoordinateID, coordinate);

        RenderTextureDescriptor descriptor = interactionMap.descriptor;
        descriptor.depthBufferBits = 0;
        descriptor.msaaSamples = 1;

        RenderTexture temporary = RenderTexture.GetTemporary(descriptor);

        try
        {
            Graphics.Blit(interactionMap, temporary);
            Graphics.Blit(temporary, interactionMap, drawMaterial, 0);
        }
        finally
        {
            RenderTexture.ReleaseTemporary(temporary);
        }

        // The same RenderTexture remains assigned. SetGlobalTexture does not
        // need to be called again after every stamp.
    }

    private void OnGUI()
    {
        if (!showDrawMap || interactionMap == null)
            return;

        GUI.DrawTexture(
            new Rect(0f, 0f, 256f, 256f),
            interactionMap,
            ScaleMode.ScaleToFit,
            false);
    }

    private void OnDestroy()
    {
        if (playerSpawner != null)
            playerSpawner.OnPlayerSpawned -= OnPlayerSpawned;

        foreach (CharacterGrassInteractor character in characters)
        {
            if (character != null)
                character.OnWalk -= Blit;
        }

        ReleaseMapResources();
    }

    private void ReleaseMapResources()
    {
        if (drawMaterial != null)
        {
            Destroy(drawMaterial);
            drawMaterial = null;
        }

        if (interactionMap != null)
        {
            if (interactionMap.IsCreated())
                interactionMap.Release();

            Destroy(interactionMap);
            interactionMap = null;
        }
    }

    private static readonly int InteractionMapID = Shader.PropertyToID("_InteractionMap");
    private static readonly int CoordinateID = Shader.PropertyToID("_Coordinate");
    private static readonly int SizeID = Shader.PropertyToID("_Size");
    private static readonly int BrushStrengthID = Shader.PropertyToID("_BrushStrength");
}