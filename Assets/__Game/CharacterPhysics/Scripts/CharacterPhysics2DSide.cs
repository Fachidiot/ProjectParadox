using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPhysics2DSide : CharacterPhysics2D
{
    #region Inspector
    [SerializeField, TabGroup("Physics"), LabelText("기본 머테리얼")] private PhysicsMaterial2D m_DefaultMat;
    [SerializeField, TabGroup("Physics"), LabelText("이동시 머테리얼")] private PhysicsMaterial2D m_MoveMat;
    [SerializeField, TabGroup("Option"), LabelText("최대 이동속도")] private float m_MoveSpeed = 2f;
    [SerializeField, TabGroup("Option"), LabelText("0->최대이속 시간")] private float m_MoveMaxSec = 0.05f;
    [SerializeField, TabGroup("Option"), LabelText("최대이속->0 시간")] private float m_MoveMinSec = 0.05f;
    [SerializeField, TabGroup("Option"), LabelText("점프 세기")] private float m_JumpPower = 6f;
    [SerializeField, TabGroup("Option"), LabelText("점프시 변경시간 배율")] private float m_JumpMoveChangeFac = 1.0f;
    [SerializeField, TabGroup("Option"), LabelText("바닥 무시 레이어")] private string[] m_IgnoreLayer;
    #endregion
    #region Property
    public EFlyState FlyState => 0 < m_GroundCol.Count ? EFlyState.None : m_FlyState;
    public FloatValue MoveSpeed { get; set; }
    public FloatValue MoveMaxSec { get; private set; }
    public FloatValue MoveMinSec { get; private set; }
    public FloatValue JumpPower { get; private set; }
    public FloatValue GravityScale { get; private set; }
    public FloatValue JumpMoveChangeFac { get; private set; }
    #endregion
    #region Value
    private List<Collider2D> m_GroundCol = new List<Collider2D>();
    private EFlyState m_FlyState;
    private int m_JumpFlyUpdate;
    private bool m_IsMovedThisFrame;
    private float m_MoveVel;
    private float m_MoveEndTimer;
    private Action<EFlyState> m_OnFlyStateChanged;
    #endregion

    #region Event
    public override void Init()
    {
        base.Init();
        m_FlyState = EFlyState.Float;
        MoveSpeed = new FloatValue(null, "", m_MoveSpeed);
        MoveMaxSec = new FloatValue(null, "", m_MoveMaxSec);
        MoveMinSec = new FloatValue(null, "", m_MoveMinSec);
        JumpPower = new FloatValue(null, "", m_JumpPower);
        GravityScale = new FloatValue(null, "", Rig.gravityScale);
        JumpMoveChangeFac = new FloatValue(null, "", m_JumpMoveChangeFac);
    }
    protected override void FixedUpdate()
    {
        //중력 동기화
        Rig.gravityScale = GravityScale.v;

        //이동 물리
        if (m_IsMovedThisFrame)
        {
            if (m_MoveVel < 0 && Rig.linearVelocity.x < m_MoveVel) { }
            else if (0 < m_MoveVel && m_MoveVel < Rig.linearVelocity.x) { }
            else
            {
                float changeSec = MoveMaxSec.v * ((FlyState == EFlyState.Jump) ? JumpMoveChangeFac.v : 1);
                SetMoveVelocityLerp(changeSec, m_MoveVel);
            }
            Rig.sharedMaterial = m_MoveMat;
        }
        else
        {
            if (FlyState == EFlyState.None)
            {
                if (0 < m_MoveEndTimer)
                {
                    SetMoveVelocityLerp(MoveMinSec.v, 0);
                    m_MoveEndTimer -= Time.deltaTime;
                }
                else
                {
                    Rig.linearVelocity = new Vector2(0, Rig.linearVelocity.y);
                    Rig.sharedMaterial = m_DefaultMat;
                }
            }
            else
                Rig.sharedMaterial = m_MoveMat;
        }

        //점프 업데이트
        EFlyState flyState = FlyState;
        if (flyState == EFlyState.Jump || flyState == EFlyState.Fly)
            ++m_JumpFlyUpdate;

        m_IsMovedThisFrame = false;
        base.FixedUpdate();
    }
    private void OnCollisionEnter2D(Collision2D _col)
    {
        Vector2 avgNor = Vector2.zero;
        Vector2 avgPos = Vector2.zero;
        foreach (var v in _col.contacts)
        {
            avgNor += v.normal;
            avgPos += v.point;
        }
        avgNor /= _col.contactCount;
        avgPos /= _col.contactCount;

        if (0 < avgNor.y && avgPos.y < transform.position.y)
            AddGroundCol(_col.collider);
    }
    private void OnCollisionStay2D(Collision2D _col)
    {
        if (5 < m_JumpFlyUpdate || FlyState != EFlyState.Jump && FlyState != EFlyState.Fly)
        {
            Vector2 avgNor = Vector2.zero;
            Vector2 avgPos = Vector2.zero;
            foreach (var v in _col.contacts)
            {
                avgNor += v.normal;
                avgPos += v.point;
            }
            avgNor /= _col.contactCount;
            avgPos /= _col.contactCount;

            if (0 < avgNor.y && avgPos.y < transform.position.y)
                AddGroundCol(_col.collider);
            else
                RemoveGroundCol(_col.collider);
        }
    }
    private void OnCollisionExit2D(Collision2D _col)
    {
        RemoveGroundCol(_col.collider);
    }
    #endregion
    #region Function
    public override void SetVelocity(Vector2 _vec)
    {
        base.SetVelocity(_vec);
        m_IsMovedThisFrame = false;
        if (0 < _vec.y)
        {
            m_FlyState = EFlyState.Fly;
            m_JumpFlyUpdate = 0;
            ClearGroundCol();
        }
    }
    public void Move(float _vec, bool _isNow = false)
    {
        m_IsMovedThisFrame = true;
        m_MoveVel = MoveSpeed.v * _vec;
        m_MoveEndTimer = MoveMinSec.v;

        if (_isNow)
            Rig.linearVelocity = new Vector2(MoveSpeed.v * _vec, Rig.linearVelocity.y);
    }
    public void Jump(float _power = 1.0f)
    {
        Rig.linearVelocity = new Vector2(Rig.linearVelocity.x, JumpPower.v * _power);
        m_FlyState = EFlyState.Jump;
        m_JumpFlyUpdate = 0;
        ClearGroundCol();
    }
    public void Clamp(float _speed)
    {
        Rig.linearVelocity = new Vector2(Mathf.Clamp(Rig.linearVelocity.x, -_speed, _speed), Rig.linearVelocity.y);
    }
    public void ClearGroundCol()
    {
        EFlyState prev = FlyState;
        m_GroundCol.Clear();
        if (prev != FlyState)
            m_OnFlyStateChanged?.Invoke(FlyState);
    }
    public void AddFlyStateChangeEvent(Action<EFlyState> _func, bool _isCallNow = true)
    {
        m_OnFlyStateChanged += _func;
        if (_isCallNow)
            _func(FlyState);
    }
    public void RemoveFlyStateChangeEvent(Action<EFlyState> _func)
    {
        m_OnFlyStateChanged -= _func;
    }
    #endregion
    #region Local Function
    private void SetMoveVelocityLerp(float _changeSec, float _move)
    {
        if (0 < _changeSec)
        {
            float velX = Mathf.Lerp(Rig.linearVelocity.x, _move, (MoveSpeed.v / Mathf.Abs(Rig.linearVelocity.x - _move)) * Time.deltaTime / _changeSec);
            Rig.linearVelocity = new Vector2(velX, Rig.linearVelocity.y);
        }
        else
            Rig.linearVelocity = new Vector2(_move, Rig.linearVelocity.y);
    }
    private void AddGroundCol(Collider2D _col)
    {
        foreach (var v in m_IgnoreLayer)
            if (_col.gameObject.layer == LayerMask.NameToLayer(v))
                return;

        EFlyState prev = FlyState;
        if (!m_GroundCol.Contains(_col))
            m_GroundCol.Add(_col);

        if (prev != FlyState)
        {
            m_FlyState = EFlyState.Float;
            m_OnFlyStateChanged?.Invoke(FlyState);
        }
    }
    private void RemoveGroundCol(Collider2D _col)
    {
        EFlyState prev = FlyState;
        m_GroundCol.Remove(_col);
        if (prev != FlyState)
            m_OnFlyStateChanged?.Invoke(FlyState);
    }
    #endregion
}
