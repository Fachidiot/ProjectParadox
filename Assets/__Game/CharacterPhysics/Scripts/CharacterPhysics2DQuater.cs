using Sirenix.OdinInspector;
using UnityEngine;

public class CharacterPhysics2DQuater : CharacterPhysics2D
{
    #region Inspector
    [SerializeField, TabGroup("Option"), LabelText("기본 이동속도")] private float m_MoveSpeed = 2f;
    [SerializeField, TabGroup("Option"), LabelText("0->이속 시간")] private float m_MoveMaxSec = 0.05f;
    [SerializeField, TabGroup("Option"), LabelText("이속->0 시간")] private float m_MoveMinSec = 0.05f;
    #endregion
    #region Property
    public FloatValue MoveSpeed { get; private set; }
    public FloatValue MoveMaxSec { get; private set; }
    public FloatValue MoveMinSec { get; private set; }
    #endregion
    #region Value
    private bool m_IsMovedThisFrame;
    private Vector2 m_MoveVel;
    private float m_MoveEndTimer;
    #endregion

    #region Event
    public override void Init()
    {
        base.Init();
        MoveSpeed = new FloatValue(null, "", m_MoveSpeed);
        MoveMaxSec = new FloatValue(null, "", m_MoveMaxSec);
        MoveMinSec = new FloatValue(null, "", m_MoveMinSec);
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (m_IsMovedThisFrame)
            SetMoveVelocityLerp(MoveMaxSec.v, m_MoveVel);
        else
        {
            if (0 < m_MoveEndTimer)
            {
                SetMoveVelocityLerp(MoveMinSec.v, Vector2.zero);
                m_MoveEndTimer -= Time.deltaTime;
            }
            else
                Rig.linearVelocity = Vector2.zero;
        }

        m_IsMovedThisFrame = false;
    }
    #endregion
    #region Function
    public override void SetVelocity(Vector2 _vec)
    {
        base.SetVelocity(_vec);
        m_IsMovedThisFrame = false;
    }
    public void Move(Vector2 _vec, bool _isNow = false)
    {
        m_IsMovedThisFrame = true;
        m_MoveVel = MoveSpeed.v * _vec;
        m_MoveEndTimer = MoveMinSec.v;

        if (_isNow)
            Rig.linearVelocity = _vec;
    }
    #endregion
    #region Local Function
    private void SetMoveVelocityLerp(float _changeSec, Vector2 _move)
    {
        if (_changeSec != 0)
        {
            float velX = Mathf.Lerp(Rig.linearVelocity.x, _move.x, (MoveSpeed.v / Mathf.Abs(Rig.linearVelocity.x - _move.x)) * Time.deltaTime / _changeSec);
            float velY = Mathf.Lerp(Rig.linearVelocity.y, _move.y, (MoveSpeed.v / Mathf.Abs(Rig.linearVelocity.y - _move.y)) * Time.deltaTime / _changeSec);
            Rig.linearVelocity = new Vector2(velX, velY);
        }
        else
            Rig.linearVelocity = _move;
    }
    #endregion
}
