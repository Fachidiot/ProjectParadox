using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>플레이어 대기 상태</summary>
public class PlayerActorState_Idle : FSMState
{
    #region Inspector
    [SerializeField, TabGroup("Transition"), LabelText("이동 상태 ID")] private string m_MoveStateID = "Move";
    [SerializeField, TabGroup("Transition"), LabelText("점프 상태 ID")] private string m_JumpStateID = "Jump";
    [SerializeField, TabGroup("Transition"), LabelText("낙하 상태 ID")] private string m_FallStateID = "Fall";
    [SerializeField, TabGroup("Transition"), LabelText("공격 상태 ID")] private string m_AttackStateID = "Attack";
    [SerializeField, TabGroup("Transition"), LabelText("감정 상태 ID")] private string m_EmotionStateID = "Emotion";
    #endregion
    #region Get,Set
    private PlayerActor Actor => (PlayerActor)ParFSM.Owner;
    public override (string stateID, string key)[] TransitionInfos
    {
        get
        {
            bool hasAttack = EquipManager.instance.HasSpecial("Attack");
            int count = 3 + (hasAttack ? 1 : 0);
            var arr = new (string, string)[count];
            arr[0] = (m_MoveStateID, "Move");
            arr[1] = (m_JumpStateID, "Jump");
            int i = 2;
            if (hasAttack) arr[i++] = (m_AttackStateID, "Attack");
            arr[i] = (m_EmotionStateID, "Emotion");
            return arr;
        }
    }
    #endregion

    #region FSM
    protected override void OnStart()
    {
        LocalInteractionManager.instance.SetCanInteract(true);
    }

    protected override FSMState OnUpdate()
    {
        if (Actor.IsJumpPressed)
        {
            Actor.Physics.Jump();
            return ParFSM.GetState(m_JumpStateID);
        }

        if (Actor.Physics.FlyState != CharacterPhysicsBase.EFlyState.None)
        {
            var fall = ParFSM.GetState<PlayerActorState_Fall>(m_FallStateID);
            if (-Actor.Physics.Rig.linearVelocity.y >= fall.EnterMinSpeed)
                return fall;
        }

        if (Actor.IsAttackPressed && EquipManager.instance.HasSpecial("Attack"))
            return ParFSM.GetState(m_AttackStateID);

        if (Actor.IsEmotionPressed)
            return ParFSM.GetState(m_EmotionStateID);

        if (Actor.MoveInput != Vector2.zero)
            return ParFSM.GetState(m_MoveStateID);

        return this;
    }
    #endregion
}
