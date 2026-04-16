using UnityEngine;

/// <summary>제작 테이블 상호작용</summary>
public class InteractionObject_Craft : InteractionObject
{
    #region Get,Set
    public override Table_Text.Data Name => null;
    public override Sprite Icon => null;
    #endregion

    #region Function
    public override void OnInteract(PlayerActor _actor)
    {
        Debug.Log("제작 테이블 상호작용");
    }
    #endregion
}
