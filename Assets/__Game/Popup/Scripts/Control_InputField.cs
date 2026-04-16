using TMPro;
using UnityEngine;

/// <summary>TMP_InputField를 래핑하여 팝업 열 때 초기화 및 비밀번호 토글을 지원</summary>
[RequireComponent(typeof(TMP_InputField))]
public class Control_InputField : ControlBase
{
    #region Property
    /// <summary>래핑된 TMP_InputField 컴포넌트</summary>
    public TMP_InputField v { get; private set; }
    #endregion
    #region Inspector
    [SerializeField] private bool m_ResetOnOpen = false;
    #endregion

    #region Event
    protected override void Awake()
    {
        base.Awake();
        v = GetComponent<TMP_InputField>();
    }
    public override void OnOpen()
    {
        base.OnOpen();
        if (m_ResetOnOpen)
            v.text = string.Empty;
    }
    #endregion
    #region Function
    /// <summary>비밀번호 표시/숨김 상태를 토글</summary>
    public void TogglePassword()
    {
        v.contentType = (v.contentType == TMP_InputField.ContentType.Standard) ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.Standard;
        v.ForceLabelUpdate();
    }
    #endregion
}
