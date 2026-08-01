using Fusion;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GrassInteractionController : MonoBehaviour
{
    private int InteractionMapProp = Shader.PropertyToID("_InteractionMap");

    private const string DrawShaderPath = "DrawTracks";

    [Header("DEBUG")] public bool showDrawMap = true;
    public bool useTessellation = true;
    private int size = 2048;
    public bool updateTrackPropertiesInRuntime = true;
    public TMP_Text testText;
    private GameObject[] layers;
    [SerializeField] private List<CharacterGrassInteractor> characters;

    [Space(15)]
    [Header("Draw Track Properties")]
    [Range(0, 5000)]
    [SerializeField]
    private int brushSize = 1;

    [Range(0, 1)][SerializeField] private float brushStrength = 1;
    [Space(15)] private List<MaterialData> materialData = new();
    [SerializeField] private Shader drawShader;

    
    private RenderTexture splatMap;
    private Material drawMat;

    private PlayerSpawner playerSpawner;

    private void Awake()
    {
        characters = new List<CharacterGrassInteractor>();
        playerSpawner = ServiceLocator.Get<PlayerSpawner>();

        if(playerSpawner!= null)
        {
            playerSpawner.OnPlayerSpawned += OnPlayerSpawned;
        }

        //drawShader = Resources.Load<Shader>(DrawShaderPath);
    }

    private void OnPlayerSpawned(NetworkObject player)
    {
        if(player.TryGetComponent<CharacterGrassInteractor>(out var interactor))
        {
            interactor.Init(materialData);
            interactor.OnWalk += Blit;
            characters.Add(interactor);
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (!updateTrackPropertiesInRuntime)
            return;

        drawMat.SetFloat(Size, brushSize);
        drawMat.SetFloat(BrushStrength, brushStrength);

    }
#endif

    private void SetMaterialData()
    {
        drawMat = new Material(drawShader);
        drawMat.SetColor(ColorID, Color.red);
        splatMap = new RenderTexture(size, size, 0, RenderTextureFormat.R8);
        foreach (var plane in layers)
        {
            if (plane.TryGetComponent<MeshRenderer>(out var mesh))
            {
                foreach (var material in mesh.materials)
                {
                    materialData.Add(new MaterialData(material, TrackMap, splatMap));
                }
            }
        }
    }


    private void Blit(Vector4 coordinate)
    {
        drawMat.SetVector(Coordinate, coordinate);

        RenderTexture temp = RenderTexture.GetTemporary(splatMap.width, splatMap.height, 0, RenderTextureFormat.R8);
        Graphics.Blit(splatMap, temp);
        Graphics.Blit(temp, splatMap, drawMat);
        RenderTexture.ReleaseTemporary(temp);
        Shader.SetGlobalTexture(InteractionMapProp, splatMap);

    }

    private void OnGUI()
    {
        if (!showDrawMap)
            return;

        GUI.DrawTexture(new Rect(0, 0, 256, 256), splatMap, ScaleMode.ScaleToFit, false, 1);
    }

    public void SetLayers(GameObject[] shells)
    {
        layers = shells;
        SetMaterialData();
        foreach(var character in characters)
        {
            character.Init(materialData);
        };
    }

    private static readonly int ColorID = Shader.PropertyToID("_DrawColor");
    private static readonly int Coordinate = Shader.PropertyToID("_Coordinate");
    private static readonly int Size = Shader.PropertyToID("_Size");
    private static readonly int BrushStrength = Shader.PropertyToID("_BrushStrength");
    private static readonly int TrackMap = Shader.PropertyToID("_TrackMap");
}
