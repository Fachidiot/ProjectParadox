using UnityEngine;

/// <summary>로그 관리 매니저, Debug.Log 래퍼</summary>
public class LogManager : GlobalManagerBase
{
    public static LogManager instance { get; private set; }
    #region Inspector
    [SerializeField] private GameObject m_DebugConsole;
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }
    public override void InitFirst()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        m_DebugConsole.SetActive(true);
#endif
        base.InitFirst();
    }
    #endregion
    #region Function
    /// <summary>시스템 태그 포함 디버그 로그 출력</summary>
    public void Log(string _system, string _msg)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"[{_system}] {_msg}");
#endif
    }
    /// <summary>시스템 태그 포함 에러 로그 출력</summary>
    public void Error(string _system, string _msg)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogError($"[{_system}] {_msg}");
#endif
    }
    #endregion
}
