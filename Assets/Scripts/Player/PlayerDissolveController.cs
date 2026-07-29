using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDissolveController : NetworkBehaviour
{
    private static readonly int EdgeWidthProp = Shader.PropertyToID("_EdgeWidth");
    private static readonly int DissolveProp = Shader.PropertyToID("_Dissolve");
    private static readonly int AxisProp = Shader.PropertyToID("_AxisProp");
    private static readonly int StartDirectionProp = Shader.PropertyToID("_DirectionProp");

    private int baseColorId = Shader.PropertyToID("_BaseColor");

    private static readonly string[] AxisKeywords =
        {
            "_AXIS_X",
            "_AXIS_Y",
            "_AXIS_Z"
        };
    private static readonly string[] StartDirectionKeywords =
    {
            "_DIRECTION_FORWARD",
            "_DIRECTION_BACK"
        };
    private static readonly List<string> AxisChoices = new()
        {
            "X",
            "Y",
            "Z"
        };
    private static readonly List<string> StartDirectionChoices = new()
        {
            "Forward",
            "Back"
        };

    [SerializeField] private Material dissolveMat;

    [Networked, OnChangedRender("OnDissolveChange")] public float Dissolve { get; set; }
    [Networked, OnChangedRender("OnEdgeWidthChange")] public float EdgeWidth { get; set; }
    [Networked, OnChangedRender("OnAxisChange")] public int Axis { get; set; }
    [Networked, OnChangedRender("OnDirectionChange")] public int Direction { get; set; }

    public override void Spawned()
    {
        dissolveMat.SetColor(baseColorId, GetComponent<SkinnedMeshRenderer>().material.color);
    }

    private void OnDissolveChange()
    {
        dissolveMat.SetFloat(DissolveProp, Dissolve);
    }

    private void OnEdgeWidthChange()
    {
        dissolveMat.SetFloat(EdgeWidthProp, EdgeWidth);
    }

    private void OnAxisChange()
    {
        Axis = Mathf.Clamp(Axis, 0, AxisKeywords.Length - 1);
        dissolveMat.SetFloat(AxisProp, Axis);
        SetExclusiveKeyword(AxisKeywords, Axis);
    }

    private void OnDirectionChange()
    {
        Direction = Mathf.Clamp(Direction, 0, StartDirectionKeywords.Length - 1);
        dissolveMat.SetFloat(StartDirectionProp, Direction);
        SetExclusiveKeyword(StartDirectionKeywords, Direction);
    }

    private void SetExclusiveKeyword(string[] keywords, int enabledIndex)
    {
        for (int index = 0; index < keywords.Length; index++)
        {
            if (index == enabledIndex)
                dissolveMat.EnableKeyword(keywords[index]);
            else
                dissolveMat.DisableKeyword(keywords[index]);
        }
    }

    public void SetDissolveMaterial(SkinnedMeshRenderer renderer)
    {
        renderer.material = dissolveMat;
    }
}
