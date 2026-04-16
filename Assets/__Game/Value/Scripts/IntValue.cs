using System;

/// <summary>int형 반응형 값으로 변경 감지/저장/로드를 지원</summary>
public class IntValue : ValueBase, IReadOnlyIntValue
{
    #region Property
    /// <summary>현재 저장된 int 값</summary>
    public int v
    {
        get => m_Value;
        set
        {
            if (m_Value != value)
            {
                m_Value = value;
                OnChanged(EChangeType.NeedSave);
            }
        }
    }
    #endregion
    #region Value
    private int m_Value;
    #endregion

    #region Event
    public IntValue(IManageValue _callBy, string _id, int _default = default) : base(_callBy, _id)
    {
        m_Value = _default;
    }
    public override object OnSave()
    {
        return m_Value;
    }
    public override void OnLoad(object _data)
    {
        m_Value = (int)_data;
        OnChanged(EChangeType.Loaded);
    }
    public override void OnLoadString(string _data)
    {
        if (string.IsNullOrEmpty(_data))
            return;

        m_Value = int.Parse(_data);
        OnChanged(EChangeType.Loaded);
    }
    #endregion
    #region Function
    /// <summary>값을 설정하고 변경 이벤트 호출 여부를 지정</summary>
    public void Set(int _v, bool _isCallChanged, bool _isCallSave)
    {
        if (_isCallChanged)
        {
            m_Value = _v;
            OnChanged(_isCallSave ? EChangeType.NeedSave : EChangeType.None);
        }
        else
            m_Value = _v;
    }
    #endregion
}
