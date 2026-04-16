using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>매니저 시스템 초기화 유틸리티 제공, 의존성 기반 단계별 활성화 담당</summary>
public class Root : MonoBehaviour
{
    #region Manual Function
    /// <summary>조건 기반 매니저 선별 및 의존성 순서대로 초기화 실행</summary>
    protected void InitManagersBase<T>(string _system, string _initName, T[] _managers, Func<T, bool> _require, Action<T> _init) where T : MonoBehaviour
    {
        List<T> manager = null;
        int loop = 0;

        try
        {
            manager = new List<T>(_managers);
            manager.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));

            var remove = new List<T>();
            var lastedCount = manager.Count;
            for (; ; loop++)
            {
                foreach (var v in manager)
                    if (_require(v))
                    {
                        LogManager.instance.Log(_system, $"loop{loop + 1} : {v.GetType()}.{_initName}()");
                        _init(v);
                        remove.Add(v);
                    }
                foreach (var v in remove)
                    manager.Remove(v);

                if (lastedCount == manager.Count)
                    throw new InvalidOperationException();
                if (manager.Count <= 0)
                    break;

                lastedCount = manager.Count;
                remove.Clear();
            }
        }
        catch (Exception e)
        {
            string lefts = "";
            foreach (var v in manager)
                lefts += v.name + ", ";

            ShutdownManager.instance.Shutdown(_system, e, $"loop{loop + 1} : {_initName}() failed\n{lefts}");
            throw;
        }
    }

    /// <summary>매니저 싱글톤 인스턴스 초기화</summary>
    protected void InitSingletons<T>(string _system, T[] _managers) where T : ManagerBase
    {
        foreach (var v in _managers)
        {
            Debug.Log($"[{_system}] {v.GetType()}.InitSingleton()");
            v.InitSingleton();
        }
    }

    /// <summary>의존성 검사 후 매니저 표준 초기화 실행</summary>
    protected void InitManagers<T>(string _system, T[] _managers) where T : ManagerBase
    {
        InitManagersBase(_system, "Init", _managers, (v) => v.RequireInit(), (v) => v.Init());
    }

    /// <summary>초기화 완료 후 후행 설정 로직 호출</summary>
    protected void AfterInitManagers<T>(string _system, T[] _managers) where T : ManagerBase
    {
        foreach (var v in _managers)
        {
            Debug.Log($"[{_system}] {v.GetType()}.AfterInit()");
            v.AfterInit();
        }
    }

    /// <summary>게임 매니저 순차 활성화</summary>
    protected void InitGameManagers<T>(string _system, T[] _managers) where T : ManagerBase
    {
        InitManagersBase(_system, "InitGame", _managers, (v) => v.RequireInitGame(), (v) => v.InitGame());
    }

    /// <summary>게임 초기화 완료 후 최종 정리 작업 수행</summary>
    protected void AfterInitGameManagers<T>(string _system, T[] _managers) where T : ManagerBase
    {
        foreach (var v in _managers)
        {
            Debug.Log($"[{_system}] {v.GetType()}.AfterInitGame()");
            v.AfterInitGame();
        }
    }
    #endregion
}
