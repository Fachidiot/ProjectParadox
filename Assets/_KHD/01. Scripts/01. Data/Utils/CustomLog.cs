using UnityEngine;
using System.Diagnostics; // [Conditional]을 사용하기 위해 필요

public static class CustomLog
{
    // 유니티 에디터 환경이거나, 빌드 세팅에서 'Development Build'를 켰을 때만 작동함
    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void Log(object message)
    {
        UnityEngine.Debug.Log(message);
    }

    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void LogWarning(object message)
    {
        UnityEngine.Debug.LogWarning(message);
    }

    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void LogError(object message)
    {
        UnityEngine.Debug.LogError(message);
    }
}