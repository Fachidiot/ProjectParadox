using System;
using System.Collections.Generic;

/// <summary>float형 Factor, Average/Add/Min/Max/Multiply 집계 지원</summary>
public sealed class FloatFactor<TKey> : FactorBase<TKey, float>, IReadOnlyFloatFactor
{
    #region Type
    /// <summary>집계 프리셋 타입</summary>
    public enum ETotalType
    {
        Average,
        Add,
        Min,
        Max,
        Multifly
    }
    #endregion

    #region Event
    public FloatFactor(IManageValue _callBy, Func<Dictionary<TKey, float>, Dictionary<TKey, float>, float> _totalFunc) : base(_callBy, _totalFunc)
    {
    }
    public FloatFactor(IManageValue _callBy, ETotalType _totalType) : base(_callBy, null)
    {
        //TotalFunc의 프리셋 함수 설정
        if (_totalType == ETotalType.Average)
            m_OnTotalFunc = (_global, _local) =>
            {
                float all = 0;
                foreach (var v in _global)
                    all += v.Value;
                foreach (var v in _local)
                    all += v.Value;

                return all / (_global.Count + _local.Count);
            };
        else if(_totalType == ETotalType.Add)
            m_OnTotalFunc = (_global, _local) =>
            {
                float all = 0;
                foreach (var v in _global)
                    all += v.Value;
                foreach (var v in _local)
                    all += v.Value;

                return all;
            };
        else if (_totalType == ETotalType.Min)
            m_OnTotalFunc = (_global, _local) =>
            {
                float min = float.MaxValue;
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
                float max = float.MinValue;
                foreach (var v in _global)
                    if (max < v.Value)
                        max = v.Value;
                foreach (var v in _local)
                    if (max < v.Value)
                        max = v.Value;

                return max;
            };
        else if (_totalType == ETotalType.Multifly)
            m_OnTotalFunc = (_global, _local) =>
            {
                float multifly = 1.0f;
                foreach (var v in _global)
                    multifly *= v.Value;
                foreach (var v in _local)
                    multifly *= v.Value;

                return multifly;
            };
    }
    #endregion
    #region Function
    /// <summary>0 이하 계수 제거</summary>
    public void RemoveZeroLess()
    {
        bool isRemoved = false;

        foreach (var v in m_Global)
            if (v.Value < 0)
            {
                m_Global.Remove(v.Key);
                isRemoved = true;
            }
        foreach (var v in m_Local)
            if (v.Value < 0)
            {
                m_Local.Remove(v.Key);
                isRemoved = true;
            }

        if (isRemoved)
            OnChanged(EChangeType.None);
    }
    /// <summary>모든 활성 계수에 값 추가</summary>
    public void AddAll(float add)
    {
        foreach (var v in m_Global)
            m_Global[v.Key] += add;
        foreach (var v in m_Local)
            m_Local[v.Key] = add;

        OnChanged(EChangeType.None);
    }
    #endregion
}
