using System;

/// <summary>DoubleValue의 읽기 전용 접근을 위한 인터페이스</summary>
public interface IReadOnlyDoubleValue
{
    /// <summary>현재 저장된 double 값</summary>
    double v { get; }
    void AddChanged(object _callBy, Action<ValueBase> _action, bool _isCallNow = false);
    void RemoveChanged(object _callBy, Action<ValueBase> _action);
}
