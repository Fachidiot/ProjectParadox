using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Toggle 컴포넌트를 래핑하여 값 변경 이벤트를 관리</summary>
[RequireComponent(typeof(Toggle))]
public class Control_Toggle : ControlBase
{
    #region Property
    /// <summary>래핑된 Toggle 컴포넌트</summary>
    public Toggle v { get; private set; }
    #endregion
    #region Value
    private Action<Control_Toggle, bool> m_OnValueChanged;
    #endregion

    #region Event
    protected override void Awake()
    {
        base.Awake();
        v = GetComponent<Toggle>();
        v.onValueChanged.AddListener(OnValueChanged);
    }
    /// <summary>토글 값 변경 시 등록된 콜백을 호출</summary>
    private void OnValueChanged(bool _dummy)
    {
        m_OnValueChanged?.Invoke(this, v.isOn);
    }
    #endregion
    #region Function
    /// <summary>토글의 켜짐/꺼짐 상태를 설정</summary>
    public void SetIsOn(bool _isOn)
    {
        v.isOn = _isOn;
    }
    /// <summary>토글의 상호작용 가능 여부를 설정</summary>
    public void SetIsInteractable(bool _isInteractable)
    {
        v.interactable = _isInteractable;
    }
    /// <summary>값 변경 이벤트 리스너를 등록</summary>
    public void AddValueChangedListener(Action<Control_Toggle, bool> _onValueChanged)
    {
        m_OnValueChanged += _onValueChanged;
    }
    #endregion
}
