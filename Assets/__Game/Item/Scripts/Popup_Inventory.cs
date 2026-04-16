using UnityEngine;

public class Popup_Inventory : PopupBase
{
    public static Popup_Inventory instance { get; private set; }

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }
    #endregion

}
