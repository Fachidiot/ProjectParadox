using UnityEngine;

/// <summary>클릭 시 부모 팝업을 닫는 버튼</summary>
[RequireComponent(typeof(Control_Button))]
public class Control_Button_PopupClose : ControlBase
{
    #region Value
    private Control_Button m_Button;
    #endregion

    #region Event
    protected override void Awake()
    {
        base.Awake();
        m_Button = GetComponent<Control_Button>();
        m_Button.AddClickListener(OnClick);
    }
    /// <summary>클릭 시 부모 팝업을 PopupManager를 통해 닫기</summary>
    private void OnClick(Control_Button _)
    {
        var popup = GetComponentInParent<PopupBase>();
        LocalPopupManager.instance.Close(popup.name);
    }
    #endregion
}
