using Fusion;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

public sealed class PlayerSurfaceController : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool showDebug;
    [SerializeField] private float debugTextureSize = 256f;
    [SerializeField] private float markerSize = 8f;
    [SerializeField] private float directionLength = 25f;
    [SerializeField] private float directionWidth = 3f;

    private NativeList<float2> sampledUVs;
    private readonly List<Vector2> previousDebugUVs = new();
    private readonly List<Vector2> debugDirections = new();

    [Tooltip("Black means road/ground. White means grass.")]
    [SerializeField] private Texture2D groundMask;

    [Tooltip("The Quad whose UV space corresponds to the mask.")]
    [SerializeField] private Terrain maskSurface;

    [Header("Terrain Layers")]
    [SerializeField, Min(0)] private int grassLayerIndex;
    [SerializeField, Min(0)] private int groundLayerIndex = 1;
    [SerializeField, Min(0)] private int metalLayerIndex = 2;

    private NativeArray<byte> surfaceMap;

    [SerializeField] private bool flipU;
    [SerializeField] private bool flipV;

    [Header("Checking")]
    [Tooltip("0 checks every frame. 0.05-0.1 is normally enough for particles.")]
    [SerializeField, Min(0f)] private float checkInterval = 0.05f;

    private readonly List<PlayerFXController> players = new();
    private readonly List<byte> previousStates = new();

    private PlayerSpawner playerSpawner;
    private TransformAccessArray playerTransforms;
    private NativeList<byte> sampledStates;

    private JobHandle jobHandle;
    private bool jobScheduled;
    private float nextCheckTime;

    private int maskWidth;
    private int maskHeight;

    private float3 surfacePosition;
    private quaternion inverseSurfaceRotation;
    private float3 inverseSurfaceScale;
    private float2 surfaceBoundsMin;
    private float2 surfaceBoundsSize;

    private Texture2D controlMap;

    private void Awake()
    {
        CacheMask();
        CacheSurface();

        playerTransforms = new TransformAccessArray(16);
        sampledStates = new NativeList<byte>(16, Unity.Collections.Allocator.Persistent);
        sampledUVs = new NativeList<float2>(16, Unity.Collections.Allocator.Persistent);

        playerSpawner = ServiceLocator.Get<PlayerSpawner>();
        playerSpawner.OnPlayerSpawned += OnPlayerSpawned;

        
    }

    private void CacheMask()
    {
        TerrainData data = maskSurface.terrainData;

        maskWidth = data.alphamapWidth;
        maskHeight = data.alphamapHeight;
        controlMap = data.GetAlphamapTexture(0);

        int layerCount = data.alphamapLayers;

        if (grassLayerIndex >= layerCount || groundLayerIndex >= layerCount || metalLayerIndex >= layerCount)
        {
            Debug.LogError($"Invalid Terrain Layer index. Terrain contains {layerCount} layers.", this);
            enabled = false;
            return;
        }

        float[,,] alphamaps = data.GetAlphamaps(0, 0, maskWidth, maskHeight);
        surfaceMap = new NativeArray<byte>(maskWidth * maskHeight, Unity.Collections.Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        for (int y = 0; y < maskHeight; y++)
        {
            for (int x = 0; x < maskWidth; x++)
            {
                float grass = alphamaps[y, x, grassLayerIndex];
                float ground = alphamaps[y, x, groundLayerIndex];
                float metal = alphamaps[y, x, metalLayerIndex];

                SurfaceType surface;

                if (metal >= grass && metal >= ground)
                    surface = SurfaceType.Metal;
                else if (grass >= ground)
                    surface = SurfaceType.Grass;
                else
                    surface = SurfaceType.Ground;

                surfaceMap[y * maskWidth + x] = (byte)surface;
            }
        }
    }

    private void CacheSurface()
    {
        //Bounds bounds = maskSurface.GetComponent<MeshFilter>().sharedMesh.bounds;
        Bounds bounds = maskSurface.terrainData.bounds;//.sharedMesh.bounds;

        surfaceBoundsMin = new float2(bounds.min.x, bounds.min.z);
        surfaceBoundsSize = new float2(bounds.size.x, bounds.size.z);

        Vector3 position = maskSurface.transform.position;
        Vector3 scale = maskSurface.transform.lossyScale;
        Quaternion rotation = Quaternion.Inverse(maskSurface.transform.rotation);

        surfacePosition = new float3(position.x, position.y, position.z);
        inverseSurfaceRotation = new quaternion(rotation.x, rotation.y, rotation.z, rotation.w);
        inverseSurfaceScale = new float3(1f / scale.x, 1f / scale.y, 1f / scale.z);
    }

    private void OnPlayerSpawned(NetworkObject player)
    {
        CompleteAndApplyJob();

        players.Add(player.GetComponent<PlayerFXController>());
        previousStates.Add(byte.MaxValue);
        previousDebugUVs.Add(Vector2.zero);
        debugDirections.Add(Vector2.zero);

        playerTransforms.Add(player.transform);
        sampledStates.Add(0);
        sampledUVs.Add(float2.zero);
    }

    private void OnGUI()
    {
        if (!showDebug)
            return;

        Rect textureRect = new Rect(10f, 10f, debugTextureSize, debugTextureSize);
        GUI.DrawTexture(textureRect, groundMask, ScaleMode.StretchToFill, false);

        for (int i = 0; i < previousDebugUVs.Count; i++)
        {
            Vector2 uv = previousDebugUVs[i];
            Vector2 position = new Vector2(
                textureRect.x + uv.x * textureRect.width,
                textureRect.y + (1f - uv.y) * textureRect.height);

            Vector2 direction = new Vector2(debugDirections[i].x, -debugDirections[i].y);
            Color markerColor = previousStates[i] == 1 ? Color.yellow : Color.green;

            Color oldColor = GUI.color;
            GUI.color = markerColor;
            GUI.DrawTexture(new Rect(position.x - markerSize * 0.5f, position.y - markerSize * 0.5f, markerSize, markerSize), Texture2D.whiteTexture);
            GUI.color = oldColor;

            DrawGUILine(position, position + direction * directionLength, Color.red, directionWidth);
        }
    }

    private static void DrawGUILine(Vector2 start, Vector2 end, Color color, float width)
    {
        Vector2 delta = end - start;
        Matrix4x4 oldMatrix = GUI.matrix;
        Color oldColor = GUI.color;

        GUI.color = color;
        GUIUtility.RotateAroundPivot(Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg, start);
        GUI.DrawTexture(new Rect(start.x, start.y - width * 0.5f, delta.magnitude, width), Texture2D.whiteTexture);

        GUI.matrix = oldMatrix;
        GUI.color = oldColor;
    }

    private void Update()
    {
        if (jobScheduled || players.Count == 0 || Time.time < nextCheckTime)
            return;

        nextCheckTime = Time.time + checkInterval;

        var job = new SampleSurfaceMaskJob
        {
            SurfaceMap = surfaceMap,
            Results = sampledStates.AsArray(),
            UVResults = sampledUVs.AsArray(),

            MaskWidth = maskWidth,
            MaskHeight = maskHeight,

            SurfacePosition = surfacePosition,
            InverseSurfaceRotation = inverseSurfaceRotation,
            InverseSurfaceScale = inverseSurfaceScale,
            SurfaceBoundsMin = surfaceBoundsMin,
            SurfaceBoundsSize = surfaceBoundsSize,

            FlipU = flipU ? (byte)1 : (byte)0,
            FlipV = flipV ? (byte)1 : (byte)0
        };

        jobHandle = IJobParallelForTransformExtensions.ScheduleReadOnlyByRef(
            ref job,
            playerTransforms,
            32,
            default);

        jobScheduled = true;
    }

    private void LateUpdate()
    {
        CompleteAndApplyJob();
    }

    private void CompleteAndApplyJob()
    {
        if (!jobScheduled)
            return;

        jobHandle.Complete();
        jobScheduled = false;

        NativeArray<byte> states = sampledStates.AsArray();
        NativeArray<float2> uvs = sampledUVs.AsArray();

        for (int i = 0; i < states.Length; i++)
        {
            Vector2 currentUV = uvs[i];
            Vector2 direction = currentUV - previousDebugUVs[i];

            if (direction.sqrMagnitude > 0.000001f)
                debugDirections[i] = direction.normalized;

            previousDebugUVs[i] = currentUV;

            if (states[i] == previousStates[i])
                continue;

            previousStates[i] = states[i];
            players[i].SetSurfaceType((SurfaceType)states[i]);
        }
    }

    private void OnDestroy()
    {
        if (jobScheduled)
            jobHandle.Complete();

        playerSpawner.OnPlayerSpawned -= OnPlayerSpawned;

        if (playerTransforms.isCreated)
            playerTransforms.Dispose();

        if (sampledStates.IsCreated)
            sampledStates.Dispose();

        if (surfaceMap.IsCreated)
            surfaceMap.Dispose();

        if (sampledUVs.IsCreated)
            sampledUVs.Dispose();
    }

    [BurstCompile]
    private struct SampleSurfaceMaskJob : IJobParallelForTransform
    {
        [Unity.Collections.ReadOnly] public NativeArray<byte> SurfaceMap;
        [WriteOnly] public NativeArray<byte> Results;
        [WriteOnly] public NativeArray<float2> UVResults;

        public int MaskWidth;
        public int MaskHeight;

        public float3 SurfacePosition;
        public quaternion InverseSurfaceRotation;
        public float3 InverseSurfaceScale;
        public float2 SurfaceBoundsMin;
        public float2 SurfaceBoundsSize;

        public byte FlipU;
        public byte FlipV;

        public void Execute(int index, TransformAccess transform)
        {
            Vector3 position = transform.position;
            float3 worldPosition = new float3(position.x, position.y, position.z);
            float3 localPosition = math.mul(InverseSurfaceRotation, worldPosition - SurfacePosition) * InverseSurfaceScale;

            float2 uv = (localPosition.xz - SurfaceBoundsMin) / SurfaceBoundsSize;

            if (FlipU != 0)
                uv.x = 1f - uv.x;

            if (FlipV != 0)
                uv.y = 1f - uv.y;

            uv = math.saturate(uv);
            UVResults[index] = uv;

            int x = math.min((int)(uv.x * MaskWidth), MaskWidth - 1);
            int y = math.min((int)(uv.y * MaskHeight), MaskHeight - 1);

            Results[index] = SurfaceMap[y * MaskWidth + x];
        }
    }
}