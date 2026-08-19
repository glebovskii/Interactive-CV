using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public sealed class GrassInteractionController : MonoBehaviour
{
    private const int MaxBrushPointsPerPass = 32;

    [Header("Interaction Map")]
    [SerializeField, Min(64)] private int textureSize = 2048;
    [SerializeField] private Shader drawShader;
    [SerializeField] private bool showDrawMap = true;

    [Header("Brush")]
    [SerializeField, Range(1f, 256f)] private float brushRadiusPixels = 24f;
    [SerializeField, Range(0f, 1f)] private float brushStrength = 1f;
    [SerializeField, Range(0.1f, 8f)] private float brushFalloff = 1f;

    [Header("UV Orientation")]
    [SerializeField] private bool flipU = true;
    [SerializeField] private bool flipV = true;

    private readonly List<Vector4> brushCoordinates = new(MaxBrushPointsPerPass);

    private Material grassMaterial;
    private Material drawMaterial;
    private RenderTexture interactionMap;
    private RenderTexture scratchMap;
    private bool initialized;

    private static readonly int InteractionMapID = Shader.PropertyToID("_InteractionMap");
    private static readonly int CoordinatesID = Shader.PropertyToID("_Coordinates");
    private static readonly int CoordinateCountID = Shader.PropertyToID("_CoordinateCount");
    private static readonly int BrushRadiusID = Shader.PropertyToID("_Size");
    private static readonly int BrushStrengthID = Shader.PropertyToID("_BrushStrength");
    private static readonly int BrushFalloffID = Shader.PropertyToID("_BrushFalloff");

    private void Awake()
    {
        ServiceLocator.Register(this);
    }

    // Signature is kept so existing setup code does not have to change.
    // UV calculation now happens once in PlayerSurfaceController's Burst job.
    public void SetTarget(Material material, Transform targetTransform, Mesh mesh)
    {
        ReleaseResources();

        grassMaterial = material;
        CreateInteractionMaps();
        grassMaterial.SetTexture(InteractionMapID, interactionMap);
        initialized = true;
    }

    public void DrawAtUVs(NativeArray<float2> uvs, NativeArray<byte> drawMask)
    {
        if (!initialized)
            return;

        bool drew = false;
        brushCoordinates.Clear();

        for (int i = 0; i < uvs.Length; i++)
        {
            if (drawMask[i] == 0)
                continue;

            float2 uv = uvs[i];

            if (flipU)
                uv.x = 1f - uv.x;

            if (flipV)
                uv.y = 1f - uv.y;

            brushCoordinates.Add(new Vector4(uv.x, uv.y, 0f, 0f));

            if (brushCoordinates.Count == MaxBrushPointsPerPass)
            {
                BlitBatch();
                drew = true;
            }
        }

        if (brushCoordinates.Count > 0)
        {
            BlitBatch();
            drew = true;
        }

        if (drew)
            grassMaterial.SetTexture(InteractionMapID, interactionMap);
    }

    private void CreateInteractionMaps()
    {
        drawMaterial = new Material(drawShader)
        {
            name = "Grass Interaction Draw Material"
        };

        interactionMap = CreateMap("Grass Interaction Map A");
        scratchMap = CreateMap("Grass Interaction Map B");

        ClearMap(interactionMap);
        ClearMap(scratchMap);

        drawMaterial.SetFloat(BrushRadiusID, brushRadiusPixels);
        drawMaterial.SetFloat(BrushStrengthID, brushStrength);
        drawMaterial.SetFloat(BrushFalloffID, brushFalloff);
    }

    private RenderTexture CreateMap(string mapName)
    {
        var map = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.R8, RenderTextureReadWrite.Linear)
        {
            name = mapName,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            useMipMap = false,
            autoGenerateMips = false
        };

        map.Create();
        return map;
    }

    private static void ClearMap(RenderTexture map)
    {
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = map;
        GL.Clear(false, true, Color.black);
        RenderTexture.active = previous;
    }

    private void BlitBatch()
    {
        drawMaterial.SetInt(CoordinateCountID, brushCoordinates.Count);
        drawMaterial.SetVectorArray(CoordinatesID, brushCoordinates);

        Graphics.Blit(interactionMap, scratchMap, drawMaterial);

        (interactionMap, scratchMap) = (scratchMap, interactionMap);
        brushCoordinates.Clear();
    }

    private void OnGUI()
    {
        if (showDrawMap && interactionMap != null)
            GUI.DrawTexture(new Rect(0f, 0f, 256f, 256f), interactionMap, ScaleMode.ScaleToFit, false);
    }

    private void OnDestroy()
    {
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

        ReleaseMap(ref interactionMap);
        ReleaseMap(ref scratchMap);

        grassMaterial = null;
        brushCoordinates.Clear();
    }

    private static void ReleaseMap(ref RenderTexture map)
    {
        if (map == null)
            return;

        map.Release();
        Destroy(map);
        map = null;
    }
}