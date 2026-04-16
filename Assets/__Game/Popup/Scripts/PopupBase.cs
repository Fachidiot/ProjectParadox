using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>팝업 베이스 클래스, Open/Close/애니메이션/컨트롤 관리</summary>
public abstract class PopupBase : MonoBehaviour, IManageValue
{
    #region Inspector
    [SerializeField] private bool m_IsCloseByCancel = false;
    [SerializeField] private bool m_IsDefaultOpen = false;
    [SerializeField] private int m_FixedOrder = -1;
    #endregion
    #region Property
    /// <summary>초기화 완료 여부</summary>
    public bool IsInited { get; private set; }
    /// <summary>Cancel 키로 닫기 가능 여부</summary>
    public bool IsCloseByCancel => m_IsCloseByCancel;
    /// <summary>시작 시 자동 열림 여부</summary>
    public bool IsDefaultOpen => m_IsDefaultOpen;
    /// <summary>고정 정렬 순서, -1이면 동적</summary>
    public int FixedOreder => m_FixedOrder;
    /// <summary>열기 애니메이션 진행 중 여부</summary>
    public bool IsOpening { get; private set; } = false;
    /// <summary>현재 열려있는 상태 여부</summary>
    public bool IsOpened { get; private set; } = false;
    /// <summary>닫기 애니메이션 진행 중 여부</summary>
    public bool IsClosing { get; private set; } = false;
    /// <summary>표시 상태 여부</summary>
    public bool IsShow { get; private set; } = true;
    /// <summary>팝업 Canvas 컴포넌트</summary>
    public Canvas PopupCanvas { get; private set; }
    /// <summary>팝업 GraphicRaycaster 컴포넌트</summary>
    public GraphicRaycaster PopupGraphicRaycaster { get; private set; }
    #endregion
    #region Value
    private List<ControlBase> m_Controls = new();
    private List<PopupAnimationBase> m_Animations = new();
    private Action<object> m_OnOpen;
    private Action<object> m_OnClose;
    protected List<ValueBase> m_ManageValue = new();
    #endregion

    #region Event
    public virtual void InitSingleton()
    {
    }
    public virtual void Init(Camera _uiCamera)
    {
        PopupCanvas = GetComponent<Canvas>();
        PopupGraphicRaycaster = GetComponent<GraphicRaycaster>();
        PopupCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        PopupCanvas.worldCamera = _uiCamera;
        IsInited = true;
    }
    public virtual void AfterInit()
    {
        foreach (var v in m_Controls)
            v.AfterInit();
        gameObject.SetActive(false);
    }
    public virtual void AfterInitGame()
    {
        foreach (var v in m_Controls)
            v.AfterInitGame();
    }
    public virtual void OnShutdown()
    {
        m_OnOpen = null;
        m_OnClose = null;
        foreach (var v in m_Controls)
        {
            v.OnShutdown();
            v.enabled = false;
        }
    }

    /// <summary>Canvas 정렬 순서 적용</summary>
    public void OnSort(int _order)
    {
        if (0 <= m_FixedOrder)
        {
            PopupCanvas.planeDistance = 100 - m_FixedOrder;
            PopupCanvas.sortingOrder = m_FixedOrder;
        }
        else
        {
            PopupCanvas.planeDistance = 100 - _order;
            PopupCanvas.sortingOrder = _order;
        }
    }
    public virtual void OnOpen(object _option = null)
    {
        IsOpened = true;
        gameObject.SetActive(IsOpened & IsShow);
        m_OnOpen?.Invoke(_option);
        foreach (var v in m_Controls)
            v.OnOpen();

        if (0 < m_Animations.Count)
        {
            IsOpening = true;
            int counter = m_Animations.Count;
            Action onEnd = () =>
            {
                counter -= 1;
                if (counter == 0)
                    IsOpening = false;
            };
            foreach (var v in m_Animations)
                v.StartOpen(onEnd);
        }
    }
    public virtual void OnStartClose(object _option = null)
    {
        if (IsClosing)
            return;

        if (0 < m_Animations.Count)
        {
            IsClosing = true;
            int counter = m_Animations.Count;
            Action onEnd = () =>
            {
                counter -= 1;
                if (counter == 0)
                {
                    OnClose(_option);
                    IsClosing = false;
                }
            };
            foreach (var v in m_Animations)
                v.StartClose(onEnd);
        }
        else
            OnClose(_option);
    }
    public virtual void OnClose(object _option = null)
    {
        IsOpened = false;
        m_OnClose?.Invoke(_option);
        gameObject.SetActive(IsOpened & IsShow);
        foreach (var v in m_Controls)
            v.OnClose();
    }
    /// <summary>팝업 표시/숨김 설정</summary>
    public void OnSetShow(bool _isShow)
    {
        IsShow = _isShow;
        gameObject.SetActive(IsOpened & IsShow);
    }
    /// <summary>컨트롤 등록</summary>
    public void OnRegisterControl(ControlBase _control)
    {
        m_Controls.Add(_control);
        if (_control is PopupAnimationBase b)
            m_Animations.Add(b);

        if (Local.instance.IsInited)
            _control.AfterInit();
        if (Local.instance.IsGameInited)
            _control.AfterInitGame();
    }

    public virtual bool OnInputCancel(InputAction.CallbackContext _context)
    {
        if (_context.performed && m_IsCloseByCancel)
        {
            Close();
            return true;
        }

        return false;
    }
    #endregion
    #region Manual Function
    void IManageValue.ManageValue(ValueBase _value)
    {
        m_ManageValue.Add(_value);
    }
    #endregion
    #region Function
    /// <summary>이 팝업 열기</summary>
    public void Open(object _option = null)
    {
        LocalPopupManager.instance.Open(gameObject.name, _option);
    }
    /// <summary>이 팝업 닫기</summary>
    public void Close(object _option = null)
    {
        LocalPopupManager.instance.Close(gameObject.name, _option);
    }
    /// <summary>열기 이벤트 리스너 등록</summary>
    public void AddOpenListener(Action<object> _onOpen)
    {
        m_OnOpen += _onOpen;
    }
    /// <summary>닫기 이벤트 리스너 등록</summary>
    public void AddCloseListener(Action<object> _onClose)
    {
        m_OnClose += _onClose;
    }
    #endregion
}
