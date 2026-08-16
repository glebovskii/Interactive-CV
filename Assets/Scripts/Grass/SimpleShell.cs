using UnityEngine;
using UnityEngine.Rendering;

public class SimpleShell : MonoBehaviour
{
    [Header("Setup")]
    public Mesh shellMesh;

    [SerializeField] private Material shellMaterial;
    [SerializeField] private Terrain terrain;
    [SerializeField] private LayerMask grassLayer;

    private static readonly int ShellCountProp = Shader.PropertyToID("_ShellCount");
    private static readonly int ShellLengthProp = Shader.PropertyToID("_ShellLength");
    private static readonly int DensityProp = Shader.PropertyToID("_Density");
    private static readonly int ThicknessProp = Shader.PropertyToID("_Thickness");
    private static readonly int AttenProp = Shader.PropertyToID("_Atten");
    private static readonly int ShellDistanceAttenuationProp = Shader.PropertyToID("_ShellDistanceAttenuation");
    private static readonly int CurvatureProp = Shader.PropertyToID("_Curvature");
    private static readonly int DisplacementStrengthProp = Shader.PropertyToID("_DisplacementStrength");
    private static readonly int OcclusionBiasProp = Shader.PropertyToID("_OcclusionBias");
    private static readonly int NoiseMinProp = Shader.PropertyToID("_NoiseMin");
    private static readonly int NoiseMaxProp = Shader.PropertyToID("_NoiseMax");
    private static readonly int ShellColorProp = Shader.PropertyToID("_ShellColor");
    private static readonly int BaseColorProp = Shader.PropertyToID("_BaseColor");
    private static readonly int ScaleProp = Shader.PropertyToID("_Scale");
    private static readonly int WindDirectionProp = Shader.PropertyToID("_WindDirection");
    private static readonly int WindStrengthProp = Shader.PropertyToID("_WindStrength");
    private static readonly int WindFrequencyProp = Shader.PropertyToID("_WindFrequency");
    private static readonly int WindHeightAttenuationProp = Shader.PropertyToID("_WindHeightAttenuation");
    private static readonly int TurbulenceStrengthProp = Shader.PropertyToID("_TurbulenceStrength");
    private static readonly int MaskProp = Shader.PropertyToID("_Mask");
    private static readonly int marginProp = Shader.PropertyToID("_FrustumMargin");
    private static readonly int cameraDistanceThresholdProp = Shader.PropertyToID("_CameraDistanceThreshold");

    public bool updateStatics = true;

    public int scale = 1600;
    public float margin = 1;
    public float cameraDistanceThreshold = 1;

    [Range(1, 256)]
    public int shellCount = 16;

    [Range(0f, 1f)]
    public float shellLength = 0.15f;

    [Range(0.01f, 300f)]
    public float distanceAttenuation = 1f;

    [Range(1f, 10000f)]
    public float density = 100f;

    [Range(0f, 1f)]
    public float noiseMin;

    [Range(0f, 1f)]
    public float noiseMax = 1f;

    [Range(0f, 10f)]
    public float thickness = 1f;

    [Range(0f, 10f)]
    public float curvature = 1f;

    [Range(0f, 1f)]
    public float displacementStrength = 0.1f;

    public Color shellColor;
    public Color baseColor;

    [Range(0f, 5f)]
    public float occlusionAttenuation = 1f;

    [Range(0f, 1f)]
    public float occlusionBias;
    [SerializeField] private GrassInteractionController grassInteractionController;
    [Header("Wind Settings")]
    [SerializeField] private Vector3 windDirection = new Vector3(1f, 0f, 0f);
    [SerializeField] private float windStrength = 0.05f;
    [SerializeField] private float windFrequency = 0.75f;
    [SerializeField] private float windHeightAttenuation = 2f;
    [SerializeField] private float turbulenceStrength = 0.1f;
    [SerializeField] private Vector3 displacementDirection;

    private Texture2D maskTexture;
    private Material runtimeMaterial;
    private Matrix4x4[] shellMatrices;
    private RenderParams renderParams;

    public Material RuntimeMaterial => runtimeMaterial;

    private void OnEnable()
    {
        maskTexture = terrain.terrainData.GetAlphamapTexture(0);

        runtimeMaterial = new Material(shellMaterial);
        runtimeMaterial.enableInstancing = true;

        renderParams = new RenderParams(runtimeMaterial)
        {
            layer = LayerMask.NameToLayer("Grass"),
            shadowCastingMode = ShadowCastingMode.Off,
            receiveShadows = false,
            reflectionProbeUsage = ReflectionProbeUsage.Off,
            lightProbeUsage = LightProbeUsage.Off
        };

        RebuildInstances();
        ApplyProperties();

        grassInteractionController.SetTarget(runtimeMaterial, transform, shellMesh);
    }

    private void Update()
    {
        if (shellMatrices.Length != shellCount)
            RebuildInstances();

        UpdateInstanceMatrices();

        if (updateStatics)
            ApplyProperties();

        Graphics.RenderMeshInstanced( renderParams, shellMesh, 0, shellMatrices, shellCount);
    }

    private void RebuildInstances()
    {
        shellMatrices = new Matrix4x4[shellCount];
        UpdateInstanceMatrices();
    }

    private void UpdateInstanceMatrices()
    {
        Matrix4x4 matrix = transform.localToWorldMatrix;

        for (int i = 0; i < shellCount; i++)
            shellMatrices[i] = matrix;
    }

    private void ApplyProperties()
    {
        runtimeMaterial.SetFloat(ShellCountProp, shellCount);
        runtimeMaterial.SetFloat(ShellLengthProp, shellLength);
        runtimeMaterial.SetFloat(DensityProp, density);
        runtimeMaterial.SetFloat(ThicknessProp, EaseInSine(thickness));
        runtimeMaterial.SetFloat(AttenProp, occlusionAttenuation);
        runtimeMaterial.SetFloat(ShellDistanceAttenuationProp, distanceAttenuation);
        runtimeMaterial.SetFloat(CurvatureProp, curvature);
        runtimeMaterial.SetFloat(DisplacementStrengthProp, displacementStrength);
        runtimeMaterial.SetFloat(OcclusionBiasProp, occlusionBias);
        runtimeMaterial.SetFloat(NoiseMinProp, noiseMin);
        runtimeMaterial.SetFloat(NoiseMaxProp, noiseMax);
        runtimeMaterial.SetColor(ShellColorProp, shellColor);
        runtimeMaterial.SetColor(BaseColorProp, baseColor);
        runtimeMaterial.SetFloat(ScaleProp, scale);

        runtimeMaterial.SetFloat(marginProp, margin);
        runtimeMaterial.SetFloat(cameraDistanceThresholdProp, cameraDistanceThreshold);

        runtimeMaterial.SetVector(WindDirectionProp, windDirection);
        runtimeMaterial.SetFloat(WindStrengthProp, windStrength);
        runtimeMaterial.SetFloat(WindFrequencyProp, windFrequency);
        runtimeMaterial.SetFloat(WindHeightAttenuationProp, windHeightAttenuation);
        runtimeMaterial.SetFloat(TurbulenceStrengthProp, turbulenceStrength);

        runtimeMaterial.SetTexture(MaskProp, maskTexture);
    }

    private float EaseInSine(float x)
    {
        return 1f - Mathf.Cos((x * Mathf.PI) / 2f);
    }

    private void OnDisable()
    {
        if (runtimeMaterial != null)
            Destroy(runtimeMaterial);

        runtimeMaterial = null;
        shellMatrices = null;
    }
}