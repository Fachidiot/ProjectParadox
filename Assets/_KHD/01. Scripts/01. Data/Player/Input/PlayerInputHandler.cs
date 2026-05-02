/// <summary>
/// InputReader로부터 전달받은 원시 입력을 시스템에서 사용하기 쉬운 상태 값이나 프로퍼티로 가공하는 컴포넌트.
/// UI 상태나 시스템적 입력 차단 여부를 소유하여 최종적인 게임플레이 입력 가능 여부를 결정합니다.
/// </summary>

using UnityEngine;
using Custom.Inputs;

public class PlayerInputHandler : MonoBehaviour
{
    #region 설정 및 내부 상태
    [Header("참조")]
    [SerializeField] private InputReader _inputReader;

    private Vector2 _moveInput;
    private Vector2 _lookInput;
    #endregion

    #region 입력 상태 및 속성
    /// <summary>
    /// 강제로 모든 게임플레이 입력을 차단하고 싶을 때 사용 (예: 시네마틱).
    /// </summary>
    public bool SuppressGameplayInput { get; set; } = false;

    /// <summary>
    /// 현재 입력을 처리할 수 없는 상태(메뉴 오픈, 메시지 팝업 등)인지 확인합니다.
    /// </summary>
    private bool IsInputBlocked => SuppressGameplayInput || (UIManager.Instance != null && UIManager.Instance.IsPopupOpen);

    // 가공된 입력 값들
    public Vector2 MoveInput => IsInputBlocked ? Vector2.zero : _moveInput;
    public Vector2 LookInput => IsInputBlocked ? Vector2.zero : _lookInput;
    
    // 트리거 성격의 입력 (소모성)
    public bool IsJumpPressed { get; private set; }
    public bool IsDashPressed { get; private set; }
    public bool IsInteractPressed { get; private set; }
    public bool IsActionPressed { get; private set; }
    public bool IsAimedPressed { get; private set; }
    public bool IsEmotionPressed { get; private set; }
    
    // 상태 성격의 입력
    public bool IsJumpHeld { get; private set; }
    public bool CrouchTriggered { get; set; }
    public bool PreviousTriggered { get; private set; }
    public bool NextTriggered { get; private set; }
    public bool SwitchSideTriggered { get; private set; }
    public bool ActionReleasedTriggered { get; private set; }
    public bool ActionTriggered { get; private set; }

    public Vector2 PlayerScrollInput { get; private set; }

    public bool Emotion1Triggered { get; private set; }
    public bool Emotion2Triggered { get; private set; }
    public bool Emotion3Triggered { get; private set; }
    public bool Emotion4Triggered { get; private set; }
    public bool Emotion5Triggered { get; private set; }
    public bool Emotion6Triggered { get; private set; }
    public bool Emotion7Triggered { get; private set; }
    public bool Emotion8Triggered { get; private set; }
    public bool Emotion9Triggered { get; private set; }

    /// <summary>
    /// 수평면(XZ) 기준의 정규화된 이동 방향을 반환합니다.
    /// </summary>
    public Vector3 MoveDirection => new Vector3(MoveInput.x, 0, MoveInput.y).normalized;
    #endregion

    #region 초기화 및 해제 (이벤트 구독)
    private void OnEnable()
    {
        if (_inputReader == null)
        {
            CustomLog.LogError("[PlayerInputHandler] InputReader가 연결되지 않았습니다!");
            return;
        }

        _inputReader.MoveEvent += OnMove;
        _inputReader.LookEvent += OnLook;
        _inputReader.JumpEvent += OnJump;
        _inputReader.SprintEvent += OnDash;
        _inputReader.CrouchEvent += OnCrouch;
        _inputReader.InteractEvent += OnInteract;
        _inputReader.PreviousEvent += OnPrevious;
        _inputReader.NextEvent += OnNext;
        _inputReader.AttackEvent += OnAction;
        _inputReader.AimedEvent += OnAimed;
        _inputReader.EmotionEvent += OnEmotion;
        _inputReader.SwitchSideEvent += OnSwitchSide;
        _inputReader.PlayerScrollWheelEvent += OnPlayerScrollWheel;

        _inputReader.Emotion1Event += OnEmotion1;
        _inputReader.Emotion2Event += OnEmotion2;
        _inputReader.Emotion3Event += OnEmotion3;
        _inputReader.Emotion4Event += OnEmotion4;
        _inputReader.Emotion5Event += OnEmotion5;
        _inputReader.Emotion6Event += OnEmotion6;
        _inputReader.Emotion7Event += OnEmotion7;
        _inputReader.Emotion8Event += OnEmotion8;
        _inputReader.Emotion9Event += OnEmotion9;
    }

