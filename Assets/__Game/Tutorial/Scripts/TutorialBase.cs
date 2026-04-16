using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>튜토리얼 조건 구현 베이스. LocalTutorialManager가 OnUpdate 호출하여 구동</summary>
public abstract class TutorialBase : ObjectBase
{
    #region Inspector
    [SerializeField, LabelText("튜토리얼 ID")] private string m_TutorialID;
    #endregion

    #region Get,Set
    protected string TutorialID => m_TutorialID;
    #endregion

    #region Event
    /// <summary>LocalTutorialManager가 매 프레임 호출</summary>
    public virtual void OnUpdate()
    {
        if (TutorialManager.instance == null) return;
        if (TutorialManager.instance.IsCleared(m_TutorialID)) return;

        if (!TutorialManager.instance.IsActive(m_TutorialID))
        {
            if (CheckTrigger())
            {
                if (TutorialManager.instance.TryActivate(m_TutorialID))
                    OnActivate();
            }
        }
        else
        {
            if (CheckComplete())
            {
                OnComplete();
                TutorialManager.instance.Complete(m_TutorialID);
            }
        }
    }
    #endregion

    #region Virtual
    /// <summary>튜토리얼 활성화 시 호출. 기본: Popup_Tutorial 열기</summary>
    protected virtual void OnActivate()
    {
        var table = TableManager.instance.Get<Table_Tutorial.Data>(m_TutorialID);
        if (table == null) return;

        var lang = LanguageManager.instance.Language.v;
        string name = table.Name.Translate(lang, false);
        string explain = table.Explain.Translate(lang, false);
        Popup_Tutorial.instance?.Open(new Popup_Tutorial.SOption(name, explain));
    }

    /// <summary>튜토리얼 완료 시 호출. 기본: Popup_Tutorial 닫기</summary>
    protected virtual void OnComplete()
    {
        Popup_Tutorial.instance?.Close();
    }
    #endregion

    #region Function
    /// <summary>reactive 튜토리얼이 외부에서 활성화 요청할 때 사용</summary>
    public void RequestActivate()
    {
        if (TutorialManager.instance == null) return;
        if (TutorialManager.instance.IsCleared(m_TutorialID)) return;

        if (TutorialManager.instance.TryActivate(m_TutorialID))
            OnActivate();
    }

    /// <summary>reactive 튜토리얼이 외부에서 즉시 클리어 요청할 때 사용</summary>
    public void RequestMarkCleared()
    {
        if (TutorialManager.instance == null) return;
        TutorialManager.instance.MarkCleared(m_TutorialID);
    }
    #endregion

    #region Abstract
    /// <summary>팝업을 표시할 조건. true이면 활성화</summary>
    protected abstract bool CheckTrigger();

    /// <summary>튜토리얼를 클리어할 조건. true이면 완료 처리</summary>
    protected abstract bool CheckComplete();
    #endregion
}
