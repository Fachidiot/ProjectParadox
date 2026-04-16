using UnityEngine;

/// <summary>상호작용 시 현재 진행상황을 SlotSave로 파일 저장하는 오브젝트</summary>
public class SlotSaveObject : InteractionObject
{
    #region Inspector
    [SerializeField] private Transform m_SavePoint;
    #endregion

    #region Get,Set
    public override Table_Text.Data Name
    {
        get
        {
            if (TableManager.instance.TryGet<object>("Text_Save_Name", out var table) && table is Table_Text.Data text)
                return text;
            return null;
        }
    }
    public override Sprite Icon => IconManager.instance.Get("Save");
    #endregion

    #region Function
    public override void OnInteract(PlayerActor _actor)
    {
        if (m_SavePoint != null)
            _actor.transform.position = m_SavePoint.position;

        SlotSaveManager.instance.Save();
    }
    #endregion
}
