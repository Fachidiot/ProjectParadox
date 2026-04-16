using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>게임 조건/플래그 관리 매니저, BoolValue 기반 SlotSave 저장</summary>
public class ConditionManager : GlobalManagerBase
{
    public static ConditionManager instance { get; private set; }

    #if UNITY_EDITOR
    #region Preview
    [Serializable] private struct SPreview
    {
        public string id;
        public bool value;
        public SPreview(string _id, bool _value) { id = _id; value = _value; }
    }
    [SerializeField, Sirenix.OdinInspector.ReadOnly] private List<SPreview> m_Preview = new();
    #endregion
    #endif

    #region Get,Set
    public IReadOnlyBoolValue IsSwordUnlocked => m_IsSwordUnlocked;
    public IReadOnlyBoolValue HasCheckedSword => m_HasCheckedSword;
    public IReadOnlyBoolValue HasSeenTutorial => m_HasSeenTutorial;
    #endregion

    #region Value
    private BoolValue m_IsSwordUnlocked;
    private BoolValue m_HasCheckedSword;
    private BoolValue m_HasSeenTutorial;
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }

    public override void Init()
    {
        m_IsSwordUnlocked = new BoolValue(this, "Condition.IsSwordUnlocked", false);
        m_HasCheckedSword = new BoolValue(this, "Condition.HasCheckedSword", false);
        m_HasSeenTutorial = new BoolValue(this, "Condition.HasSeenTutorial", false);

        SlotSaveManager.instance.Create(this, m_IsSwordUnlocked);
        SlotSaveManager.instance.Create(this, m_HasCheckedSword);
        SlotSaveManager.instance.Create(this, m_HasSeenTutorial);

        #if UNITY_EDITOR
        m_IsSwordUnlocked.AddChanged(this, _ => RefreshPreview());
        m_HasCheckedSword.AddChanged(this, _ => RefreshPreview());
        m_HasSeenTutorial.AddChanged(this, _ => RefreshPreview());
        #endif

        base.Init();
        #if UNITY_EDITOR
        RefreshPreview();
        #endif
    }
    #endregion

    #region Function
    public void SetSwordUnlocked(bool _value) => m_IsSwordUnlocked.v = _value;
    public void SetCheckedSword(bool _value) => m_HasCheckedSword.v = _value;
    public void SetSeenTutorial(bool _value) => m_HasSeenTutorial.v = _value;
    #endregion

    #if UNITY_EDITOR
    #region Editor Function
    private void RefreshPreview()
    {
        m_Preview.Clear();
        m_Preview.Add(new SPreview("IsSwordUnlocked", m_IsSwordUnlocked.v));
        m_Preview.Add(new SPreview("HasCheckedSword", m_HasCheckedSword.v));
        m_Preview.Add(new SPreview("HasSeenTutorial", m_HasSeenTutorial.v));
    }
    #endregion
    #endif
}
