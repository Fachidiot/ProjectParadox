/// <summary>
/// Unity New Input System의 데이터를 읽어와 C# 이벤트로 전파하는 ScriptableObject.
/// 입력 장치와 게임 플레이어 간의 가교 역할을 하며, 키 바인딩(Rebinding) 관리 기능도 포함합니다.
/// </summary>

using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Custom.Inputs;

namespace Custom.Inputs
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Input/Input Reader")]
    public class InputReader : ScriptableObject, PlayerActionMap.IPlayerActions, PlayerActionMap.IUIActions
    {
        public static InputReader Instance { get; private set; }

        #region 게임플레이 이벤트
        public event Action<Vector2> MoveEvent;
        public event Action<Vector2> LookEvent;
        public event Action<bool> AttackEvent;
        public event Action<bool> AimedEvent;
        public event Action InteractEvent;
        public event Action CrouchEvent;
        public event Action<bool> JumpEvent;
        public event Action EmotionEvent;
        public event Action<bool> SprintEvent;
        public event Action PreviousEvent;
        public event Action NextEvent;
        public event Action SwitchSideEvent;
        public event Action<Vector2> PlayerScrollWheelEvent;

        // Number Key Emotions
        public event Action Emotion1Event;
        public event Action Emotion2Event;
        public event Action Emotion3Event;
        public event Action Emotion4Event;
        public event Action Emotion5Event;
        public event Action Emotion6Event;
        public event Action Emotion7Event;
        public event Action Emotion8Event;
        public event Action Emotion9Event;
        #endregion

        #region UI 이벤트
        public event Action SubmitEvent;
        public event Action EscapeEvent;
        public event Action<Vector2> MousePositionEvent;
        public event Action ClickEvent;
        public event Action RightClickEvent;
        public event Action MiddleClickEvent;
        public event Action<Vector2> WheelScrollEvent;
        #endregion

        private PlayerActionMap _inputActions;
        public InputActionAsset InputActions => _inputActions.asset;

        /// <summary>
        /// 폴링 방식으로 Escape 키 입력을 확인합니다.
        /// </summary>
        public bool EscapeDown => _inputActions != null && _inputActions.UI.Cancel.WasPressedThisFrame();

        private int _lastEscapeFrame = -1;

        /// <summary>
        /// Escape 입력을 한 번만 소모합니다 (중복 처리 방지).
        /// </summary>
        public bool ConsumeEscape()
        {
            if (EscapeDown && _lastEscapeFrame != Time.frameCount)
            {
                _lastEscapeFrame = Time.frameCount;
                return true;
            }
            return false;
        }

        #region 초기화 및 활성화
        private void OnEnable()
        {
            Instance = this;
            if (_inputActions == null)
            {
                _inputActions = new PlayerActionMap();
                _inputActions.Player.SetCallbacks(this);
                _inputActions.UI.SetCallbacks(this);
            }
            _inputActions.Player.Enable();
            _inputActions.UI.Enable();
        }

        private void OnDisable()
        {
            if (_inputActions != null) _inputActions.Disable();
        }

        private void OnDestroy()
        {
            if (_inputActions != null)
            {
                _inputActions.Player.SetCallbacks(null);
                _inputActions.UI.SetCallbacks(null);
                _inputActions.Dispose();
                _inputActions = null;
            }
        }
        #endregion

        #region IPlayerActions 구현 (Input System 콜백)
        public void OnMove(InputAction.CallbackContext context) => MoveEvent?.Invoke(context.ReadValue<Vector2>());
        public void OnLook(InputAction.CallbackContext context) => LookEvent?.Invoke(context.ReadValue<Vector2>());

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.performed) AttackEvent?.Invoke(true);
            if (context.canceled) AttackEvent?.Invoke(false);
        }

        public void OnAimed(InputAction.CallbackContext context)
        {
            if (context.performed) AimedEvent?.Invoke(true);
            if (context.canceled) AimedEvent?.Invoke(false);
        }

        public void OnInteract(InputAction.CallbackContext context) { if (context.performed) InteractEvent?.Invoke(); }
        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed) JumpEvent?.Invoke(true);
            if (context.canceled) JumpEvent?.Invoke(false);
        }
        public void OnEmotion(InputAction.CallbackContext context) { if (context.performed) EmotionEvent?.Invoke(); }
        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.performed) SprintEvent?.Invoke(true);
            if (context.canceled) SprintEvent?.Invoke(false);
        }
        public void OnCrouch(InputAction.CallbackContext context) { if (context.performed) CrouchEvent?.Invoke(); }
        public void OnPrevious(InputAction.CallbackContext context) { if (context.performed) PreviousEvent?.Invoke(); }
        public void OnNext(InputAction.CallbackContext context) { if (context.performed) NextEvent?.Invoke(); }

        public void OnSwitchSide(InputAction.CallbackContext context) { if (context.performed) SwitchSideEvent?.Invoke(); }
        public void OnScrollWheel(InputAction.CallbackContext context) { PlayerScrollWheelEvent?.Invoke(context.ReadValue<Vector2>()); }

        public void OnEmotion1(InputAction.CallbackContext context) { if (context.performed) Emotion1Event?.Invoke(); }
        public void OnEmotion2(InputAction.CallbackContext context) { if (context.performed) Emotion2Event?.Invoke(); }
        public void OnEmotion3(InputAction.CallbackContext context) { if (context.performed) Emotion3Event?.Invoke(); }
        public void OnEmotion4(InputAction.CallbackContext context) { if (context.performed) Emotion4Event?.Invoke(); }
        public void OnEmotion5(InputAction.CallbackContext context) { if (context.performed) Emotion5Event?.Invoke(); }
        public void OnEmotion6(InputAction.CallbackContext context) { if (context.performed) Emotion6Event?.Invoke(); }
        public void OnEmotion7(InputAction.CallbackContext context) { if (context.performed) Emotion7Event?.Invoke(); }
        public void OnEmotion8(InputAction.CallbackContext context) { if (context.performed) Emotion8Event?.Invoke(); }
        public void OnEmotion9(InputAction.CallbackContext context) { if (context.performed) Emotion9Event?.Invoke(); }
        #endregion

        #region IUIActions 구현 (Input System 콜백)
        public void OnSubmit(InputAction.CallbackContext context) { if (context.performed) SubmitEvent?.Invoke(); }
        public void OnCancel(InputAction.CallbackContext context) { if (context.performed) EscapeEvent?.Invoke(); }
        public void OnPoint(InputAction.CallbackContext context) => MousePositionEvent?.Invoke(context.ReadValue<Vector2>());
        public void OnClick(InputAction.CallbackContext context) { if (context.performed) ClickEvent?.Invoke(); }
        public void OnRightClick(InputAction.CallbackContext context) { if (context.performed) RightClickEvent?.Invoke(); }
        public void OnMiddleClick(InputAction.CallbackContext context) { if (context.performed) MiddleClickEvent?.Invoke(); }
        #endregion

        #region 키 바인딩 (Rebinding) 로직
        /// <summary>
        /// 런타임에서 특정 액션의 키 바인딩을 변경합니다.
        /// </summary>
        public void StartRebinding(string actionName, int bindingIndex, Action onComplete, Action onCancel = null)
        {
            var action = _inputActions.FindAction(actionName);
            action.Disable();

            action.PerformInteractiveRebinding(bindingIndex)
                .WithCancelingThrough("<Keyboard>/escape")
                .OnComplete(op =>
                {
                    op.Dispose();
                    action.Enable();
                    SaveBindings();
                    onComplete?.Invoke();
                })
                .OnCancel(op =>
                {
                    op.Dispose();
                    action.Enable();
                    onCancel?.Invoke();
                })
                .Start();
        }

        public void SaveBindings()
        {
            var json = _inputActions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString("InputBindings", json);
            PlayerPrefs.Save();
            CustomLog.Log($"[InputReader] 바인딩 저장 완료: {json}");
        }

        public void LoadBindings()
        {
            if (PlayerPrefs.HasKey("InputBindings"))
            {
                var json = PlayerPrefs.GetString("InputBindings");
                _inputActions.asset.LoadBindingOverridesFromJson(json);
            }
        }

        /// <summary>
        /// 현재 바인딩된 키의 물리적 이름을 문자열로 반환합니다 (예: "LMB", "E").
        /// </summary>
        public string GetBindingName(string actionName, int bindingIndex)
        {
            var action = _inputActions.FindAction(actionName);
            if (action == null || action.bindings.Count <= bindingIndex) return "";

            var binding = action.bindings[bindingIndex];
            string path = binding.hasOverrides ? binding.overridePath : binding.path;

            if (string.IsNullOrEmpty(path)) return "";

            int slashIndex = path.LastIndexOf('/');
            if (slashIndex >= 0 && slashIndex < path.Length - 1)
            {
                string keyName = path.Substring(slashIndex + 1).ToUpper();
                if (keyName == "LEFTBUTTON") return "LMB";
                if (keyName == "RIGHTBUTTON") return "RMB";
                return keyName;
            }
            return "";
        }
        #endregion
    }
}