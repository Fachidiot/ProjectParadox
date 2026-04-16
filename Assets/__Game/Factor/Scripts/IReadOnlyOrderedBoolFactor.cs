using System;
using System.Collections.Generic;

/// <summary>OrderedBoolFactor 읽기 전용 인터페이스</summary>
public interface IReadOnlyOrderedBoolFactor<TKey> : IReadOnlyBoolFactor
{
    /// <summary>Global 키 목록 (삽입 순서)</summary>
    IReadOnlyList<TKey> GlobalKeys { get; }
    /// <summary>Local 키 목록 (삽입 순서)</summary>
    IReadOnlyList<TKey> LocalKeys { get; }
}
