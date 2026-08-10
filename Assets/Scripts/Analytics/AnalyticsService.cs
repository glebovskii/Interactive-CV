using Fusion;
using System;
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

    private static void LogEvent(string name, string key, string value)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        FirebaseLogEventString(name, key, value);
#else
        Debug.Log($"Analytics: {name}, {key} = {value}");
#endif
    }

    public static void JoinRoomSuccess(string name)
    {
        LogEvent("join_room", "success", name);
    }

    public static void JoinRoomFail(ShutdownReason reason)
    {
        LogEvent("join_room", "fail", reason.ToString());
    }

    public static void JoinRoomClick(string name)
    {
        LogEvent("join_room", "try", name);
    }

    public static void NameChanged(string name)
    {
        LogEvent("name", "change_name", name);
    }

    public static void LinkClicked(string name)
    {
        LogEvent("link", "click", name);
    }

    public static void LinkOpened(string name)
    {
        LogEvent("link", "open", name);
    }

    public static void ShaderOpened(string name)
    {
        LogEvent("shader", "open", name);
    }

    public static void InfoOpened(string name)
    {
        LogEvent("info", "open", name);
    }

    public static void ProjectOpened(string name)
    {
        LogEvent("project", "open", name);
    }

    public static void LocaleChanged(int locale)
    {
        LogEvent("localization", "selected_locale", locale);
    }

    private static void LogEvent(string name, string key, float value)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        FirebaseLogEventNumber(name, key, value);
#else
        Debug.Log($"Analytics: {name}, {key} = {value}");
#endif
    }

    
}