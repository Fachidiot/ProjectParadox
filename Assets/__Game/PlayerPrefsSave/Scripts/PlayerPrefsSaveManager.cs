using System.Collections.Generic;
using UnityEngine;

/// <summary>PlayerPrefs를 사용하여 ValueBase 데이터를 자동 저장/로드하는 매니저</summary>
public class PlayerPrefsSaveManager : GlobalManagerBase
{
    public static PlayerPrefsSaveManager instance { get; private set; }

    #region Value
    private Dictionary<string, ValueBase> m_Value = new();
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }
    #endregion
    #region Function
    /// <summary>ValueBase 인스턴스를 저장 대상으로 등록하고 기존 값을 로드</summary>
    public T Create<T>(GlobalManagerBase _callBy, T _value) where T : ValueBase
    {
        m_Value.Add(_value.ID, _value);

        var text = PlayerPrefs.GetString(_value.ID, null);
        _value.OnLoadString(text);
        _value.AddSaveChanged(this, (_v) =>
        {
            PlayerPrefs.SetString(_v.ID, _v.OnSaveString());
        }, text == null);

        return _value;
    }
    #endregion
    #if UNITY_EDITOR
    #region Editor Function
    /// <summary>등록된 모든 ValueBase 딕셔너리를 반환 (에디터 전용)</summary>
    public Dictionary<string, ValueBase> GetValues()
    {
        return m_Value;
    }
    #endregion
    #endif
}
