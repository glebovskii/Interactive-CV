#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PanelUI))]
public sealed class PanelUIEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PanelUI panel = (PanelUI)target;

        EditorGUILayout.Space(10f);

        using (new EditorGUI.DisabledScope(panel.TiltSettings == null))
        {
            if (GUILayout.Button("Save Runtime Transform To Preset"))
                SaveToPreset(panel);

            if (GUILayout.Button("Load Transform From Preset"))
                LoadFromPreset(panel);
        }
    }

    private static void SaveToPreset(PanelUI panel)
    {
        PanelTiltSettings settings = panel.TiltSettings;
        Transform panelTransform = panel.transform;

        Undo.RecordObject(settings, "Save Panel Transform Preset");

        settings.localPosition = panelTransform.localPosition;
        settings.localEulerAngles = panelTransform.localEulerAngles;
        settings.localScale = panelTransform.localScale;
        settings.maximumXTilt = panel.MaximumXTilt;
        settings.maximumYTilt = panel.MaximumYTilt;

        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
    }

    private static void LoadFromPreset(PanelUI panel)
    {
        Undo.RecordObject(panel.transform, "Load Panel Transform Preset");
        Undo.RecordObject(panel, "Load Panel Tilt Preset");

        panel.ApplyTiltSettings();

        EditorUtility.SetDirty(panel.transform);
        EditorUtility.SetDirty(panel);
    }
}
#endif