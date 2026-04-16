using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;

/// <summary>RectTransform 위치 이동 기반의 팝업 열기/닫기 애니메이션</summary>
public class PopupAnimation_Position : PopupAnimationBase
{
    #region Inspector
    [SerializeField] private RectTransform m_Target;

    [Title("Open")]
    [SerializeField] private Vector2 m_OpenStartPosition = new Vector2(0f, -1080f); // 예: 아래에서 올라옴
    [SerializeField] private Vector2 m_OpenEndPosition = Vector2.zero; // 예: 원점
    [SerializeField] private float m_OpenStartDelay = 0f;
    [SerializeField] private AnimationCurve m_OpenCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float m_OpenTime = 0.5f;

    [Title("Close")]
    [SerializeField] private Vector2 m_CloseStartPosition = Vector2.zero;
    [SerializeField] private Vector2 m_CloseEndPosition = new Vector2(0f, -1080f);
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

    /// <summary>위치 이동 열기 애니메이션 코루틴</summary>
    private IEnumerator OpenCoroutine(Action _onEnd)
    {
        float timer = 0;

        // UI는 AnchoredPosition을 제어해야 앵커 기준 상대 좌표로 정확히 움직입니다.
        if (m_Target != null) m_Target.anchoredPosition = m_OpenStartPosition;

        yield return new WaitForSecondsRealtime(m_OpenStartDelay);

        while (timer < m_OpenTime)
        {
            timer += Time.unscaledDeltaTime;

            if (m_Target != null)
            {
                m_Target.anchoredPosition = Vector2.LerpUnclamped(
                    m_OpenStartPosition,
                    m_OpenEndPosition,
                    m_OpenCurve.Evaluate(timer / m_OpenTime)
                );
            }

            yield return null;
        }

        if (m_Target != null) m_Target.anchoredPosition = m_OpenEndPosition;
        _onEnd?.Invoke();
    }

    /// <summary>위치 이동 닫기 애니메이션 코루틴</summary>
    private IEnumerator CloseCoroutine(Action _onEnd)
    {
        float timer = 0;

        if (m_Target != null) m_Target.anchoredPosition = m_CloseStartPosition;

        yield return new WaitForSecondsRealtime(m_CloseStartDelay);

        while (timer < m_CloseTime)
        {
            timer += Time.unscaledDeltaTime;

            if (m_Target != null)
            {
                m_Target.anchoredPosition = Vector2.LerpUnclamped(
                    m_CloseStartPosition,
                    m_CloseEndPosition,
                    m_CloseCurve.Evaluate(timer / m_CloseTime)
                );
            }

            yield return null;
        }

        if (m_Target != null) m_Target.anchoredPosition = m_CloseEndPosition;
        _onEnd?.Invoke();
    }
    #endregion

    #region Editor Helper
    /// <summary>현재 위치를 열기 종료 위치로 설정 (에디터 전용)</summary>
    [Button("Set Current Pos as Open End"), PropertyOrder(10)]
    private void SetCurrentAsOpenEnd()
    {
        if (m_Target != null) m_OpenEndPosition = m_Target.anchoredPosition;
    }

    /// <summary>현재 위치를 닫기 시작 위치로 설정 (에디터 전용)</summary>
    [Button("Set Current Pos as Close Start"), PropertyOrder(11)]
    private void SetCurrentAsCloseStart()
    {
        if (m_Target != null) m_CloseStartPosition = m_Target.anchoredPosition;
    }
    #endregion
}
