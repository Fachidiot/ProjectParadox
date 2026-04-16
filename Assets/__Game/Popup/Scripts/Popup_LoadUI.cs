using UnityEngine;

/// <summary>·Îºñ UI ÆË¾÷</summary>
public class Popup_LoadUI : PopupBase
{
    public static Popup_LoadUI instance { get; private set; }

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }
    #endregion
}
