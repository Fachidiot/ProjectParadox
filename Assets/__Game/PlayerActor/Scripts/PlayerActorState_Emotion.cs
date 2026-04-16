using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>플레이어 감정 상태, 랜덤 감정 재생 후 숫자 키로 변경 가능</summary>
public class PlayerActorState_Emotion : FSMState
{
    #region Inspector
    [SerializeField, TabGroup("Transition"), LabelText("이동 상태 ID")] private string m_MoveStateID = "Move";
    [SerializeField, TabGroup("Transition"), LabelText("점프 상태 ID")] private string m_JumpStateID = "Jump";
    [SerializeField, TabGroup("Transition"), LabelText("공격 상태 ID")] private string m_AttackStateID = "Attack";
    [SerializeField, TabGroup("Option"), LabelText("감정 개수")] private int m_EmotionCount = 2;
    #endregion
    #region Get,Set
    private PlayerActor Actor => (PlayerActor)ParFSM.Owner;
    public override (string stateID, string key)[] TransitionInfos
    {
        get
        {
            var idle = ParFSM.GetState(m_MoveStateID).TransitionInfos;
            int baseLen = idle != null ? idle.Length : 0;
            var arr = new (string, string)[baseLen + m_EmotionCount];
            for (int i = 0; i < baseLen; i++)
                arr[i] = idle[i];
            for (int i = 0; i < m_EmotionCount; i++)
                arr[baseLen + i] = ($"Emotion{i + 1}", $"Emotion{i + 1}");
            return arr;
        }
    }
    #endregion
    #region Value
    private bool[] m_EmotionPressed;
    #endregion

    #region FSM
    protected override void OnInit()
    {
        m_EmotionPressed = new bool[m_EmotionCount];
        var input = LocalInputManager.instance;
        for (int i = 0; i < m_EmotionCount; i++)
        {
            int idx = i;
            input.Create(LocalInteractionManager.instance, $"Emotion{i + 1}", LocalInputManager.EPriority.Game,
                _ctx => { if (_ctx.performed) m_EmotionPressed[idx] = true; return false; });
        }
    }

    protected override void OnStart()
    {
        LocalInteractionManager.instance.SetCanInteract(false);
        int idx = Random.Range(1, m_EmotionCount + 1);
        Actor.Animator.SetTrigger($"Emotion{idx}");
    }

    protected override FSMState OnUpdate()
    {
        // 숫자 키로 감정 변경
        for (int i = 0; i < m_EmotionCount; i++)
        {
            if (m_EmotionPressed[i])
            {
                m_EmotionPressed[i] = false;
                Actor.Animator.SetTrigger($"Emotion{i + 1}");
                break;
            }
        }

        // 점프
        if (Actor.IsJumpPressed)
        {
            Actor.Animator.SetTrigger("Reset");
            Actor.Physics.Jump();
            return ParFSM.GetState(m_JumpStateID);
        }

        // 공격
        if (Actor.IsAttackPressed && EquipManager.instance.HasSpecial("Attack"))
            return ParFSM.GetState(m_AttackStateID);

        // 이동
        if (Actor.MoveInput != Vector2.zero)
        {
            Actor.Animator.SetTrigger("Reset");
            return ParFSM.GetState(m_MoveStateID);
        }

        return this;
    }
    #endregion
}
