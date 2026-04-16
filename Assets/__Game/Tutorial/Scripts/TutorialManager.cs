using System.Collections.Generic;

/// <summary>튜토리얼 클리어 상태 관리 매니저, SlotSave 연동. 팝업 제어는 TutorialBase 쪽에서 담당</summary>
public class TutorialManager : GlobalManagerBase
{
    public static TutorialManager instance { get; private set; }

    #region Get,Set
    /// <summary>현재 활성 중인 튜토리얼 ID (없으면 null)</summary>
    public string ActiveID { get; private set; }
    #endregion
    #region Value
    private Dictionary<string, BoolValue> m_ClearedMap = new();
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }

    public override bool RequireInit()
    {
        return InitUtil.IsInit(new ManagerBase[] { TableManager.instance, SlotSaveManager.instance });
    }

    public override void Init()
    {
        foreach (var id in TableManager.instance.Tutorial.ID)
        {
            var value = new BoolValue(this, $"Tutorial.{id}", false);
            SlotSaveManager.instance.Create(this, value);
            m_ClearedMap[id] = value;
        }
        base.Init();
    }
    #endregion

    #region Function
    /// <summary>해당 튜토리얼 클리어 여부</summary>
    public bool IsCleared(string _id) =>
        m_ClearedMap.TryGetValue(_id, out var v) && v.v;

    /// <summary>해당 튜토리얼이 현재 활성 중인지 여부</summary>
    public bool IsActive(string _id) => ActiveID == _id;

    /// <summary>튜토리얼 활성화 시도. 상태만 관리하며 팝업은 열지 않음</summary>
    public bool TryActivate(string _id)
    {
        if (IsCleared(_id)) return false;
        if (ActiveID != null) return false;
        if (!m_ClearedMap.ContainsKey(_id)) return false;

        ActiveID = _id;
        return true;
    }

    /// <summary>튜토리얼 클리어 처리. 상태만 관리하며 팝업은 닫지 않음</summary>
    public void Complete(string _id)
    {
        if (ActiveID != _id) return;

        ActiveID = null;
        if (m_ClearedMap.TryGetValue(_id, out var v))
            v.v = true;
    }

    /// <summary>튜토리얼 클리어만 처리 (활성화 없이)</summary>
    public void MarkCleared(string _id)
    {
        if (m_ClearedMap.TryGetValue(_id, out var v))
            v.v = true;
    }
    #endregion
}
