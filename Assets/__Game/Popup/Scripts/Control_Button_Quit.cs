using UnityEngine;

/// <summary>클릭 시 애플리케이션을 종료하는 버튼</summary>
[RequireComponent(typeof(Control_Button))]
public class Control_Button_Quit : ControlBase
{
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
    /// <summary>클릭 시 에디터 또는 빌드 환경에 맞게 종료 처리</summary>
    private void OnClick(Control_Button _)
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    #endregion
}
