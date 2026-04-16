using System;

/// <summary>EnumValue의 읽기 전용 접근을 위한 인터페이스</summary>
public interface IReadOnlyEnumValue<T> where T : Enum
{
    /// <summary>현재 저장된 열거형 값</summary>
    T v { get; }
    void AddChanged(object _callBy, Action<ValueBase> _action, bool _isCallNow = false);
    void RemoveChanged(object _callBy, Action<ValueBase> _action);
}
