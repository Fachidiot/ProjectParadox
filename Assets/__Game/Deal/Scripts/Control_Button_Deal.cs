using System;
using UnityEngine;

/// <summary>재화 거래(요구/지불/교환) 처리 버튼</summary>
[RequireComponent(typeof(Control_Button))]
public class Control_Button_Deal : ControlBase
{
    #region Inspector
    [SerializeField] private SDeal[] m_Need;
    [SerializeField] private SDeal[] m_Pay;
    [SerializeField] private SDeal[] m_Change;

    [SerializeField] private bool m_IsLock;
    [SerializeField] private GameObject m_LockObject;

    [SerializeField] private bool m_IsOpenChangeResult;
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
        base.AfterInit();

        if (0 < m_Need.Length && m_IsLock)
        {
            Action<ValueBase> onChanged = (_) =>
            {
                bool isInteractable = DealManager.instance.NeedAll(m_Need);
                m_Button.SetInteractable(isInteractable);
                if (m_LockObject)
                    m_LockObject.SetActive(!isInteractable);
            };
            foreach (var v in DealManager.instance.NeedAllValue(m_Need))
                v.AddChanged(this, onChanged);
            onChanged(null);
        }
        else
        {
            if (m_LockObject)
                m_LockObject.SetActive(false);
        }
    }

    private void OnClick(Control_Button _)
    {
        if (0 < m_Need.Length && !DealManager.instance.NeedAll(m_Need))
            return;
        if (0 < m_Pay.Length && DealManager.instance.PayAll(m_Pay) == null)
            return;
        if (0 < m_Change.Length)
        {
            var result = DealManager.instance.ChangeAll(m_Change);
            if (m_IsOpenChangeResult)
                Popup_ChangeResult.instance.Open(new Popup_ChangeResult.SOption(result));
        }
    }
    #endregion
}
