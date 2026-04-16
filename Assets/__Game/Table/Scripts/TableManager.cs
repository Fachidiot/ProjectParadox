using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class TableManager : GlobalManagerBase
{
    public static TableManager instance { get; private set; }

    #if UNITY_EDITOR
    #region Preview
    [SerializeField, ReadOnly] private List<string> m_TextPreview;
    [SerializeField, ReadOnly] private List<string> m_ItemPreview;
    [SerializeField, ReadOnly] private List<string> m_TutorialPreview;
    #endregion
    #endif

    #region Inspector
    [SerializeField] private Table_Text m_Text;
    [SerializeField] private Table_Item m_Item;
    [SerializeField] private Table_Tutorial m_Tutorial;
    #endregion

    #region Property
    public Table_Text Text => m_Text;
    public Table_Item Item => m_Item;
    public Table_Tutorial Tutorial => m_Tutorial;
    #endregion

    #region Value
    private Dictionary<string, object> m_AllData = new();
    private List<string> m_AllID = new();
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }
    public override void InitFirst()
    {
        m_Text.Init(this);
        base.InitFirst();

        #if UNITY_EDITOR
        m_TextPreview = m_AllID;
        #endif
    }
    public override void Init()
    {
        m_Item.Init(this);
        m_Tutorial.Init(this);
        base.Init();
        #if UNITY_EDITOR
        m_ItemPreview = Item.ID as List<string>;
        m_TutorialPreview = Tutorial.ID as List<string>;
        #endif
    }

    #endregion

    #region Function
    public void Apply(string _id, object _data)
    {
        m_AllData.Set(_id, _data);

        if (!m_AllID.Contains(_id))
            m_AllID.Add(_id);
    }

    public T Get<T>(string _path)
    {
        var paths = _path.Split('.');
        if (!m_AllData.TryGetValue(paths[0], out var root))
            return default;
        if (paths.Length == 1)
            return (T)root;

        var table = root;
        for (int i = 1; i < paths.Length; ++i)
        {
            if (table is ITableType iTable)
                table = iTable[paths[i]];
            else
                return default;
        }
        return (T)table;
    }

    public bool TryGet<T>(string _path, out T _out)
    {
        var o = Get<object>(_path);
        _out = (T)o;
        return o != null;
    }
    #endregion
}
