using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>슬롯 기반 파일 저장/로드 매니저, 문서 폴더에 JSON 파일로 관리</summary>
public class SlotSaveManager : GlobalManagerBase
{
    public static SlotSaveManager instance { get; private set; }

    #if UNITY_EDITOR
    #region Preview
    [Serializable] private struct SPreview
    {
        public string id;
        public string by;
        public string value;
        public SPreview(string _id, string _by, string _value)
        {
            id = _id;
            by = _by;
            value = _value;
        }
    }
    [SerializeField, Sirenix.OdinInspector.ReadOnly] private List<SPreview> m_Preview = new();
    #endregion
    #endif
    #region Get,Set
    /// <summary>현재 슬롯 번호</summary>
    public int CurrentSlot { get; private set; } = 0;
    /// <summary>마지막 로드한 슬롯의 저장 씬 이름</summary>
    public string LoadedSceneName { get; private set; }
    #endregion

    #region Const
    private const string SCENE_KEY = "__SceneName";
    #endregion
    #region Value
    private Dictionary<string, ValueBase> m_Values = new();
    private HashSet<string> m_LocalKeys = new();
    private Dictionary<string, string> m_LoadedData = new();
    private Action m_OnBeforeSaveGlobal;
    private Action m_OnBeforeSaveLocal;
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }

    public override void InitValue()
    {
        base.InitValue();
        m_OnBeforeSaveLocal = null;
        foreach (var key in m_LocalKeys)
            m_Values.Remove(key);
        m_LocalKeys.Clear();
    }
    #endregion

    #region Function
    /// <summary>현재 슬롯 번호 설정</summary>
    public void SetCurrentSlot(int _slot)
    {
        CurrentSlot = _slot;
    }

    /// <summary>저장 직전 콜백 등록</summary>
    public void AddOnBeforeSave(object _callBy, Action _action)
    {
        if (_callBy is GlobalManagerBase)
            m_OnBeforeSaveGlobal += _action;
        else
            m_OnBeforeSaveLocal += _action;
    }

    /// <summary>저장 직전 콜백 해제</summary>
    public void RemoveOnBeforeSave(object _callBy, Action _action)
    {
        if (_callBy is GlobalManagerBase)
            m_OnBeforeSaveGlobal -= _action;
        else
            m_OnBeforeSaveLocal -= _action;
    }

    /// <summary>ValueBase 인스턴스를 저장 대상으로 등록, 로드된 데이터가 있으면 자동 적용</summary>
    public T Create<T>(object _callBy, T _value) where T : ValueBase
    {
        m_Values[_value.ID] = _value;

        if (_callBy is LocalManagerBase)
            m_LocalKeys.Add(_value.ID);

        if (m_LoadedData.TryGetValue(_value.ID, out var loaded))
            _value.OnLoadString(loaded);

        #if UNITY_EDITOR
        string byName = _callBy is UnityEngine.Object obj ? obj.name : _callBy.GetType().Name;
        m_Preview.Add(new SPreview(_value.ID, byName, _value.OnSaveString()));
        #endif

        return _value;
    }

    /// <summary>슬롯에서 로드</summary>
    public bool Load(int _slot)
    {
        string path = GetSlotPath(_slot);
        if (!File.Exists(path)) return false;

        string json = File.ReadAllText(path);
        JsonData data = JsonMapper.ToObject(json);

        m_LoadedData.Clear();
        foreach (string key in data.Keys)
            m_LoadedData[key] = data[key].ToString();

        foreach (var kv in m_Values)
        {
            if (m_LoadedData.TryGetValue(kv.Key, out var val))
                kv.Value.OnLoadString(val);
        }

        LoadedSceneName = m_LoadedData.TryGetValue(SCENE_KEY, out var scene) ? scene : null;
        CurrentSlot = _slot;
        #if UNITY_EDITOR
        RefreshPreview();
        #endif
        return true;
    }

    /// <summary>슬롯에 저장</summary>
    public void Save(int _slot)
    {
        m_OnBeforeSaveGlobal?.Invoke();
        m_OnBeforeSaveLocal?.Invoke();

        foreach (var kv in m_Values)
            m_LoadedData[kv.Key] = kv.Value.OnSaveString();

        m_LoadedData[SCENE_KEY] = SceneManager.GetActiveScene().name;

        JsonData data = new JsonData();
        data.SetJsonType(JsonType.Object);

        foreach (var kv in m_LoadedData)
            data[kv.Key] = kv.Value;

        string path = GetSlotPath(_slot);
        string dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var writer = new JsonWriter { PrettyPrint = true };
        JsonMapper.ToJson(data, writer);
        File.WriteAllText(path, writer.ToString());
        CurrentSlot = _slot;

        var saveText = LanguageManager.instance.Get("Text_Notify_Save");
        Popup_NotifyUI.instance.Open(new Popup_NotifyUI.SOption(saveText));
        #if UNITY_EDITOR
        RefreshPreview();
        #endif
    }

    /// <summary>현재 슬롯에 저장</summary>
    public void Save()
    {
        if (CurrentSlot < 0) return;
        Save(CurrentSlot);
    }

    /// <summary>슬롯 파일 삭제</summary>
    public bool Delete(int _slot)
    {
        string path = GetSlotPath(_slot);
        if (!File.Exists(path)) return false;

        File.Delete(path);
        if (CurrentSlot == _slot)
        {
            CurrentSlot = -1;
            m_LoadedData.Clear();
        }
        return true;
    }

    /// <summary>슬롯 파일 존재 여부</summary>
    public bool Exists(int _slot)
    {
        return File.Exists(GetSlotPath(_slot));
    }

    /// <summary>슬롯 파일 경로 반환</summary>
    public static string GetSlotPath(int _slot)
    {
        string docs = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        return Path.Combine(docs, "MyGames", "DontWakeup", $"Save_{_slot}.json");
    }
    #endregion
    #if UNITY_EDITOR
    #region Editor Function
    /// <summary>등록된 모든 ValueBase 딕셔너리를 반환 (에디터 전용)</summary>
    public Dictionary<string, ValueBase> GetValues()
    {
        return m_Values;
    }

    /// <summary>Preview 리스트를 현재 값으로 갱신</summary>
    private void RefreshPreview()
    {
        for (int i = 0; i < m_Preview.Count; i++)
        {
            var p = m_Preview[i];
            if (m_Values.TryGetValue(p.id, out var v))
                m_Preview[i] = new SPreview(p.id, p.by, v.OnSaveString());
        }
    }
    #endregion
    #endif
}
