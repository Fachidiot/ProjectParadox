using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Button 컴포넌트를 래핑하여 클릭 딜레이와 이벤트를 관리</summary>
[RequireComponent(typeof(Button))]
public class Control_Button : ControlBase
{
    #region Property
    /// <summary>래핑된 Unity Button 컴포넌트</summary>
    public Button v { get; private set; }
    #endregion
    #region Inspector
    [SerializeField] private float m_Delay = 0.5f;
    #endregion
    #region Value
    private Action<Control_Button> m_OnClick;
    private Coroutine m_DelayCor;
    #endregion

    #region Event
    protected override void Awake()
    {
        base.Awake();
        v = GetComponent<Button>();
        v.onClick.AddListener(OnClick);
    }
    protected override void OnDisable()
    {
        m_DelayCor = null;
        base.OnDisable();
    }

    /// <summary>버튼 클릭 시 딜레이 체크 후 등록된 콜백을 호출</summary>
    private void OnClick()
    {
        if (m_DelayCor != null)
            return;

        m_DelayCor = StartCoroutine(DelayCoroutine());

        m_OnClick?.Invoke(this);
    }
    #endregion
    #region Function
    /// <summary>클릭 이벤트 리스너를 등록</summary>
    public void AddClickListener(Action<Control_Button> _onClick)
    {
        m_OnClick += _onClick;
    }
    /// <summary>버튼의 상호작용 가능 여부를 설정</summary>
    public void SetInteractable(bool _interactable)
    {
        v.interactable = _interactable;
    }

    /// <summary>연속 클릭 방지를 위한 딜레이 코루틴</summary>
    private IEnumerator DelayCoroutine()
    {
        yield return new WaitForSeconds(m_Delay);
        m_DelayCor = null;
    }
    #endregion
}
