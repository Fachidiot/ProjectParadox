using TMPro;
using UnityEngine;

/// <summary>튜토리얼 팝업, 오른쪽 위에 Name/Explain 표시. TutorialManager가 자동으로 열고 닫음</summary>
public class Popup_Tutorial : PopupBase
{
    public static Popup_Tutorial instance { get; private set; }

    #region Type
    public struct SOption
    {
        public string name;
        public string explain;

        public SOption(string _name, string _explain)
        {
            name = _name;
            explain = _explain;
        }
    }
    #endregion

    #region Inspector
    [SerializeField] private TMP_Text m_NameText;
    [SerializeField] private TMP_Text m_ExplainText;
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }

    public override void OnOpen(object _option = null)
    {
        base.OnOpen(_option);
        if (_option is SOption o)
        {
            m_NameText.text = o.name;
            m_ExplainText.text = o.explain;
        }
    }
    #endregion
}
