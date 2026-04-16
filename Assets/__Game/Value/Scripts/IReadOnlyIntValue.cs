using System;

/// <summary>IntValue의 읽기 전용 접근을 위한 인터페이스</summary>
public interface IReadOnlyIntValue
{
    /// <summary>현재 저장된 int 값</summary>
    int v { get; }
    void AddChanged(object _callBy, Action<ValueBase> _action, bool _isCallNow = false);
    void RemoveChanged(object _callBy, Action<ValueBase> _action);
}
