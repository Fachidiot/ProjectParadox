using UnityEngine;

/// <summary>칼 수집 오브젝트, 해금 조건 충족 시에만 획득 가능</summary>
public class CollectableObject_Sword : CollectableObject
{
    public override void OnInteract(PlayerActor _actor)
    {
        if (!ConditionManager.instance.IsSwordUnlocked.v)
        {
            Popup_NotifyUI.instance.Open(new Popup_NotifyUI.SOption("Locked: Try getting some sleep first"));
            ConditionManager.instance.SetCheckedSword(true);
            return;
        }

        base.OnInteract(_actor);
        SetInteractable(false);
    }
}
