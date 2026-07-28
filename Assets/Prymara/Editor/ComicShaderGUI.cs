using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

namespace Prymara
{
    public class ComicShaderGUI : ShaderGUI
    {
        #region PROPERTY NAMES

        private static readonly string EdgeStrength = "_Edge_Strength";
        private static readonly string NearEdgeColor = "_NearEdgeColor";
        private static readonly string FarEdgeColor = "_FarEdgeColor";
        private static readonly string Thickness = "_Thickness";
        private static readonly string Threshold = "_Threshold";
        private static readonly string EdgePower = "_EdgePower";
        private static readonly string Softness = "_Softness";
        private static readonly string MinEdgeDepth = "_Min_Edge_Depth";

        private static readonly string OffsetRed = "_OffsetRed";
        private static readonly string OffsetGreen = "_OffsetGreen";
        private static readonly string OffsetBlue = "_OffsetBlue";
        private static readonly string FrameComparison = "_FrameComparison";
        private static readonly string AberrationMinDepth = "_AberrationMinDepth";

        private static readonly string NearGridColor = "_NearGridColor";
        private static readonly string FarGridColor = "_FarGridColor";
        private static readonly string GridSize = "_GridSize";
        private static readonly string MinDepth = "_MinDepth";
        private static readonly string Alpha = "_Alpha";
        private static readonly string GridSoftness = "_GridSoftness";
        private static readonly string Radius = "_Radius";
        private static readonly string GridTex = "_Grid";

        private static readonly string KernelSize = "_Kernel_Size";
        private static readonly string N = "_n";
        private static readonly string Hardness = "_Hardness";
        private static readonly string Q = "_Q";
        private static readonly string KuwaharaAlpha = "_Kuwahara_Alpha";
        private static readonly string ZeroCrossing = "_Zero_crossing";
        private static readonly string Zeta = "_Zeta";
        private static readonly string KuwaharaMinDepth = "_KuwaharaMinDepth";

        private static readonly string OilRadius = "_Oil_Radius";
        private static readonly string OilMinDepth = "_Oil_minDepth";
        private static readonly string OilThickness = "_Oil_thickness";

        #endregion

        #region KEYWORDS

        private const string KW_EDGE = "_USE_EDGE";
        private const string KW_ABERRATION = "_USE_ABERRATION";
        private const string KW_GRID = "_USE_GRID";
        private const string KW_GRID_TEX = "_USE_GRID_TEXTURE";
        private const string KW_KUWAHARA = "_USE_KUWAHARA";
        private const string KW_INVERT_ALPHA = "_INVERTALPHA";
        private const string KW_OIL = "_USE_OIL";

        #endregion

        private AnimBool edgeAnim = new AnimBool(true);
        private AnimBool aberrationAnim = new AnimBool(true);
        private AnimBool gridAnim = new AnimBool(true);
        private AnimBool kuwaharaAnim = new AnimBool(true);
        private AnimBool oilAnim = new AnimBool(true);

        private MaterialEditor _editor;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            _editor = materialEditor;
            Material mat = materialEditor.target as Material;

            InitAnim(edgeAnim);
            InitAnim(aberrationAnim);
            InitAnim(gridAnim);
            InitAnim(kuwaharaAnim);
            InitAnim(oilAnim);

            DrawSection("Edge Outline", KW_EDGE, edgeAnim, mat, () =>
            {
                DrawProp(NearEdgeColor, "Near Edge Color", properties);
                DrawProp(FarEdgeColor, "Far Edge Color", properties);
                DrawProp(EdgeStrength, "Edge Strength", properties);
                DrawProp(Thickness, "Thickness", properties);
                DrawProp(Threshold, "Threshold", properties);
                DrawProp(EdgePower, "Edge Power", properties);
                DrawProp(Softness, "Softness", properties);
                DrawProp(MinEdgeDepth, "Min Edge Depth", properties);
            });

