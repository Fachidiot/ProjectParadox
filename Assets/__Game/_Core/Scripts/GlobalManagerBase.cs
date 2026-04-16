/// <summary>전역 매니저 베이스 클래스, DontDestroyOnLoad 적용, InitFirst→InitValue→InitGame 순 초기화</summary>
public abstract class GlobalManagerBase : ManagerBase
{
    #region Property
    /// <summary>InitFirst 완료 여부</summary>
    public bool IsFirstInited { get; private set; } = false;
    #endregion

    #region Event
    public virtual void InitFirst()
    {
        IsFirstInited = true;
    }

    public virtual void InitValue()
    {
        foreach (var v in m_ManageValue)
            v.OnResetLocalChanged();
    }

    public override void InitGame()
    {
        base.InitGame();
    }
    #endregion
}
