using System;

/// <summary>전역 매니저 생명주기 관리 및 DontDestroyOnLoad 기반 초기화 시스템</summary>
public class Global : Root
{
    public static Global instance { get; private set; }

    #region Property
    /// <summary>기본 초기화 완료 여부</summary>
    public bool IsInited { get; private set; }

    /// <summary>셧다운 상태 여부</summary>
    public bool IsShutdown { get; private set; }
    #endregion

    #region Value
    private GlobalManagerBase[] m_Managers;
    #endregion

    #region Event
    private void Awake()
    {
        if (instance)
        {
            if (instance.IsShutdown)
                Destroy(instance.gameObject);
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        instance = this;
        m_Managers = GetComponentsInChildren<GlobalManagerBase>();
        InitSingletons("Global", m_Managers);
        DontDestroyOnLoad(this);

        try
        {
            InitManagersBase("Global", "InitFirst", m_Managers, (v) => true, (v) => v.InitFirst());
        }
        catch (Exception e)
        {
            ShutdownManager.instance.Shutdown("Global", e);
        }
    }
    #endregion

    #region Manual Function
    /// <summary>매니저 가동 전 변수 초기값 설정</summary>
    public void InitValue()
    {
        InitManagersBase("Global", "InitValue", m_Managers, (v) => true, (v) => v.InitValue());
    }

    /// <summary>전역 매니저 시스템 활성화</summary>
    public void Init()
    {
        InitManagers("Global", m_Managers);
        IsInited = true;
    }

    /// <summary>기본 초기화 완료 후 후속 처리</summary>
    public void AfterInit()
    {
        InitManagersBase("Global", "AfterInit", m_Managers, (v) => true, (v) => v.AfterInit());
    }

    /// <summary>인게임 매니저 시스템 가동</summary>
    public void InitGame()
    {
        InitGameManagers("Global", m_Managers);
    }

    /// <summary>게임 초기화 완료 후 최종 동기화</summary>
    public void AfterInitGame()
    {
        InitManagersBase("Global", "AfterInitGame", m_Managers, (v) => true, (v) => v.AfterInitGame());
    }

    /// <summary>전역 매니저 기능 정지 및 상태 해제</summary>
    public void Shutdown()
    {
        IsShutdown = true;

        foreach (var v in GetComponentsInChildren<GlobalManagerBase>())
        {
            v.OnShutdown();
            v.enabled = false;
        }
    }
    #endregion
}
