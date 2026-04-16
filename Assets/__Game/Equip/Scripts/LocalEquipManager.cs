using System.Collections.Generic;
using UnityEngine;

/// <summary>장착 장비 비주얼 관리 매니저, EquipManager 변경에 따라 프리팹 생성/삭제</summary>
public class LocalEquipManager : LocalManagerBase
{
    public static LocalEquipManager instance { get; private set; }

    #region Value
    private Dictionary<Table_Item.EKind, GameObject[]> m_SpawnedEquips = new();
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }

    public override bool RequireInit()
    {
        return InitUtil.IsInit(new ManagerBase[] { LocalPlayerActorManager.instance });
    }

    public override void Init()
    {
        foreach (var kind in EquipManager.SLOT)
        {
            var k = kind;
            var value = EquipManager.instance.GetValue(k);
            if (value != null)
                value.AddChanged(this, _ => RefreshEquip(k), true);
        }

        base.Init();
    }
    #endregion

    #region Function
    private void RefreshEquip(Table_Item.EKind _kind)
    {
        if (m_SpawnedEquips.TryGetValue(_kind, out var old))
        {
            foreach (var go in old)
                if (go != null) Destroy(go);
            m_SpawnedEquips.Remove(_kind);
        }

        string itemId = EquipManager.instance.GetEquipped(_kind);
        if (string.IsNullOrEmpty(itemId)) return;

        var table = TableManager.instance.Get<Table_Item.Data>(itemId);
        if (table == null) return;

        var actor = LocalPlayerActorManager.instance.CurActor;

        if (_kind == Table_Item.EKind.Shoes)
        {
            GameObject leftPrefab = null, rightPrefab = null;
            foreach (var p in table.EquipPrefabs)
            {
                if (p == null) continue;
                if (p.name.EndsWith("_Left")) leftPrefab = p;
                else if (p.name.EndsWith("_Right")) rightPrefab = p;
            }
            var goLeft = leftPrefab != null ? Instantiate(leftPrefab, actor.SlotShoesLeft) : null;
            var goRight = rightPrefab != null ? Instantiate(rightPrefab, actor.SlotShoesRight) : null;
            m_SpawnedEquips[_kind] = new[] { goLeft, goRight };
        }
        else
        {
            var slot = actor.GetSlot(_kind);
            if (slot == null || table.EquipPrefabs.Count == 0) return;

            var go = Instantiate(table.EquipPrefabs[0], slot);
            m_SpawnedEquips[_kind] = new[] { go };
        }
    }
    #endregion
}
