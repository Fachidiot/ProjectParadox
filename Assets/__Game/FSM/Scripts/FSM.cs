using System.Collections.Generic;
using UnityEngine;

public class FSM : MonoBehaviour
{
    #region Property
    public object Owner { get; private set; }
    public IReadOnlyList<FSMState> States => m_StateList;
    public IReadOnlyCollection<FSMState> CurStates => m_CurStates;
    #endregion
    #region Value
    private List<FSMState> m_StateList;
    private Dictionary<string, FSMState> m_StateDic;
    private FSMState[] m_CurStates;
    #endregion

    #region Event
    public void Init(object _owner = null)
    {
        Owner = _owner;
        m_StateList = new List<FSMState>(GetComponentsInChildren<FSMState>());
        m_StateDic = new Dictionary<string, FSMState>();
        foreach (var v in m_StateList)
            if (!string.IsNullOrEmpty(v.ID))
                m_StateDic[v.ID] = v;

        var defaults = new List<FSMState>();
        foreach (var v in m_StateList)
        {
            v.Init(this);
            if (v.IsDefault)
                defaults.Add(v);
        }

        m_CurStates = new FSMState[defaults.Count];
        for (int i = 0; i < defaults.Count; ++i)
        {
            m_CurStates[i] = defaults[i];
            m_CurStates[i].StartState();
        }
    }
    protected virtual void Update()
    {
        for (int i = 0; i < m_CurStates.Length; ++i)
            if (m_CurStates[i])
            {
                var next = m_CurStates[i].UpdateState();
                if (next != m_CurStates[i])
                    Set(next, i);
            }
    }
    protected virtual void FixedUpdate()
    {
        for (int i = 0; i < m_CurStates.Length; ++i)
            if (m_CurStates[i])
            {
                var next = m_CurStates[i].FixedUpdateState();
                if (next != m_CurStates[i])
                    Set(next, i);
            }
    }
    #endregion
    #region Function
    /// <summary>ID로 상태 조회</summary>
    public FSMState GetState(string _id) => m_StateDic.TryGetValue(_id, out var state) ? state : null;
    /// <summary>ID로 상태 조회 (제네릭)</summary>
    public T GetState<T>(string _id) where T : FSMState => GetState(_id) as T;

    public void Set(FSMState _state, int _layer = 0)
    {
        if (0 <= _layer && _layer < m_CurStates.Length && m_StateList.Contains(_state))
        {
            if (_state.IsEnable && m_CurStates[_layer] != _state)
            {
                FSMState old = m_CurStates[_layer];
                m_CurStates[_layer] = _state;
                old?.EndState();
                m_CurStates[_layer].StartState();
            }
        }
    }
    #endregion
}
