using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>플레이어 더블점프 상태, 공중에서 한번 더 점프 후 비행 전환 가능</summary>
public class PlayerActorState_DoubleJump : FSMState
{
    #region Inspector
    [SerializeField, TabGroup("Transition"), LabelText("대기 상태 ID")] private string m_IdleStateID = "Idle";
    [SerializeField, TabGroup("Transition"), LabelText("이동 상태 ID")] private string m_MoveStateID = "Move";
    [SerializeField, TabGroup("Transition"), LabelText("비행 상태 ID")] private string m_FlyStateID = "Fly";
    #endregion
    #region Get,Set
    private PlayerActor Actor => (PlayerActor)ParFSM.Owner;
    public override (string stateID, string key)[] TransitionInfos
    {
        get
        {
            bool hasFly = EquipManager.instance.HasSpecial("Fly");
            var arr = new (string, string)[hasFly ? 2 : 1];
            arr[0] = (m_MoveStateID, "Move");
            if (hasFly) arr[1] = (m_FlyStateID, "Jump");
            return arr;
        }
    }
    #endregion

    #region FSM
    protected override void OnStart()
    {
        LocalInteractionManager.instance.SetCanInteract(false);
        Actor.Physics.Jump();
        Actor.Animator.SetTrigger("DoubleJump");
    }

    protected override FSMState OnUpdate()
    {
        // 공중 이동
        if (Actor.MoveInput != Vector2.zero)
        {
            Vector2 moveDir = Actor.GetCameraMoveDir();
            Actor.Physics.Move(moveDir);
            Actor.RotateTowardMoveDir(moveDir);
        }

        // 비행 전환
        if (Actor.IsJumpPressed && EquipManager.instance.HasSpecial("Fly"))
            return ParFSM.GetState(m_FlyStateID);

        // 착지
        if (Actor.Physics.FlyState == CharacterPhysicsBase.EFlyState.None)
        {
            Actor.Animator.SetTrigger("Land");
            return Actor.MoveInput != Vector2.zero
                ? ParFSM.GetState(m_MoveStateID)
                : ParFSM.GetState(m_IdleStateID);
        }

        return this;
    }
    #endregion
}
