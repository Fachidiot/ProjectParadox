using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>플레이어 공격 상태, 트리거 범위 내 IAttackable 공격</summary>
public class PlayerActorState_Attack : FSMState
{
    #region Inspector
    [SerializeField, TabGroup("Transition"), LabelText("대기 상태 ID")] private string m_IdleStateID = "Idle";
    [SerializeField, TabGroup("Transition"), LabelText("이동 상태 ID")] private string m_MoveStateID = "Move";
    [SerializeField, TabGroup("Option"), LabelText("공격 지속시간")] private float m_Duration = 0.3f;
    [SerializeField, TabGroup("Option"), LabelText("공격 처리 딜레이")] private float m_AttackDelay = 0.1f;
    [SerializeField, TabGroup("Option"), LabelText("휘두르기 SE")] private AudioClip m_SwingSE;
    #endregion
    #region Get,Set
    private PlayerActor Actor => (PlayerActor)ParFSM.Owner;
    public override (string stateID, string key)[] TransitionInfos
        => ParFSM.GetState(m_IdleStateID).TransitionInfos;
    #endregion
    #region Value
    private float m_Timer;
    private bool m_AttackApplied;
    #endregion

    #region FSM
    protected override void OnStart()
    {
        LocalInteractionManager.instance.SetCanInteract(false);
        Actor.Animator.SetTrigger("Attack");
        SoundManager.instance.PlaySE(m_SwingSE, Actor.transform.position);
        m_Timer = m_Duration;
        m_AttackApplied = false;
    }

    protected override void OnEnd()
    {
        Actor.AttackTrigger?.End();
    }

    protected override FSMState OnUpdate()
    {
        m_Timer -= Time.deltaTime;

        if (!m_AttackApplied && m_Timer <= m_Duration - m_AttackDelay)
        {
            m_AttackApplied = true;
            if (Actor.AttackTrigger != null) Actor.AttackTrigger.Begin(Actor);
        }

        if (m_Timer <= 0)
        {
            if (Actor.MoveInput != Vector2.zero)
                return ParFSM.GetState(m_MoveStateID);
            return ParFSM.GetState(m_IdleStateID);
        }
        return this;
    }
    #endregion
}
