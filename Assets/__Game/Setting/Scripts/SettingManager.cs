using UnityEngine;

/// <summary>게임 설정 매니저, UI 표시 옵션 등 관리</summary>
public class SettingManager : GlobalManagerBase
{
    public static SettingManager instance { get; private set; }

    #region Get,Set
    /// <summary>ActionUI 사용 여부</summary>
    public BoolValue UseActionUI { get; private set; }
    /// <summary>InteractUI 사용 여부</summary>
    public BoolValue UseInteractUI { get; private set; }
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }

    public override void InitFirst()
    {
        UseActionUI = PlayerPrefsSaveManager.instance.Create(this, new BoolValue(this, "Setting.UseActionUI", true));
        UseInteractUI = PlayerPrefsSaveManager.instance.Create(this, new BoolValue(this, "Setting.UseInteractUI", false));
        base.InitFirst();
    }
    #endregion
}
