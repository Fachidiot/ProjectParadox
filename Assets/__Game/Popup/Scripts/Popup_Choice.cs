using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Popup_Choice : PopupBase
{
    public static Popup_Choice instance { get; private set; }

    #region Type
    public struct SOption
    {
        public string text;
        public Action onYes;
        public Action onNo;
        public SOption(string _text, Action _onYes, Action _onNo)
        {
            text = _text;
            onYes = _onYes;
            onNo = _onNo;
        }
    }
    #endregion

    #region Inspector
    [SerializeField] private TMP_Text m_Text;
    [SerializeField] private Button m_YesButton;
    [SerializeField] private Button m_NoButton;
    #endregion

    #region Value
    private SOption m_Option;
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }

    public override void Init(Camera _uiCamera)
    {
        m_YesButton.onClick.AddListener(OnClickYes);
        m_NoButton.onClick.AddListener(OnClickNo);
        base.Init(_uiCamera);
    }

    public override void OnOpen(object _option = null)
    {
        base.OnOpen(_option);
        if (_option is SOption o)
        {
            m_Option = o;
            m_Text.text = o.text;
        }
    }

    public override void OnClose(object _option = null)
    {
        base.OnClose(_option);
        m_Option = default;
    }
    #endregion

    #region Function
    private void OnClickYes()
    {
        m_Option.onYes?.Invoke();
        Close();
    }

    private void OnClickNo()
    {
        m_Option.onNo?.Invoke();
        Close();
    }
    #endregion
}
