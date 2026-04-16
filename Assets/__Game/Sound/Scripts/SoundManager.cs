using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>BGM/SE 볼륨을 관리하고 DealManager와 연동하는 사운드 매니저</summary>
public class SoundManager : GlobalManagerBase
{
    public static SoundManager instance { get; private set; }

    #if UNITY_EDITOR
    #region Preview
    [Serializable] private struct SPreview
    {
        public string id;
        public float value;
        public SPreview(string _id, float _value) { id = _id; value = _value; }
    }
    [SerializeField, ReadOnly] private List<SPreview> m_Preview = new();
    #endregion
    #endif
    #region Property
    /// <summary>BGM 볼륨 값 (로컬 저장)</summary>
    public IReadOnlyFloatValue BGMVolume => m_BGMVolume;
    /// <summary>SE 볼륨 값 (로컬 저장)</summary>
    public IReadOnlyFloatValue SEVolume => m_SEVolume;
    #endregion
    #region Value
    private FloatValue m_BGMVolume;
    private FloatValue m_SEVolume;
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }
    public override void Init()
    {
        m_BGMVolume = PlayerPrefsSaveManager.instance.Create(this, new FloatValue(this, "BGMVolume", 1.0f));
        m_SEVolume = PlayerPrefsSaveManager.instance.Create(this, new FloatValue(this, "SEVolume", 1.0f));

        m_BGMVolume.AddConstraintChanged(this, (_) =>
        {
            m_BGMVolume.Set(Mathf.Clamp01(m_BGMVolume.v), false, false);
        });
        m_SEVolume.AddConstraintChanged(this, (_) =>
        {
            m_SEVolume.Set(Mathf.Clamp01(m_SEVolume.v), false, false);
        });

        DealManager.instance.Create(this, "BGMVolume", (_deal, _seed) =>
        {   //Need
            return m_BGMVolume.v < _deal.CountFloat;
        }, (_deal) =>
        {   //NeedValue
            return new ValueBase[] { m_BGMVolume };
        }, (_deal, _seed) =>
        {   //Set
            SetBGMVolume(_deal.CountFloat);
            return new SDeal[] { _deal };
        }, (_deal, _seed) =>
        {   //Change
            SetBGMVolume(m_BGMVolume.v + _deal.CountFloat);
            return new SDeal[] { _deal };
        }, null);

        DealManager.instance.Create(this, "SEVolume", (_deal, _seed) =>
        {   //Need
            return m_SEVolume.v < _deal.CountFloat;
        }, (_deal) =>
        {   //NeedValue
            return new ValueBase[] { m_SEVolume };
        }, (_deal, _seed) =>
        {   //Set
            SetSEVolume(_deal.CountFloat);
            return new SDeal[] { _deal };
        }, (_deal, _seed) =>
        {   //Change
            SetSEVolume(m_SEVolume.v + _deal.CountFloat);
            return new SDeal[] { _deal };
        }, null);

        LanguageManager.instance.Create(this, "BGMVolume", m_BGMVolume, (_, _table) => _table.SetEng($"{Mathf.RoundToInt(m_BGMVolume.v * 100)}%"));
        LanguageManager.instance.Create(this, "SEVolume", m_SEVolume, (_, _table) => _table.SetEng($"{Mathf.RoundToInt(m_SEVolume.v * 100)}%"));

        #if UNITY_EDITOR
        m_Preview.Add(new("BGMVolume", m_BGMVolume.v));
        m_Preview.Add(new("SEVolume", m_SEVolume.v));
        m_BGMVolume.AddChanged(this, _ => RefreshPreview());
        m_SEVolume.AddChanged(this, _ => RefreshPreview());
        #endif

        base.Init();
    }
    #endregion
    #region Function
    /// <summary>BGM 볼륨 값을 설정</summary>
    public void SetBGMVolume(float _value)
    {
        m_BGMVolume.v = _value;
    }
    /// <summary>SE 볼륨 값을 설정</summary>
    public void SetSEVolume(float _value)
    {
        m_SEVolume.v = _value;
    }
    /// <summary>SE 클립을 월드 위치에서 재생 (SEVolume 적용)</summary>
    public void PlaySE(AudioClip _clip, Vector3 _position, float _volume = 1f)
    {
        if (_clip != null)
            AudioSource.PlayClipAtPoint(_clip, _position, _volume * m_SEVolume.v);
    }
    #endregion
    #if UNITY_EDITOR
    #region Editor Function
    private void RefreshPreview()
    {
        m_Preview.Clear();
        m_Preview.Add(new("BGMVolume", m_BGMVolume.v));
        m_Preview.Add(new("SEVolume", m_SEVolume.v));
    }
    #endregion
    #endif
}
