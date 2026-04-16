using System.Collections.Generic;

/// <summary>삽입 순서 보장 BoolFactor, First 집계 지원 및 키 목록 조회</summary>
public class OrderedBoolFactor<TKey> : FactorBase<TKey, bool>, IReadOnlyOrderedBoolFactor<TKey>
{
    #region Get,Set
    /// <summary>Global 키 목록 (삽입 순서)</summary>
    public IReadOnlyList<TKey> GlobalKeys => m_GlobalOrder;
    /// <summary>Local 키 목록 (삽입 순서)</summary>
    public IReadOnlyList<TKey> LocalKeys => m_LocalOrder;
    #endregion
    #region Value
    private List<TKey> m_GlobalOrder = new();
    private List<TKey> m_LocalOrder = new();
    #endregion

    #region Event
    public OrderedBoolFactor(IManageValue _callBy) : base(_callBy, null)
    {
        m_OnTotalFunc = (_global, _local) =>
        {
            if (m_GlobalOrder.Count > 0)
                return _global[m_GlobalOrder[0]];
            if (m_LocalOrder.Count > 0)
                return _local[m_LocalOrder[0]];
            return false;
        };
    }

    public override void OnResetLocalChanged()
    {
        base.OnResetLocalChanged();
        m_LocalOrder.Clear();
    }
    #endregion

    #region Function
    /// <summary>효과 데이터 저장 (삽입 순서 추적)</summary>
    public new void Set(object _callBy, TKey id, bool factor = default)
    {
        if (_callBy as GlobalManagerBase)
        {
            if (!m_Global.ContainsKey(id))
                m_GlobalOrder.Add(id);
        }
        else
        {
            if (!m_Local.ContainsKey(id))
                m_LocalOrder.Add(id);
        }
        base.Set(_callBy, id, factor);
    }

    /// <summary>효과 데이터 제거 (삽입 순서 추적)</summary>
    public new void Remove(TKey _id)
    {
        if (m_Global.ContainsKey(_id))
            m_GlobalOrder.Remove(_id);
        else if (m_Local.ContainsKey(_id))
            m_LocalOrder.Remove(_id);
        base.Remove(_id);
    }
    #endregion
}
