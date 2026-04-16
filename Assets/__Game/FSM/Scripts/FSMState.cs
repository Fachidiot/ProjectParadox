using Sirenix.OdinInspector;
using UnityEngine;

public abstract class FSMState : MonoBehaviour
{
    #region Inspector
    [SerializeField, TabGroup("Base"), LabelText("기본 상태")] private bool m_IsDefault;
    #endregion
    #region Property
    public string ID => gameObject.name;
    public bool IsDefault => m_IsDefault;
    public FSM ParFSM { get; private set; }
    public virtual bool IsEnable => true;
    public virtual (string stateID, string key)[] TransitionInfos => null;
    #endregion

    #region Event
    public void Init(FSM _fsm)
    {
        ParFSM = _fsm;
        OnInit();
    }
    protected virtual void OnInit() { }
    internal void StartState() => OnStart();
    internal FSMState UpdateState() => OnUpdate();
    internal FSMState FixedUpdateState() => OnFixedUpdate();
    internal void EndState() => OnEnd();
    #endregion
    #region Local Function
    protected virtual void OnStart() { }
    protected virtual FSMState OnUpdate() => this;
    protected virtual FSMState OnFixedUpdate() => this;
    protected virtual void OnEnd() { }
    #endregion
}
