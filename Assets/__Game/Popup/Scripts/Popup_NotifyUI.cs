using System.Collections;
using TMPro;
using UnityEngine;

public class Popup_NotifyUI : PopupBase
{
    public static Popup_NotifyUI instance { get; private set; }

    #region Type
    public struct SOption
    {
        public string text;
        public float time;

        public SOption(string _text, float _time = 2f)
        {
            text = _text;
            time = _time;
        }
    }
    #endregion

    #region Inspector
    [SerializeField] private TMP_Text m_Text;
    [SerializeField] private float m_TypingSpeed = 0.05f;
    [SerializeField] private float m_BlinkSpeed = 0.4f;
    #endregion

    #region Value
    private Coroutine m_CurrentCoroutine;
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }

    public override void OnOpen(object _option = null)
    {
        if (_option == null) return;

        // 이미 출력 중이면 즉시 중단
        if (m_CurrentCoroutine != null)
        {
            StopCoroutine(m_CurrentCoroutine);
            m_CurrentCoroutine = null;
        }

        base.OnOpen(_option);

        var o = (SOption)_option;
        m_Text.gameObject.SetActive(true);
        m_CurrentCoroutine = StartCoroutine(TypeAndBlink(o));
    }

    public override void OnClose(object _option = null)
    {
        if (m_CurrentCoroutine != null)
        {
            StopCoroutine(m_CurrentCoroutine);
            m_CurrentCoroutine = null;
        }
        m_Text.gameObject.SetActive(false);
        base.OnClose(_option);
    }
    #endregion

    #region Function
    private IEnumerator TypeAndBlink(SOption _option)
    {
        m_Text.text = "";

        foreach (char c in _option.text)
        {
            m_Text.text += c;
            yield return new WaitForSeconds(m_TypingSpeed);
        }

        float timer = 0f;
        bool show = true;
        while (timer < _option.time)
        {
            m_Text.text = show ? _option.text + "." : _option.text;
            show = !show;
            yield return new WaitForSeconds(m_BlinkSpeed);
            timer += m_BlinkSpeed;
        }

        m_CurrentCoroutine = null;
        Close();
    }
    #endregion
}
