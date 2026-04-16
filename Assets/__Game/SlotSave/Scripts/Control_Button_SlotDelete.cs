using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>클릭 시 확인 팝업을 띄운 뒤 지정 슬롯을 삭제하는 버튼</summary>
[RequireComponent(typeof(Control_Button))]
public class Control_Button_SlotDelete : ControlBase
{
    #region Inspector
    [SerializeField, LabelText("슬롯 번호"), Range(1, 3)] private int m_Slot;
    [SerializeField, LabelText("슬롯 로드 버튼")] private Control_Button_SlotLoad m_SlotLoad;
    [SerializeField, LabelText("슬롯 활성 컨트롤")] private Control_Active_Slot[] m_ActiveSlots;
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

    public override void AfterInit()
    {
        RefreshVisibility();
        base.AfterInit();
    }

    private void OnClick(Control_Button _)
    {
        if (!SlotSaveManager.instance.Exists(m_Slot)) return;

        string title = LanguageManager.instance.Get("Text_Notify_Title");
        string msg = LanguageManager.instance.Get("Text_Notify_SlotDelete");
        string yes = LanguageManager.instance.Get("Text_Common_Yes");
        string no = LanguageManager.instance.Get("Text_Common_No");

        Popup_Notify.instance.Open(new Popup_Notify.SOption(
            title, msg, yes, OnConfirmDelete, no, null));
    }

    private void OnConfirmDelete()
    {
        SlotSaveManager.instance.Delete(m_Slot);
        RefreshVisibility();

        if (m_SlotLoad != null)
            m_SlotLoad.RefreshUI();

        if (m_ActiveSlots != null)
        {
            foreach (var slot in m_ActiveSlots)
            {
                if (slot != null) slot.Refresh();
            }
        }
    }
    #endregion

    #region Function
    /// <summary>슬롯 존재 여부에 따라 삭제 버튼 표시/숨김</summary>
    private void RefreshVisibility()
    {
        gameObject.SetActive(SlotSaveManager.instance.Exists(m_Slot));
    }
    #endregion
}
