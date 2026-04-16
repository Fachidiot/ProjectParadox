using UnityEngine;

/// <summary>프로필 설정 팝업</summary>
public class Popup_CustomProfile : PopupBase
{
    public static Popup_CustomProfile instance { get; private set; }

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }
    #endregion
}
