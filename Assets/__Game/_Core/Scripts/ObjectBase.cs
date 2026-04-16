using System.Collections.Generic;
using UnityEngine;

/// <summary>LocalManager 하위 오브젝트의 베이스 클래스, 라이프사이클 및 ValueBase 관리, 계층적 자식 ObjectBase 자동 관리</summary>
public abstract class ObjectBase : MonoBehaviour, IManageValue
{
    #region Property
    /// <summary>초기화 완료 여부</summary>
    public bool IsInited { get; private set; }
    #endregion
    #region Value
    protected List<ValueBase> m_ManageValue = new();
    protected ObjectBase[] m_Children;
    #endregion

    #region Event
    public virtual void InitSingleton()
    {
        m_Children = ObjectUtil.FindDirectChildren(transform);
        foreach (var v in m_Children)
            v.InitSingleton();
    }
    public virtual void Init()
    {
        foreach (var v in m_Children)
            v.Init();
        IsInited = true;
    }
    public virtual void AfterInit()
    {
        foreach (var v in m_Children)
            v.AfterInit();
    }
    public virtual void AfterInitGame()
    {
        foreach (var v in m_Children)
            v.AfterInitGame();
    }
    public virtual void OnShutdown()
    {
        foreach (var v in m_Children)
            v.OnShutdown();
    }
    #endregion
    #region Manual Function
    void IManageValue.ManageValue(ValueBase _value)
    {
        m_ManageValue.Add(_value);
    }
    #endregion
}
