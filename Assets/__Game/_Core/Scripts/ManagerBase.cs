using System.Collections.Generic;
using UnityEngine;

/// <summary>모든 매니저의 최상위 베이스 클래스, 초기화 라이프사이클 및 ValueBase 관리</summary>
public abstract class ManagerBase : MonoBehaviour, IManageValue
{
    #region Property
    /// <summary>기본 초기화 완료 여부</summary>
    public bool IsInited { get; private set; } = false;
    /// <summary>게임 초기화 완료 여부</summary>
    public bool IsGameInited { get; private set; } = false;
    #endregion
    #region Value
    protected List<ValueBase> m_ManageValue = new();
    #endregion

    #region Event
    public virtual void InitSingleton()
    {
    }
    public virtual bool RequireInit()
    {
        return true;
    }
    public virtual void Init()
    {
        IsInited = true;
    }
    public virtual void AfterInit()
    {
    }
    public virtual bool RequireInitGame()
    {
        return true;
    }
    public virtual void InitGame()
    {
        IsGameInited = true;
    }
    public virtual void AfterInitGame()
    {
    }

    public virtual void OnShutdown()
    {
    }
    #endregion
    #region Manual Function
    void IManageValue.ManageValue(ValueBase _value)
    {
        m_ManageValue.Add(_value);
    }
    #endregion
}
