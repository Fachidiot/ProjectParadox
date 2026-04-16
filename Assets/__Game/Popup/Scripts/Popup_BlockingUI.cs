using UnityEngine;

/// <summary>BoolFactor 기반으로 다중 요청을 관리하는 로딩/블로킹 UI 팝업</summary>
public class Popup_BlockingUI : PopupBase
{
    public static Popup_BlockingUI instance { get; private set; }

    #region Type
    public struct SOption
    {
        public MonoBehaviour factor;

        public SOption(MonoBehaviour _factor)
        {
            factor = _factor;
        }
    }
    #endregion

    #region Value
    private BoolFactor<MonoBehaviour> m_CloseFactor;
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }
    public override void Init(Camera _uiCamera)
    {
        m_CloseFactor = new(this, BoolFactor<MonoBehaviour>.ETotalType.Or);

        base.Init(_uiCamera);
    }
    public override void OnOpen(object _option = null)
    {
        if (_option != null)
        {
            var option = (SOption)_option;
            m_CloseFactor.Set(this, option.factor, true);
        }

        base.OnOpen(_option);
    }
    public override void OnClose(object _option = null)
    {
        if (_option != null)
        {
            var option = (SOption)_option;
            m_CloseFactor.Remove(option.factor);
        }

        base.OnClose(_option);

        if (m_CloseFactor.Total)
            Open();
    }
    #endregion
}
