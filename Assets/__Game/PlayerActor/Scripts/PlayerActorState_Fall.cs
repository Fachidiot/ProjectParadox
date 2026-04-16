using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>플레이어 낙하 상태, 낙하 중 이동 및 착지 시 속도 기반 Land 애니 재생</summary>
public class PlayerActorState_Fall : FSMState
{
    #region Inspector
    [SerializeField, TabGroup("Transition"), LabelText("대기 상태 ID")] private string m_IdleStateID = "Idle";
    [SerializeField, TabGroup("Transition"), LabelText("이동 상태 ID")] private string m_MoveStateID = "Move";
    [SerializeField, TabGroup("Transition"), LabelText("더블점프 상태 ID")] private string m_DoubleJumpStateID = "DoubleJump";
    [SerializeField, TabGroup("Transition"), LabelText("비행 상태 ID")] private string m_FlyStateID = "Fly";
    [SerializeField, TabGroup("Option"), LabelText("진입 최소 낙하속도")] private float m_EnterMinSpeed = 1f;
    [SerializeField, TabGroup("Option"), LabelText("Land 애니 최소 낙하속도")] private float m_LandAnimMinSpeed = 3f;
    #endregion
    #region Get,Set
    private PlayerActor Actor => (PlayerActor)ParFSM.Owner;
    public float EnterMinSpeed => m_EnterMinSpeed;
    public override (string stateID, string key)[] TransitionInfos
    {
        get
        {
            bool hasDouble = EquipManager.instance.HasSpecial("DoubleJump");
            bool hasFly = !hasDouble && EquipManager.instance.HasSpecial("Fly");
            var arr = new (string, string)[hasDouble || hasFly ? 2 : 1];
            arr[0] = (m_MoveStateID, "Move");
            if (hasDouble) arr[1] = (m_DoubleJumpStateID, "Jump");
            else if (hasFly) arr[1] = (m_FlyStateID, "Jump");
            return arr;
        }
    }
    #endregion
    #region Value
    private float m_MaxFallSpeed;
    #endregion

    #region FSM
    protected override void OnStart()
    {
        LocalInteractionManager.instance.SetCanInteract(false);
        m_MaxFallSpeed = 0f;
    }

    protected override void OnEnd()
    {
        Actor.Animator.SetFloat("Walk", 0f);
    }

    protected override FSMState OnUpdate()
    {
        float fallSpeed = -Actor.Physics.Rig.linearVelocity.y;
        if (fallSpeed > m_MaxFallSpeed) m_MaxFallSpeed = fallSpeed;

        if (Actor.MoveInput != Vector2.zero)
        {
            Vector2 moveDir = Actor.GetCameraMoveDir();
            Actor.Physics.Move(moveDir);
            Actor.RotateTowardMoveDir(moveDir);
        }

        var vel = Actor.Physics.Rig.linearVelocity;
        float xzSpeed = new Vector2(vel.x, vel.z).magnitude;
        float maxSpeed = Actor.Physics.MoveSpeed.v;
        Actor.Animator.SetFloat("Walk", maxSpeed > 0f ? Mathf.Clamp01(xzSpeed / maxSpeed) : 0f);

        if (Actor.IsJumpPressed)
        {
            if (EquipManager.instance.HasSpecial("DoubleJump"))
                return ParFSM.GetState(m_DoubleJumpStateID);
            else if (EquipManager.instance.HasSpecial("Fly"))
                return ParFSM.GetState(m_FlyStateID);
        }

        if (Actor.Physics.FlyState == CharacterPhysicsBase.EFlyState.None)
        {
            if (m_MaxFallSpeed >= m_LandAnimMinSpeed)
                Actor.Animator.SetTrigger("Land");

            return Actor.MoveInput != Vector2.zero
                ? ParFSM.GetState(m_MoveStateID)
                : ParFSM.GetState(m_IdleStateID);
        }

        return this;
    }
    #endregion
}
