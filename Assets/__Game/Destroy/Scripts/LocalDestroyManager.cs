using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>파괴 오브젝트 관리 매니저, 파괴 상태 저장/복원</summary>
public class LocalDestroyManager : LocalManagerBase
{
    public static LocalDestroyManager instance { get; private set; }

    #if UNITY_EDITOR
    #region Preview
    [Serializable] private struct SPreview
    {
        public string id;
        public bool destroyed;
        public SPreview(string _id, bool _destroyed)
        {
            id = _id;
            destroyed = _destroyed;
        }
    }
    [SerializeField, Sirenix.OdinInspector.ReadOnly] private List<SPreview> m_Preview = new();
    #endregion
    #endif
    #region Value
    private Dictionary<string, BoolValue> m_Destroyed = new();
    private List<DestroyableObject> m_PendingObjects = new();
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }

    public override void Init()
    {
        foreach (var obj in m_PendingObjects)
            RegisterInternal(obj);
        m_PendingObjects.Clear();

        base.Init();
        #if UNITY_EDITOR
        RefreshPreview();
        #endif
    }
    #endregion

    #region Function
    /// <summary>파괴 가능 오브젝트 등록</summary>
    public void Register(DestroyableObject _obj)
    {
        if (!IsInited)
        {
            m_PendingObjects.Add(_obj);
            return;
        }

        RegisterInternal(_obj);
    }

    private void RegisterInternal(DestroyableObject _obj)
    {
        if (m_Destroyed.ContainsKey(_obj.ID))
        {
            LogManager.instance.Error("Destroy", $"중복 ID: {_obj.ID}");
            return;
        }

        var value = new BoolValue(this, $"Destroy.{_obj.ID}", false);
        SlotSaveManager.instance.Create(this, value);
        m_Destroyed[_obj.ID] = value;

        value.AddChanged(this, _ =>
        {
            _obj.ApplyDestroyedState(value.v, true);
            #if UNITY_EDITOR
            RefreshPreview();
            #endif
        });
        _obj.ApplyDestroyedState(value.v, false);
    }

    /// <summary>오브젝트 파괴 처리</summary>
    public void Destroy(string _id)
    {
        if (m_Destroyed.TryGetValue(_id, out var value))
            value.v = true;
    }
    #endregion
    #if UNITY_EDITOR
    #region Editor Function
    private void RefreshPreview()
    {
        m_Preview.Clear();
        foreach (var kv in m_Destroyed)
            m_Preview.Add(new SPreview(kv.Key, kv.Value.v));
    }
    #endregion
    #endif
}