            DrawSection("Chromatic Aberration", KW_ABERRATION, aberrationAnim, mat, () =>
            {
                DrawProp(OffsetRed, "Offset Red", properties);
                DrawProp(OffsetGreen, "Offset Green", properties);
                DrawProp(OffsetBlue, "Offset Blue", properties);
                DrawProp(FrameComparison, "Frame Comparison", properties);
                DrawProp(AberrationMinDepth, "Min Depth", properties);
            });

            DrawSection("Background Grid", KW_GRID, gridAnim, mat, () =>
            {
                DrawKeywordToggle(mat, KW_GRID_TEX, "Use Grid Texture");

                bool useTex = mat.IsKeywordEnabled(KW_GRID_TEX);

                DrawProp(NearGridColor, "Near Grid Color", properties);
                DrawProp(FarGridColor, "Far Grid Color", properties);
                DrawProp(GridSize, "Grid Size", properties);
                DrawProp(MinDepth, "Min Depth", properties);
                DrawProp(Alpha, "Alpha", properties);

                if (useTex)
                {
                    _editor.TexturePropertySingleLine(
                        new GUIContent("Grid Texture"),
                        FindProperty(GridTex, properties)
                    );

                }
                else
                {
                    DrawProp(GridSoftness, "Softness", properties);
                    DrawProp(Radius, "Radius", properties);
                }
                DrawKeywordToggle(mat, KW_INVERT_ALPHA, "Invert Alpha");
            });

            DrawSection("Kuwahara", KW_KUWAHARA, kuwaharaAnim, mat, () =>
            {
                DrawProp(KernelSize, "Kernel Size", properties);
                DrawProp(N, "Sector Count", properties);
                DrawProp(Hardness, "Sharpness", properties);
                DrawProp(Q, "Variance Power", properties);
                DrawProp(KuwaharaAlpha, "Anisotropy", properties);
                DrawProp(ZeroCrossing, "Zero Crossing Angle", properties);
                DrawProp(Zeta, "Edge Sensitivity", properties);
                DrawProp(KuwaharaMinDepth, "Min Depth", properties);
            });

            DrawSection("Oil", KW_OIL, oilAnim, mat, () =>
            {
                DrawProp(OilRadius, "Radius", properties);
                DrawProp(OilThickness, "Thickness", properties);
                DrawProp(OilMinDepth, "Min Depth", properties);
            });
        }

        #region SECTION

        void DrawSection(string title, string keyword, AnimBool anim, Material mat, System.Action content)
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();

            anim.target = EditorGUILayout.Foldout(anim.target, title, true);

            GUILayout.FlexibleSpace();

            bool enabled = mat.IsKeywordEnabled(keyword);
            bool newEnabled = GUILayout.Toggle(enabled, "ON", "Button", GUILayout.Width(50));

            if (newEnabled != enabled)
            {
                if (newEnabled) mat.EnableKeyword(keyword);
                else mat.DisableKeyword(keyword);
            }

            EditorGUILayout.EndHorizontal();

            if (EditorGUILayout.BeginFadeGroup(anim.faded))
            {
                EditorGUI.BeginDisabledGroup(!mat.IsKeywordEnabled(keyword));

                EditorGUILayout.Space(4);
                content.Invoke();
                EditorGUILayout.Space(4);

                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.EndFadeGroup();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        #endregion

        #region HELPERS

        void InitAnim(AnimBool anim)
        {
            anim.valueChanged.RemoveAllListeners();
            anim.valueChanged.AddListener(_editor.Repaint);
        }

        void DrawProp(string name, string label, MaterialProperty[] props)
        {
            _editor.ShaderProperty(FindProperty(name, props), label);
        }

        void DrawKeywordToggle(Material mat, string keyword, string label)
        {
            bool enabled = mat.IsKeywordEnabled(keyword);
            bool newEnabled = EditorGUILayout.Toggle(label, enabled);

            if (newEnabled != enabled)
            {
                if (newEnabled) mat.EnableKeyword(keyword);
                else mat.DisableKeyword(keyword);
            }
        }

        #endregion
    }
}