using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>아이템 보유 상태 관리 매니저, SlotSave 연동</summary>
public class ItemManager : GlobalManagerBase
{
    public static ItemManager instance { get; private set; }

    #if UNITY_EDITOR
    #region Preview
    [Serializable] private struct SPreview
    {
        public string id;
        public int count;
        public string time;
        public SPreview(string _id, int _count, string _time)
        {
            id = _id;
            count = _count;
            time = _time;
        }
    }
    [SerializeField, Sirenix.OdinInspector.ReadOnly] private List<SPreview> m_Preview = new();
    #endregion
    #endif
    #region Get,Set
    /// <summary>보유 아이템 데이터 맵 (count + 획득 시각)</summary>
    public IReadOnlyDictionary<string, StructValue<SItem>> Items => m_Items;
    public int TotalCount => m_TotalCount.v;
    public int OwnedCount => m_OwnedCount.v;
    public IntValue TotalCountValue => m_TotalCount;
    public IntValue OwnedCountValue => m_OwnedCount;
    #endregion
    #region Value
    private Dictionary<string, StructValue<SItem>> m_Items = new();
    private IntValue m_TotalCount;
    private IntValue m_OwnedCount;
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }

    public override bool RequireInit()
    {
        return InitUtil.IsInit(new ManagerBase[] { TableManager.instance });
    }

    public override void Init()
    {
        foreach (var id in TableManager.instance.Item.ID)
        {
            var value = new StructValue<SItem>(this, $"Item.{id}", default);
            SlotSaveManager.instance.Create(this, value);
            #if UNITY_EDITOR
            value.AddChanged(this, _ => RefreshPreview());
            #endif
            m_Items.Add(id, value);
        }

        m_TotalCount = new IntValue(this, "Item.TotalCount", 0);
        m_TotalCount.Set(m_Items.Count, true, false);
        m_OwnedCount = new IntValue(this, "Item.OwnedCount", 0);

        foreach (var kv in m_Items)
            kv.Value.AddChanged(this, _ => RefreshOwnedCount());

        LanguageManager.instance.Create(this, "Text_ItemCount", m_OwnedCount, (_, t) =>
        {
            t.Clear().SetEng($"{m_OwnedCount.v}/{m_TotalCount.v}");
        });
        RefreshOwnedCount();

        DealManager.instance.Create(this, "Item",
            (_deal, _seed) =>
            {   //Need
                return _deal.CountLong <= GetCount(_deal.Key);
            },
            (_deal) =>
            {   //NeedValue
                return m_Items.TryGetValue(_deal.Key, out var v) ? new ValueBase[] { v } : Array.Empty<ValueBase>();
            },
            (_deal, _seed) =>
            {   //Set
                SetCount(_deal.Key, (int)_deal.CountLong);
                return new SDeal[] { _deal };
            },
            (_deal, _seed) =>
            {   //Change
                Add(_deal.Key, (int)_deal.CountLong);
                return new SDeal[] { _deal };
            },
            (_deal, _seed) =>
            {   //Pay
                Add(_deal.Key, (int)_deal.CountLong);
                return new SDeal[] { _deal };
            }
        );

        base.Init();
        #if UNITY_EDITOR
        RefreshPreview();
        #endif
    }
    #endregion

    #region Function
    public int GetCount(string _id) => m_Items.TryGetValue(_id, out var v) ? v.v.count : 0;
    public bool Has(string _id) => GetCount(_id) > 0;
    public DateTime GetAcquiredTime(string _id) => m_Items.TryGetValue(_id, out var v) ? v.v.AcquiredTime : DateTime.MinValue;

    public void Add(string _id, int _count = 1)
    {
        if (!m_Items.TryGetValue(_id, out var v)) return;
        var s = v.v;
        s.count += _count;
        if (_count > 0)
            s.acquiredTicks = DateTime.UtcNow.Ticks;
        v.v = s;
    }

    public bool Remove(string _id, int _count = 1)
    {
        if (!m_Items.TryGetValue(_id, out var v)) return false;
        if (v.v.count < _count) return false;

        var s = v.v;
        s.count -= _count;
        v.v = s;
        return true;
    }

    public void SetCount(string _id, int _count)
    {
        if (!m_Items.TryGetValue(_id, out var v)) return;
        var s = v.v;
        int prev = s.count;
        s.count = _count;
        if (_count > prev)
            s.acquiredTicks = DateTime.UtcNow.Ticks;
        v.v = s;
    }

    private void RefreshOwnedCount()
    {
        int count = 0;
        foreach (var kv in m_Items)
            if (kv.Value.v.count > 0) count++;
        m_OwnedCount.v = count;
    }
    #endregion
    #if UNITY_EDITOR
    #region Editor Function
    private void RefreshPreview()
    {
        m_Preview.Clear();
        foreach (var kv in m_Items)
        {
            var s = kv.Value.v;
            m_Preview.Add(new SPreview(kv.Key, s.count, s.count > 0 ? s.AcquiredTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") : ""));
        }
    }
    #endregion
    #endif
}
