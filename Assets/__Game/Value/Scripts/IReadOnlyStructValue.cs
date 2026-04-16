using System;

/// <summary>StructValue의 읽기 전용 접근을 위한 인터페이스</summary>
public interface IReadOnlyStructValue<T> where T : struct
{
    /// <summary>현재 저장된 struct 값</summary>
    T v { get; }
    void AddChanged(object _callBy, Action<ValueBase> _action, bool _isCallNow = false);
    void RemoveChanged(object _callBy, Action<ValueBase> _action);
}
