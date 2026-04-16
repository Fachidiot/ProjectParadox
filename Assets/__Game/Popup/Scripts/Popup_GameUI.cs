using UnityEngine;

/// <summary>·Îºñ UI ÆË¾÷</summary>
public class Popup_GameUI : PopupBase
{
    public static Popup_GameUI instance { get; private set; }

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }
    #endregion
}
