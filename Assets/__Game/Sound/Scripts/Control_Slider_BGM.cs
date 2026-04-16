using UnityEngine;

/// <summary>BGM 볼륨 슬라이더</summary>
[RequireComponent(typeof(Control_Slider))]
public class Control_Slider_BGM : ControlBase
{
    #region Value
    private Control_Slider m_Slider;
    #endregion

    #region Event
    public override void AfterInit()
    {
        m_Slider = GetComponent<Control_Slider>();
        m_Slider.AddChangeListener(OnValueChanged);
        base.AfterInit();
    }

    public override void AfterInitGame()
    {
        m_Slider.Set(SoundManager.instance.BGMVolume.v);
        base.AfterInitGame();
    }

    public override void OnOpen()
    {
        m_Slider.Set(SoundManager.instance.BGMVolume.v);
        base.OnOpen();
    }
    #endregion

    #region Function
    private void OnValueChanged(Control_Slider _)
    {
        SoundManager.instance.SetBGMVolume(m_Slider.v.value);
    }
    #endregion
}
