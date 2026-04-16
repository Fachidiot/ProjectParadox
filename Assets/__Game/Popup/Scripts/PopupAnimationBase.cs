using System;
using UnityEngine;

/// <summary>팝업 열기/닫기 애니메이션의 추상 베이스 클래스</summary>
public class PopupAnimationBase : ControlBase
{
    #region Type
    private enum EState
    {
        None = 0,
        Open,
        Close
    }
    #endregion

    #region Value
    private EState m_State = EState.None;
    private Action m_OnEndOpen;
    private Action m_OnEndClose;
    #endregion

    #region Event
    /// <summary>열기 애니메이션 시작 시 호출되는 콜백</summary>
    protected virtual void OnStartOpen(Action _onEnd)
    {
        OnEndOpen();
    }
    /// <summary>열기 애니메이션 완료 시 등록된 콜백을 호출</summary>
    protected virtual void OnEndOpen()
    {
        m_OnEndOpen.Invoke();
    }
    /// <summary>닫기 애니메이션 시작 시 호출되는 콜백</summary>
    protected virtual void OnStartClose(Action _onEnd)
    {
        OnEndClose();
    }
    /// <summary>닫기 애니메이션 완료 시 등록된 콜백을 호출</summary>
    protected virtual void OnEndClose()
    {
        m_OnEndClose.Invoke();
    }
    /// <summary>현재 진행 중인 애니메이션 상태를 취소하고 초기화</summary>
    protected virtual void OnCancelState()
    {
        if (m_State == EState.Open)
            m_OnEndOpen?.Invoke();
        else if (m_State == EState.Close)
            m_OnEndClose?.Invoke();

        m_State = EState.None;
        m_OnEndOpen = null;
        m_OnEndClose = null;
    }
    #endregion
    #region Manual Function
    /// <summary>열기 애니메이션을 시작</summary>
    public void StartOpen(Action _onEnd)
    {
        OnCancelState();
        OnStartOpen(_onEnd);
    }
    /// <summary>닫기 애니메이션을 시작</summary>
    public void StartClose(Action _onEnd)
    {
        OnCancelState();
        OnStartClose(_onEnd);
    }
    #endregion
}
