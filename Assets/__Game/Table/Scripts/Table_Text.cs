using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Table_Text", menuName = "Table/Text")]
public partial class Table_Text : ScriptableObject
{
    #region Type
    [Serializable]
    public struct SEntry
    {
        [LabelText("ID")] public string id;
        [LabelText("한국어")] public string kor;
        [LabelText("영어")] public string eng;
        [LabelText("일본어")] public string jap;
        [LabelText("리치텍스트")] public string rich;
    }
    #endregion

    #region Inspector
    [SerializeField, LabelText("텍스트 데이터"), ListDrawerSettings(ShowIndexLabels = true)]
    private List<SEntry> m_Entries = new();
    #endregion

    #region Property
    public IReadOnlyList<string> ID => m_ID;
    #endregion

    #region Value
    private Dictionary<string, Data> m_Data = new();
    private List<string> m_ID = new();
    #endregion

    #region Event
    public void Init(TableManager _manager)
    {
        m_Data.Clear();
        m_ID.Clear();

        foreach (var e in m_Entries)
        {
            var text = new Data("Text", e.id, "",
                string.IsNullOrEmpty(e.eng) ? null : e.eng,
                string.IsNullOrEmpty(e.kor) ? null : e.kor,
                string.IsNullOrEmpty(e.jap) ? null : e.jap,
                string.IsNullOrEmpty(e.rich) ? null : e.rich);
            m_Data.Add(e.id, text);
            m_ID.Add(e.id);
            _manager.Apply(e.id, text);
        }
    }
    #endregion

    public partial class Data : TableType
    {
        #region Property
        public string Kor { get; private set; }
        public string Eng { get; private set; }
        public string Jap { get; private set; }
        public string Rich { get; private set; }
        #endregion

        #region Event
        public Data(string _table, string _baseID, string _addID, string _eng, string _kor = null, string _jap = null, string _rich = null)
        {
            Table = _table;
            ID = $"{_baseID}.{_addID}";
            Kor = _kor;
            Eng = _eng;
            Jap = _jap;
            Rich = _rich;

            m_Data.Add("Kor", Kor);
            m_Data.Add("Eng", Eng);
            m_Data.Add("Jap", Jap);
            m_Data.Add("Rich", Rich);
        }
        #endregion

        #region Function
        public string Translate(SystemLanguage _lang, bool _isRich)
        {
            switch (_lang)
            {
                case SystemLanguage.Korean:
                    if (Kor != null)
                        return Kor;
                    break;
                case SystemLanguage.English:
                    if (Eng != null)
                        return Eng;
                    break;
                case SystemLanguage.Japanese:
                    if (Jap != null)
                        return Jap;
                    break;
            }
            return Eng != null ? Eng : ID;
        }

        public Data Clear()
        {
            Kor = null;
            Eng = null;
            Jap = null;
            Rich = null;
            return this;
        }

        public Data SetEng(string _eng)
        {
            Eng = _eng;
            return this;
        }
        #endregion
    }
}
