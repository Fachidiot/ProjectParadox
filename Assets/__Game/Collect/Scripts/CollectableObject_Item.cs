/// <summary>일반 아이템 수집 오브젝트, 상호작용 시 획득 + 팝업 표시</summary>
public class CollectableObject_Item : CollectableObject
{
    public override void OnInteract(PlayerActor _actor)
    {
        base.OnInteract(_actor);
        Popup_ItemGet.instance.Open(new Popup_ItemGet.SOption(ItemID));
        SetInteractable(false);
    }
}
