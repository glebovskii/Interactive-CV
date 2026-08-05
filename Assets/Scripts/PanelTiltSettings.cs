using UnityEngine;

[CreateAssetMenu(fileName = "Panel Tilt Settings", menuName = "Portfolio UI/Panel Tilt Settings")]
public sealed class PanelTiltSettings : ScriptableObject
{
    public Vector3 localPosition;
    public Vector3 localEulerAngles;
    public Vector3 localScale = Vector3.one;

    [Range(-45f, 45f)] public float maximumXTilt = 6f;
    [Range(-90f, 90f)] public float maximumYTilt = 18f;
}