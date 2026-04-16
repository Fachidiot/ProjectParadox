using System;

/// <summary>씬 로컬 매니저 생명주기 관리 및 Global 연동 초기화 시스템</summary>
public class Local : Root
{
    public static Local instance { get; private set; }

    #region Property
    /// <summary>기본 초기화 완료 여부</summary>
    public bool IsInited { get; private set; }

    /// <summary>게임 초기화 완료 여부</summary>
    public bool IsGameInited { get; private set; }
    #endregion

    #region Value
    private LocalManagerBase[] m_Managers;
    #endregion

    #region Event
    private void Awake()
    {
        instance = this;
        m_Managers = GetComponentsInChildren<LocalManagerBase>();
        InitSingletons("Game", m_Managers);
    }

    private void Start()
    {
        if (ShutdownManager.instance && ShutdownManager.instance.IsShutdown)
        {
            Shutdown();
            return;
        }

        try
        {
            Global.instance.InitValue();

            if (!Global.instance.IsInited)
                Global.instance.Init();
            Init();

            Global.instance.AfterInit();
            AfterInit();

            {
                Global.instance.InitGame();
                InitGame();

                Global.instance.AfterInitGame();
                AfterInitGame();
            }
        }
        catch (Exception e)
        {
            ShutdownManager.instance.Shutdown("Local", e);
        }
    }

    private void Init()
    {
        InitManagers("Game", m_Managers);
        IsInited = true;
    }

    private void AfterInit()
    {
        InitManagersBase("Local", "AfterInit", m_Managers, (v) => true, (v) => v.AfterInit());
    }

    /// <summary>로컬 매니저 게임 시스템 가동</summary>
    public void InitGame()
    {
        InitGameManagers("Game", m_Managers);
        IsGameInited = true;
    }

    /// <summary>게임 초기화 완료 후 최종 동기화</summary>
    public void AfterInitGame()
    {
        InitManagersBase("Local", "AfterInitGame", m_Managers, (v) => true, (v) => v.AfterInitGame());
    }
    #endregion

    #region Manual Function
    /// <summary>로컬 매니저 기능 정지 및 상태 해제</summary>
    public void Shutdown()
    {
        foreach (var v in GetComponentsInChildren<LocalManagerBase>())
        {
            v.OnShutdown();
            v.enabled = false;
        }
    }
    #endregion
}
