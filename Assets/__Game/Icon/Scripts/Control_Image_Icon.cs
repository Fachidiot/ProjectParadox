using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>IconManager와 연동하여 ID 기반으로 스프라이트를 자동 설정하는 이미지</summary>
[RequireComponent(typeof(Image))]
public class Control_Image_Icon : ControlBase
{
    #region Inspector
    [SerializeField] private string m_ID;
    [SerializeField] private bool m_AutoReflesh = false;
    #endregion
    #region Value
    private Image m_Image;
    private List<ValueBase> m_LinkedValues = new();
    #endregion

    #region Event
    protected override void Awake()
    {
        m_Image = GetComponent<Image>();
        base.Awake();
    }
    public override void AfterInit()
    {
        if (m_AutoReflesh)
            ConnectValue(m_ID);

        UpdateIcon();
        base.AfterInit();
    }
    public override void AfterInitGame()
    {
        if (m_AutoReflesh)
            ConnectValue(m_ID);

        UpdateIcon();
        base.AfterInitGame();
    }

    /// <summary>ID 설정 후 아이콘 갱신</summary>
    public void SetID(string _id)
    {
        m_ID = _id;
        UpdateIcon();
    }

    /// <summary>IconManager에서 ID에 해당하는 스프라이트를 가져와 적용</summary>
    private void UpdateIcon(ValueBase _ = null)
    {
        m_Image.sprite = IconManager.instance.Get(m_ID);
    }
    #endregion
    #region Local Function
    /// <summary>IconManager의 Value에 변경 이벤트를 연결</summary>
    private void ConnectValue(string id)
    {
        var value = IconManager.instance.GetValue(id);
        if (value != null && !m_LinkedValues.Contains(value))
        {
            value.AddChanged(this, UpdateIcon);
            m_LinkedValues.Add(value);
        }
    }
    #endregion
}
