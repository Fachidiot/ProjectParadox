using LitJson;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>장비 장착 상태 관리 매니저, SlotSave 연동</summary>
public class EquipManager : GlobalManagerBase
{
    public static EquipManager instance { get; private set; }

    #if UNITY_EDITOR
    #region Preview
    [Serializable] private struct SPreviewEquip
    {
        public string slot;
        public string itemId;
        public SPreviewEquip(string _slot, string _itemId)
        {
            slot = _slot;
            itemId = _itemId;
        }
    }
    [Serializable] private struct SPreviewSpecial
    {
        public string key;
        public bool active;
        public SPreviewSpecial(string _key, bool _active)
        {
            key = _key;
            active = _active;
        }
    }
    [SerializeField, Sirenix.OdinInspector.ReadOnly] private List<SPreviewEquip> m_PreviewEquip = new();
    [SerializeField, Sirenix.OdinInspector.ReadOnly] private List<SPreviewSpecial> m_PreviewSpecial = new();
    #endregion
    #endif
    #region Get,Set
    /// <summary>장착 슬롯별 StringValue 맵</summary>
    public IReadOnlyDictionary<Table_Item.EKind, StringValue> Equips => m_Equips;
    /// <summary>Special BoolValue 맵</summary>
    public IReadOnlyDictionary<string, BoolValue> Specials => m_Specials;
    #endregion
    #region Const
    public static readonly Table_Item.EKind[] SLOT = { Table_Item.EKind.Shoes, Table_Item.EKind.Sword, Table_Item.EKind.Hat, Table_Item.EKind.Wing };
    #endregion
    #region Value
    private Dictionary<Table_Item.EKind, StringValue> m_Equips = new();
    private Dictionary<string, BoolValue> m_Specials = new();
    private Dictionary<Table_Item.EKind, List<string>> m_SlotItems = new();
    private Dictionary<Table_Item.EKind, List<(StructValue<SItem> value, Action<ValueBase> callback)>> m_AutoEquipListeners = new();
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }

    public override bool RequireInit()
    {
        return InitUtil.IsInit(new ManagerBase[] { TableManager.instance, ItemManager.instance });
    }

    public override void Init()
    {
        // 모든 아이템 Special에서 키 수집, BoolValue 생성
        foreach (var id in TableManager.instance.Item.ID)
        {
            var table = TableManager.instance.Get<Table_Item.Data>(id);
            if (table == null) continue;
            foreach (var specialJson in table.Special)
            {
                var data = JsonMapper.ToObject(specialJson);
                foreach (string key in data.Keys)
                {
                    if (!m_Specials.ContainsKey(key))
                        m_Specials.Add(key, new BoolValue(this, $"Equip.Special.{key}", false));
                }
            }
        }

        // 장착 슬롯 생성
        foreach (var kind in SLOT)
        {
            var value = new StringValue(this, $"Equip.{kind}", "");
            SlotSaveManager.instance.Create(this, value);
            value.AddChanged(this, _ => OnEquipChanged());
            m_Equips.Add(kind, value);
        }

        // 슬롯별 장착 가능 아이템 매핑
        foreach (var id in TableManager.instance.Item.ID)
        {
            var table = TableManager.instance.Get<Table_Item.Data>(id);
            if (table == null || table.Kind == Table_Item.EKind.Item) continue;
            if (!m_SlotItems.TryGetValue(table.Kind, out var list))
            {
                list = new List<string>();
                m_SlotItems.Add(table.Kind, list);
            }
            list.Add(id);
        }

        // 빈 슬롯 자동장착 이벤트 구독
        foreach (var kind in SLOT)
        {
            if (!IsEquipped(kind))
                SubscribeSlot(kind);
        }

        base.Init();
        #if UNITY_EDITOR
        RefreshPreview();
        #endif
    }
    #endregion

    #region Function
    /// <summary>슬롯의 StringValue 반환</summary>
    public StringValue GetValue(Table_Item.EKind _kind) => m_Equips.TryGetValue(_kind, out var v) ? v : null;

    /// <summary>슬롯에 장착된 아이템 ID 반환</summary>
    public string GetEquipped(Table_Item.EKind _kind) => m_Equips.TryGetValue(_kind, out var v) ? v.v : "";

    /// <summary>슬롯에 장착 여부</summary>
    public bool IsEquipped(Table_Item.EKind _kind) => !string.IsNullOrEmpty(GetEquipped(_kind));

    /// <summary>Special BoolValue 조회</summary>
    public BoolValue GetSpecial(string _key) => m_Specials.TryGetValue(_key, out var v) ? v : null;

    /// <summary>Special 활성화 여부</summary>
    public bool HasSpecial(string _key) => m_Specials.TryGetValue(_key, out var v) && v.v;

    /// <summary>아이템 장착, Kind를 테이블에서 조회하여 해당 슬롯에 설정</summary>
    public void Equip(string _itemId)
    {
        var table = TableManager.instance.Get<Table_Item.Data>(_itemId);
        if (table == null) return;
        if (m_Equips.TryGetValue(table.Kind, out var v))
            v.v = _itemId;
    }

    /// <summary>슬롯 장착 해제</summary>
    public void Unequip(Table_Item.EKind _kind)
    {
        if (m_Equips.TryGetValue(_kind, out var v))
            v.v = "";
    }

    private void OnEquipChanged()
    {
        RefreshSpecials();

        foreach (var kind in SLOT)
        {
            if (IsEquipped(kind))
                UnsubscribeSlot(kind);
            else
                SubscribeSlot(kind);
        }

        #if UNITY_EDITOR
        RefreshPreview();
        #endif
    }

    private void SubscribeSlot(Table_Item.EKind _kind)
    {
        if (m_AutoEquipListeners.ContainsKey(_kind)) return;
        if (!m_SlotItems.TryGetValue(_kind, out var items)) return;

        var listeners = new List<(StructValue<SItem> value, Action<ValueBase> callback)>();
        foreach (var id in items)
        {
            if (ItemManager.instance.Items.TryGetValue(id, out var value))
            {
                var itemId = id;
                Action<ValueBase> cb = _ =>
                {
                    if (value.v.count > 0 && !IsEquipped(_kind))
                        Equip(itemId);
                };
                value.AddChanged(this, cb);
                listeners.Add((value, cb));
            }
        }
        m_AutoEquipListeners[_kind] = listeners;
    }

    private void UnsubscribeSlot(Table_Item.EKind _kind)
    {
        if (!m_AutoEquipListeners.TryGetValue(_kind, out var listeners)) return;
        foreach (var (value, cb) in listeners)
            value.RemoveChanged(this, cb);
        m_AutoEquipListeners.Remove(_kind);
    }

    /// <summary>현재 장착된 아이템들의 Special을 기반으로 BoolValue 갱신</summary>
    private void RefreshSpecials()
    {
        // 전부 끄기
        foreach (var kv in m_Specials)
            kv.Value.Set(false, false, false);

        // 장착된 아이템별 Special 켜기
        foreach (var kind in SLOT)
        {
            string itemId = GetEquipped(kind);
            if (string.IsNullOrEmpty(itemId)) continue;

            var table = TableManager.instance.Get<Table_Item.Data>(itemId);
            if (table == null) continue;

            foreach (var specialJson in table.Special)
            {
                var data = JsonMapper.ToObject(specialJson);
                foreach (string key in data.Keys)
                {
                    if (data[key].IsBoolean && (bool)data[key] && m_Specials.TryGetValue(key, out var bv))
                        bv.v = true;
                }
            }
        }
    }
    #endregion
    #if UNITY_EDITOR
    #region Editor Function
    private void RefreshPreview()
    {
        m_PreviewEquip.Clear();
        foreach (var kind in SLOT)
            m_PreviewEquip.Add(new SPreviewEquip(kind.ToString(), GetEquipped(kind)));

        m_PreviewSpecial.Clear();
        foreach (var kv in m_Specials)
            m_PreviewSpecial.Add(new SPreviewSpecial(kv.Key, kv.Value.v));
    }
    #endregion
    #endif
}
