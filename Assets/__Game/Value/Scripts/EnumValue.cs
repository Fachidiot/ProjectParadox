using System;
using System.Collections.Generic;

/// <summary>Enum형 반응형 값으로 int 직렬화를 통해 저장/로드</summary>
public class EnumValue<T> : ValueBase, IReadOnlyEnumValue<T> where T : struct, Enum
{
    #region Property
    /// <summary>현재 저장된 열거형 값</summary>
    public T v
    {
        get => m_Value;
        set
        {
            if (!EqualityComparer<T>.Default.Equals(m_Value, value))
            {
                m_Value = value;
                OnChanged(EChangeType.NeedSave);
            }
        }
    }
    #endregion
    #region Value
    private T m_Value;
    #endregion

    #region Event
    public EnumValue(IManageValue _callBy, string _id, T _default = default) : base(_callBy, _id)
    {
        m_Value = _default;
    }
    public override object OnSave()
    {
        return Convert.ToInt32(m_Value);
    }
    public override void OnLoad(object _data)
    {
        m_Value = (T)Enum.ToObject(typeof(T), (int)_data);
        OnChanged(EChangeType.Loaded);
    }
    public override void OnLoadString(string _data)
    {
        if (string.IsNullOrEmpty(_data))
            return;

        m_Value = (T)Enum.ToObject(typeof(T), int.Parse(_data));
        OnChanged(EChangeType.Loaded);
    }
    #endregion
    #region Function
    /// <summary>값을 설정하고 변경 이벤트 호출 여부를 지정</summary>
    public void Set(T _v, bool _isCallChanged, bool _isCallSave)
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
