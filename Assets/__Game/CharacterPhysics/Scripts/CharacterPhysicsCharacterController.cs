using Sirenix.OdinInspector;
using UnityEngine;

public class CharacterPhysicsCharacterController : CharacterPhysicsBase
{
    #region Inspector
    [SerializeField, TabGroup("Component"), LabelText("CharacterController")] private CharacterController m_Controller;
    [SerializeField, TabGroup("Option"), LabelText("시뮬레이션 배율")] private float m_SimScale = 1.0f;
    [SerializeField, TabGroup("Option"), LabelText("최대 이동속도")] private float m_MoveSpeed = 2f;
    [SerializeField, TabGroup("Option"), LabelText("0->최대이속 시간")] private float m_MoveMaxSec = 0.05f;
    [SerializeField, TabGroup("Option"), LabelText("최대이속->0 시간")] private float m_MoveMinSec = 0.05f;
    [SerializeField, TabGroup("Physics"), LabelText("중력가속도")] private float m_Gravity = -9.81f;
    #endregion
    #region Property
    public float SimScale { get => m_SimScale; set => m_SimScale = value; }
    public FloatValue MoveSpeed { get; set; }
    #endregion
    #region Value
    private bool m_IsMovedThisFrame;
    private Vector2 m_MoveVel;
    private float m_MoveEndTimer;
    private Vector2 m_Velocity;
    private float m_UpDown;
    #endregion

    #region Event
    public override void Init()
    {
        base.Init();
        MoveSpeed = new FloatValue(null, "", m_MoveSpeed);
    }
    protected void Update()
    {
        //중력
        m_UpDown += m_Gravity * Time.unscaledDeltaTime * m_SimScale;

        //이동
        if (m_IsMovedThisFrame)
            SetMoveVelocityLerp(m_MoveMaxSec, m_MoveVel * MoveSpeed.v);
        else
        {
            if (0 < m_MoveEndTimer)
            {
                SetMoveVelocityLerp(m_MoveMinSec, Vector2.zero);
                m_MoveEndTimer -= Time.deltaTime;
            }
            else
                m_Velocity = Vector2.zero;
        }

        //적용
        CollisionFlags flag = m_Controller.Move(new Vector3(m_Velocity.x, m_UpDown, m_Velocity.y) * Time.deltaTime * m_SimScale);
        if ((flag & CollisionFlags.Below) != 0)
            m_UpDown = 0;

        m_IsMovedThisFrame = false;
    }
    #endregion
    #region Function
    public void Move(Vector2 _vec, bool _isNow = false)
    {
        m_IsMovedThisFrame = true;
        m_MoveVel = MoveSpeed.v * _vec;
        m_MoveEndTimer = m_MoveMinSec;

        if (_isNow)
            m_Velocity = _vec;
    }
    #endregion
    #region Local Function
    private void SetMoveVelocityLerp(float _changeSec, Vector2 _move)
    {
        if (_changeSec != 0)
        {
            float velX = Mathf.Lerp(m_Velocity.x, _move.x, (MoveSpeed.v / Mathf.Abs(m_Velocity.x - _move.x)) * Time.deltaTime / _changeSec);
            float velY = Mathf.Lerp(m_Velocity.y, _move.y, (MoveSpeed.v / Mathf.Abs(m_Velocity.y - _move.y)) * Time.deltaTime / _changeSec);
            m_Velocity = new Vector2(velX, velY);
        }
        else
            m_Velocity = _move;
    }
    #endregion
}
