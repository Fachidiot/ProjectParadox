using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>플레이어 액터, FSM 기반 이동/점프/공격 처리</summary>
public class PlayerActor : MonoBehaviour
{
    #region Inspector
    [SerializeField, TabGroup("Component"), LabelText("물리")] private CharacterPhysics3D m_Physics;
    [SerializeField, TabGroup("Component"), LabelText("FSM")] private FSM m_FSM;
    [SerializeField, TabGroup("Component"), LabelText("애니메이터")] private Animator m_Animator;

    [Title("장비 슬롯")]
    [SerializeField, TabGroup("Component"), LabelText("검")] private Transform m_SlotSword;
    [SerializeField, TabGroup("Component"), LabelText("모자")] private Transform m_SlotHat;
    [SerializeField, TabGroup("Component"), LabelText("날개")] private Transform m_SlotWing;
    [SerializeField, TabGroup("Component"), LabelText("신발 (왼쪽)")] private Transform m_SlotShoesLeft;
    [SerializeField, TabGroup("Component"), LabelText("신발 (오른쪽)")] private Transform m_SlotShoesRight;

    [Title("이동")]
    [SerializeField, TabGroup("Option"), LabelText("이동 입력 이름")] private string m_MoveActionName = "Move";
    [SerializeField, TabGroup("Option"), LabelText("점프 입력 이름")] private string m_JumpActionName = "Jump";
    [SerializeField, TabGroup("Option"), LabelText("회전 속도")] private float m_RotSpeed = 10.0f;

    [Title("추락 복귀")]
    [SerializeField, TabGroup("Option"), LabelText("추락 복귀 상태 ID")] private string m_FallRecoveryStateID = "FallRecovery";
    [SerializeField, TabGroup("Option"), LabelText("추락 판정 Y")] private float m_FallThresholdY = -5f;

    [Title("공격")]
    [SerializeField, TabGroup("Option"), LabelText("공격 입력 이름")] private string m_AttackActionName = "Attack";
    [SerializeField, TabGroup("Component"), LabelText("공격 트리거")] private AttackTrigger m_AttackTrigger;

    [Title("감정")]
    [SerializeField, TabGroup("Option"), LabelText("감정 입력 이름")] private string m_EmotionActionName = "Emotion";
    #endregion
    #region Get,Set
    /// <summary>물리 컴포넌트</summary>
    public CharacterPhysics3D Physics => m_Physics;
    /// <summary>FSM</summary>
    public FSM FSM => m_FSM;
    /// <summary>애니메이터</summary>
    public Animator Animator => m_Animator;
    /// <summary>현재 이동 입력</summary>
    public Vector2 MoveInput { get; private set; }
    /// <summary>점프 버튼 눌림 (프레임 단위)</summary>
    public bool IsJumpPressed { get; private set; }
    /// <summary>점프 버튼 홀드 중</summary>
    public bool IsJumpHeld { get; private set; }
    /// <summary>공격 버튼 눌림 (프레임 단위)</summary>
    public bool IsAttackPressed { get; private set; }
    /// <summary>감정 버튼 눌림 (프레임 단위)</summary>
    public bool IsEmotionPressed { get; private set; }
    /// <summary>공격 트리거</summary>
    public AttackTrigger AttackTrigger => m_AttackTrigger;
    /// <summary>마지막 안전 위치 (지면 위 있을 때 갱신)</summary>
    public Vector3 LastSafePos { get; private set; }
    /// <summary>신발 슬롯 (왼쪽)</summary>
    public Transform SlotShoesLeft => m_SlotShoesLeft;
    /// <summary>신발 슬롯 (오른쪽)</summary>
    public Transform SlotShoesRight => m_SlotShoesRight;
    #endregion

    #region Event
    public void Init()
    {
        LastSafePos = transform.position;
        m_Physics.Init();
        m_FSM.Init(this);

        var input = LocalInputManager.instance;
        input.Create(LocalPlayerActorManager.instance, m_MoveActionName, LocalInputManager.EPriority.Game, OnInputMove);
        input.Create(LocalPlayerActorManager.instance, m_JumpActionName, LocalInputManager.EPriority.Game, OnInputJump);
        input.Create(LocalPlayerActorManager.instance, m_AttackActionName, LocalInputManager.EPriority.Game, OnInputAttack);
        input.Create(LocalPlayerActorManager.instance, m_EmotionActionName, LocalInputManager.EPriority.Game, OnInputEmotion);
    }

    private void Update()
    {
        if (m_Physics.FlyState == CharacterPhysicsBase.EFlyState.None)
            LastSafePos = transform.position;

        if (transform.position.y < m_FallThresholdY)
            m_FSM.Set(m_FSM.GetState(m_FallRecoveryStateID));
    }

    private void LateUpdate()
    {
        IsJumpPressed = false;
        IsAttackPressed = false;
        IsEmotionPressed = false;
    }
    #endregion

    #region Input Callback
    private bool OnInputMove(InputAction.CallbackContext _context)
    {
        if (_context.performed)
            MoveInput = _context.ReadValue<Vector2>();
        else if (_context.canceled)
            MoveInput = Vector2.zero;
        return false;
    }

    private bool OnInputJump(InputAction.CallbackContext _context)
    {
        if (_context.performed)
        {
            IsJumpPressed = true;
            IsJumpHeld = true;
        }
        else if (_context.canceled)
            IsJumpHeld = false;
        return false;
    }

    private bool OnInputAttack(InputAction.CallbackContext _context)
    {
        if (_context.performed)
            IsAttackPressed = true;
        return false;
    }

    private bool OnInputEmotion(InputAction.CallbackContext _context)
    {
        if (_context.performed)
            IsEmotionPressed = true;
        return false;
    }
    #endregion

    #region Function
    /// <summary>장비 슬롯 Transform 반환 (Shoes 제외)</summary>
    public Transform GetSlot(Table_Item.EKind _kind) => _kind switch
    {
        Table_Item.EKind.Sword => m_SlotSword,
        Table_Item.EKind.Hat => m_SlotHat,
        Table_Item.EKind.Wing => m_SlotWing,
        _ => null,
    };

    /// <summary>카메라 방향 기반 이동 벡터 계산</summary>
    public Vector2 GetCameraMoveDir()
    {
        Transform camRot = LocalCameraManager.instance.RotRoot;
        Vector3 forward = camRot.forward;
        Vector3 right = camRot.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = forward * MoveInput.y + right * MoveInput.x;
        return new Vector2(moveDir.x, moveDir.z);
    }

    /// <summary>이동 방향으로 캐릭터 회전</summary>
    public void RotateTowardMoveDir(Vector2 moveDir)
    {
        if (moveDir == Vector2.zero) return;

        Quaternion targetRot = Quaternion.LookRotation(new Vector3(moveDir.x, 0, moveDir.y));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, m_RotSpeed * Time.deltaTime);
    }
    #endregion
}
