using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>다국어 텍스트 자동 표시, LanguageManager 연동</summary>
[RequireComponent(typeof(TMP_Text))]
public class Control_Text_Language : ControlBase
{
    #region Inspector
    [SerializeField] private string m_ID = "";
    [SerializeField] private string[] m_FormatID;
    [SerializeField] private bool m_UseRich = false;
    [SerializeField] private bool m_AutoReflesh = false;
    #endregion
    #region Value
    private TMP_Text m_Text;
    private List<ValueBase> m_LinkedValues = new();
    #endregion

    #region Event
    protected override void Awake()
    {
        m_Text = GetComponent<TMP_Text>();
        base.Awake();
    }
    public override void AfterInit()
    {
        LanguageManager.instance.Language.AddChanged(this, UpdateText);
        if (m_AutoReflesh)
        {
            ConnectValue(m_ID);
            foreach (var v in m_FormatID)
                ConnectValue(v);
        }
        UpdateText();
        base.AfterInit();
    }

    private void UpdateText(ValueBase _ = null)
    {
        var t = LanguageManager.instance.Get(m_ID, true, m_UseRich);
        string[] format = new string[m_FormatID.Length];
        for (int i = 0; i < m_FormatID.Length; i++)
            format[i] = LanguageManager.instance.Get(m_FormatID[i], true, m_UseRich);

        string text = t;
        if (0 < format.Length)
            text = string.Format(t, format);
        m_Text.text = LanguageManager.instance.UseFormat(text, m_UseRich);
    }
    #endregion
    #region Local Function
    private void ConnectValue(string id)
    {
        var value = LanguageManager.instance.GetValue(id);

        if (value != null && !m_LinkedValues.Contains(value))
        {
            value.AddChanged(this, UpdateText);
            m_LinkedValues.Add(value);
        }
    }
    #endregion
}
