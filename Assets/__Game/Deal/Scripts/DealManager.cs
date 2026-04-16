using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>거래 시스템 매니저, Need/Set/Change/Pay 로직 위임 및 Key/Table 기반 매핑</summary>
public class DealManager : GlobalManagerBase
{
    public static DealManager instance { get; private set; }

    #if UNITY_EDITOR
    #region Preview
    [Serializable] private struct SPreview
    {
        public string id_table;
        public string by;
        public bool need;
        public bool needValue;
        public bool set;
        public bool change;
        public bool pay;
        public SPreview(string _id_table, string _by, bool _need, bool _needValue, bool _set, bool _change, bool _pay)
        {
            id_table = _id_table;
            by = _by;
            need = _need;
            needValue = _needValue;
            set = _set;
            change = _change;
            pay = _pay;
        }
    }
    [SerializeField, ReadOnly] private List<SPreview> m_Preview = new();
    #endregion
    #endif
    #region Value
    private Dictionary<string, Func<SDeal, int, bool>> m_OnNeed = new();
    private Dictionary<string, Func<SDeal, ValueBase[]>> m_OnNeedValue = new();
    private Dictionary<string, Func<SDeal, int, SDeal[]>> m_OnSet = new();
    private Dictionary<string, Func<SDeal, int, SDeal[]>> m_OnChange = new();
    private Dictionary<string, Func<SDeal, int, SDeal[]>> m_OnPay = new();
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }
    #endregion
    #region Function
    /// <summary>Key/Table ID와 거래 처리 함수 등록, Init에서만 호출 가능</summary>
    public void Create(GlobalManagerBase _callBy, string _id_table, Func<SDeal, int, bool> _onNeed, Func<SDeal, ValueBase[]> _onNeedValue, Func<SDeal, int, SDeal[]> _onSet, Func<SDeal, int, SDeal[]> _onChange, Func<SDeal, int, SDeal[]> _onPay)
    {
        if (_callBy == null)
            throw new ArgumentNullException();
        if (_callBy.IsInited)
            throw new InvalidOperationException();

        m_OnNeed.Add(_id_table, _onNeed);
        m_OnNeedValue.Add(_id_table, _onNeedValue);
        m_OnSet.Add(_id_table, _onSet);
        m_OnChange.Add(_id_table, _onChange);
        m_OnPay.Add(_id_table, _onPay);

        #if UNITY_EDITOR
        m_Preview.Add(new SPreview(_id_table, _callBy.gameObject.name, _onNeed != null, _onNeedValue != null, _onSet != null, _onChange != null, _onPay != null));
        #endif
    }

    /// <summary>거래 조건 충족 여부 확인</summary>
    public bool Need(SDeal _deal, int _seed)
    {
        Func<SDeal, int, bool> func = null;
        if (TableManager.instance.TryGet<object>(_deal.Key, out var table) && table is ITableType iTable)
            m_OnNeed.TryGetValue(iTable.Table, out func);
        if (func == null)
            m_OnNeed.TryGetValue(_deal.Key, out func);
        if (func == null)
            throw new ArgumentException();

        var result = func.Invoke(_deal, _seed);

        if (_deal.Action.Contains("Not"))
            result = !result;

        return result;
    }
    /// <summary>거래 조건 충족 여부 확인 (랜덤 시드 자동)</summary>
    public bool Need(SDeal _deal)
    {
        return Need(_deal, UnityEngine.Random.Range(int.MinValue, int.MaxValue));
    }
    /// <summary>복수 거래 조건 전체 충족 여부 확인</summary>
    public bool NeedAll(SDeal[] _deals, int _seed)
    {
        bool result = true;
        for (int i = 0; i < _deals.Length; i++)
        {
            result = result && Need(_deals[i], _seed + i);
            if (!result)
                break;
        }

        return result;
    }
    /// <summary>복수 거래 조건 전체 충족 여부 확인 (랜덤 시드 자동)</summary>
    public bool NeedAll(SDeal[] _deals)
    {
        return NeedAll(_deals, UnityEngine.Random.Range(int.MinValue, int.MaxValue));
    }

    /// <summary>거래 조건 충족 여부 감지용 ValueBase 배열 반환</summary>
    public ValueBase[] NeedValue(SDeal _deal)
    {
        if (!m_OnNeedValue.TryGetValue(_deal.Key, out var func) || func == null)
            throw new ArgumentException();

        var result = func.Invoke(_deal);

        return result;
    }
    /// <summary>복수 거래 조건 충족 여부 감지용 ValueBase 리스트 반환</summary>
    public List<ValueBase> NeedAllValue(SDeal[] _deals)
    {
        var result = new List<ValueBase>();
        foreach (var v in _deals)
            if (m_OnNeedValue.TryGetValue(v.Key, out var func) && func != null)
                result.AddRange(func.Invoke(v));

        return result;
    }

    /// <summary>거래 값 설정 후 실제 설정된 거래 반환</summary>
    public SDeal[] Set(SDeal _deal, int _seed)
    {
        Func<SDeal, int, SDeal[]> func = null;
        if (TableManager.instance.TryGet<object>(_deal.Key, out var table) && table is ITableType iTable)
            m_OnSet.TryGetValue(iTable.Table, out func);
        if (func == null)
            m_OnSet.TryGetValue(_deal.Key, out func);
        if (func == null)
            throw new ArgumentException();
        var result = func.Invoke(_deal, _seed);
        return result;
    }
    /// <summary>거래 값 설정 후 실제 설정된 거래 반환 (랜덤 시드 자동)</summary>
    public SDeal[] Set(SDeal _deal)
    {
        return Set(_deal, UnityEngine.Random.Range(int.MinValue, int.MaxValue));
    }
    /// <summary>복수 거래 값 설정 후 실제 설정된 거래 배열 반환</summary>
    public SDeal[] SetAll(SDeal[] _deals, int _seed)
    {
        var result = new List<SDeal>();
        for (int i = 0; i < _deals.Length; i++)
            result.AddRange(Set(_deals[i], _seed + i));

        return result.ToArray();
    }
    /// <summary>복수 거래 값 설정 후 실제 설정된 거래 배열 반환 (랜덤 시드 자동)</summary>
    public SDeal[] SetAll(SDeal[] _deals)
    {
        return SetAll(_deals, UnityEngine.Random.Range(int.MinValue, int.MaxValue));
    }

    /// <summary>거래량만큼 변경 후 실제 변동된 거래 반환</summary>
    public SDeal[] Change(SDeal _deal, int _seed)
    {
        Func<SDeal, int, SDeal[]> func = null;
        if (TableManager.instance.TryGet<object>(_deal.Key, out var table) && table is ITableType iTable)
            m_OnChange.TryGetValue(iTable.Table, out func);
        if (func == null)
            m_OnChange.TryGetValue(_deal.Key, out func);
        if (func == null)
            throw new ArgumentException();

        var result = func.Invoke(_deal, _seed);

        return result;
    }
    /// <summary>거래량만큼 변경 후 실제 변동된 거래 반환 (랜덤 시드 자동)</summary>
    public SDeal[] Change(SDeal _deal)
    {
        return Change(_deal, UnityEngine.Random.Range(int.MinValue, int.MaxValue));
    }
    /// <summary>복수 거래량만큼 변경 후 실제 변동된 거래 배열 반환</summary>
    public SDeal[] ChangeAll(SDeal[] _deals, int _seed)
    {
        var result = new List<SDeal>();
        for (int i = 0; i < _deals.Length; i++)
            result.AddRange(Change(_deals[i], _seed + i));

        return result.ToArray();
    }
    /// <summary>복수 거래량만큼 변경 후 실제 변동된 거래 배열 반환 (랜덤 시드 자동)</summary>
    public SDeal[] ChangeAll(SDeal[] _deals)
    {
        return ChangeAll(_deals, UnityEngine.Random.Range(int.MinValue, int.MaxValue));
    }

    /// <summary>거래량만큼 지불 후 실제 지불 거래 반환, 실패시 null</summary>
    public SDeal[] Pay(SDeal _deal, int _seed)
    {
        if (!Need(_deal, _seed))
            return null;

        Func<SDeal, int, SDeal[]> func = null;
        if (TableManager.instance.TryGet<object>(_deal.Key, out var table) && table is ITableType iTable)
            m_OnPay.TryGetValue(iTable.Table, out func);
        if (func == null)
            m_OnPay.TryGetValue(_deal.Key, out func);
        if (func == null)
            throw new ArgumentException();

        var result = func.Invoke(_deal, _seed);

        return result;
    }
    /// <summary>거래량만큼 지불 후 실제 지불 거래 반환, 실패시 null (랜덤 시드 자동)</summary>
    public SDeal[] Pay(SDeal _deal)
    {
        return Pay(_deal, UnityEngine.Random.Range(int.MinValue, int.MaxValue));
    }
    /// <summary>복수 거래량만큼 지불 후 실제 지불 거래 배열 반환, 실패시 null</summary>
    public SDeal[][] PayAll(SDeal[] _deals, int _seed)
    {
        if (!NeedAll(_deals, _seed))
            return null;

        var result = new SDeal[_deals.Length][];
        for (int i = 0; i < _deals.Length; i++)
            result[i] = Pay(_deals[i], _seed + i);

        return result;
    }
    /// <summary>복수 거래량만큼 지불 후 실제 지불 거래 배열 반환, 실패시 null (랜덤 시드 자동)</summary>
    public SDeal[][] PayAll(SDeal[] _deals)
    {
        return PayAll(_deals, UnityEngine.Random.Range(int.MinValue, int.MaxValue));
    }
    #endregion
}
