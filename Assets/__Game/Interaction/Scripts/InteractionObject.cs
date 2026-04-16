using UnityEngine;

/// <summary>상호작용 가능한 오브젝트 베이스 클래스</summary>
public abstract class InteractionObject : ObjectBase
{
    #region Inspector
    [SerializeField] private Transform m_InteractPos;
    #endregion
    #region Get,Set
    /// <summary>상호작용 UI 표시 위치</summary>
    public Transform InteractPos => m_InteractPos;
    /// <summary>상호작용 이름</summary>
    public abstract Table_Text.Data Name { get; }
    /// <summary>상호작용 아이콘</summary>
    public abstract Sprite Icon { get; }
    /// <summary>상호작용 가능 여부</summary>
    public bool IsInteractable { get; private set; } = true;
    #endregion

    #region Event
    private void Start()
    {
        LocalInteractionManager.instance.Register(this);
    }
    #endregion

    #region Function
    /// <summary>상호작용 실행</summary>
    public abstract void OnInteract(PlayerActor _actor);
    /// <summary>매니저 Init 시점에 호출, 하위 클래스에서 초기화 로직 구현</summary>
    public virtual void OnRegister() { }
    /// <summary>상호작용 활성/비활성 설정, 비활성 시 매니저에서 제거</summary>
    public void SetInteractable(bool _value)
    {
        IsInteractable = _value;
        if (!_value && LocalInteractionManager.instance != null)
            LocalInteractionManager.instance.Remove(this);
    }
    #endregion
}
