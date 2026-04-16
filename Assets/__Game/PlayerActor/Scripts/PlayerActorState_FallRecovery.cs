using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>플레이어 추락 복귀 상태, 포물선 아크로 마지막 안전 위치로 복귀</summary>
public class PlayerActorState_FallRecovery : FSMState
{
    #region Inspector
    [SerializeField, TabGroup("Transition"), LabelText("대기 상태 ID")] private string m_IdleStateID = "Idle";
    [SerializeField, TabGroup("Option"), LabelText("복귀 시간(초)")] private float m_Duration = 1.5f;
    [SerializeField, TabGroup("Option"), LabelText("아크 높이")] private float m_ArcHeight = 8f;
    #endregion
    #region Get,Set
    private PlayerActor Actor => (PlayerActor)ParFSM.Owner;
    #endregion
    #region Value
    private Vector3 m_StartPos;
    private Vector3 m_EndPos;
    private Vector3 m_ControlPos;
    private float m_Timer;
    #endregion

    #region FSM
    protected override void OnStart()
    {
        LocalInteractionManager.instance.SetCanInteract(false);

        Actor.Physics.Rig.linearVelocity = Vector3.zero;
        Actor.Physics.Rig.isKinematic = true;

        m_StartPos = Actor.transform.position;
        m_EndPos = Actor.LastSafePos;
        m_ControlPos = new Vector3(
            (m_StartPos.x + m_EndPos.x) * 0.5f,
            m_EndPos.y + m_ArcHeight,
            (m_StartPos.z + m_EndPos.z) * 0.5f
        );
        m_Timer = 0f;

        Actor.Animator.SetTrigger("Jump");
    }

    protected override FSMState OnUpdate()
    {
        m_Timer += Time.deltaTime;
        float t = Mathf.Clamp01(m_Timer / m_Duration);
        float smooth = Mathf.SmoothStep(0f, 1f, t);

        Actor.transform.position = BezierQuad(m_StartPos, m_ControlPos, m_EndPos, smooth);

        Vector3 toEnd = m_EndPos - m_StartPos;
        toEnd.y = 0;
        if (toEnd.sqrMagnitude > 0.01f)
            Actor.transform.rotation = Quaternion.LookRotation(toEnd.normalized);

        if (t >= 1f)
        {
            Actor.Animator.SetTrigger("Land");
            return ParFSM.GetState(m_IdleStateID);
        }

        return this;
    }

    protected override void OnEnd()
    {
        Actor.Physics.Rig.isKinematic = false;
    }
    #endregion

    #region Local Function
    private static Vector3 BezierQuad(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1f - t;
        return u * u * p0 + 2f * u * t * p1 + t * t * p2;
    }
    #endregion
}
