using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Table_Tutorial", menuName = "Table/Tutorial")]
public partial class Table_Tutorial : ScriptableObject
{
    #region Type
    [Serializable]
    public struct SEntry
    {
        [LabelText("ID")] public string id;
        [LabelText("이름(한)")] public string nameKor;
        [LabelText("이름(영)")] public string nameEng;
        [LabelText("이름(일)")] public string nameJap;
        [LabelText("설명(한)")] public string explainKor;
        [LabelText("설명(영)")] public string explainEng;
        [LabelText("설명(일)")] public string explainJap;
    }
    #endregion

    #region Inspector
    [SerializeField, LabelText("튜토리얼 데이터"), ListDrawerSettings(ShowIndexLabels = true)]
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
            var tutorial = new Data("Tutorial", e.id, e);
            m_Data.Add(e.id, tutorial);
            m_ID.Add(e.id);
            _manager.Apply(e.id, tutorial);
        }
    }
    #endregion

    public partial class Data : TableType
    {
        #region Property
        public Table_Text.Data Name { get; private set; }
        public Table_Text.Data Explain { get; private set; }
        #endregion

        #region Event
        public Data(string _table, string _id, SEntry _entry)
        {
            Table = _table;
            ID = $"{_id}.";
            Name = new Table_Text.Data(_table, ID, "Name",
                string.IsNullOrEmpty(_entry.nameEng) ? null : _entry.nameEng,
                string.IsNullOrEmpty(_entry.nameKor) ? null : _entry.nameKor,
                string.IsNullOrEmpty(_entry.nameJap) ? null : _entry.nameJap);
            Explain = new Table_Text.Data(_table, ID, "Explain",
                string.IsNullOrEmpty(_entry.explainEng) ? null : _entry.explainEng,
                string.IsNullOrEmpty(_entry.explainKor) ? null : _entry.explainKor,
                string.IsNullOrEmpty(_entry.explainJap) ? null : _entry.explainJap);

            m_Data.Add("Name", Name);
            m_Data.Add("Explain", Explain);
        }
        #endregion
    }
}
