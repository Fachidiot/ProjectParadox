using System;
using System.Collections.Generic;

/// <summary>변경 감지/저장/로드 및 Global/Local/Control 이벤트 분리를 지원하는 반응형 값의 추상 베이스 클래스</summary>
public abstract class ValueBase
{
    #region Type
    /// <summary>값 변경 시 호출 유형</summary>
    public enum EChangeType
    {
        /// <summary>저장/로드 이벤트 없음</summary>
        None,
        /// <summary>저장 이벤트 호출 필요</summary>
        NeedSave,
        /// <summary>로드 완료 후 호출</summary>
        Loaded
    }
    #endregion

    #region Property
    /// <summary>값의 고유 식별자</summary>
    public string ID { get; private set; }
    #endregion
    #region Value
    protected Dictionary<ControlBase, Action<ValueBase>> m_OnControlChanged = new();
    protected Action<ValueBase> m_OnLocalChanged;
    protected Action<ValueBase> m_OnConstraintChanged;
    protected Action<ValueBase> m_OnResourceChanged;
    protected Action<ValueBase> m_OnGlobalChanged;
    protected Action<ValueBase> m_OnSaveChanged;
    protected Action<ValueBase> m_OnLoaded;
    #endregion

    #region Event
    protected ValueBase(IManageValue _callBy, string _id)
    {
        _callBy?.ManageValue(this);
        ID = _id;
    }
    /// <summary>현재 값을 저장용 객체로 반환</summary>
    public virtual object OnSave()
    {
        return "";
    }
    /// <summary>현재 값을 저장용 문자열로 반환</summary>
    public virtual string OnSaveString()
    {
        return OnSave().ToString();
    }
    /// <summary>객체 데이터에서 값을 로드</summary>
    public virtual void OnLoad(object _data)
    {
    }
    /// <summary>문자열 데이터에서 값을 로드</summary>
    public virtual void OnLoadString(string _data)
    {
    }

    /// <summary>값 변경 시 등록된 모든 이벤트를 순서대로 호출</summary>
    protected virtual void OnChanged(EChangeType _type)
    {
        m_OnConstraintChanged?.Invoke(this);
        if (_type == EChangeType.Loaded)
            m_OnLoaded?.Invoke(this);
        m_OnResourceChanged?.Invoke(this);
        m_OnGlobalChanged?.Invoke(this);
        m_OnLocalChanged?.Invoke(this);
        if (_type == EChangeType.NeedSave)
            m_OnSaveChanged?.Invoke(this);
        foreach (var v in m_OnControlChanged)
            if (v.Key.IsActive)
                v.Value?.Invoke(this);
    }
    /// <summary>Local/Control 이벤트 리스너를 모두 초기화</summary>
    public virtual void OnResetLocalChanged()
    {
        m_OnLocalChanged = null;
        m_OnControlChanged.Clear();
    }
    #endregion
    #region Function
    /// <summary>특정 컨트롤에 등록된 이벤트를 즉시 호출</summary>
    public void CallControlEvent(ControlBase _ctrl)
    {
        if (m_OnControlChanged.TryGetValue(_ctrl, out var action))
            action?.Invoke(this);
    }
    /// <summary>저장 이벤트 리스너를 등록</summary>
    public void AddSaveChanged(GlobalManagerBase _callBy, Action<ValueBase> _action, bool _isCallNow = false)
    {
        m_OnSaveChanged += _action;
        if (_isCallNow)
            _action?.Invoke(this);
    }
    /// <summary>리소스 변경 이벤트 리스너를 등록</summary>
    public void AddResourceChanged(GlobalManagerBase _callBy, Action<ValueBase> _action, bool _isCallNow = false)
    {
        m_OnResourceChanged += _action;
        if (_isCallNow)
            _action?.Invoke(this);
    }
    /// <summary>제약조건 변경 이벤트 리스너를 등록</summary>
    public void AddConstraintChanged(GlobalManagerBase _callBy, Action<ValueBase> _action)
    {
        m_OnConstraintChanged += _action;
        _action?.Invoke(this);
    }
    /// <summary>변경 이벤트를 수동으로 발생시킴</summary>
    public void PostChanged(EChangeType _type)
    {
        OnChanged(_type);
    }
    /// <summary>호출자 타입에 따라 적절한 변경 이벤트 리스너를 등록</summary>
    public void AddChanged(object _callBy, Action<ValueBase> _action, bool _isCallNow = false)
    {
        if (_callBy as GlobalManagerBase)
            m_OnGlobalChanged += _action;
        else if (_callBy as LocalManagerBase || _callBy as ObjectBase || _callBy as PopupBase)
            m_OnLocalChanged += _action;
        else if (_callBy is ControlBase ctrl)
        {
            if (m_OnControlChanged.ContainsKey(ctrl))
                m_OnControlChanged[ctrl] += _action;
            else
                m_OnControlChanged.Add(ctrl, _action);
            ctrl.AddValueBase(this);
        }
        else
            throw new InvalidOperationException();

        if (_isCallNow)
            _action?.Invoke(this);
    }
    /// <summary>등록된 변경 이벤트 리스너를 제거</summary>
    public void RemoveChanged(object _callBy, Action<ValueBase> _action)
    {
        if (_callBy as GlobalManagerBase)
            m_OnGlobalChanged -= _action;
        else if (_callBy as LocalManagerBase || _callBy as ObjectBase || _callBy as PopupBase)
            m_OnLocalChanged -= _action;
        else if (_callBy is ControlBase ctrl)
        {
            if (m_OnControlChanged.TryGetValue(ctrl, out var existingDelegate))
            {
                existingDelegate -= _action;
                if (existingDelegate == null)
                {
                    m_OnControlChanged.Remove(ctrl);
                    if (ctrl != null)
                        ctrl.RemoveValueBase(this);
                }
                else
                    m_OnControlChanged[ctrl] = existingDelegate;
            }
        }
        else
            throw new InvalidOperationException();
    }
    /// <summary>로드 완료 시 호출될 이벤트 리스너를 등록 (GlobalManagerBase만 허용)</summary>
    public void AddLoadEvent(object _callBy, Action<ValueBase> _action, bool _isCallNow = false)
    {
        if (_callBy as GlobalManagerBase)
        {
            m_OnLoaded += _action;
        }
        else
            throw new InvalidOperationException();

        if (_isCallNow)
            _action?.Invoke(this);
    }
    #endregion
}
