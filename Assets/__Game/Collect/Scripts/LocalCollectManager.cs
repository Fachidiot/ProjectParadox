using System.Collections.Generic;
using UnityEngine;

/// <summary>수집 오브젝트 관리 매니저, Deal Need로 이미 획득한 오브젝트 비활성화</summary>
public class LocalCollectManager : LocalManagerBase
{
    public static LocalCollectManager instance { get; private set; }

    #region Value
    private List<CollectableObject> m_Objects = new();
    private List<CollectableObject> m_PendingObjects = new();
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
    }
    #endregion

    #region Function
    public void Register(CollectableObject _obj)
    {
        if (!IsInited)
        {
            m_PendingObjects.Add(_obj);
            return;
        }

        RegisterInternal(_obj);
    }

    private void RegisterInternal(CollectableObject _obj)
    {
        m_Objects.Add(_obj);

        if (ItemManager.instance.Items.TryGetValue(_obj.ItemID, out var value))
        {
            value.AddChanged(this, _ =>
            {
                bool have = DealManager.instance.Need(new SDeal(_obj.ItemID, "", 1));
                _obj.gameObject.SetActive(!have);
            });
        }

        bool alreadyHave = DealManager.instance.Need(new SDeal(_obj.ItemID, "", 1));
        _obj.gameObject.SetActive(!alreadyHave);
    }
    #endregion
}
