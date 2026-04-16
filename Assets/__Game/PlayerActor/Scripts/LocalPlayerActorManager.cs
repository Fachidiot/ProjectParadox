using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>플레이어 액터 관리 매니저, 위치 저장/로드</summary>
public class LocalPlayerActorManager : LocalManagerBase
{
    public static LocalPlayerActorManager instance { get; private set; }

    #region Inspector
    [SerializeField, TabGroup("Component"), LabelText("플레이어 액터")] private PlayerActor m_PlayerActor;
    #endregion
    #region Get,Set
    /// <summary>현재 플레이어 액터</summary>
    public PlayerActor CurActor => m_PlayerActor;
    #endregion
    #region Value
    private StructValue<Vector3> m_PosValue;
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }

    public override bool RequireInit()
    {
        return InitUtil.IsInit(new ManagerBase[] { LocalInteractionManager.instance });
    }

    public override void Init()
    {
        m_PosValue = new StructValue<Vector3>(this, "PlayerActor.Position");
        SlotSaveManager.instance.Create(this, m_PosValue);
        SlotSaveManager.instance.AddOnBeforeSave(this, OnBeforeSave);

        m_PlayerActor.Init();
        m_PosValue.AddChanged(this, OnPosLoaded);

        if (SlotSaveManager.instance.CurrentSlot >= 0)
            ApplyLoadedPosition();

        base.Init();
    }

    private void OnBeforeSave()
    {
        m_PosValue.Set(m_PlayerActor.transform.position, false, false);
    }

    private void OnPosLoaded(ValueBase _)
    {
        ApplyLoadedPosition();
    }
    #endregion

    #region Function
    /// <summary>저장된 위치를 PlayerActor에 적용</summary>
    public void ApplyLoadedPosition()
    {
        m_PlayerActor.transform.position = m_PosValue.v;
    }
    #endregion
}
