using System.Collections.Generic;
using UnityEngine;

public static class ObjectUtil
{
    /// <summary>직접 자식 ObjectBase만 수집 (중간에 ObjectBase가 없는 경로는 재귀 탐색)</summary>
    public static ObjectBase[] FindDirectChildren(Transform _root)
    {
        var list = new List<ObjectBase>();
        collectFrom(_root, list);
        return list.ToArray();
    }
    private static void collectFrom(Transform _parent, List<ObjectBase> _list)
    {
        foreach (Transform child in _parent)
        {
            var obj = child.GetComponent<ObjectBase>();
            if (obj)
                _list.Add(obj);
            else
                collectFrom(child, _list);
        }
    }
}
