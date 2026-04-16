using System;

/// <summary>FloatValue의 읽기 전용 접근을 위한 인터페이스</summary>
public interface IReadOnlyFloatValue
{
    /// <summary>현재 저장된 float 값</summary>
    float v { get; }
    void AddChanged(object _callBy, Action<ValueBase> _action, bool _isCallNow = false);
    void RemoveChanged(object _callBy, Action<ValueBase> _action);
}
