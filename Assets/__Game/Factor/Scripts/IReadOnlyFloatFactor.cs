using System;
using UnityEngine;

/// <summary>FloatFactor 읽기 전용 인터페이스</summary>
public interface IReadOnlyFloatFactor
{
    /// <summary>모든 계수가 적용된 최종 float 값</summary>
    float Total { get; }

    /// <summary>값 변경 이벤트 등록</summary>
    void AddChanged(object _callBy, Action<ValueBase> _action, bool _isCallNow = false);

    /// <summary>값 변경 이벤트 해제</summary>
    void RemoveChanged(object _callBy, Action<ValueBase> _action);
}
