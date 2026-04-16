using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>플레이어 이동 상태, 카메라 방향 기반 물리 이동</summary>
public class PlayerActorState_Move : FSMState
{
    #region Inspector
    [SerializeField, TabGroup("Transition"), LabelText("대기 상태 ID")] private string m_IdleStateID = "Idle";
    [SerializeField, TabGroup("Transition"), LabelText("점프 상태 ID")] private string m_JumpStateID = "Jump";
    [SerializeField, TabGroup("Transition"), LabelText("낙하 상태 ID")] private string m_FallStateID = "Fall";
    [SerializeField, TabGroup("Transition"), LabelText("공격 상태 ID")] private string m_AttackStateID = "Attack";
    #endregion
    #region Get,Set
    private PlayerActor Actor => (PlayerActor)ParFSM.Owner;
    public override (string stateID, string key)[] TransitionInfos
    {
        get
        {
            bool hasAttack = EquipManager.instance.HasSpecial("Attack");
            var arr = new (string, string)[hasAttack ? 3 : 2];
            arr[0] = (ID, "Move");
            arr[1] = (m_JumpStateID, "Jump");
            if (hasAttack) arr[2] = (m_AttackStateID, "Attack");
            return arr;
        }
    }
    #endregion

    #region FSM
    protected override void OnStart()
    {
        LocalInteractionManager.instance.SetCanInteract(true);
    }

    protected override void OnEnd()
    {
        Actor.Animator.SetFloat("Walk", 0f);
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

        if (Actor.MoveInput == Vector2.zero)
            return ParFSM.GetState(m_IdleStateID);

        Vector2 moveDir = Actor.GetCameraMoveDir();
        Actor.Physics.Move(moveDir);
        Actor.RotateTowardMoveDir(moveDir);

        // xz 속도를 최대속도 기준으로 0~1 정규화
        var vel = Actor.Physics.Rig.linearVelocity;
        float xzSpeed = new Vector2(vel.x, vel.z).magnitude;
        float maxSpeed = Actor.Physics.MoveSpeed.v;
        Actor.Animator.SetFloat("Walk", maxSpeed > 0f ? Mathf.Clamp01(xzSpeed / maxSpeed) : 0f);

        return this;
    }
    #endregion
}
