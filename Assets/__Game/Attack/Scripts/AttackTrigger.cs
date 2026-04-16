using System.Collections.Generic;
using UnityEngine;

/// <summary>공격 트리거, 활성 중 진입한 IAttackable을 중복 없이 공격</summary>
public class AttackTrigger : MonoBehaviour
{
    #region Value
    private HashSet<IAttackable> m_Attacked = new();
    private PlayerActor m_Actor;
    private bool m_IsActive;
    #endregion

    #region Function
    /// <summary>공격 시작, 중복 목록 초기화</summary>
    public void Begin(PlayerActor _actor)
    {
        m_Actor = _actor;
        m_Attacked.Clear();
        m_IsActive = true;
    }

    /// <summary>공격 종료</summary>
    public void End()
    {
        m_IsActive = false;
    }
    #endregion

    #region Event
    private void OnTriggerStay(Collider other)
    {
        if (!m_IsActive) return;
        var attackable = other.GetComponent<IAttackable>();
        if (attackable != null && m_Attacked.Add(attackable))
            attackable.OnAttacked(m_Actor);
    }
    #endregion
}
