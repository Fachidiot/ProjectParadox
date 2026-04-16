using System;

/// <summary>long형 반응형 값으로 변경 감지/저장/로드를 지원</summary>
public class LongValue : ValueBase, IReadOnlyLongValue
{
    #region Property
    /// <summary>현재 저장된 long 값</summary>
    public long v
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
    private long m_Value;
    #endregion

    #region Event
    public LongValue(IManageValue _callBy, string _id, long _default = default) : base(_callBy, _id)
    {
        m_Value = _default;
    }
    public override object OnSave()
    {
        return m_Value;
    }
    public override void OnLoad(object _data)
    {
        m_Value = (long)_data;
        OnChanged(EChangeType.Loaded);
    }
    public override void OnLoadString(string _data)
    {
        if (string.IsNullOrEmpty(_data))
            return;

        m_Value = long.Parse(_data);
        OnChanged(EChangeType.Loaded);
    }
    #endregion
    #region Function
    /// <summary>값을 설정하고 변경 이벤트 호출 여부를 지정</summary>
    public void Set(long _v, bool _isCallChanged, bool _isCallSave)
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
