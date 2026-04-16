using UnityEngine;

/// <summary>게임 기본 UI 팝업 (상단바, 프로필, 자원 등)</summary>
public class Popup_CustomGameBaseUI : PopupBase
{
    public static Popup_CustomGameBaseUI instance { get; private set; }

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }
    #endregion
}
