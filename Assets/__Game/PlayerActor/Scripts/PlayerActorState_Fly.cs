using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>플레이어 비행 상태, 점프 버튼 홀드 중 체공</summary>
public class PlayerActorState_Fly : FSMState
{
    #region Inspector
    [SerializeField, TabGroup("Transition"), LabelText("이동 상태 ID")] private string m_MoveStateID = "Move";
    [SerializeField, TabGroup("Transition"), LabelText("낙하 상태 ID")] private string m_FallStateID = "Fall";
    #endregion
    #region Get,Set
    private PlayerActor Actor => (PlayerActor)ParFSM.Owner;
    public override (string stateID, string key)[] TransitionInfos => new[]
    {
        (m_MoveStateID, "Move"),
    };
    #endregion

    #region FSM
    protected override void OnStart()
    {
        LocalInteractionManager.instance.SetCanInteract(false);
        Actor.Physics.StartFly();
    }

    protected override FSMState OnUpdate()
    {
        // 버튼 떼면 낙하
        if (!Actor.IsJumpHeld)
        {
            Actor.Physics.StopFly();
            return ParFSM.GetState(m_FallStateID);
        }

        // 공중 이동
        if (Actor.MoveInput != Vector2.zero)
        {
            Vector2 moveDir = Actor.GetCameraMoveDir();
            Actor.Physics.Move(moveDir);
            Actor.RotateTowardMoveDir(moveDir);
        }

        // 착지 판정
        if (Actor.Physics.FlyState == CharacterPhysicsBase.EFlyState.None)
            return ParFSM.GetState(m_FallStateID);

        return this;
    }

    protected override void OnEnd()
    {
        if (Actor.Physics.FlyState == CharacterPhysicsBase.EFlyState.Fly)
            Actor.Physics.StopFly();
    }
    #endregion
}
