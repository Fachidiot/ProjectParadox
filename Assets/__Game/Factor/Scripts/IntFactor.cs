using System;
using System.Collections.Generic;

/// <summary>int형 Factor, Add/Min/Max 집계 지원</summary>
public sealed class IntFactor<TKey> : FactorBase<TKey, int>, IReadOnlyIntFactor
{
    #region Type
    public enum ETotalType
    {
        Add,
        Min,
        Max,
    }
    #endregion

    #region Event
    public IntFactor(IManageValue _callBy, Func<Dictionary<TKey, int>, Dictionary<TKey, int>, int> _totalFunc) : base(_callBy, _totalFunc)
    {
    }
    public IntFactor(IManageValue _callBy, ETotalType _totalType) : base(_callBy, null)
    {
        //TotalFunc의 프리셋 함수 설정
        if (_totalType == ETotalType.Add)
            m_OnTotalFunc = (_global, _local) =>
            {
                int all = 0;
                foreach (var v in _global)
                    all += v.Value;
                foreach (var v in _local)
                    all += v.Value;

                return all;
            };
        else if (_totalType == ETotalType.Min)
            m_OnTotalFunc = (_global, _local) =>
            {
                int min = int.MaxValue;
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
                int max = int.MinValue;
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
