using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>현재 FSM 상태에서 전환 가능한 액션 리스트 UI, 상태 변경 시 자동 갱신</summary>
public class Control_ActionList : ControlBase
{
    #region Inspector
    [SerializeField, LabelText("템플릿")] private GameObject m_Template;
    [SerializeField, LabelText("풀 크기")] private int m_PoolSize = 5;
    #endregion
    #region Value
    private ObjectPool m_Pool;
    private FSMState m_PrevState;
    #endregion

    #region Event
    public override void AfterInitGame()
    {
        m_Pool = new ObjectPool(m_Template, m_Template.transform.parent, m_PoolSize);

        foreach (var kv in EquipManager.instance.Specials)
            kv.Value.AddChanged(this, _ => Refresh());

        SettingManager.instance.UseActionUI.AddChanged(this, _ => Refresh());

        base.AfterInitGame();
    }

    private void LateUpdate()
    {
        if (!SettingManager.instance.UseActionUI.v) return;

        var actor = LocalPlayerActorManager.instance?.CurActor;
        if (actor == null) return;

        FSMState curState = null;
        foreach (var s in actor.FSM.CurStates)
        {
            curState = s;
            break;
        }

        if (curState != m_PrevState)
        {
            m_PrevState = curState;
            Refresh();
        }
    }
    #endregion

    #region Function
    private void Refresh()
    {
        m_Pool.Clear();

        if (!SettingManager.instance.UseActionUI.v) return;

        var infos = m_PrevState?.TransitionInfos;
        if (infos == null) return;

        foreach (var (stateID, key) in infos)
        {
            var obj = m_Pool.Get();
            if (obj == null) break;

            obj.transform.SetAsLastSibling();
            obj.GetComponent<Control_ActionTemplate>().Set(stateID, key);
        }
    }
    #endregion
}
