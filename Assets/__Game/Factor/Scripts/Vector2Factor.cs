using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Vector2형 Factor, Average/Add 집계 지원</summary>
public sealed class Vector2Factor<TKey> : FactorBase<TKey, Vector2>, IReadOnlyVector2Factor
{
    #region Type
    /// <summary>집계 프리셋 타입</summary>
    public enum ETotalType
    {
        Average,
        Add,
    }
    #endregion

    #region Event
    public Vector2Factor(IManageValue _callBy, Func<Dictionary<TKey, Vector2>, Dictionary<TKey, Vector2>, Vector2> _totalFunc) : base(_callBy, _totalFunc)
    {
    }
    public Vector2Factor(IManageValue _callBy, ETotalType _totalType) : base(_callBy, null)
    {
        //TotalFunc의 프리셋 함수 설정
        if (_totalType == ETotalType.Average)
            m_OnTotalFunc = (_global, _local) =>
            {
                Vector2 all = Vector2.zero;
                foreach (var v in _global)
                    all += v.Value;
                foreach (var v in _local)
                    all += v.Value;

                return all / (_global.Count + _local.Count);
            };
        else if (_totalType == ETotalType.Add)
            m_OnTotalFunc = (_global, _local) =>
            {
                Vector2 all = Vector2.zero;
                foreach (var v in _global)
                    all += v.Value;
                foreach (var v in _local)
                    all += v.Value;

                return all;
            };
    }
    #endregion
}
