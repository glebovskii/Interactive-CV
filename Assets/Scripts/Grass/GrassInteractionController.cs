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

    private Material grassMaterial;
    private Material drawMaterial;
    private RenderTexture interactionMap;

    private Transform surfaceTransform;
    private Bounds surfaceBounds;
    private Vector3 surfaceScale;

    private bool initialized;

    private static readonly int InteractionMapID = Shader.PropertyToID("_InteractionMap");
    private static readonly int CoordinateID = Shader.PropertyToID("_Coordinate");
    private static readonly int BrushRadiusID = Shader.PropertyToID("_Size");
    private static readonly int BrushStrengthID = Shader.PropertyToID("_BrushStrength");
    private static readonly int BrushFalloffID = Shader.PropertyToID("_BrushFalloff");

    private void Awake()
    {
        ServiceLocator.Register(this);
    }

    public void SetTarget(Material material, Transform targetTransform, Mesh mesh)
    {
        ReleaseResources();

        grassMaterial = material;
        surfaceTransform = targetTransform;
        surfaceBounds = mesh.bounds;
        surfaceScale = surfaceTransform.lossyScale;

        CreateInteractionMap();
        grassMaterial.SetTexture(InteractionMapID, interactionMap);

        initialized = true;
    }

    private void CreateInteractionMap()
    {
        drawMaterial = new Material(drawShader)
        {
            name = "Grass Interaction Draw Material"
        };

        interactionMap = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.R8, RenderTextureReadWrite.Linear)
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

    private void DrawAtWorldPosition(Vector3 worldPosition)
    {
        if (!initialized)
            return;

        Vector3 localPosition = surfaceTransform.InverseTransformPoint(worldPosition);

        float height = Mathf.Abs(localPosition.y - surfaceBounds.center.y) * Mathf.Abs(surfaceScale.y);

        if (height > maximumHeightAboveSurface)
            return;

        if (localPosition.x < surfaceBounds.min.x || localPosition.x > surfaceBounds.max.x ||
            localPosition.z < surfaceBounds.min.z || localPosition.z > surfaceBounds.max.z)
            return;

        Vector2 uv = new(
            1f - Mathf.InverseLerp(surfaceBounds.min.x, surfaceBounds.max.x, localPosition.x),
            1f - Mathf.InverseLerp(surfaceBounds.min.z, surfaceBounds.max.z, localPosition.z));

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

    public void Register(CharacterGrassInteractor characterGrassInteractor)
    {
        characters.Add(characterGrassInteractor);
        characterGrassInteractor.OnWalk += DrawAtWorldPosition;
    }

    public void Unregister(CharacterGrassInteractor characterGrassInteractor)
    {
        characters.Remove(characterGrassInteractor);
        characterGrassInteractor.OnWalk -= DrawAtWorldPosition;
    }

    private void OnGUI()
    {
        if (showDrawMap && interactionMap != null)
            GUI.DrawTexture(new Rect(0f, 0f, 256f, 256f), interactionMap, ScaleMode.ScaleToFit, false);
    }

    private void OnDestroy()
    {
        foreach (CharacterGrassInteractor character in characters)
        {
            if (character != null)
                character.OnWalk -= DrawAtWorldPosition;
        }

        ReleaseResources();
    }

    private void ReleaseResources()
    {
        initialized = false;

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

        grassMaterial = null;
        surfaceTransform = null;
    }
}