using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>거래 데이터 구조체, Key/Action/Count 포함</summary>
[Serializable] public struct SDeal : ITableType
{
    /// <summary>소속 테이블</summary>
    public string Table { get; private set; }
    /// <summary>고유 ID</summary>
    public string ID { get; private set; }
    /// <summary>키로 데이터 조회</summary>
    public object this[string _id]
    {
        get
        {
            if (m_Data.TryGetValue(_id, out var data))
                return data;

            return null;
        }
    }

    /// <summary>거래 대상 키</summary>
    public string Key;
    /// <summary>거래 행동 타입 (Optional)</summary>
    public string Action;
    /// <summary>거래 수량</summary>
    public double Count;

    /// <summary>수량 (int)</summary>
    public int CountInt => (int)Count;
    /// <summary>수량 (long)</summary>
    public long CountLong => (long)Count;
    /// <summary>수량 (float)</summary>
    public float CountFloat => (float)Count;

    private Dictionary<string, object> m_Data;

    /// <summary>테이블 데이터 딕셔너리로 초기화</summary>
    public SDeal(string _table, string _baseID, string _addID, Dictionary<string, object> _dic)
    {
        Table = _table;
        ID = $"{_baseID}.{_addID}";

        m_Data = new Dictionary<string, object>(_dic);
        object o = null;

        if (_dic.TryGetValue("Key", out o))
            Key = o as string;
        else
            Key = string.Empty;
        m_Data.Add("Key", Key);

        if (_dic.TryGetValue("Action", out o))
            Action = o as string;
        else
            Action = string.Empty;
        m_Data.Add("Action", Action);

        if (_dic.TryGetValue("Count", out o))
            Count = System.Convert.ToDouble(o);
        else
            Count = 0;
        m_Data.Add("Count", Count);
    }
    /// <summary>개별 파라미터로 초기화</summary>
    public SDeal(string _table, string _baseID, string _addID, string _key, string _action, double _count)
    {
        Table = _table;
        ID = $"{_baseID}.{_addID}";

        m_Data = new Dictionary<string, object>();
        Key = _key;
        m_Data.Add("Key", Key);
        Action = _action;
        m_Data.Add("Action", Action);
        Count = _count;
        m_Data.Add("Count", Count);
    }
    /// <summary>거래 실행 전용 간편 초기화</summary>
    public SDeal(string _key, string _action, double _count)
    {
        Table = null;
        ID = null;
        m_Data = null;

        Key = _key;
        Action = _action;
        Count = _count;
    }

    /// <summary>수량 설정 후 자신 반환 (체이닝용)</summary>
    public SDeal SetCount(double _count)
    {
        Count = _count;
        return this;
    }
    /// <summary>수량 설정 후 자신 반환 (체이닝용)</summary>
    public SDeal SetCount(int _count)
    {
        Count = _count;
        return this;
    }
    /// <summary>수량 설정 후 자신 반환 (체이닝용)</summary>
    public SDeal SetCount(long _count)
    {
        Count = _count;
        return this;
    }
    /// <summary>수량 설정 후 자신 반환 (체이닝용)</summary>
    public SDeal SetCount(float _count)
    {
        Count = _count;
        return this;
    }
}
