using Fusion;
using System.Collections.Generic;
using UnityEngine;

public sealed class GrassInteractionController : MonoBehaviour
{
    [Header("Interaction Map")]
    [SerializeField, Min(64)] private int textureSize = 2048;
    [SerializeField] private Shader drawShader;
    [SerializeField] private bool showDrawMap = true;

    [Header("Brush")]
    [SerializeField, Range(1f, 256f)] private float brushRadiusPixels = 24f;
    [SerializeField, Range(0f, 1f)] private float brushStrength = 1f;
    [SerializeField, Range(0.1f, 8f)] private float brushFalloff = 1f;

    [Header("Surface")]
    [SerializeField, Min(0f)] private float maximumHeightAboveSurface = 1f;

    private readonly List<CharacterGrassInteractor> characters = new();
    private readonly List<MeshRenderer> shellRenderers = new();

    private PlayerSpawner playerSpawner;
    private MaterialPropertyBlock propertyBlock;
    private Material drawMaterial;
    private RenderTexture interactionMap;

    private Transform surfaceTransform;
    private Bounds surfaceBounds;
    private Vector3 surfaceScale;

    private void Awake()
    {
        playerSpawner = ServiceLocator.Get<PlayerSpawner>();
        playerSpawner.OnPlayerSpawned += OnPlayerSpawned;
    }

    //TODO: REMOVE ONCE DESIDE ON VALUES
    private void Update()
    {
        drawMaterial.SetFloat(BrushRadiusID, brushRadiusPixels);
        drawMaterial.SetFloat(BrushStrengthID, brushStrength);
        drawMaterial.SetFloat(BrushFalloffID, brushFalloff);
    }

    private void OnPlayerSpawned(NetworkObject player)
    {
        CharacterGrassInteractor interactor = player.GetComponent<CharacterGrassInteractor>();

        characters.Add(interactor);
        interactor.OnWalk += DrawAtWorldPosition;
    }

    public void SetLayers(GameObject[] shells)
    {
        ReleaseResources();

        shellRenderers.Clear();

        foreach (GameObject shell in shells)
            shellRenderers.Add(shell.GetComponent<MeshRenderer>());

        surfaceTransform = shells[0].transform;
        surfaceBounds = shells[0].GetComponent<MeshFilter>().sharedMesh.bounds;
        surfaceScale = surfaceTransform.lossyScale;

        propertyBlock = new MaterialPropertyBlock();

        CreateInteractionMap();
        AssignInteractionMap();
    }

    private void CreateInteractionMap()
    {
        drawMaterial = new Material(drawShader)
        {
            name = "Grass Interaction Draw Material"
        };

        interactionMap = new RenderTexture(
            textureSize,
            textureSize,
            0,
            RenderTextureFormat.R8,
            RenderTextureReadWrite.Linear)
        {
            name = "Grass Interaction Map",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            useMipMap = false,
            autoGenerateMips = false
        };

        interactionMap.Create();

        RenderTexture previous = RenderTexture.active;

        RenderTexture.active = interactionMap;
        GL.Clear(false, true, Color.black);

        RenderTexture.active = previous;

        drawMaterial.SetFloat(BrushRadiusID, brushRadiusPixels);
        drawMaterial.SetFloat(BrushStrengthID, brushStrength);
        drawMaterial.SetFloat(BrushFalloffID, brushFalloff);
    }

    private void AssignInteractionMap()
    {
        foreach (MeshRenderer renderer in shellRenderers)
        {
            propertyBlock.Clear();
            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetTexture(InteractionMapID, interactionMap);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }

    private void DrawAtWorldPosition(Vector3 worldPosition)
    {
        Vector3 localPosition = surfaceTransform.InverseTransformPoint(worldPosition);

        float height = Mathf.Abs(localPosition.y - surfaceBounds.center.y) * Mathf.Abs(surfaceScale.y);

        if (height > maximumHeightAboveSurface)
            return;

        if (localPosition.x < surfaceBounds.min.x || localPosition.x > surfaceBounds.max.x ||
            localPosition.z < surfaceBounds.min.z || localPosition.z > surfaceBounds.max.z)
            return;

        Vector2 uv = new(
           1 - Mathf.InverseLerp(surfaceBounds.min.x, surfaceBounds.max.x, localPosition.x),
           1 - Mathf.InverseLerp(surfaceBounds.min.z, surfaceBounds.max.z, localPosition.z));

        Blit(uv);
    }

    private void Blit(Vector2 uv)
    {
        drawMaterial.SetVector(CoordinateID, uv);

        RenderTexture temporary = RenderTexture.GetTemporary(interactionMap.descriptor);

        Graphics.Blit(interactionMap, temporary);
        Graphics.Blit(temporary, interactionMap, drawMaterial);

        RenderTexture.ReleaseTemporary(temporary);
    }

    private void OnGUI()
    {
        if (showDrawMap)
            GUI.DrawTexture(new Rect(0f, 0f, 256f, 256f), interactionMap, ScaleMode.ScaleToFit, false);
    }

    private void OnDestroy()
    {
        playerSpawner.OnPlayerSpawned -= OnPlayerSpawned;

        foreach (CharacterGrassInteractor character in characters)
        {
            if (character != null)
                character.OnWalk -= DrawAtWorldPosition;
        }

        ReleaseResources();
    }

    private void ReleaseResources()
    {
        if (drawMaterial != null)
        {
            Destroy(drawMaterial);
            drawMaterial = null;
        }

        if (interactionMap != null)
        {
            interactionMap.Release();
            Destroy(interactionMap);
            interactionMap = null;
        }
    }

    private static readonly int InteractionMapID = Shader.PropertyToID("_InteractionMap");
    private static readonly int CoordinateID = Shader.PropertyToID("_Coordinate");
    private static readonly int BrushRadiusID = Shader.PropertyToID("_Size");
    private static readonly int BrushStrengthID = Shader.PropertyToID("_BrushStrength");
    private static readonly int BrushFalloffID = Shader.PropertyToID("_BrushFalloff");
}