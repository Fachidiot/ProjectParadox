using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>슬롯 존재 여부에 따라 대상 오브젝트의 활성 상태를 설정</summary>
public class Control_Active_Slot : ControlBase
{
    #region Inspector
    [SerializeField, LabelText("슬롯 번호"), Range(1, 3)] private int m_Slot;
    [SerializeField, LabelText("비었을 때 활성화")] private bool m_ActiveOnEmpty = true;
    [SerializeField, LabelText("대상 오브젝트")] private GameObject[] m_Targets;
    #endregion

    #region Event
    public override void AfterInit()
    {
        Refresh();
        base.AfterInit();
    }
    #endregion

    #region Function
    /// <summary>슬롯 상태에 따라 대상 오브젝트 활성 상태 갱신</summary>
    public void Refresh()
    {
        bool empty = !SlotSaveManager.instance.Exists(m_Slot);
        bool active = m_ActiveOnEmpty ? empty : !empty;

        if (m_Targets == null) return;
        foreach (var obj in m_Targets)
        {
            if (obj != null) obj.SetActive(active);
        }
    }
    #endregion
}
