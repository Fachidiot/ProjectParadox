using UnityEngine;

public class GameManager : LocalManagerBase
{
    public static GameManager instance { get; private set; }

    #region Event
    public override void InitSingleton()
    {
        base.InitSingleton();
        instance = this;
    }
    #endregion
}
