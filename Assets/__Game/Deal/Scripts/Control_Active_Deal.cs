using System;
using UnityEngine;

/// <summary>재화 충족 여부에 따라 게임오브젝트 활성화/비활성화 제어</summary>
public class Control_Active_Deal : ControlBase
{
    #region Inspector
    [SerializeField] private SDeal[] m_Need;
    [SerializeField] private GameObject m_Target;
    [SerializeField] private bool m_IsActive = true;
    #endregion

    #region Event
    public override void AfterInit()
    {
        base.AfterInit();

        Action<ValueBase> onChanged = (_) =>
        {
            if (m_IsActive)
                m_Target.SetActive(DealManager.instance.NeedAll(m_Need));
            else
                m_Target.SetActive(!DealManager.instance.NeedAll(m_Need));
        };
        foreach (var v in DealManager.instance.NeedAllValue(m_Need))
            v.AddChanged(this, onChanged);
        onChanged(null);
    }
    #endregion
}
