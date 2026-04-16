using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

/// <summary>입력 바인딩 텍스트 표시, 키 변경 시 자동 갱신</summary>
[RequireComponent(typeof(TMP_Text))]
public class Control_Text_InputBinding : MonoBehaviour
{
    #region Inspector
    [SerializeField, LabelText("액션 이름")] private string m_ActionName;
    #endregion
    #region Value
    private TMP_Text m_Text;
    #endregion

    #region Event
    private void Awake()
    {
        m_Text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (LocalInputManager.instance != null)
        {
            LocalInputManager.instance.OnBindingsChanged += Refresh;
            Refresh();
        }
    }

    private void OnDisable()
    {
        if (LocalInputManager.instance != null)
            LocalInputManager.instance.OnBindingsChanged -= Refresh;
    }
    #endregion

    #region Function
    private void Refresh()
    {
        m_Text.text = LocalInputManager.instance.GetBindingDisplayString(m_ActionName);
    }
    #endregion
}
