using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Slider 컴포넌트를 래핑하여 값 변경 이벤트를 관리</summary>
[RequireComponent(typeof(Slider))]
public class Control_Slider : ControlBase
{
    #region Property
    /// <summary>래핑된 Slider 컴포넌트</summary>
    public Slider v { get; private set; }
    #endregion
    #region Value
    private Action<Control_Slider> m_OnChanged;
    #endregion

    #region Event
    protected override void Awake()
    {
        base.Awake();
        v = GetComponent<Slider>();
        v.onValueChanged.AddListener(OnValueChanged);
    }
    /// <summary>슬라이더 값 변경 시 등록된 콜백을 호출</summary>
    private void OnValueChanged(float value)
    {
        m_OnChanged?.Invoke(this);
    }
    #endregion
    #region Function
    /// <summary>슬라이더 값을 설정</summary>
    public void Set(float _value)
    {
        v.value = _value;
    }
    /// <summary>슬라이더의 최소/최대 범위를 설정</summary>
    public void SetMinMax(float _min, float _max)
    {
        v.minValue = _min;
        v.maxValue = _max;
    }
    /// <summary>값 변경 이벤트 리스너를 등록</summary>
    public void AddChangeListener(Action<Control_Slider> _onClick)
    {
        m_OnChanged += _onClick;
    }
    #endregion
}
