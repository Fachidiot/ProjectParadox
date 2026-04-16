using System.Collections.Generic;
using UnityEngine;

/// <summary>Dictionary 확장 메서드 제공 (ContainsKeys, AddEx, Set, RemoveAll)</summary>
public static class DictionaryUtil
{
    #region Function
    /// <summary>복수 키 존재 여부 확인</summary>
    public static bool ContainsKeys<TKey, TValue>(this Dictionary<TKey, TValue> _dic, params TKey[] keys)
    {
        if (_dic == null || keys == null)
            return false;

        foreach (var key in keys)
            if (!_dic.ContainsKey(key))
                return false;

        return true;
    }
    /// <summary>키 미존재시에만 추가</summary>
    public static void AddEx<TKey, TValue>(this Dictionary<TKey, TValue> _dic, TKey _key, TValue _value)
    {
        if (!_dic.ContainsKey(_key))
            _dic.Add(_key, _value);
    }
    /// <summary>키 존재시 덮어쓰기, 미존재시 추가</summary>
    public static void Set<TKey, TValue>(this Dictionary<TKey, TValue> _dic, TKey _key, TValue _value)
    {
        if (_dic.ContainsKey(_key))
            _dic[_key] = _value;
        else
            _dic.Add(_key, _value);
    }
    /// <summary>배열의 모든 키 제거</summary>
    public static void RemoveAll<TKey, TValue>(this Dictionary<TKey, TValue> _list, TKey[] _remove)
    {
        foreach (var v in _remove)
            _list.Remove(v);
    }
    /// <summary>리스트의 모든 키 제거</summary>
    public static void RemoveAll<TKey, TValue>(this Dictionary<TKey, TValue> _list, List<TKey> _remove)
    {
        foreach (var v in _remove)
            _list.Remove(v);
    }
    #endregion
}
