using UnityEngine;

public class Popup_Setting : PopupBase
{
    public static Popup_Setting instance { get; private set; }

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }
    #endregion
}
