using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>아이템 획득 시 reactive하게 튜토리얼 팝업 표시 후 즉시 클리어 처리</summary>
public class Tutorial_ItemAcquire : TutorialBase
{
    #region Inspector
    [SerializeField, LabelText("아이템 ID")] private string m_ItemID;
    #endregion

    #region Event
    public override void Init()
    {
        base.Init();

        if (ItemManager.instance != null && ItemManager.instance.Items.TryGetValue(m_ItemID, out var itemValue))
        {
            itemValue.AddChanged(this, _ => OnItemChanged());
        }
    }

    /// <summary>아이템 값 변경 시 reactive 호출</summary>
    private void OnItemChanged()
    {
        if (!ItemManager.instance.Has(m_ItemID)) return;

        RequestActivate();
        RequestMarkCleared();
    }

    /// <summary>polling 불필요 - reactive로 동작</summary>
    public override void OnUpdate() { }
    #endregion

    #region Override
    protected override bool CheckTrigger() => false;
    protected override bool CheckComplete() => true;
    #endregion
}
