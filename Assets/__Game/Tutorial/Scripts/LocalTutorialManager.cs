using UnityEngine;

/// <summary>씬 내 TutorialBase 인스턴스 관리 매니저. 직접 자식으로 TutorialBase를 배치, OnUpdate 구동</summary>
public class LocalTutorialManager : LocalManagerBase
{
    public static LocalTutorialManager instance { get; private set; }

    #region Value
    private TutorialBase[] m_Tutorials;
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
        m_Tutorials = GetComponentsInChildren<TutorialBase>(true);
    }

    private void Update()
    {
        if (m_Tutorials == null) return;
        foreach (var t in m_Tutorials)
            t.OnUpdate();
    }

    public override void OnShutdown()
    {
        instance = null;
        base.OnShutdown();
    }
    #endregion
}
