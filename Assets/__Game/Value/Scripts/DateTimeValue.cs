using System;

/// <summary>DateTime형 반응형 값으로 Binary 직렬화를 통해 저장/로드</summary>
public class DateTimeValue : ValueBase, IReadOnlyDateTimeValue
{
    #region Property
    /// <summary>현재 저장된 DateTime 값</summary>
    public DateTime v
    {
        get => m_Value;
        set
        {
            m_Value = value;
            OnChanged(EChangeType.NeedSave);
        }
    }
    #endregion
    #region Value
    private DateTime m_Value;
    #endregion

    #region Event
    public DateTimeValue(IManageValue _callBy, string _id, DateTime _default = default) : base(_callBy, _id)
    {
        m_Value = _default;
    }
    public override object OnSave()
    {
        return m_Value.ToBinary();
    }
    public override void OnLoad(object _data)
    {
        m_Value = DateTime.FromBinary((long)_data);
        OnChanged(EChangeType.Loaded);
    }
    public override void OnLoadString(string _data)
    {
        if (string.IsNullOrEmpty(_data))
            return;

        m_Value = DateTime.FromBinary(long.Parse(_data));
        OnChanged(EChangeType.Loaded);
    }
    #endregion
    #region Function
    /// <summary>값을 설정하고 변경 이벤트 호출 여부를 지정</summary>
    public void Set(DateTime _v, bool _isCallChanged, bool _isCallSave)
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
