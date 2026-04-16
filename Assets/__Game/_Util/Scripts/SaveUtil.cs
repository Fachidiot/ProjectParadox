using UnityEngine;

/// <summary>ValueBase 저장 방식 팩토리 (PlayerPrefs/DB/Cloud 선택)</summary>
public static class SaveUtil
{
    #region Type
    public enum EType
    {
        None,
        PlayerPrefs,
    }
    #endregion

    #region Event
    /// <summary>저장 타입에 따른 ValueBase 생성</summary>
    public static T Create<T>(GlobalManagerBase _callBy, string _containerName, T _value, EType _type) where T : ValueBase
    {
        switch (_type)
        {
            case EType.PlayerPrefs:
                return PlayerPrefsSaveManager.instance.Create(_callBy, _value);
        }
        return null;
    }
    #endregion
}
