using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>카메라 조작 매니저, 줌/회전/좌우전환 입력 처리</summary>
public class LocalCameraControlManager : LocalManagerBase
{
    public static LocalCameraControlManager instance { get; private set; }

    #region Type
    public enum ESide
    {
        Right,
        Left,
    }
    #endregion
    #region Inspector
    [Title("카메라 줌")]
    [SerializeField, TabGroup("Option"), LabelText("줌 속도")] private float m_ZoomSpeed = 5.0f;
    [SerializeField, TabGroup("Option"), LabelText("줌 입력 이름")] private string m_ZoomActionName = "ScrollWheel";

    [Title("카메라 회전")]
    [SerializeField, TabGroup("Option"), LabelText("회전 속도")] private float m_RotateSpeed = 0.2f;
    [SerializeField, TabGroup("Option"), LabelText("마우스 이동 입력 이름")] private string m_LookActionName = "Look";
    [SerializeField, TabGroup("Option"), LabelText("마우스 버튼 입력 이름")] private string m_MouseButtonActionName = "RightClick";

    [Title("카메라 좌우 전환")]
    [SerializeField, TabGroup("Option"), LabelText("전환 입력 이름")] private string m_SwitchSideActionName = "SwitchSide";
    [SerializeField, TabGroup("Option"), LabelText("좌우 오프셋 X")] private float m_SideOffsetX = 2.0f;
    [SerializeField, TabGroup("Option"), LabelText("초기 방향")] private ESide m_InitialSide = ESide.Right;
    #endregion
    #region Get,Set
    /// <summary>현재 좌우 방향</summary>
    public ESide CurrentSide { get; private set; }
    #endregion
    #region Value
    private bool m_IsMouseButtonDown;
    private Vector2 m_LookDelta;
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }

    public override void Init()
    {
        CurrentSide = m_InitialSide;
        ApplySideOffset(false);

        var input = LocalInputManager.instance;
        input.Create(this, m_ZoomActionName, LocalInputManager.EPriority.Game, OnInputZoom);
        input.Create(this, m_LookActionName, LocalInputManager.EPriority.Game, OnInputLook);
        input.Create(this, m_MouseButtonActionName, LocalInputManager.EPriority.Game, OnInputMouseButton);
        input.Create(this, m_SwitchSideActionName, LocalInputManager.EPriority.Game, OnInputSwitchSide);

        base.Init();
    }

    private void Update()
    {
        UpdateCameraRotation();
    }

    private void LateUpdate()
    {
        var actor = LocalPlayerActorManager.instance;
        if (actor != null && actor.CurActor != null)
            LocalCameraManager.instance.transform.position = actor.CurActor.transform.position;
    }

    private void UpdateCameraRotation()
    {
        if (!m_IsMouseButtonDown) return;
        if (m_LookDelta == Vector2.zero) return;

        Quaternion curRot = LocalCameraManager.instance.TargetRot;
        Vector3 euler = curRot.eulerAngles;

        euler.y += m_LookDelta.x * m_RotateSpeed;
        euler.x -= m_LookDelta.y * m_RotateSpeed;

        LocalCameraManager.instance.SetRot(Quaternion.Euler(euler));
        m_LookDelta = Vector2.zero;
    }
    #endregion

    #region Input Callback
    private bool OnInputZoom(InputAction.CallbackContext _context)
    {
        Vector2 scrollValue = _context.ReadValue<Vector2>();
        if (scrollValue.y == 0) return false;

        float curZoom = LocalCameraManager.instance.TargetZoom;
        float newZoom = curZoom - scrollValue.y * m_ZoomSpeed * Time.deltaTime;
        LocalCameraManager.instance.ChangeZoom(newZoom);
        return true;
    }

    private bool OnInputLook(InputAction.CallbackContext _context)
    {
        if (!m_IsMouseButtonDown) return false;

        m_LookDelta += _context.ReadValue<Vector2>();
        return true;
    }

    private bool OnInputMouseButton(InputAction.CallbackContext _context)
    {
        if (_context.performed)
            m_IsMouseButtonDown = true;
        else if (_context.canceled)
        {
            m_IsMouseButtonDown = false;
            m_LookDelta = Vector2.zero;
        }
        return false;
    }

    private bool OnInputSwitchSide(InputAction.CallbackContext _context)
    {
        if (!_context.performed) return false;

        CurrentSide = CurrentSide == ESide.Right ? ESide.Left : ESide.Right;
        ApplySideOffset(true);
        return true;
    }
    #endregion

    #region Function
    /// <summary>현재 방향에 따라 좌우 오프셋 적용</summary>
    private void ApplySideOffset(bool animate)
    {
        Vector3 pos = LocalCameraManager.instance.TargetPos;
        float targetX = CurrentSide == ESide.Right ? m_SideOffsetX : -m_SideOffsetX;
        pos.x = targetX;

        if (animate)
            LocalCameraManager.instance.ChangePos(pos);
        else
            LocalCameraManager.instance.SetPos(pos);
    }

    /// <summary>좌우 방향 설정</summary>
    public void SetSide(ESide side, bool animate = true)
    {
        CurrentSide = side;
        ApplySideOffset(animate);
    }
    #endregion
}
