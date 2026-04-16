using Sirenix.OdinInspector;
using UnityEngine;

public abstract class CharacterPhysics2D : CharacterPhysicsBase
{
    #region Inspector
    [SerializeField, TabGroup("Component"), LabelText("Rigidbody2D")] private Rigidbody2D m_Rig;
    [SerializeField, TabGroup("Option"), LabelText("Velocity제한")] private Vector2 m_LimitVel = new Vector2(float.MaxValue, float.MaxValue);
    #endregion
    #region Property
    public Rigidbody2D Rig => m_Rig;
    public Vector2 LimitVelocity { get => m_LimitVel; set => m_LimitVel = value; }
    #endregion

    #region Event
    protected virtual void FixedUpdate()
    {
        m_Rig.linearVelocity = new Vector2(
            Mathf.Clamp(m_Rig.linearVelocity.x, -m_LimitVel.x, m_LimitVel.x),
            Mathf.Clamp(m_Rig.linearVelocity.y, -m_LimitVel.y, m_LimitVel.y));
    }
    #endregion
    #region Function
    public virtual void SetVelocity(Vector2 _vec)
    {
        m_Rig.linearVelocity = _vec;
    }
    #endregion
}
