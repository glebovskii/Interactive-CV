using System;
using UnityEngine;

public class SimpleShell : MonoBehaviour
{
    public Mesh shellMesh;
    public Shader shellShader;

    private int shellCountProp = Shader.PropertyToID("_ShellCount");
    private int shellIndexProp = Shader.PropertyToID("_ShellIndex");
    private int shellLengthProp = Shader.PropertyToID("_ShellLength");
    private int densityProp = Shader.PropertyToID("_Density");
    private int thicknessProp = Shader.PropertyToID("_Thickness");
    private int attenProp = Shader.PropertyToID("_Atten");
    private int shellDistanceAttenuationProp = Shader.PropertyToID("_ShellDistanceAttenuation");
    private int curvatureProp = Shader.PropertyToID("_Curvature");
    private int displacementStrengthProp = Shader.PropertyToID("_DisplacementStrength");
    private int occlusionBiasProp = Shader.PropertyToID("_OcclusionBias");
    private int noiseMinProp = Shader.PropertyToID("_NoiseMin");
    private int noiseMaxProp = Shader.PropertyToID("_NoiseMax");
    private int shellColorProp = Shader.PropertyToID("_ShellColor");
    private int baseColorProp = Shader.PropertyToID("_BaseColor");
    private int scaleProp = Shader.PropertyToID("_Scale");
    private int windDirectionProp = Shader.PropertyToID("_WindDirection");
    private int windStrengthProp = Shader.PropertyToID("_WindStrength");
    private int windSpeedProp = Shader.PropertyToID("_WindSpeed");
    private int windFrequencyProp = Shader.PropertyToID("_WindFrequency");
    private int windHeightAttenuationProp = Shader.PropertyToID("_WindHeightAttenuation");
    private int gustStrengthProp = Shader.PropertyToID("_GustStrength");
    private int gustFrequencyProp = Shader.PropertyToID("_GustFrequency");
    private int turbulenceStrengthProp = Shader.PropertyToID("_TurbulenceStrength");
    private int maskProp = Shader.PropertyToID("_Mask");

    public bool updateStatics = true;

    // These variables and what they do are explained on the shader code side of things
    // You can see below (line 70) which shader uniforms match up with these variables
    public int scale = 1600;

    [Range(1, 256)]
    public int shellCount = 16;

    [Range(0.0f, 1.0f)]
    public float shellLength = 0.15f;

    [Range(0.01f, 300.0f)]
    public float distanceAttenuation = 1.0f;

    [Range(1.0f, 10000.0f)]
    public float density = 100.0f;

    [Range(0.0f, 1.0f)]
    public float noiseMin = 0.0f;

    [Range(0.0f, 1.0f)]
    public float noiseMax = 1.0f;

    [Range(0.0f, 10.0f)]
    public float thickness = 1.0f;

    [Range(0f, 10.0f)]
    public float curvature = 1.0f;

    [Range(0.0f, 1f)]
    public float displacementStrength = 0.1f;

    public Color shellColor;
    public Color baseColor;

    [Range(0.0f, 5.0f)]
    public float occlusionAttenuation = 1.0f;

    [Range(0.0f, 1.0f)]
    public float occlusionBias = 0.0f;

    [Header("Wind Settings")]
    [SerializeField] private Vector3 windDirection = new Vector3(1, 0, 0);
    [SerializeField] private float windStrength = 0.05f;
    [SerializeField] private float windSpeed = 1.5f;
    [SerializeField] private float windFrequency = 0.75f;
    [SerializeField] private float windHeightAttenuation = 2f;
    [SerializeField] private float gustStrength = 0.25f;
    [SerializeField] private float gustFrequency = 0.4f;
    [SerializeField] private float turbulenceStrength = 0.1f;

    [Space(10)]
    [SerializeField] private GrassInteractionController grassInteractionController;

    [Space(10)]
    [SerializeField] private Texture2D maskTexture;
    [SerializeField] private Terrain terrain;

    [SerializeField] private LayerMask grassLayer;

    private Material shellMaterial;
    private GameObject[] shells;

    [SerializeField] private Vector3 displacementDirection = new Vector3(0, 0, 0);

