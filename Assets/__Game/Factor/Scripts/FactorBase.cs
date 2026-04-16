using System;
using System.Collections.Generic;

/// <summary>다중 효과 합산 시스템 베이스, Key별 값 저장 후 Total로 집계</summary>
public class FactorBase<TKey, TValue> : ValueBase
{
    #region Get,Set
    /// <summary>현재 데이터의 최종 집계값</summary>
    public TValue Total { get => m_OnTotalFunc(m_Global, m_Local); }
    #endregion
    #region Value
    protected Dictionary<TKey, TValue> m_Local = new();
    protected Dictionary<TKey, TValue> m_Global = new();
    protected Func<Dictionary<TKey, TValue>, Dictionary<TKey, TValue>, TValue> m_OnTotalFunc;
    #endregion

    #region Event
    public FactorBase(IManageValue _callBy, Func<Dictionary<TKey, TValue>, Dictionary<TKey, TValue>, TValue> _totalFunc) : base(_callBy, null)
    {
        m_OnTotalFunc = _totalFunc;
    }

    public override void OnResetLocalChanged()
    {
        base.OnResetLocalChanged();
        m_Local.Clear();
    }
    #endregion
    #region Function
    /// <summary>효과 데이터 저장 (Global/Local 자동 분류)</summary>
    public void Set(object _callBy, TKey id, TValue factor = default)
    {
        if (_callBy as GlobalManagerBase)
            m_Global.Set(id, factor);
        else
            m_Local.Set(id, factor);

        OnChanged(EChangeType.None);
    }
    /// <summary>해당 ID의 효과 데이터 존재 여부</summary>
    public bool GetContains(TKey id)
    {
        return m_Global.ContainsKey(id);
    }
    /// <summary>해당 ID의 효과 데이터 조회</summary>
    public TValue Get(TKey id)
    {
        if (m_Global.TryGetValue(id, out var v))
            return v;
        else
            return default;
    }
    /// <summary>해당 ID의 효과 데이터 제거</summary>
    public void Remove(TKey _id)
    {
        if (m_Global.Remove(_id))
            OnChanged(EChangeType.None);
        else if (m_Local.Remove(_id))
            OnChanged(EChangeType.None);
    }
    #endregion
}
