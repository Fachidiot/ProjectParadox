using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>클릭 시 지정 슬롯을 로드(또는 신규 생성)하고 GameScene으로 전환하는 버튼</summary>
[RequireComponent(typeof(Control_Button))]
public class Control_Button_SlotLoad : ControlBase
{
    #region Inspector
    [SerializeField, LabelText("슬롯 번호"), Range(1, 3)] private int m_Slot;
    [SerializeField, LabelText("저장 있을 때 활성화")] private GameObject m_SaveExist;
    [SerializeField, LabelText("저장 없을 때 활성화")] private GameObject m_SaveEmpty;
    #endregion
    #region Value
    private Control_Button m_Button;
    #endregion

    #region Event
    protected override void Awake()
    {
        base.Awake();
        m_Button = GetComponent<Control_Button>();
        m_Button.AddClickListener(OnClick);
    }

    public override void AfterInit()
    {
        RefreshUI();
        base.AfterInit();
    }

    /// <summary>슬롯 존재 여부에 따라 UI 갱신</summary>
    public void RefreshUI()
    {
        bool exists = SlotSaveManager.instance.Exists(m_Slot);
        if (m_SaveExist != null)
            m_SaveExist.SetActive(exists);
        if (m_SaveEmpty != null)
            m_SaveEmpty.SetActive(!exists);
    }

    private void OnClick(Control_Button _)
    {
        string targetScene = SceneChangeManager.instance.GameSceneID;

        if (SlotSaveManager.instance.Exists(m_Slot))
        {
            SlotSaveManager.instance.Load(m_Slot);
            if (!string.IsNullOrEmpty(SlotSaveManager.instance.LoadedSceneName))
                targetScene = SlotSaveManager.instance.LoadedSceneName;
        }
        else
            SlotSaveManager.instance.SetCurrentSlot(m_Slot);

        SceneChangeManager.instance.SceneChange(targetScene);
    }
    #endregion
}
