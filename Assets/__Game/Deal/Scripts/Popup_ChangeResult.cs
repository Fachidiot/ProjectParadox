using UnityEngine;

/// <summary>교환 결과 아이템 목록 표시 팝업</summary>
public class Popup_ChangeResult : PopupBase
{
    public static Popup_ChangeResult instance { get; private set; }

    #region Type
    /// <summary>팝업 옵션 데이터</summary>
    public struct SOption
    {
        public SDeal[] deals;
        public int startIndex;

        public SOption(SDeal[] _deals)
        {
            deals = _deals;
            startIndex = 0;
        }
    }
    #endregion

    #region Inspector
    [SerializeField] private GameObject m_ChangeIcon;
    [SerializeField] private int m_IconCount;
    #endregion
    #region Value
    private SOption m_Option;
    private ObjectPool m_IconPool;
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }
    public override void Init(Camera _uiCamera)
    {
        m_IconPool = new ObjectPool(m_ChangeIcon, m_ChangeIcon.transform.parent, m_IconCount);
        base.Init(_uiCamera);
    }

    public override void OnOpen(object _option = null)
    {
        m_IconPool.Clear();

        if (_option != null)
        {
            m_Option = (SOption)_option;

            int lange = Mathf.Min(m_Option.startIndex + m_IconCount, m_Option.deals.Length);
            for (int i = m_Option.startIndex; i < lange; i++)
            {
                var control = m_IconPool.Get().GetComponent<Control_Button_ChangeIcon>();
                control.Set(m_Option.deals[i]);
            }
        }

        base.OnOpen(_option);
    }

    public override void OnClose(object _option = null)
    {
        int nextStartIndex = m_Option.startIndex + m_IconCount;
        if (nextStartIndex < m_Option.deals.Length)
        {
            m_Option.startIndex = nextStartIndex;
            Open(m_Option);
        }

        base.OnClose(_option);
    }
    #endregion
}
