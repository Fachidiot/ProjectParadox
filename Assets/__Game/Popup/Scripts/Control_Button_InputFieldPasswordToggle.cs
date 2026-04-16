using UnityEngine;

/// <summary>클릭 시 연결된 InputField의 비밀번호 표시/숨김을 토글</summary>
[RequireComponent(typeof(Control_Button))]
public class Control_Button_InputFieldPasswordToggle : ControlBase
{
    #region Inspector
    [SerializeField] private Control_InputField m_InputField;
    #endregion
    #region Value
    private Control_Button m_Button;
    #endregion

    #region Event
    protected override void Awake()
    {
        base.Awake();
        m_Button = GetComponent<Control_Button>();
        m_Button.AddClickListener(OnClick);
    }
    /// <summary>클릭 시 InputField의 비밀번호 표시 상태를 토글</summary>
    private void OnClick(Control_Button _)
    {
        m_InputField.TogglePassword();
    }
    #endregion
}
