using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

/// <summary>아이콘 관리 매니저, ID→Sprite 매핑 및 Table/Resources 폴백</summary>
public class IconManager : GlobalManagerBase
{
    public static IconManager instance { get; private set; }

    #if UNITY_EDITOR
    #region Preview
    [Serializable] private struct SPreview
    {
        public string id;
        public string by;
        public Sprite icon;

        public SPreview(string _id, string _by, Sprite _icon)
        {
            id = _id;
            by = _by;
            icon = _icon;
        }
    }
    [SerializeField, ReadOnly] private List<SPreview> m_Preview = new();
    #endregion
    #endif
    #region Value
    private Dictionary<string, Sprite> m_Icon = new Dictionary<string, Sprite>();
    private Dictionary<string, ValueBase> m_Value = new Dictionary<string, ValueBase>();
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }
    #endregion
    #region Function
    /// <summary>ID에 아이콘 및 ValueBase 연동 등록</summary>
    public void Create(GlobalManagerBase _callBy, string _id, ValueBase _value, Func<ValueBase, Sprite> _onChanged)
    {
        if (_callBy == null)
            throw new ArgumentNullException();
        if (_callBy.IsInited)
            throw new InvalidOperationException();

        #if UNITY_EDITOR
        int index = m_Preview.Count;
        m_Preview.Add(new(_id, _callBy.name, null));
        #endif

        Table_Text.Data t = new Table_Text.Data("", _id, "", "");
        m_Icon.Add(_id, null);
        m_Value.Add(_id, _value);
        if (_value != null)
            _value.AddResourceChanged(this, (_value) =>
            {
                m_Icon[_id] = _onChanged(_value);

                #if UNITY_EDITOR
                m_Preview[index] = new(_id, _callBy.name, m_Icon[_id]);
                #endif
            }, true);
        else
        {
            m_Icon[_id] = _onChanged(_value);
            #if UNITY_EDITOR
            m_Preview[index] = new(_id, _callBy.name, m_Icon[_id]);
            #endif
        }
    }
    /// <summary>ID로 아이콘 조회, 등록→Table 순 폴백</summary>
    public Sprite Get(string _id)
    {
        if (m_Icon.TryGetValue(_id, out var icon) && !_id.Contains("."))
            return icon;
        else if (TableManager.instance.TryGet<object>(_id, out var table))
        {
            if (table is Sprite sprite)
                return sprite;
            else if (table is ITableType iTable && iTable["Icon"] is Sprite tableIcon)
                return tableIcon;
        }

        return null;
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
    #endregion
}
