using UnityEngine;

/// <summary>게임 커스텀 메뉴 팝업</summary>
public class Popup_CustomMenu : PopupBase
{
    public static Popup_CustomMenu instance { get; private set; }

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }
    #endregion
}
