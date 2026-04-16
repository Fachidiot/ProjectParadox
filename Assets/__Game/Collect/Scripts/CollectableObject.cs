using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>수집 가능한 오브젝트, Interaction 시 Deal Set으로 획득</summary>
public class CollectableObject : InteractionObject
{
    #region Inspector
    [SerializeField, LabelText("아이템 ID")] private string m_ItemID;
    #endregion
    #region Get,Set
    /// <summary>아이템 ID</summary>
    public string ItemID => m_ItemID;
    public override Table_Text.Data Name
    {
        get
        {
            if (TableManager.instance.TryGet<object>(m_ItemID, out var table) && table is ITableType iTable)
                return iTable["Name"] as Table_Text.Data;
            return null;
        }
    }
    public override Sprite Icon => IconManager.instance.Get(m_ItemID);
    #endregion

    #region Event
    public override void OnRegister()
    {
        base.OnRegister();
        LocalCollectManager.instance.Register(this);
    }
    public override void OnInteract(PlayerActor _actor)
    {
        DealManager.instance.Set(new SDeal(m_ItemID, "", 1));
    }
    #endregion
}
