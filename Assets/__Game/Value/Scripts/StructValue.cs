using System;
using UnityEngine;

/// <summary>struct형 반응형 값으로 JSON 직렬화를 통해 저장/로드</summary>
public class StructValue<T> : ValueBase, IReadOnlyStructValue<T> where T : struct
{
    #region Property
    /// <summary>현재 저장된 struct 값</summary>
    public T v
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
    private T m_Value;
    #endregion

    #region Event
    public StructValue(IManageValue _callBy, string _id, T _default = default) : base(_callBy, _id)
    {
        m_Value = _default;
    }
    public override object OnSave()
    {
        return JsonUtility.ToJson(m_Value);
    }
    public override void OnLoad(object _data)
    {
        m_Value = JsonUtility.FromJson<T>((string)_data);
        OnChanged(EChangeType.Loaded);
    }
    public override void OnLoadString(string _data)
    {
        if (string.IsNullOrEmpty(_data))
            return;

        m_Value = JsonUtility.FromJson<T>(_data);
        OnChanged(EChangeType.Loaded);
    }
    #endregion
    #region Function
    /// <summary>값을 설정하고 변경 이벤트 호출 여부를 지정</summary>
    public void Set(T _v, bool _isCallChanged, bool _isCallSave)
    {
        m_Value = _v;
        if (_isCallChanged)
            OnChanged(_isCallSave ? EChangeType.NeedSave : EChangeType.None);
    }
    #endregion
}
