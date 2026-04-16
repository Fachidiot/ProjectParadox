using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>클릭 시 지정된 ID의 팝업을 여는 버튼</summary>
[RequireComponent(typeof(Control_Button))]
public class Control_Button_PopupOpen : ControlBase
{
    #region Inspector
    [SerializeField] private string m_ID;
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
    /// <summary>클릭 시 팝업이 닫혀있으면 열기</summary>
    private void OnClick(Control_Button _)
    {
        var popup = LocalPopupManager.instance.Get(m_ID);
        if (!popup.IsOpened)
            LocalPopupManager.instance.Open(m_ID);
    }
    #endregion
}
