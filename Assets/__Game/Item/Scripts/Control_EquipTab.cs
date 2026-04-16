using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

/// <summary>장비 탭. 템플릿 복제 + 6좌표 페이징, 최근 획득 순 정렬</summary>
public class Control_EquipTab : ControlBase
{
    public const int PAGE_SIZE = 6;

    #region Inspector
    [SerializeField, LabelText("아이템 템플릿")] private GameObject m_Template;
    [SerializeField, LabelText("배치 좌표 (6개)")] private Transform[] m_Positions;
    [SerializeField, LabelText("이전 페이지 버튼")] private Control_Button m_PrevButton;
    [SerializeField, LabelText("다음 페이지 버튼")] private Control_Button m_NextButton;
    #endregion

    #region Value
    private readonly Dictionary<string, GameObject> m_Slots = new();
    private readonly List<string> m_Sorted = new();
    private int m_Page;
    #endregion

    #region Event
    protected override void Awake()
    {
        base.Awake();
        if (m_PrevButton != null) m_PrevButton.AddClickListener(_ => ChangePage(-1));
        if (m_NextButton != null) m_NextButton.AddClickListener(_ => ChangePage(1));
    }

    public override void AfterInitGame()
    {
        SpawnSlots();

        foreach (var kv in ItemManager.instance.Items)
            kv.Value.AddChanged(this, _ => Refresh());

        if (m_Template != null) m_Template.SetActive(false);
        Refresh();
        base.AfterInitGame();
    }

    public override void OnOpen()
    {
        base.OnOpen();
        Refresh();
    }
    #endregion

    #region Function
    private void SpawnSlots()
    {
        if (m_Template == null) return;

        var parent = m_Template.transform.parent;
        foreach (var id in TableManager.instance.Item.ID)
        {
            var data = TableManager.instance.Get<Table_Item.Data>(id);
            if (data == null) continue;
            if (data.Kind == Table_Item.EKind.Item) continue;

            var go = Instantiate(m_Template, parent);
            go.SetActive(false);

            var img = go.GetComponentInChildren<Image>(true);
            if (img != null)
                img.sprite = IconManager.instance.Get(id);

            m_Slots[id] = go;
        }
    }

    private void Refresh()
    {
        m_Sorted.Clear();
        foreach (var kv in m_Slots)
        {
            if (ItemManager.instance.Has(kv.Key))
                m_Sorted.Add(kv.Key);
        }
        m_Sorted.Sort((a, b) => ItemManager.instance.GetAcquiredTime(b).CompareTo(ItemManager.instance.GetAcquiredTime(a)));

        int maxPage = Mathf.Max(0, (m_Sorted.Count - 1) / PAGE_SIZE);
        m_Page = Mathf.Clamp(m_Page, 0, maxPage);

        foreach (var kv in m_Slots)
            kv.Value.SetActive(false);

        int start = m_Page * PAGE_SIZE;
        int len = Mathf.Min(PAGE_SIZE, m_Positions.Length);
        for (int i = 0; i < len; i++)
        {
            int index = start + i;
            if (index >= m_Sorted.Count) break;

            var go = m_Slots[m_Sorted[index]];
            go.transform.SetParent(m_Positions[i], false);
            go.transform.localPosition = Vector3.zero;
            go.SetActive(true);
        }
    }

    private void ChangePage(int _delta)
    {
        m_Page += _delta;
        Refresh();
    }
    #endregion
}
