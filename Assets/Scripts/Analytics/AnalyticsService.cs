using System.Runtime.InteropServices;
using UnityEngine;

public static class AnalyticsService
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void FirebaseLogEvent(string name);

    [DllImport("__Internal")]
    private static extern void FirebaseLogEventString(string name, string key, string value);

    [DllImport("__Internal")]
    private static extern void FirebaseLogEventNumber(string name, string key, float value);
#endif

    public static void LogEvent(string name)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        FirebaseLogEvent(name);
#else
        Debug.Log($"Analytics: {name}");
#endif
    }

    public static void LogEvent(string name, string key, string value)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        FirebaseLogEventString(name, key, value);
#else
        Debug.Log($"Analytics: {name}, {key} = {value}");
#endif
    }

    public static void LogEvent(string name, string key, float value)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        FirebaseLogEventNumber(name, key, value);
#else
        Debug.Log($"Analytics: {name}, {key} = {value}");
#endif
    }
}