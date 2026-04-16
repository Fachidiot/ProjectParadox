using UnityEngine;

/// <summary>설정 팝업</summary>
public class Popup_CustomSetting : PopupBase
{
    public static Popup_CustomSetting instance { get; private set; }

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }
    #endregion
}
