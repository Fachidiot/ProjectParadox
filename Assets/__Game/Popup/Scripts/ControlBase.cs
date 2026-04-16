using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

/// <summary>팝업 내 UI 컨트롤의 베이스 클래스로 라이프사이클 콜백을 제공</summary>
public class ControlBase : MonoBehaviour
{
    #region Property
    /// <summary>컨트롤이 현재 활성화 상태인지 여부</summary>
    public bool IsActive { get; protected set; }
    #endregion
    #region Value
    private PopupBase m_ParentPopup;
    private List<ValueBase> m_ValueBase = new();
    #endregion

    #region Event
    protected virtual void Awake()
    {
        m_ParentPopup = GetComponentInParent<PopupBase>();
    }
    protected virtual void Start()
    {
        if (m_ParentPopup)
            m_ParentPopup.OnRegisterControl(this);
    }
    protected virtual void OnEnable()
    {
        IsActive = true;
        foreach(var v in m_ValueBase)
            v.CallControlEvent(this);
    }
    protected virtual void OnDisable()
    {
        IsActive = false;
    }

    /// <summary>기본 초기화 완료 후 호출되는 콜백</summary>
    public virtual void AfterInit()
    {
    }
    /// <summary>게임 초기화 완료 후 호출되는 콜백</summary>
    public virtual void AfterInitGame()
    {
    }
    /// <summary>팝업이 열릴 때 호출되는 콜백</summary>
    public virtual void OnOpen()
    {
    }
    /// <summary>팝업이 닫힐 때 호출되는 콜백</summary>
    public virtual void OnClose()
    {
    }
    /// <summary>시스템 종료 시 호출되는 콜백</summary>
    public virtual void OnShutdown()
    {
    }
    #endregion
    #region Manual Function
    /// <summary>ValueBase 변경 감지 대상으로 등록</summary>
    public void AddValueBase(ValueBase _v)
    {
        m_ValueBase.Remove(_v);
        m_ValueBase.Add(_v);
    }
    /// <summary>ValueBase 변경 감지 대상에서 제거</summary>
    public void RemoveValueBase(ValueBase _v)
    {
        m_ValueBase.Remove(_v);
    }
    #endregion
}
