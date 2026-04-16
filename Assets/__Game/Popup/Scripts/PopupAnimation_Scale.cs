using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;

/// <summary>Transform 스케일 변경 기반의 팝업 열기/닫기 애니메이션</summary>
public class PopupAnimation_Scale : PopupAnimationBase
{
    #region Inspector
    [SerializeField] private Transform m_Target;

    [Title("Open")]
    [SerializeField] private Vector3 m_OpenStartScale = Vector3.one * 0.01f;
    [SerializeField] private Vector3 m_OpenEndScale = Vector3.one;
    [SerializeField] private float m_OpenStartDelay = 0f;
    [SerializeField] private AnimationCurve m_OpenCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float m_OpenTime = 0.5f;

    [Title("Close")]
    [SerializeField] private Vector3 m_CloseStartScale = Vector3.one;
    [SerializeField] private Vector3 m_CloseEndScale = Vector3.one * 0.01f;
    [SerializeField] private float m_CloseStartDelay = 0f;
    [SerializeField] private AnimationCurve m_CloseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float m_CloseTime = 0.5f;
    #endregion
    #region Value
    private Coroutine m_OpenCoroutine = null;
    private Coroutine m_CloseCoroutine = null;
    #endregion

    #region Event
    protected override void OnStartOpen(Action _onEnd)
    {
        m_OpenCoroutine = StartCoroutine(OpenCoroutine(_onEnd));
    }
    protected override void OnStartClose(Action _onEnd)
    {
        m_CloseCoroutine = StartCoroutine(CloseCoroutine(_onEnd));
    }
    protected override void OnCancelState()
    {
        base.OnCancelState();

        if (m_OpenCoroutine != null)
        {
            StopCoroutine(m_OpenCoroutine);
            m_OpenCoroutine = null;
        }
        if (m_CloseCoroutine != null)
        {
            StopCoroutine(m_CloseCoroutine);
            m_CloseCoroutine = null;
        }
    }

    /// <summary>스케일 변경 열기 애니메이션 코루틴</summary>
    private IEnumerator OpenCoroutine(Action _onEnd)
    {
        float timer = 0;

        m_Target.localScale = m_OpenStartScale;
        yield return new WaitForSecondsRealtime(m_OpenStartDelay);

        while (timer < m_OpenTime)
        {
            timer += Time.unscaledDeltaTime;
            m_Target.localScale = Vector3.LerpUnclamped(m_OpenStartScale, m_OpenEndScale, m_OpenCurve.Evaluate(timer / m_OpenTime));

            yield return null;
        }

        m_Target.localScale = m_OpenEndScale;
        _onEnd();
    }
    /// <summary>스케일 변경 닫기 애니메이션 코루틴</summary>
    private IEnumerator CloseCoroutine(Action _onEnd)
    {
        float timer = 0;

        m_Target.localScale = m_CloseStartScale;
        yield return new WaitForSecondsRealtime(m_CloseStartDelay);

        while (timer < m_CloseTime)
        {
            timer += Time.unscaledDeltaTime;
            m_Target.localScale = Vector3.LerpUnclamped(m_CloseStartScale, m_CloseEndScale, m_CloseCurve.Evaluate(timer / m_CloseTime));

            yield return null;
        }

        m_Target.localScale = m_CloseEndScale;
        _onEnd();
    }
    #endregion
}
