using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Toggle 그룹과 컨텐츠 패널을 연동하여 탭 UI를 구현</summary>
public class Control_Tab : ControlBase
{
    #region Inspector
    [SerializeField] private RectTransform m_TabRoot;
    [SerializeField] private RectTransform m_ToggleRoot;
    [SerializeField] private string m_DefaultTab = "";
    [SerializeField] private bool m_ResetOnOpen = false;
    #endregion
    #region Value
    private Dictionary<string, Control_Toggle> m_TabID = new();
    private Dictionary<Control_Toggle, GameObject> m_Tab = new();
    private Control_Toggle m_Current = null;
    #endregion

    #region Event
    public override void AfterInit()
    {
        var toggleGroup = m_ToggleRoot.GetComponent<ToggleGroup>();
        foreach (var v in m_ToggleRoot.GetComponentsInChildren<Control_Toggle>(true))
        {
            v.v.group = toggleGroup;
            m_TabID.Add(v.name, v);
            v.AddValueChangedListener(OnValueChanged);
        }
        foreach (var v in m_TabRoot.GetComponentsInChildren<RectTransform>(true))
            if (m_TabID.TryGetValue(v.name, out var toggle))
                m_Tab.Add(toggle, v.gameObject);

        m_Current = m_TabID[m_DefaultTab];
        m_Current.SetIsOn(true);
        UpdateToggle();

        base.AfterInit();
    }
    public override void OnOpen()
    {
        base.OnOpen();

        if (m_ResetOnOpen)
        {
            m_Current = m_TabID[m_DefaultTab];
            m_Current.SetIsOn(true);
            UpdateToggle();
        }
    }

    /// <summary>토글 값 변경 시 현재 탭을 업데이트</summary>
    private void OnValueChanged(Control_Toggle _toggle, bool _isOn)
    {
        if (!_isOn)
            return;

        m_Current = _toggle;
        UpdateToggle();
    }
    /// <summary>현재 선택된 탭에 해당하는 컨텐츠만 활성화</summary>
    private void UpdateToggle()
    {
        foreach (var v in m_Tab)
        {
            bool isSelect = v.Key == m_Current;
            v.Value.SetActive(isSelect);
        }
    }
    #endregion
}
