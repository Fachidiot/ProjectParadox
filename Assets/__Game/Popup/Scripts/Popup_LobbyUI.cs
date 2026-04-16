using UnityEngine;

/// <summary>로비 UI 팝업</summary>
public class Popup_LobbyUI : PopupBase
{
    public static Popup_LobbyUI instance { get; private set; }

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }
    #endregion
}
