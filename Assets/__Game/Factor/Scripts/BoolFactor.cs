using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>bool형 Factor, And/Or 집계 지원</summary>
public class BoolFactor<TKey> : FactorBase<TKey, bool>, IReadOnlyBoolFactor
{
    #region Type
    /// <summary>집계 프리셋 타입</summary>
    public enum ETotalType
    {
        And,
        Or,
    }
    #endregion

    #region Event
    public BoolFactor(IManageValue _callBy, Func<Dictionary<TKey, bool>, Dictionary<TKey, bool>, bool> _totalFunc) : base(_callBy, _totalFunc)
    {
    }
    public BoolFactor(IManageValue _callBy, ETotalType _totalType) : base(_callBy, null)
    {
        //TotalFunc의 프리셋 함수 설정
        if (_totalType == ETotalType.And)
            m_OnTotalFunc = (_global, _local) =>
            {
                bool and = true;
                foreach (var v in _global)
                    and &= v.Value;
                foreach (var v in _local)
                    and &= v.Value;

                return and;
            };
        else if (_totalType == ETotalType.Or)
            m_OnTotalFunc = (_global, _local) =>
            {
                bool or = false;
                foreach (var v in _global)
                    or |= v.Value;
                foreach (var v in _local)
                    or |= v.Value;

                return or;
            };
    }
    #endregion
}
