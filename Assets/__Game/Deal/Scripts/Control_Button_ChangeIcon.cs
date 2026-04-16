using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>교환 딜의 아이콘, 수량, 액션 표시 버튼</summary>
[RequireComponent(typeof(Control_Button))]
public class Control_Button_ChangeIcon : ControlBase
{
    #region Inspector
    [SerializeField] private Image m_Icon;
    [SerializeField] private TMP_Text m_CountText;
    [SerializeField] private RectTransform m_ActionRoot;

    [SerializeField] private bool m_IsUnit;
    #endregion
    #region Value
    private Control_Button m_Button;
    private Dictionary<string, GameObject> m_Action = new();
    #endregion

    #region Event
    protected override void Awake()
    {
        base.Awake();
        foreach (var v in m_ActionRoot.GetComponentsInChildren<Transform>())
            if (v != m_ActionRoot)
                m_Action.Add(v.name, v.gameObject);

        m_Button = GetComponent<Control_Button>();
        m_Button.AddClickListener(OnClick);
    }

    private void OnClick(Control_Button _)
    {
    }
    #endregion
    #region Local Function
    private void SetAction(string _id)
    {
        foreach (var v in m_Action)
            v.Value.SetActive(_id == v.Key);
    }
    #endregion
    #region Function
    /// <summary>딜 정보로 아이콘/수량/액션 설정</summary>
    public void Set(SDeal _deal)
    {
        m_Icon.sprite = IconManager.instance.Get(_deal.Key);
        m_CountText.text = $"x{_deal.CountLong.ToStringLong(m_IsUnit)}";
        SetAction(_deal.Action);
    }
    #endregion
}
