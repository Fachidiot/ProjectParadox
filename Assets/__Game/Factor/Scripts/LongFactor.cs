using System;
using System.Collections.Generic;

/// <summary>long형 Factor, Add/Min/Max 집계 지원</summary>
public sealed class LongFactor<TKey> : FactorBase<TKey, long>, IReadOnlyLongFactor
{
    #region Type
    /// <summary>집계 프리셋 타입</summary>
    public enum ETotalType
    {
        Add,
        Min,
        Max,
    }
    #endregion

    #region Event
    public LongFactor(IManageValue _callBy, Func<Dictionary<TKey, long>, Dictionary<TKey, long>, long> _totalFunc) : base(_callBy, _totalFunc)
    {
    }
    public LongFactor(IManageValue _callBy, ETotalType _totalType) : base(_callBy, null)
    {
        //TotalFunc의 프리셋 함수 설정
        if (_totalType == ETotalType.Add)
            m_OnTotalFunc = (_global, _local) =>
            {
                long all = 0;
                foreach (var v in _global)
                    all += v.Value;
                foreach (var v in _local)
                    all += v.Value;

                return all;
            };
        else if (_totalType == ETotalType.Min)
            m_OnTotalFunc = (_global, _local) =>
            {
                long min = long.MaxValue;
                foreach (var v in _global)
                    if (v.Value < min)
                        min = v.Value;
                foreach (var v in _local)
                    if (v.Value < min)
                        min = v.Value;

                return min;
            };
        else if (_totalType == ETotalType.Max)
            m_OnTotalFunc = (_global, _local) =>
            {
                long max = long.MinValue;
                foreach (var v in _global)
                    if (max < v.Value)
                        max = v.Value;
                foreach (var v in _local)
                    if (max < v.Value)
                        max = v.Value;

                return max;
            };
    }
    #endregion
}
