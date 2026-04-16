using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Table_Item", menuName = "Table/Item")]
public partial class Table_Item : ScriptableObject
{
    #region Type
    public class Data : TableType
    {
        #region Property
        public Table_Text.Data Name { get; private set; }
        public Table_Text.Data Description { get; private set; }
        public EKind Kind { get; private set; }
        public IReadOnlyList<string> Special { get; private set; }
        public Sprite Icon { get; private set; }
        public GameObject ItemModel { get; private set; }
        public float PopupDistance { get; private set; }
        public bool IsIngredient => Kind == EKind.Item;
        public IReadOnlyList<GameObject> EquipPrefabs { get; private set; }
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
            Description = new Table_Text.Data(_table, ID, "Desc",
                string.IsNullOrEmpty(_entry.descEng) ? null : _entry.descEng,
                string.IsNullOrEmpty(_entry.descKor) ? null : _entry.descKor,
                string.IsNullOrEmpty(_entry.descJap) ? null : _entry.descJap);
            Kind = _entry.kind;
            Icon = _entry.icon;
            var special = new List<string>();
            if (_entry.special != null)
            {
                foreach (var s in _entry.special)
                {
                    if (string.IsNullOrEmpty(s)) break;
                    special.Add(s);
                }
            }
            Special = special;
            ItemModel = _entry.itemModel;
            PopupDistance = _entry.popupDistance > 0 ? _entry.popupDistance : 5f;
            EquipPrefabs = _entry.equipPrefabs ?? new List<GameObject>();

            m_Data.Add("Name", Name);
            m_Data.Add("Desc", Description);
            m_Data.Add("Kind", Kind);
            m_Data.Add("Icon", Icon);
            m_Data.Add("Special", Special);
        }
        #endregion
    }
    [Serializable] public struct SEntry
    {
        [LabelText("ID")] public string id;
        [LabelText("아이콘"), PreviewField(50)] public Sprite icon;
        [LabelText("종류")] public EKind kind;
        [LabelText("이름(한)")] public string nameKor;
        [LabelText("이름(영)")] public string nameEng;
        [LabelText("이름(일)")] public string nameJap;
        [LabelText("설명(한)"), TextArea(2, 4)] public string descKor;
        [LabelText("설명(영)"), TextArea(2, 4)] public string descEng;
        [LabelText("설명(일)"), TextArea(2, 4)] public string descJap;
        [LabelText("특수 데이터")] public List<string> special;
        [LabelText("아이템 모델")] public GameObject itemModel;
        [LabelText("팝업 거리")] public float popupDistance;
        [LabelText("장비 프리팹")] public List<GameObject> equipPrefabs;
    }
    #endregion

    #region Inspector
    [SerializeField, LabelText("아이템 데이터"), ListDrawerSettings(ShowIndexLabels = true)]
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
            var item = new Data("Item", e.id, e);
            m_Data.Add(e.id, item);
            m_ID.Add(e.id);
            _manager.Apply(e.id, item);
        }
    }
    #endregion

    public enum EKind
    {
        Shoes = 0,
        Sword = 1,
        Hat = 2,
        Wing = 3,
        Item = 4,
    }
}
