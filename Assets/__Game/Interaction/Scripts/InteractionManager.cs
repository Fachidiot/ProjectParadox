/// <summary>상호작용 글로벌 상태 관리 매니저, 최초 상호작용 여부를 SlotSave로 저장</summary>
public class InteractionManager : GlobalManagerBase
{
    public static InteractionManager instance { get; private set; }

    #region Get,Set
    /// <summary>상호작용 경험 여부 (한 번이라도 상호작용했으면 true)</summary>
    public IReadOnlyBoolValue HasInteracted => m_HasInteracted;
    #endregion

    #region Value
    private BoolValue m_HasInteracted;
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }

    public override void Init()
    {
        m_HasInteracted = new BoolValue(this, "Interaction.HasInteracted", false);
        SlotSaveManager.instance.Create(this, m_HasInteracted);
        base.Init();
    }
    #endregion

    #region Function
    /// <summary>최초 상호작용 완료 처리</summary>
    public void MarkInteracted()
    {
        m_HasInteracted.v = true;
    }
    #endregion
}
