using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

namespace Dissolver.Editor
{
    public class SkinnedDissolveGUI : BaseShaderGUI
    {
        private MaterialProperty MainTex;
        private MaterialProperty NormalMap;
        private MaterialProperty Emission;
        private MaterialProperty EdgeWidth;
        private MaterialProperty Tint;
        private MaterialProperty Noise; //Texture
        private MaterialProperty Direction;
        private MaterialProperty Dissolve;
        private MaterialProperty Delay;
        private MaterialProperty TimeScale;
        private MaterialProperty NormalScale;
        private MaterialProperty AxisKeyword;
        private MaterialProperty DirectionKeyword;
        private MaterialProperty NoiseKeyword;
        private MaterialProperty NoiseScale;
        private MaterialProperty AngleOffset;
        private MaterialProperty CellDensity;
        private MaterialProperty Metallic;
        private MaterialProperty Smoothness;
        private MaterialProperty AxisProp;
        private MaterialProperty DirectionProp;
        private MaterialProperty NoiseProp;

        private Axis selectedAxis;
        private Direction selectedDirection;
        private NoiseType selectedNoise;

        private bool isSkinnedMesh;
        private Material targetMat;
        
        public static readonly GUIContent directionAxisType = EditorGUIUtility.TrTextContent("Dissolve Direction:",
            "Select axis to dissolve along");
        
        public static readonly GUIContent directionType = EditorGUIUtility.TrTextContent("Dissolve Start At:",
            "Select where dissolve starts");
        
        public static readonly GUIContent noiseType = EditorGUIUtility.TrTextContent("Noise:",
            "Select noise type");
        
        public static readonly string[] axisNames = Enum.GetNames(typeof(Axis));
        public static readonly string[] directionNames = Enum.GetNames(typeof(Direction));
        public static readonly string[] noiseNames = Enum.GetNames(typeof(NoiseType));
        
        private Dictionary<Axis, string> axisOptions = new()
        {
            { Axis.X, "_AXIS_X" },
            { Axis.Y, "_AXIS_Y" },
            { Axis.Z, "_AXIS_Z" }
        };
        
        private Dictionary<Direction, string> directionOptions = new()
        {
            { Editor.Direction.Forward, "_DIRECTION_FORWARD" },
            { Editor.Direction.Back, "_DIRECTION_BACK" }
        };
        
        private Dictionary<NoiseType, string> noiseOptions = new()
        {
            { NoiseType.Simple, "_NOISE_SIMPLE" },
            { NoiseType.Texture, "_NOISE_FROM_TEXTURE" },
            { NoiseType.Voronoi, "_NOISE_VORONOI" },
            { NoiseType.Gradient, "_NOISE_GRADIENT" }
        };
        
        private void FindProps(MaterialProperty[] props)
        {
            baseMapProp = ShaderGUI.FindProperty("_BaseMap", props);
            NormalMap = ShaderGUI.FindProperty("_Normal", props);
            NormalScale = ShaderGUI.FindProperty("_NormalScale", props);
            Emission = ShaderGUI.FindProperty("_Emission", props);
            EdgeWidth = ShaderGUI.FindProperty("_EdgeWidth", props);
            baseColorProp = ShaderGUI.FindProperty("_BaseColor", props);
            Noise = ShaderGUI.FindProperty("_Noise", props);
            Direction = ShaderGUI.FindProperty("_Direction", props);
            Dissolve = ShaderGUI.FindProperty("_Dissolve", props);
            Delay = ShaderGUI.FindProperty("_Delay", props);
            TimeScale = ShaderGUI.FindProperty("_TimeScale", props);
            NoiseScale = ShaderGUI.FindProperty("_NoiseScale", props);
            AngleOffset = ShaderGUI.FindProperty("_AngleOffset", props);
            CellDensity = ShaderGUI.FindProperty("_CellDensity", props);
            Metallic = ShaderGUI.FindProperty("_Metallic", props);
            Smoothness = ShaderGUI.FindProperty("_Smoothness", props);
            AxisProp = ShaderGUI.FindProperty("_AxisProp", props);
            DirectionProp = ShaderGUI.FindProperty("_DirectionProp", props);
            NoiseProp = ShaderGUI.FindProperty("_NoiseProp", props);
        }

