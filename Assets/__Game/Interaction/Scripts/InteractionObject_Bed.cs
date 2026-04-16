using UnityEngine;

/// <summary>침대 상호작용, 칼 해금 상태에 따라 씬 전환</summary>
public class InteractionObject_Bed : InteractionObject
{
    #region Get,Set
    public override Table_Text.Data Name => null;
    public override Sprite Icon => null;
    #endregion

    #region Function
    public override void OnInteract(PlayerActor _actor)
    {
        if (ConditionManager.instance.IsSwordUnlocked.v)
        {
            Popup_Choice.instance.Open(new Popup_Choice.SOption(
                "Are you sleep again?..",
                () => SceneChangeHelper.Change("FallingDream", "You are falling into a deep sleep again..."),
                () => Debug.Log("Player chose not to sleep.")));
        }
        else if (ConditionManager.instance.HasCheckedSword.v)
        {
            SceneChangeHelper.Change("FallingDream", "You are falling into a deep sleep...");
        }
        else
        {
            Popup_NotifyUI.instance.Open(new Popup_NotifyUI.SOption("Not sleepy yet"));
        }
    }
    #endregion
}
