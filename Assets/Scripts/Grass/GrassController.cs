using System.Collections.Generic;
using UnityEngine;

public class GrassController : MonoBehaviour
{
    private int densityProp = Shader.PropertyToID("_Density");
    private int baseColorProp = Shader.PropertyToID("_BaseColor");
    private int highColorProp = Shader.PropertyToID("_HighColor");
    private int scaleProp = Shader.PropertyToID("_Scale");
    private int attenProp = Shader.PropertyToID("_Atten");
    private int distanceProp = Shader.PropertyToID("_Distance");
    private int thicknessProp = Shader.PropertyToID("_Thickness");

    [SerializeField] private Shader grassShader;
    [SerializeField] private Mesh mesh;

    [SerializeField] private Vector3 baseRotation = new Vector3(90, 0, 0);
    [SerializeField] private float scaleFactor = 1f;

    [Header("Shader Settings")]
    [SerializeField] private float scale;
    [SerializeField] private Color baseColor;
    [SerializeField] private Color highColor;
    [SerializeField] private float height;
    [SerializeField] private float distance;
    [Range(0f, 1f)]
    [SerializeField] private float baseDensity;
    [SerializeField] private float attenuation = 2f;
    [SerializeField] private float thickness = 0.2f;

    private float baseHeight = 0.001f;

    private int layers = 0;

    private List<Material> grassLayers;

    private void Awake()
    {
        grassLayers = new();
        layers = Mathf.RoundToInt(height / distance);
        float currentheight = baseHeight;

        for (int i = 0; i < layers; i++)
        {
            GameObject gameObject = new GameObject($"Layer_{i}");
            gameObject.transform.localScale = new Vector3(5, 5, 5)*scaleFactor;
            gameObject.transform.rotation = Quaternion.Euler(baseRotation);
            MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
            var meshFilter = renderer.gameObject.AddComponent<MeshFilter>();
            var grassLayer = Instantiate(mesh);
            meshFilter.mesh = grassLayer;
            renderer.material = new Material(grassShader);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.material.SetFloat(densityProp, EaseInSine(Mathf.Clamp01(baseDensity + (1 * i)/(float)layers)));
            
            currentheight += distance;


            grassLayers.Add(renderer.material);
        }
    }

    private void Update()
    {
        foreach(var layer in grassLayers)
        {
            layer.SetColor(baseColorProp, baseColor);
            layer.SetColor(highColorProp, highColor);
            layer.SetFloat(scaleProp, scale);
            layer.SetFloat(attenProp, attenuation);
            layer.SetFloat(distanceProp, distance);
            layer.SetFloat(thicknessProp, thickness);
        }
    }

    private float EaseOutQuart(float x)
    {
        return 1 - Mathf.Pow(1 - x, 4);
    }

    private float EaseInCirc(float x)
    {
        return 1 - Mathf.Sqrt(1 - Mathf.Pow(x, 2));
    }

    private float EaseInSine(float x)
    {
        return 1 - Mathf.Cos((x * Mathf.PI) / 2);
    }
}
