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
    private int shellDirectionProp = Shader.PropertyToID("_ShellDirection");
    private int scaleProp = Shader.PropertyToID("_Scale");

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

    [Range(0.0f, 5.0f)]
    public float occlusionAttenuation = 1.0f;

    [Range(0.0f, 1.0f)]
    public float occlusionBias = 0.0f;

    [Space(10)]
    [SerializeField] private GrassInteractionController grassInteractionController;

    [SerializeField] private LayerMask grassLayer;

    private Material shellMaterial;
    private GameObject[] shells;

    [SerializeField] private Vector3 displacementDirection = new Vector3(0, 0, 0);

    void OnEnable()
    {
        shellMaterial = new Material(shellShader);

        shells = new GameObject[shellCount];

        for (int i = 0; i < shellCount; ++i)
        {
            shells[i] = new GameObject("Shell " + i.ToString());
            shells[i].transform.rotation = Quaternion.Euler(90,0,0);
            shells[i].transform.localScale *= 10;
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
        }

        grassInteractionController.SetLayers(shells);
    }

    void Update()
    {
        //float velocity = 1.0f;

        //Vector3 direction = new Vector3(0, 0, 0);
        //Vector3 oppositeDirection = new Vector3(0, 0, 0);

        //// This determines the direction we are moving from wasd input. It's probably a better idea to use Unity's input system, since it handles
        //// all possible input devices at once, but I did it the old fashioned way for simplicity.
        //direction.x = Convert.ToInt32(Input.GetKey(KeyCode.D)) - Convert.ToInt32(Input.GetKey(KeyCode.A));
        //direction.y = Convert.ToInt32(Input.GetKey(KeyCode.W)) - Convert.ToInt32(Input.GetKey(KeyCode.S));
        //direction.z = Convert.ToInt32(Input.GetKey(KeyCode.Q)) - Convert.ToInt32(Input.GetKey(KeyCode.E));

        //// This moves the ball according the input direction
        //Vector3 currentPosition = this.transform.position;
        //direction.Normalize();
        //currentPosition += direction * velocity * Time.deltaTime;
        //this.transform.position = currentPosition;

        //// This changes the direction that the hair is going to point in, when we are not inputting any movements then we subtract the gravity vector
        //// The gravity vector just being (0, -1, 0)
        //displacementDirection -= direction * Time.deltaTime * 10.0f;
        //if (direction == Vector3.zero)
        //    displacementDirection.y -= 10.0f * Time.deltaTime;

        //if (displacementDirection.magnitude > 1) displacementDirection.Normalize();

        //// In order to avoid setting this variable on every single shell's material instance, we instead set this is as a global shader variable
        //// That every shader will have access to, which sounds bad, because it kind of is, but just be aware of your global variable names and it's not a big deal.
        //// Regardless, setting the variable one time instead of 256 times is just better.
        //Shader.SetGlobalVector(shellDirectionProp, displacementDirection);

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
                mat.SetFloat(scaleProp, scale);


                mat.SetVector(shellDirectionProp, displacementDirection);
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