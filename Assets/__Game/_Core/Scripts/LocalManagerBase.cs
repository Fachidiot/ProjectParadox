/// <summary>씬 로컬 매니저 베이스 클래스, 씬 전환 시 파괴, Init→InitGame 순 초기화, 하위 ObjectBase 자동 관리</summary>
public abstract class LocalManagerBase : ManagerBase
{
    #region Value
    protected ObjectBase[] m_ChildObjects;
    #endregion

    #region Event
    public override void InitSingleton()
    {
        m_ChildObjects = ObjectUtil.FindDirectChildren(transform);
        foreach (var v in m_ChildObjects)
            v.InitSingleton();
        base.InitSingleton();
    }
    public override void Init()
    {
        foreach (var v in m_ChildObjects)
            v.Init();
        base.Init();
    }
    public override void AfterInit()
    {
        foreach (var v in m_ChildObjects)
            v.AfterInit();
        base.AfterInit();
    }
    public override void AfterInitGame()
    {
        foreach (var v in m_ChildObjects)
            v.AfterInitGame();
        base.AfterInitGame();
    }
    public override void OnShutdown()
    {
        foreach (var v in m_ChildObjects)
            v.OnShutdown();
        base.OnShutdown();
    }
    #endregion
}
