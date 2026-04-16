using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>상호작용 버튼, 아이콘과 이름 표시 및 클릭 시 OnInteract 호출</summary>
[RequireComponent(typeof(Control_Button))]
public class Control_Button_Interact : ControlBase
{
    #region Inspector
    [SerializeField] private Image m_IconImage;
    [SerializeField] private TMP_Text m_NameText;
    [SerializeField] private GameObject m_HighlightObject;
    #endregion
    #region Value
    private Control_Button m_Button;
    private InteractionObject m_Interactable;
    private PlayerActor m_Actor;
    #endregion

    #region Event
    protected override void Awake()
    {
        base.Awake();
        m_Button = GetComponent<Control_Button>();
        m_Button.AddClickListener(OnClick);
    }

    private void OnClick(Control_Button _)
    {
        if (m_Interactable != null && m_Actor != null)
            LocalInteractionManager.instance.Interact(m_Interactable, m_Actor);
    }
    #endregion

    #region Function
    /// <summary>상호작용 대상 설정</summary>
    public void Set(InteractionObject _interactable, PlayerActor _actor)
    {
        m_Interactable = _interactable;
        m_Actor = _actor;
        m_IconImage.sprite = _interactable.Icon;
        var name = _interactable.Name;
        m_NameText.text = name != null ? name.Translate(LanguageManager.instance.Language.v, false) : "";
    }

    /// <summary>선택 강조 표시 on/off</summary>
    public void SetHighlight(bool _on)
    {
        if (m_HighlightObject != null)
            m_HighlightObject.SetActive(_on);
    }
    #endregion
}
