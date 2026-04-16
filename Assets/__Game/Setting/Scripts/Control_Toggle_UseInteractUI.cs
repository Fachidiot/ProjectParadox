using UnityEngine;

/// <summary>InteractUI 사용 여부 토글</summary>
[RequireComponent(typeof(Control_Toggle))]
public class Control_Toggle_UseInteractUI : ControlBase
{
    #region Value
    private Control_Toggle m_Toggle;
    #endregion

    #region Event
    public override void AfterInit()
    {
        m_Toggle = GetComponent<Control_Toggle>();
        m_Toggle.AddValueChangedListener(OnValueChanged);
        base.AfterInit();
    }

    public override void AfterInitGame()
    {
        m_Toggle.SetIsOn(SettingManager.instance.UseInteractUI.v);
        base.AfterInitGame();
    }

    public override void OnOpen()
    {
        m_Toggle.SetIsOn(SettingManager.instance.UseInteractUI.v);
        base.OnOpen();
    }
    #endregion

    #region Function
    private void OnValueChanged(Control_Toggle _, bool _isOn)
    {
        SettingManager.instance.UseInteractUI.v = _isOn;
    }
    #endregion
}
