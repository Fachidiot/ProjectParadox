using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Image의 fillAmount를 사용하여 게이지를 표시하는 UI 컨트롤</summary>
[RequireComponent(typeof(Image))]
public class Control_Guage : ControlBase
{
    #region Property
    /// <summary>래핑된 Image 컴포넌트</summary>
    public Image v { get; private set; }
    #endregion

    #region Event
    protected override void Awake()
    {
        base.Awake();
        v = GetComponent<Image>();
    }
    #endregion
    #region Function
    /// <summary>게이지 값을 0~1 범위로 설정</summary>
    public void Set(float _value)
    {
        v.fillAmount = Mathf.Clamp01(_value);
    }
    #endregion
}
