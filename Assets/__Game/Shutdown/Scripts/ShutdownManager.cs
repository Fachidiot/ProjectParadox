using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>치명적 오류 발생 시 UI를 표시하고 자동 재접속을 처리하는 매니저</summary>
public class ShutdownManager : GlobalManagerBase
{
    public static ShutdownManager instance { get; private set; }
    #region Inspector
    [SerializeField] private GameObject m_ShutdownUI;
    [SerializeField] private TMP_Text m_Text;
    [SerializeField] private string m_ReconnectScene = "LobbyScene";
    #endregion
    #region Property
    /// <summary>게임이 중단 상태인지 여부</summary>
    public bool IsShutdown { get; private set; } = false;
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }
    public override void InitFirst()
    {
        m_ShutdownUI.SetActive(false);
        base.InitFirst();
    }

    /// <summary>일정 시간 후 Global 인스턴스를 제거하고 재접속 씬을 로드</summary>
    private IEnumerator ShutdownCor()
    {
        yield return new WaitForSecondsRealtime(5);

        Destroy(Global.instance.gameObject);
        SceneManager.LoadScene(m_ReconnectScene);
    }
    #endregion
    #region Function
    /// <summary>치명적 오류로 게임을 중단하고 오류 UI를 표시</summary>
    public void Shutdown(string _system, Exception _e = null, string _debugMsg = "Unknown Error")
    {
        if (IsShutdown)
            return;
        IsShutdown = true;

        m_ShutdownUI.SetActive(true);
        m_Text.text = (LanguageManager.instance != null) ? LanguageManager.instance.Get("Text_Shutdown_Text") : "!!ERROR!!";

        Debug.LogError((_e != null) ? $"[{_system}] {_debugMsg}\n\n{_e.Message}\n\n{_e.StackTrace}" : $"[{_system}] {_debugMsg}");

        Stop();

        StartCoroutine(ShutdownCor());
    }
    /// <summary>타임스케일을 0으로 설정하고 오디오를 일시정지</summary>
    public void Stop()
    {
        if (TimeManager.instance && TimeManager.instance.IsInited)
            TimeManager.instance.SetTimeScale(this, this, 0);

        // TODO : SoundManager 추가 후 옮기기
        AudioListener.pause = true;

        Global.instance?.Shutdown();
        Local.instance?.Shutdown();
    }
#endregion
}
