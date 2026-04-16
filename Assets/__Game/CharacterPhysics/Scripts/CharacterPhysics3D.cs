using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPhysics3D : CharacterPhysicsBase
{
    #region Inspector
    [SerializeField, TabGroup("Component"), LabelText("Rigidbody")] private Rigidbody m_Rig;
    [SerializeField, TabGroup("Component"), LabelText("캐릭터 캡슐")] private CapsuleCollider m_Capsule;
    [SerializeField, TabGroup("Physics"), LabelText("기본 머테리얼")] private PhysicsMaterial m_DefaultMat;
    [SerializeField, TabGroup("Physics"), LabelText("이동시 머테리얼")] private PhysicsMaterial m_MoveMat;
    [SerializeField, TabGroup("Option"), LabelText("Velocity제한")] private Vector3 m_LimitVel = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
    [SerializeField, TabGroup("Option"), LabelText("최대 이동속도")] private float m_MoveSpeed = 2f;
    [SerializeField, TabGroup("Option"), LabelText("0->최대이속 시간")] private float m_MoveMaxSec = 0.05f;
    [SerializeField, TabGroup("Option"), LabelText("최대이속->0 시간")] private float m_MoveMinSec = 0.05f;
    [SerializeField, TabGroup("Option"), LabelText("점프 세기")] private float m_JumpPower = 6f;
    [SerializeField, TabGroup("Option"), LabelText("점프시 이동시간 배율")] private float m_JumpMoveChangeFac = 5f;
    [SerializeField, TabGroup("Option"), LabelText("중력 배율")] private float m_GravityScale = 2f;
    #endregion
    #region Property
    public EFlyState FlyState => 0 < m_GroundCol.Count ? EFlyState.None : m_FlyState;
    public Rigidbody Rig => m_Rig;
    public FloatValue MoveSpeed { get; set; }
    public FloatValue JumpPower { get; private set; }
    public Action<EFlyState> OnFlyStateChanged;
    #endregion
    #region Value
    private List<Collider> m_GroundCol = new List<Collider>();
    private EFlyState m_FlyState;
    private bool m_IsMovedThisFrame;
    private Vector2 m_MoveVel;
    private float m_MoveEndTimer;
    #endregion

    #region Event
    public override void Init()
    {
        base.Init();
        m_FlyState = EFlyState.Float;
        MoveSpeed = new FloatValue(null, "", m_MoveSpeed);
        JumpPower = new FloatValue(null, "", m_JumpPower);
    }
    protected virtual void FixedUpdate()
    {
        if (FlyState == EFlyState.Fly)
        {
            m_Rig.AddForce(-Physics.gravity, ForceMode.Acceleration);
        }
        else if (m_GravityScale != 1f && FlyState != EFlyState.None)
        {
            Vector3 extra = (m_GravityScale - 1f) * Physics.gravity;
            m_Rig.AddForce(extra, ForceMode.Acceleration);
        }

        if (m_IsMovedThisFrame)
        {
            float changeSec = m_MoveMaxSec * ((FlyState == EFlyState.Jump) ? m_JumpMoveChangeFac : 1);
            SetMoveVelocityLerp(changeSec, m_MoveVel * MoveSpeed.v);
            m_Capsule.sharedMaterial = m_MoveMat;
        }
        else
        {
            if (FlyState == EFlyState.None)
            {
                if (0 < m_MoveEndTimer)
                {
                    SetMoveVelocityLerp(m_MoveMinSec, Vector2.zero);
                    m_MoveEndTimer -= Time.deltaTime;
                }
                else
                {
                    m_Rig.linearVelocity = new Vector3(0, m_Rig.linearVelocity.y, 0);
                    m_Capsule.sharedMaterial = m_DefaultMat;
                }
            }
            else
                m_Capsule.sharedMaterial = m_MoveMat;
        }

        //속도제한
        m_Rig.linearVelocity = new Vector3(
            Mathf.Clamp(m_Rig.linearVelocity.x, -m_LimitVel.x, m_LimitVel.x),
            Mathf.Clamp(m_Rig.linearVelocity.y, -m_LimitVel.y, m_LimitVel.y),
            Mathf.Clamp(m_Rig.linearVelocity.z, -m_LimitVel.z, m_LimitVel.z));

        m_IsMovedThisFrame = false;
    }
    private void OnCollisionEnter(Collision _col)
    {
        Vector3 avgPos = Vector3.zero;
        for (int i = 0; i < _col.contactCount; ++i)
            avgPos += _col.contacts[i].point;
        avgPos /= _col.contactCount;

        if (avgPos.y < transform.position.y)
            AddGroundCol(_col.collider);
    }
    private void OnCollisionStay(Collision _col)
    {
        if (FlyState != EFlyState.Jump)
        {
            Vector3 avgPos = Vector3.zero;
            for (int i = 0; i < _col.contactCount; ++i)
                avgPos += _col.contacts[i].point;
            avgPos /= _col.contactCount;

            if (avgPos.y < transform.position.y)
                AddGroundCol(_col.collider);
            else
                RemoveGroundCol(_col.collider);
        }
    }
    private void OnCollisionExit(Collision _col)
    {
        RemoveGroundCol(_col.collider);
    }
    #endregion
    #region Function
    public void Move(Vector2 _vec, bool _isNow = false)
    {
        m_IsMovedThisFrame = true;
        m_MoveVel = MoveSpeed.v * _vec;
        m_MoveEndTimer = m_MoveMinSec;

        if (_isNow)
            m_Rig.linearVelocity = new Vector3(_vec.x, m_Rig.linearVelocity.y, _vec.y);
    }
    public void Jump(float _power = 1.0f)
    {
        m_Rig.linearVelocity = new Vector3(m_Rig.linearVelocity.x, JumpPower.v * _power, m_Rig.linearVelocity.z);
        m_FlyState = EFlyState.Jump;
        ClearGroundCol();
    }
    public void StartFly()
    {
        m_FlyState = EFlyState.Fly;
        ClearGroundCol();
        m_Rig.linearVelocity = new Vector3(m_Rig.linearVelocity.x, 0, m_Rig.linearVelocity.z);
    }
    public void StopFly()
    {
        m_FlyState = EFlyState.Float;
    }
    #endregion
    #region Local Function
    private void SetMoveVelocityLerp(float _changeSec, Vector2 _move)
    {
        if (_changeSec != 0)
        {
            float velX = Mathf.Lerp(m_Rig.linearVelocity.x, _move.x, (MoveSpeed.v / Mathf.Abs(m_Rig.linearVelocity.x - _move.x)) * Time.deltaTime / _changeSec);
            float velZ = Mathf.Lerp(m_Rig.linearVelocity.z, _move.y, (MoveSpeed.v / Mathf.Abs(m_Rig.linearVelocity.z - _move.y)) * Time.deltaTime / _changeSec);
            m_Rig.linearVelocity = new Vector3(velX, m_Rig.linearVelocity.y, velZ);
        }
        else
            m_Rig.linearVelocity = new Vector3(_move.x, m_Rig.linearVelocity.y, _move.y);
    }
    private void AddGroundCol(Collider _col)
    {
        if (!m_GroundCol.Contains(_col))
            m_GroundCol.Add(_col);
        m_FlyState = EFlyState.Float;
    }
    private void RemoveGroundCol(Collider _col)
    {
        EFlyState prev = FlyState;
        m_GroundCol.Remove(_col);
        if (prev != FlyState)
            OnFlyStateChanged?.Invoke(FlyState);
    }
    private void ClearGroundCol()
    {
        EFlyState prev = FlyState;
        m_GroundCol.Clear();
        if (prev != FlyState)
            OnFlyStateChanged?.Invoke(FlyState);
    }
    #endregion
}
