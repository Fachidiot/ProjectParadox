using UnityEngine;

/// <summary>앱 종료 확인 팝업</summary>
public class Popup_Quit : PopupBase
{
    public static Popup_Quit instance { get; private set; }

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }
    #endregion
}
