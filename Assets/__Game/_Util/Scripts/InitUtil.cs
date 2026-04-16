using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>매니저 배열 초기화 상태 체크 유틸리티</summary>
public static class InitUtil
{
    #region Function
    /// <summary>매니저 배열 기본 초기화 완료 여부</summary>
    public static bool IsInit<T>(T[] _mgrs) where T : ManagerBase
    {
        foreach (var v in _mgrs)
            if (!v.IsInited)
                return false;
        return true;
    }
    /// <summary>매니저 배열 게임 초기화 완료 여부</summary>
    public static bool IsInitGame<T>(T[] _mgrs) where T : ManagerBase
    {
        foreach (var v in _mgrs)
            if (!v.IsGameInited)
                return false;
        return true;
    }
    #endregion
}
