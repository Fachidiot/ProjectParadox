using Sirenix.OdinInspector;
using Custom.Inputs;
using UnityEngine;

[DefaultExecutionOrder(-50)]
public class UIInputHandler : MonoBehaviour
{
    [SerializeField, TabGroup("Reference")]
    private InputReader _inputReader;

    [Title("UI 입력 데이터")]
    [ShowInInspector, ReadOnly]
    private Vector2 _mousePosition;
    [ShowInInspector, ReadOnly]
    private Vector2 _scrollInput;

    // 가공된 UI 입력 값들
    public Vector2 MousePosition => _mousePosition;
    public Vector2 ScrollInput => _scrollInput;

    // 트리거 성격의 입력 (소모성)
    public bool IsSubmitTriggered { get; private set; }
    public bool IsEscapeTriggered { get; private set; }
    public bool IsClickTriggered { get; private set; }
    public bool IsRightClickTriggered { get; private set; }
    public bool IsMiddleClickTriggered { get; private set; }

    #region 초기화 및 해제 (이벤트 구독)
    private void OnEnable()
    {
        if (_inputReader == null)
        {
            CustomLog.LogError("[UIInputHandler] InputReader가 연결되지 않았습니다!");
            return;
        }

        _inputReader.SubmitEvent += OnSubmit;
        _inputReader.EscapeEvent += OnEscape;
        _inputReader.MousePositionEvent += OnMousePosition;
        _inputReader.ClickEvent += OnClick;
        _inputReader.RightClickEvent += OnRightClick;
        _inputReader.MiddleClickEvent += OnMiddleClick;
        _inputReader.WheelScrollEvent += OnWheelScroll;
    }

    private void OnDisable()
    {
        if (_inputReader == null) return;

        _inputReader.SubmitEvent -= OnSubmit;
        _inputReader.EscapeEvent -= OnEscape;
        _inputReader.MousePositionEvent -= OnMousePosition;
        _inputReader.ClickEvent -= OnClick;
        _inputReader.RightClickEvent -= OnRightClick;
        _inputReader.MiddleClickEvent -= OnMiddleClick;
        _inputReader.WheelScrollEvent -= OnWheelScroll;
    }
    #endregion

    #region 고정 업데이트 (트리거 리셋)
    private void FixedUpdate()
    {
        // 물리 업데이트 주기에 맞춰 단발성 입력 상태를 리셋
        IsSubmitTriggered = false;
        IsEscapeTriggered = false;
        IsClickTriggered = false;
        IsRightClickTriggered = false;
        IsMiddleClickTriggered = false;
        _scrollInput = Vector2.zero; // 휠 스크롤도 단발성으로 초기화
    }
    #endregion

    #region 입력 이벤트 콜백 (InputReader로부터 수신)
    private void OnSubmit() => IsSubmitTriggered = true;
    private void OnEscape() => IsEscapeTriggered = true;
    private void OnMousePosition(Vector2 pos) => _mousePosition = pos;
    private void OnClick() => IsClickTriggered = true;
    private void OnRightClick() => IsRightClickTriggered = true;
    private void OnMiddleClick() => IsMiddleClickTriggered = true;
    private void OnWheelScroll(Vector2 delta) => _scrollInput = delta;
    #endregion
}
