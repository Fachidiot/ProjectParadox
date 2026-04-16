using System.Collections.Generic;

/// <summary>List 확장 메서드 제공 (RemoveAll, EnsureListIndex)</summary>
public static class ListUtil
{
    #region Function
    /// <summary>배열의 모든 요소 제거</summary>
    public static void RemoveAll<T>(this List<T> _list, T[] _remove)
    {
        foreach (var v in _remove)
            _list.Remove(v);
    }
    /// <summary>리스트의 모든 요소 제거</summary>
    public static void RemoveAll<T>(this List<T> _list, List<T> _remove)
    {
        foreach (var v in _remove)
            _list.Remove(v);
    }
    /// <summary>목표 인덱스까지 기본값으로 리스트 확장</summary>
    public static void EnsureListIndex<T>(this List<T> _list, int _targetIndex, T _default)
    {
        while (_list.Count <= _targetIndex)
            _list.Add(_default);
    }
    #endregion
}
