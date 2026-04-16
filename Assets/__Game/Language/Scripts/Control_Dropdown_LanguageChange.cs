using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>언어 선택 드롭다운, 선택 시 LanguageManager 언어 변경</summary>
[RequireComponent(typeof(Control_Dropdown))]
public class Control_Dropdown_LanguageChange : ControlBase
{
    #region Value
    private Control_Dropdown m_Dropdown;
    #endregion

    #region Event
    protected override void Awake()
    {
        base.Awake();
        m_Dropdown = GetComponent<Control_Dropdown>();
        m_Dropdown.AddChangeListener(OnValueChanged);
    }
    public override void AfterInit()
    {
        base.AfterInit();

        m_Dropdown.v.options = new List<TMP_Dropdown.OptionData>();

        foreach (var v in LanguageManager.instance.LanguageList)
        {
            var text = LanguageManager.instance.Get("Text_Common_Language", false, false, v);
            m_Dropdown.v.options.Add(new TMP_Dropdown.OptionData(text));
        }

        m_Dropdown.v.value = LanguageManager.instance.LanguageIndex[LanguageManager.instance.Language.v];
        m_Dropdown.v.RefreshShownValue();
    }
    private void OnValueChanged(Control_Dropdown _dropdown)
    {
        switch (_dropdown.v.value)
        {
            case 1:
                LanguageManager.instance.SetLanguage(SystemLanguage.Korean);
                break;
            case 2:
                LanguageManager.instance.SetLanguage(SystemLanguage.Japanese);
                break;
            default:
                LanguageManager.instance.SetLanguage(SystemLanguage.English);
                break;
        }
    }
    #endregion
}
