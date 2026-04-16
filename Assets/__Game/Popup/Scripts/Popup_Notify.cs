using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>확인/취소 버튼을 제공하며 옵션 큐와 비동기 대기를 지원하는 알림 팝업</summary>
public class Popup_Notify : PopupBase
{
    public static Popup_Notify instance { get; private set; }
    #region Type
    public struct SOption
    {
        public string title;
        public string text;
        public string btn1Text;
        public Action btn1Event;
        public string btn2Text;
        public Action btn2Event;

        public SOption(string _title, string _text, string _btn1Text, Action _btn1Event, string _btn2Text = null, Action _btn2Event = null)
        {
            title = _title;
            text = _text;
            btn1Text = _btn1Text;
            btn1Event = _btn1Event;
            btn2Text = _btn2Text;
            btn2Event = _btn2Event;
        }
    }
    public struct SOptionAwait
    {
        public string title;
        public string text;
        public string btn1Text;
        public string btn2Text;
        public SOptionAwait(string _title, string _text, string _btn1Text, string _btn2Text = null)
        {
            title = _title;
            text = _text;
            btn1Text = _btn1Text;
            btn2Text = _btn2Text;
        }
    }
    #endregion

    #region Inspector
    [SerializeField] private TMP_Text m_Title;
    [SerializeField] private TMP_Text m_Text;
    [SerializeField] private Control_Button m_Btn1;
    [SerializeField] private TMP_Text m_Btn1Text;
    [SerializeField] private Control_Button m_Btn2;
    [SerializeField] private TMP_Text m_Btn2Text;
    #endregion
    #region Value
    private SOption m_Option;
    private Queue<SOption> m_OptionQueue = new();
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }
    public override void Init(Camera _uiCamera)
    {
        m_Btn1.AddClickListener(OnClickBtn1);
        m_Btn2.AddClickListener(OnClickBtn2);
        base.Init(_uiCamera);
    }

    public override void OnOpen(object _option = null)
    {
        bool isAlreadyOpen = IsOpened & !IsClosing;
        base.OnOpen(_option);
        if (_option != null)
            m_OptionQueue.Enqueue((SOption)_option);
        if (isAlreadyOpen)
            return;

        SOption option = m_OptionQueue.Dequeue();
        m_Option = option;
        m_Title.text = option.title;
        m_Text.text = option.text;

        if (option.btn1Text != null)
        {
            m_Btn1.gameObject.SetActive(true);
            m_Btn1Text.text = option.btn1Text;
        }
        else
            m_Btn1.gameObject.SetActive(false);

        if (option.btn2Text != null)
        {
            m_Btn2.gameObject.SetActive(true);
            m_Btn2Text.text = option.btn2Text;
        }
        else
            m_Btn2.gameObject.SetActive(false);
    }
    public override void OnClose(object _option = null)
    {
        base.OnClose(_option);

        m_Option = default;
        if (0 < m_OptionQueue.Count)
            Open();
    }
    /// <summary>첫 번째 버튼 클릭 시 이벤트 호출 후 팝업 닫기</summary>
    private void OnClickBtn1(Control_Button _)
    {
        m_Option.btn1Event?.Invoke();
        Close();
    }
    /// <summary>두 번째 버튼 클릭 시 이벤트 호출 후 팝업 닫기</summary>
    private void OnClickBtn2(Control_Button _)
    {
        m_Option.btn2Event?.Invoke();
        Close();
    }
    #endregion
}
