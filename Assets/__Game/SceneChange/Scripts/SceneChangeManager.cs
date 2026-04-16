using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>씬 전환을 관리하고 애니메이션과 연동하여 SceneManager.LoadScene을 래핑</summary>
public class SceneChangeManager : GlobalManagerBase
{
    public static SceneChangeManager instance { get; private set; }
    #region Inspector
    [SerializeField, TabGroup("Component"), LabelText("씬변경 애니메이션")] private SceneChangeAni[] m_SceneChangeAni;
    [SerializeField] private string m_LobbySceneID = "LobbyScene";
    [SerializeField] private string m_Room1Scene = "Room1";
    #endregion
    #region Property
    /// <summary>로비 씬의 이름 식별자</summary>
    public string LobbySceneID => m_LobbySceneID;
    /// <summary>게임 씬의 이름 식별자</summary>
    public string GameSceneID => m_Room1Scene;
    #endregion
    #region Value
    private Dictionary<string, SceneChangeAni> m_Ani = new();
    private SceneChangeAni m_CurAni;
    private string m_TargetSceneID;         //변경할 씬
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }
    public override void Init()
    {
        foreach(var v in m_SceneChangeAni)
        {
            m_Ani.Add(v.name, v);
            v.Init();
        }
        SceneManager.sceneLoaded += OnSceneLoaded;

        base.Init();
    }
    public override void OnShutdown()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        base.OnShutdown();
    }

    /// <summary>씬 로드 완료 시 종료 애니메이션을 재생</summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        m_CurAni?.EndAni();
    }

    /// <summary>애니메이션에서 호출되어 실제 씬 로드를 수행</summary>
    internal void OnChange()
    {
        SceneManager.LoadScene(m_TargetSceneID, LoadSceneMode.Single);
    }
    /// <summary>애니메이션 완료 시 현재 애니메이션 참조를 초기화</summary>
    internal void OnEnd()
    {
        m_CurAni = null;
    }
    #endregion
    #region Function
    /// <summary>이름으로 등록된 애니메이션 반환</summary>
    public SceneChangeAni GetAni(string _aniName) => m_Ani.TryGetValue(_aniName, out var v) ? v : null;

    /// <summary>지정된 씬으로 애니메이션과 함께 전환</summary>
    public void SceneChange(string _nextScene, string _aniName = "Default")
    {
        if (m_CurAni)
            return;

        m_TargetSceneID = _nextScene;

        m_CurAni = m_Ani[_aniName];
        m_CurAni.StartAni();
    }
    #endregion
}
