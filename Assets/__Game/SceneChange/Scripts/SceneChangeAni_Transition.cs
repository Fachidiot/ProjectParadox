using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>암전 → 텍스트 → 섬광 → 씬로드 → 섬광 페이드아웃 트랜지션</summary>
public class SceneChangeAni_Transition : SceneChangeAni
{
    #region Inspector
    [SerializeField] private Image m_BlackScreen;
    [SerializeField] private TMP_Text m_Text;
    [SerializeField] private Image m_WhiteFlash;
    [Header("타이밍")]
    [SerializeField] private float m_FadeInTime = 1.5f;
    [SerializeField] private float m_TextFadeTime = 1.0f;
    [SerializeField] private float m_TextHoldTime = 2.0f;
    [SerializeField] private float m_FlashFadeTime = 1.5f;
    [SerializeField] private float m_FlashOutTime = 2.0f;
    #endregion

    #region Value
    private string m_Message;
    #endregion

    #region Event
    public override void Init()
    {
        ResetColors();
        base.Init();
    }
    #endregion

    #region Function
    public void SetMessage(string _message)
    {
        m_Message = _message;
    }

    public override void StartAni()
    {
        ResetColors();
        base.StartAni();
        StartCoroutine(TransitionIn());
    }

    public override void EndAni()
    {
        base.EndAni();
        StartCoroutine(TransitionOut());
    }

    private IEnumerator TransitionIn()
    {
        // 1. 암전
        yield return Fade(m_BlackScreen, 0f, 1f, m_FadeInTime);

        // 2. 텍스트 등장
        if (m_Text != null)
        {
            m_Text.text = m_Message ?? "";
            yield return FadeText(m_Text, 0f, 1f, m_TextFadeTime);
            yield return new WaitForSecondsRealtime(m_TextHoldTime);
        }

        // 3. 하얀 섬광 + 텍스트 사라짐
        float timer = 0f;
        while (timer < m_FlashFadeTime)
        {
            timer += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(timer / m_FlashFadeTime);
            m_WhiteFlash.color = new Color(1, 1, 1, p);
            if (m_Text != null)
                m_Text.color = new Color(m_Text.color.r, m_Text.color.g, m_Text.color.b, 1f - p);
            yield return null;
        }
        m_BlackScreen.color = new Color(0, 0, 0, 0);

        // 4. 씬 로드 요청
        PostChange();
    }

    private IEnumerator TransitionOut()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        // 5. 섬광 걷힘
        yield return Fade(m_WhiteFlash, 1f, 0f, m_FlashOutTime);

        PostEnd();
    }

    private IEnumerator Fade(Image _image, float _from, float _to, float _duration)
    {
        float timer = 0f;
        Color c = _image.color;
        while (timer < _duration)
        {
            timer += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(_from, _to, timer / _duration);
            _image.color = c;
            yield return null;
        }
        c.a = _to;
        _image.color = c;
    }

    private IEnumerator FadeText(TMP_Text _text, float _from, float _to, float _duration)
    {
        float timer = 0f;
        Color c = _text.color;
        while (timer < _duration)
        {
            timer += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(_from, _to, timer / _duration);
            _text.color = c;
            yield return null;
        }
        c.a = _to;
        _text.color = c;
    }

    private void ResetColors()
    {
        if (m_BlackScreen != null) m_BlackScreen.color = new Color(0, 0, 0, 0);
        if (m_Text != null) m_Text.color = new Color(1, 1, 1, 0);
        if (m_WhiteFlash != null) m_WhiteFlash.color = new Color(1, 1, 1, 0);
    }
    #endregion
}
