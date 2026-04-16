using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>다국어 관리 매니저, ID→번역 텍스트 및 플레이스홀더 치환</summary>
public class LanguageManager : GlobalManagerBase
{
    public static LanguageManager instance { get; private set; }

    #if UNITY_EDITOR
    #region Preview
    [Serializable] private struct SPreview
    {
        public string id;
        public string by;
        public string eng;
        public string kor;
        public SPreview(string _id, string _by, string _eng, string _kor)
        {
            id = _id;
            by = _by;
            eng = _eng;
            kor = _kor;
        }
    }
    [SerializeField, ReadOnly] private List<SPreview> m_Preview = new();
    #endregion
    #endif
    #region Property
    /// <summary>현재 선택된 언어</summary>
    public IReadOnlyEnumValue<SystemLanguage> Language => m_Language;

    /// <summary>지원하는 언어 목록</summary>
    public IReadOnlyList<SystemLanguage> LanguageList => m_LanguageList;

    /// <summary>언어별 인덱스 매핑</summary>
    public IReadOnlyDictionary<SystemLanguage, int> LanguageIndex => m_LanguageIndex;
    #endregion
    #region Value
    private EnumValue<SystemLanguage> m_Language;
    private List<SystemLanguage> m_LanguageList = new List<SystemLanguage>
    {
        SystemLanguage.English,
        SystemLanguage.Korean,
        SystemLanguage.Japanese
    };
    private Dictionary<SystemLanguage, int> m_LanguageIndex = new Dictionary<SystemLanguage, int>
    {
        { SystemLanguage.English, 0 },
        { SystemLanguage.Korean, 1 },
        { SystemLanguage.Japanese, 2 }
    };

    private Dictionary<string, Table_Text.Data> m_Text = new Dictionary<string, Table_Text.Data>();
    private Dictionary<string, ValueBase> m_Value = new Dictionary<string, ValueBase>();
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }
    public override void InitFirst()
    {
        m_Language = PlayerPrefsSaveManager.instance.Create(this, new EnumValue<SystemLanguage>(this, "lang_lng", Application.systemLanguage));
        base.InitFirst();
    }
    #endregion
    #region Function
    /// <summary>현재 언어 설정</summary>
    public void SetLanguage(SystemLanguage _language)
    {
        m_Language.v = _language;
    }

    /// <summary>ID에 텍스트 번역 데이터 등록</summary>
    public void Create(GlobalManagerBase _callBy, string _id, ValueBase _value, Action<ValueBase, Table_Text.Data> _onChanged)
    {
        if (_callBy == null)
            throw new ArgumentNullException();
        if (_callBy.IsInited)
            throw new InvalidOperationException();

        #if UNITY_EDITOR
        int index = m_Preview.Count;
        m_Preview.Add(new(_id, _callBy.name, "", ""));
        #endif

        Table_Text.Data t = new Table_Text.Data("", _id, "", "");
        m_Text.Add(_id, t);
        if (_value != null)
        {
            m_Value.Add(_id, _value);
            _value.AddResourceChanged(this, (_value) =>
            {
                _onChanged(_value, t);
                #if UNITY_EDITOR
                m_Preview[index] = new(_id, _callBy.name, t.Eng, t.Kor);
                #endif
            }, true);
        }
        else
        {
            _onChanged(_value, t);
            #if UNITY_EDITOR
            m_Preview[index] = new(_id, _callBy.name, t.Eng, t.Kor);
            #endif
        }
    }
    /// <summary>ID로 현재 언어 번역 텍스트 조회</summary>
    public string Get(string _id, bool _isFormat = true, bool _isRich = false)
    {
        return Get(_id, _isFormat, _isRich, Language.v);
    }
    /// <summary>ID로 지정 언어 번역 텍스트 조회</summary>
    public string Get(string _id, bool _isFormat, bool _isRich, SystemLanguage _lang)
    {
        if (_id == "Event_Exp2X.Explain")
            Debug.Log("ASDF");
        string msg = _id;

        if (m_Text.TryGetValue(_id, out var v) && !_id.Contains("."))
            msg = v.Translate(_lang, _isRich);
        else if (TableManager.instance.TryGet<object>(_id, out var table))
        {
            if (table is Table_Text.Data textTable)
                msg = textTable.Translate(_lang, _isRich);
            else if (table is ITableType iTable && iTable["Name"] is Table_Text.Data n)
                msg = n.Translate(_lang, _isRich);
            else
                msg = table.ToString();
        }

        if (_isFormat)
            return UseFormat(msg, _isRich, _lang);
        else
            return msg;
    }
    /// <summary>ID로 연동된 ValueBase 조회</summary>
    public ValueBase GetValue(string _id)
    {
        if (m_Value.TryGetValue(_id, out var v))
            return v;
        return null;
    }
    /// <summary>복수 ID로 연동된 ValueBase 배열 조회</summary>
    public ValueBase[] GetValues(string[] _ids)
    {
        ValueBase[] values = new ValueBase[_ids.Length];
        for (int i = 0; i < _ids.Length; i++)
            values[i] = GetValue(_ids[i]);

        return values;
    }

    /// <summary>텍스트 내 플레이스홀더 치환 (현재 언어)</summary>
    public string UseFormat(string _source, bool _isRich = false)
    {
        return UseFormat(_source, _isRich, Language.v);
    }
    /// <summary>텍스트 내 플레이스홀더 치환 (지정 언어)</summary>
    public string UseFormat(string _source, bool _isRich, SystemLanguage _lang)
    {
        string result = Regex.Replace(_source, @"\{([^\}]+)\}", (_match) =>
        {
            string key = _match.Groups[1].Value;
            if (m_Text.TryGetValue(key, out var v))
                return v.Translate(_lang, _isRich);
            else if (TableManager.instance.TryGet<object>(key, out var table))
            {
                if (table is Table_Text.Data textTable)
                    return textTable.Translate(_lang, _isRich);
                else if (table is ITableType iTable && iTable["Name"] is Table_Text.Data n)
                    return n.Translate(_lang, _isRich);
            }

            return _match.Value;
        });
        return result;
    }
    #endregion
}
