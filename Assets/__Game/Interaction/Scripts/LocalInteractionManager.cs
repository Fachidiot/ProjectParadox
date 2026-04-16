using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>상호작용 관리 매니저, InteractionObject 추적 및 InteractUI 배치</summary>
public class LocalInteractionManager : LocalManagerBase
{
    public static LocalInteractionManager instance { get; private set; }

    #if UNITY_EDITOR
    #region Preview
    [Serializable] private struct SPreview
    {
        public string name;
        public SPreview(string _name) { name = _name; }
    }
    [SerializeField, Sirenix.OdinInspector.ReadOnly] private List<SPreview> m_Preview = new();
    #endregion
    #endif
    #region Inspector
    [SerializeField] private GameObject m_InteractUI;
    [SerializeField] private GameObject m_FirstTimeUI;
    [SerializeField] private GameObject m_DotUI;
    #endregion
    #region Get,Set
    /// <summary>상호작용 Factor (읽기 전용)</summary>
    public IReadOnlyOrderedBoolFactor<InteractionObject> Interactables => m_Factor;
    /// <summary>상호작용 가능 여부 (읽기 전용)</summary>
    public IReadOnlyBoolValue CanInteract => m_CanInteract;
    #endregion
    #region Value
    private OrderedBoolFactor<InteractionObject> m_Factor;
    private BoolValue m_CanInteract;
    private Control_Button_Interact m_InteractButton;
    private List<InteractionObject> m_PendingObjects = new();
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }

    public override void Init()
    {
        m_Factor = new OrderedBoolFactor<InteractionObject>(this);
        m_CanInteract = new BoolValue(this, "Interaction.CanInteract", true);

        if (m_InteractUI != null)
        {
            m_InteractButton = m_InteractUI.GetComponentInChildren<Control_Button_Interact>();
            m_InteractUI.SetActive(false);
        }

        var input = LocalInputManager.instance;
        input.Create(this, "Interact", LocalInputManager.EPriority.UI, OnInteractInput);

        m_Factor.AddChanged(this, _ => RefreshUI());
        m_CanInteract.AddChanged(this, _ => RefreshUI());
        SettingManager.instance.UseInteractUI.AddChanged(this, _ => RefreshUI());

        foreach (var obj in m_PendingObjects)
            obj.OnRegister();
        m_PendingObjects.Clear();

        base.Init();
    }

    private void LateUpdate()
    {
        if (m_InteractUI == null || !m_InteractUI.activeSelf) return;

        var first = GetFirst();
        if (first == null || first.InteractPos == null) return;

        m_InteractUI.transform.position = first.InteractPos.position;
    }
    #endregion

    #region Input
    private bool OnInteractInput(InputAction.CallbackContext _context)
    {
        if (!_context.performed) return false;
        if (SettingManager.instance.UseInteractUI.v) return false;
        if (!m_CanInteract.v) return false;

        var first = GetFirst();
        if (first == null) return false;

        var actor = LocalPlayerActorManager.instance.CurActor;
        Interact(first, actor);
        return true;
    }
    #endregion

    #region Function
    /// <summary>상호작용 가능 여부 설정</summary>
    public void SetCanInteract(bool _value)
    {
        m_CanInteract.v = _value;
    }

    /// <summary>InteractionObject 등록, Init 전이면 Pending 처리</summary>
    public void Register(InteractionObject _obj)
    {
        if (!IsInited)
        {
            m_PendingObjects.Add(_obj);
            return;
        }

        _obj.OnRegister();
    }

    /// <summary>InteractionObject 등록</summary>
    public void Add(InteractionObject _interactable)
    {
        if (!_interactable.IsInteractable) return;
        m_Factor.Set(this, _interactable, true);
        #if UNITY_EDITOR
        RefreshPreview();
        #endif
    }

    /// <summary>상호작용 실행 후 비활성화된 대상 제거, 최초 상호작용 시 토글</summary>
    public void Interact(InteractionObject _interactable, PlayerActor _actor)
    {
        if (!InteractionManager.instance.HasInteracted.v)
            InteractionManager.instance.MarkInteracted();

        _interactable.OnInteract(_actor);
        if (!_interactable.gameObject.activeInHierarchy)
            Remove(_interactable);

        RefreshUI();
    }

    /// <summary>InteractionObject 해제</summary>
    public void Remove(InteractionObject _interactable)
    {
        m_Factor.Remove(_interactable);
        #if UNITY_EDITOR
        RefreshPreview();
        #endif
    }

    /// <summary>가장 가까운(첫 번째) InteractionObject 반환</summary>
    private InteractionObject GetFirst()
    {
        var keys = m_Factor.LocalKeys;
        return keys.Count > 0 ? keys[0] : null;
    }

    private void RefreshUI()
    {
        if (m_InteractUI == null) return;

        var first = GetFirst();
        bool show = m_CanInteract.v && first != null && !SettingManager.instance.UseInteractUI.v;
        m_InteractUI.SetActive(show);

        if (show)
        {
            bool isFirst = !InteractionManager.instance.HasInteracted.v;
            if (m_FirstTimeUI != null) m_FirstTimeUI.SetActive(isFirst);
            if (m_DotUI != null) m_DotUI.SetActive(!isFirst);

            if (m_InteractButton != null)
            {
                var actor = LocalPlayerActorManager.instance.CurActor;
                m_InteractButton.Set(first, actor);
            }
        }
    }
    #endregion
    #if UNITY_EDITOR
    #region Editor Function
    private void RefreshPreview()
    {
        m_Preview.Clear();
        foreach (var v in m_Factor.LocalKeys)
            m_Preview.Add(new SPreview(v.gameObject.name));
    }
    #endregion
    #endif
}
