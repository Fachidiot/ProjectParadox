using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>게임오브젝트 풀링 클래스, Get/Return으로 재사용</summary>
public class ObjectPool
{
    #region Value
    private GameObject m_Prefab;
    private Transform m_Root;
    public Action<GameObject> m_InitFunc;
    public Action<GameObject, bool> m_SetReusableFunc;
    private Queue<GameObject> m_Pool = new();
    private List<GameObject> m_Using = new();
    #endregion

    #region Event
    public ObjectPool(GameObject _prefab, Transform _root, int _size, Action<GameObject> _initFunc = null, Action<GameObject, bool> _setResuableFunc = null)
    {
        m_Prefab = _prefab;
        m_Root = _root;
        m_InitFunc = _initFunc;
        m_SetReusableFunc = (_setResuableFunc != null) ? _setResuableFunc : DefaultSetReusableFunc;

        for (int i = 0; i < _size; ++i)
        {
            var obj = GameObject.Instantiate(m_Prefab, m_Root);
            m_Pool.Enqueue(obj);
            m_InitFunc?.Invoke(obj);
            m_SetReusableFunc(obj, true);
        }
        m_Prefab.gameObject.SetActive(false);
    }
    #endregion
    #region Local Function
    private static void DefaultSetReusableFunc(GameObject _obj, bool _isAct)
    {
        _obj.gameObject.SetActive(!_isAct);
    }
    #endregion
    #region Function
    /// <summary>재활용 가능한 오브젝트 획득</summary>
    public GameObject Get()
    {
        if (m_Pool.Count == 0)
            return null;

        var obj = m_Pool.Dequeue();
        m_Using.Add(obj);
        m_SetReusableFunc(obj, false);

        return obj;
    }
    /// <summary>사용 완료 오브젝트 반환</summary>
    public void Return(GameObject _object)
    {
        if (m_Using.Remove(_object))
        {
            m_SetReusableFunc(_object, true);
            m_Pool.Enqueue(_object);
        }
    }
    /// <summary>모든 오브젝트 재활용 상태로 초기화</summary>
    public void Clear()
    {
        for (int i = 0; i < m_Using.Count; ++i)
        {
            var o = m_Using[i];
            m_SetReusableFunc(o, true);
            m_Pool.Enqueue(o);
        }
        m_Using.Clear();
    }
    #endregion
}
