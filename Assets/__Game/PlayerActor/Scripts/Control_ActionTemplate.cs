using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>액션 리스트 템플릿 항목, 아이콘과 키 텍스트 표시, 바인딩 변경 시 자동 갱신</summary>
public class Control_ActionTemplate : MonoBehaviour
{
    #region Inspector
    [SerializeField, LabelText("아이콘")] private Image m_Icon;
    [SerializeField, LabelText("키 텍스트")] private TMP_Text m_KeyText;
    #endregion
    #region Value
    private string m_ActionName;
    #endregion

    #region Event
    private void OnEnable()
    {
        if (LocalInputManager.instance != null)
            LocalInputManager.instance.OnBindingsChanged += RefreshKeyText;
    }

    private void OnDisable()
    {
        if (LocalInputManager.instance != null)
            LocalInputManager.instance.OnBindingsChanged -= RefreshKeyText;
    }
    #endregion

    #region Function
    public void Set(string _stateID, string _actionName)
    {
        m_Icon.sprite = IconManager.instance.Get(_stateID);
        m_ActionName = _actionName;
        RefreshKeyText();
    }

    private void RefreshKeyText()
    {
        m_KeyText.text = LocalInputManager.instance.GetBindingDisplayString(m_ActionName);
    }
    #endregion
}