    void OnEnable()
    {
        maskTexture = terrain.terrainData.GetAlphamapTexture(0);

        shellMaterial = new Material(shellShader);

        shells = new GameObject[shellCount];

        for (int i = 0; i < shellCount; ++i)
        {
            shells[i] = new GameObject("Shell " + i.ToString());
            //shells[i].transform.rotation = Quaternion.Euler(90, 0, 0);
            //shells[i].transform.localScale *= 10;
            shells[i].layer = LayerMask.NameToLayer("Grass");
            shells[i].AddComponent<MeshFilter>();
            shells[i].AddComponent<MeshRenderer>();
            shells[i].GetComponent<MeshFilter>().mesh = shellMesh;
            shells[i].GetComponent<MeshRenderer>().material = shellMaterial;
            var mat = shells[i].GetComponent<MeshRenderer>().sharedMaterial;
            shells[i].transform.SetParent(this.transform, false);
            shells[i].GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // In order to tell the GPU what its uniform variable values should be, we use these "Set" functions which will set the
            // values over on the GPU. 
            mat.SetFloat(shellCountProp, (float)shellCount);
            mat.SetFloat(shellIndexProp, (float)i);
            mat.SetFloat(shellLengthProp, shellLength);
            mat.SetFloat(densityProp, density);
            mat.SetFloat(thicknessProp, thickness);
            mat.SetFloat(attenProp, occlusionAttenuation);
            mat.SetFloat(shellDistanceAttenuationProp, distanceAttenuation);
            mat.SetFloat(curvatureProp, curvature);
            mat.SetFloat(displacementStrengthProp, displacementStrength);
            mat.SetFloat(occlusionBiasProp, occlusionBias);
            mat.SetFloat(noiseMinProp, noiseMin);
            mat.SetFloat(noiseMaxProp, noiseMax);
            mat.SetVector(shellColorProp, shellColor);
            mat.SetVector(baseColorProp, baseColor);

            mat.SetVector(windDirectionProp, windDirection);
            mat.SetFloat(windStrengthProp, windStrength);
            mat.SetFloat(windSpeedProp, windSpeed);
            mat.SetFloat(windFrequencyProp, windFrequency);
            mat.SetFloat(windHeightAttenuationProp, windHeightAttenuation);
            mat.SetFloat(gustStrengthProp, gustStrength);
            mat.SetFloat(gustFrequencyProp, gustFrequency);
            mat.SetFloat(turbulenceStrengthProp, turbulenceStrength);

            mat.SetTexture(maskProp, maskTexture);
        }

        grassInteractionController.SetLayers(shells);
    }

    void Update()
    {
        if (updateStatics)
        {
            for (int i = 0; i < shellCount; ++i)
            {
                var mat = shells[i].GetComponent<MeshRenderer>().material;
                mat.SetFloat(shellCountProp, (float)shellCount);
                mat.SetFloat(shellIndexProp, (float)i);
                mat.SetFloat(shellLengthProp, shellLength);
                mat.SetFloat(densityProp, density);
                //mat.SetFloat(thicknessProp, EaseInSine(thickness));
                mat.SetFloat(thicknessProp, (thickness));
                mat.SetFloat(attenProp, occlusionAttenuation);
                mat.SetFloat(shellDistanceAttenuationProp, distanceAttenuation);
                mat.SetFloat(curvatureProp, curvature);
                mat.SetFloat(displacementStrengthProp, displacementStrength);
                mat.SetFloat(occlusionBiasProp, occlusionBias);
                mat.SetFloat(noiseMinProp, noiseMin);
                mat.SetFloat(noiseMaxProp, noiseMax);
                mat.SetVector(shellColorProp, shellColor);
                mat.SetVector(baseColorProp, baseColor);
                mat.SetFloat(scaleProp, scale);

                mat.SetVector(windDirectionProp, windDirection);
                mat.SetFloat(windStrengthProp, windStrength);
                mat.SetFloat(windSpeedProp, windSpeed);
                mat.SetFloat(windFrequencyProp, windFrequency);
                mat.SetFloat(windHeightAttenuationProp, windHeightAttenuation);
                mat.SetFloat(gustStrengthProp, gustStrength);
                mat.SetFloat(gustFrequencyProp, gustFrequency);
                mat.SetFloat(turbulenceStrengthProp, turbulenceStrength);
            }
        }
    }

    private float EaseInSine(float x)
    {
        return 1 - Mathf.Cos((x * Mathf.PI) / 2);
    }

    void OnDisable()
    {
        for (int i = 0; i < shells.Length; ++i)
        {
            Destroy(shells[i]);
        }

        shells = null;
    }
}