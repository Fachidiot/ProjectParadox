using System;
using UnityEngine;

/// <summary>BoolFactor 읽기 전용 인터페이스</summary>
public interface IReadOnlyBoolFactor
{
    /// <summary>모든 계수가 적용된 최종 bool 값</summary>
    bool Total { get; }

    /// <summary>값 변경 이벤트 등록</summary>
    void AddChanged(object _callBy, Action<ValueBase> _action, bool _isCallNow = false);

    /// <summary>값 변경 이벤트 해제</summary>
    void RemoveChanged(object _callBy, Action<ValueBase> _action);
}