        public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] props)
        {
            if (materialEditorIn == null)
                throw new ArgumentNullException("materialEditorIn");

            materialEditor = materialEditorIn;
            targetMat = materialEditor.target as Material;

            if (m_FirstTimeApply)
            {
                OnOpenGUI(targetMat, materialEditor);
                m_FirstTimeApply = false;
            }
            
            // SetDefaultAxis();
            SetDefaultDirection();
            SetDefaultNoise();
            
            FindProps(props);
            base.DrawSurfaceInputs(targetMat);
            materialEditor.TexturePropertySingleLine(Styles.normalMapText, NormalMap,
                NormalMap.textureValue != null ? NormalScale : null);
            materialEditor.FloatProperty(Metallic, "Metallic");
            materialEditor.FloatProperty(Smoothness, "Smoothness");
            materialEditor.ColorProperty(Emission, "Emission");
            materialEditor.RangeProperty(EdgeWidth, "Edge Width");
            materialEditor.Vector3ShaderProperty(Direction, new GUIContent("Dissolve Direction"));
            materialEditor.FloatProperty(Dissolve, "Dissolve");
            materialEditor.FloatProperty(Delay, "Dissolve Position Change Delay");
            materialEditor.FloatProperty(TimeScale, "Noise Time Scale");

            DoPopup(directionAxisType, AxisProp, axisNames);
            selectedAxis = (Axis)AxisProp.floatValue;
            axisOptions.Where(x => x.Key != selectedAxis).ToList().ForEach(x => SetKeyword(x.Value, false));
            SetKeyword(axisOptions[selectedAxis], true);
            
            DoPopup(directionType, DirectionProp, directionNames);
            selectedDirection = (Direction)DirectionProp.floatValue;
            directionOptions.Where(x => x.Key != selectedDirection).ToList().ForEach(x => SetKeyword(x.Value, false));
            SetKeyword(directionOptions[selectedDirection], true);

            DoPopup(noiseType, NoiseProp, noiseNames);
            selectedNoise = (NoiseType)NoiseProp.floatValue;
            noiseOptions.Where(x => x.Key != selectedNoise).ToList().ForEach(x => SetKeyword(x.Value, false));
            SetKeyword(noiseOptions[selectedNoise], true);


            if (selectedNoise == NoiseType.Gradient || selectedNoise == NoiseType.Simple)
            {
                materialEditor.FloatProperty(NoiseScale, "Noise Scale");
            }
            else if (selectedNoise == NoiseType.Texture)
            {
                materialEditor.TextureProperty(Noise, "Noise Texture");
            }
            else if (selectedNoise == NoiseType.Voronoi)
            {
                materialEditor.FloatProperty(AngleOffset, "Angle Offset");
                materialEditor.FloatProperty(CellDensity, "Cell Density");
            }
            

            materialEditor.EnableInstancingField();
        }
        
        private void SetDefaultAxis()
        {
            foreach (var axis in axisOptions)
            {
                if(targetMat.IsKeywordEnabled(axis.Value))
                {
                    selectedAxis = axis.Key;
                    break;
                }

            }
        }
        
        private void SetDefaultDirection()
        {
            foreach (var direction in directionOptions)
            {
                if(targetMat.IsKeywordEnabled(direction.Value))
                {
                    selectedDirection = direction.Key;
                    break;
                }

            }
        }
        
        private void SetDefaultNoise()
        {
            foreach (var noise in noiseOptions)
            {
                if(targetMat.IsKeywordEnabled(noise.Value))
                {
                    selectedNoise = noise.Key;
                    break;
                }

            }
        }
        
        private void SetKeyword( string keyword, bool state)
        {
            if (state)
            {
                targetMat.EnableKeyword(keyword);
            }
            else
            {
                targetMat.DisableKeyword(keyword);
            }
        }
        
        private bool GetBool(float prop)
        {
            return prop > 0;
        }
    }
}