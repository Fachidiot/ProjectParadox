using System;
using TMPro;
using UnityEngine;

/// <summary>TMP_Dropdown 컴포넌트를 래핑하여 값 변경 이벤트를 관리</summary>
[RequireComponent(typeof(TMP_Dropdown))]
public class Control_Dropdown : ControlBase
{
    #region Property
    /// <summary>래핑된 TMP_Dropdown 컴포넌트</summary>
    public TMP_Dropdown v { get; private set; }
    #endregion
    #region Value
    private Action<Control_Dropdown> m_OnChanged;
    #endregion

    #region Event
    protected override void Awake()
    {
        base.Awake();
        v = GetComponent<TMP_Dropdown>();
        v.onValueChanged.AddListener(OnValueChanged);
    }
    /// <summary>드롭다운 값 변경 시 등록된 콜백을 호출</summary>
    private void OnValueChanged(int value)
    {
        m_OnChanged?.Invoke(this);
    }
    #endregion
    #region Function
    /// <summary>값 변경 이벤트 리스너를 등록</summary>
    public void AddChangeListener(Action<Control_Dropdown> _onClick)
    {
        m_OnChanged += _onClick;
    }
    #endregion
}