    private void OnDisable()
    {
        if (_inputReader == null) return;

        _inputReader.MoveEvent -= OnMove;
        _inputReader.LookEvent -= OnLook;
        _inputReader.JumpEvent -= OnJump;
        _inputReader.SprintEvent -= OnDash;
        _inputReader.CrouchEvent -= OnCrouch;
        _inputReader.InteractEvent -= OnInteract;
        _inputReader.PreviousEvent -= OnPrevious;
        _inputReader.NextEvent -= OnNext;
        _inputReader.AttackEvent -= OnAction;
        _inputReader.AimedEvent -= OnAimed;
        _inputReader.EmotionEvent -= OnEmotion;
        _inputReader.SwitchSideEvent -= OnSwitchSide;
        _inputReader.PlayerScrollWheelEvent -= OnPlayerScrollWheel;

        _inputReader.Emotion1Event -= OnEmotion1;
        _inputReader.Emotion2Event -= OnEmotion2;
        _inputReader.Emotion3Event -= OnEmotion3;
        _inputReader.Emotion4Event -= OnEmotion4;
        _inputReader.Emotion5Event -= OnEmotion5;
        _inputReader.Emotion6Event -= OnEmotion6;
        _inputReader.Emotion7Event -= OnEmotion7;
        _inputReader.Emotion8Event -= OnEmotion8;
        _inputReader.Emotion9Event -= OnEmotion9;
    }
    #endregion

    #region 고정 업데이트 (트리거 리셋)
    private void FixedUpdate()
    {
        // 물리 업데이트 주기에 맞춰 단발성 입력 상태를 리셋하여 "소모성" 입력을 보장함
        IsJumpPressed = false;
        PreviousTriggered = false;
        NextTriggered = false;
        SwitchSideTriggered = false;
        ActionTriggered = false;
        IsInteractPressed = false;
        IsEmotionPressed = false;
        ActionReleasedTriggered = false;
        PlayerScrollInput = Vector2.zero;

        Emotion1Triggered = false;
        Emotion2Triggered = false;
        Emotion3Triggered = false;
        Emotion4Triggered = false;
        Emotion5Triggered = false;
        Emotion6Triggered = false;
        Emotion7Triggered = false;
        Emotion8Triggered = false;
        Emotion9Triggered = false;
    }
    #endregion

    #region 입력 이벤트 콜백 (InputReader로부터 수신)
    private void OnMove(Vector2 input) => _moveInput = input;
    private void OnLook(Vector2 input) => _lookInput = input;

    private void OnJump(bool active)
    {
        if (IsInputBlocked) { IsJumpPressed = false; IsJumpHeld = false; return; }
        IsJumpHeld = active;
        if (active) IsJumpPressed = true;
    }
    private void OnDash(bool active) => IsDashPressed = !IsInputBlocked && active;
    private void OnInteract() { if (!IsInputBlocked) IsInteractPressed = true; }
    private void OnEmotion() { if (!IsInputBlocked) IsEmotionPressed = true; }

    private void OnCrouch() { if (!IsInputBlocked) CrouchTriggered = !CrouchTriggered; }

    private void OnPrevious() { if (!IsInputBlocked) PreviousTriggered = true; }
    private void OnNext() { if (!IsInputBlocked) NextTriggered = true; }
    private void OnSwitchSide() { if (!IsInputBlocked) SwitchSideTriggered = true; }
    private void OnPlayerScrollWheel(Vector2 input) => PlayerScrollInput = IsInputBlocked ? Vector2.zero : input;

    private void OnAction(bool active)
    {
        if (IsInputBlocked) { IsActionPressed = false; ActionTriggered = false; return; }
        
        IsActionPressed = active;
        if (active) ActionTriggered = true; // 버튼을 누른 순간
        if (!active) ActionReleasedTriggered = true; // 버튼을 뗀 순간
    }

    private void OnAimed(bool active) => IsAimedPressed = !IsInputBlocked && active;

    private void OnEmotion1() { if (!IsInputBlocked) Emotion1Triggered = true; }
    private void OnEmotion2() { if (!IsInputBlocked) Emotion2Triggered = true; }
    private void OnEmotion3() { if (!IsInputBlocked) Emotion3Triggered = true; }
    private void OnEmotion4() { if (!IsInputBlocked) Emotion4Triggered = true; }
    private void OnEmotion5() { if (!IsInputBlocked) Emotion5Triggered = true; }
    private void OnEmotion6() { if (!IsInputBlocked) Emotion6Triggered = true; }
    private void OnEmotion7() { if (!IsInputBlocked) Emotion7Triggered = true; }
    private void OnEmotion8() { if (!IsInputBlocked) Emotion8Triggered = true; }
    private void OnEmotion9() { if (!IsInputBlocked) Emotion9Triggered = true; }
    #endregion
}