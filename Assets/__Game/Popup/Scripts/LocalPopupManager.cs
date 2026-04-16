using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>팝업 관리 매니저, Open/Close/정렬/Cancel 입력 처리</summary>
public class LocalPopupManager : LocalManagerBase
{
    public static LocalPopupManager instance { get; private set; }
    #region Inspector
    [SerializeField] private Camera m_UICamera;
    [SerializeField] private bool m_UICameraOrthographic = false;
    [SerializeField] private int m_BottomFixedOrder = 50;
    [SerializeField] private bool m_IsCloseByCancel = true;
    #endregion
    #region Property
    public Camera CurCam { get => m_UICamera; }
    /// <summary>등록된 모든 팝업 ID 배열</summary>
    public string[] IDs => m_PopupDic.Keys.ToArray();
    #endregion
    #region Value
    private PopupBase[] m_Popup;
    private Dictionary<string, PopupBase> m_PopupDic = new Dictionary<string, PopupBase>();
    private List<PopupBase> m_OpenList = new List<PopupBase>();
    #endregion
  
    #region Event
    public override void InitSingleton()
    {
        instance = this;

        m_Popup = GetComponentsInChildren<PopupBase>(true);
        foreach (var v in m_Popup)
            v.InitSingleton();

        base.InitSingleton();
    }
    public override void Init()
    {
        m_UICamera.orthographic = m_UICameraOrthographic;

        foreach (var v in m_Popup)
        {
            m_PopupDic.Add(v.name, v);
            v.Init(m_UICamera);
        }

        LocalInputManager.instance.Create(this, "Cancel", LocalInputManager.EPriority.UI, OnInputCancel);

        base.Init();
    }
    public override void AfterInit()
    {
        foreach (var v in m_Popup)
        {
            v.AfterInit();

            if (v.IsDefaultOpen)
            {
                v.OnOpen();
                m_OpenList.Add(v);
            }
        }
        sort();

        base.AfterInit();
    }
    public override void AfterInitGame()
    {
        foreach (var v in m_Popup)
            v.AfterInitGame();

        base.AfterInitGame();
    }
    public override void OnShutdown()
    {
        if (EventSystem.current != null)
            EventSystem.current.enabled = false;

        foreach (var v in m_Popup)
        {
            v.OnShutdown();
            v.enabled = false;
        }

        base.OnShutdown();
    }

    private bool OnInputCancel(InputAction.CallbackContext _context)
    {
        for (int i = m_OpenList.Count - 1; i >= 0; i--)
            if (m_OpenList[i].OnInputCancel(_context))
                return true;

        if (_context.performed && m_IsCloseByCancel)
            Popup_Quit.instance.Open();
        
        return false;
    }
    #endregion
    #region Local Function
    private void sort()
    {
        int startOrder = -1;
        foreach (var v in m_OpenList)
            if (startOrder < v.FixedOreder && v.FixedOreder < m_BottomFixedOrder)
                startOrder = v.FixedOreder;

        for (int i = 0; i < m_OpenList.Count; i++)
            m_OpenList[i].OnSort(i + startOrder + 1);
    }
    #endregion
    #region Function
    /// <summary>ID로 팝업 열기</summary>
    public void Open(string _id, object _option = null)
    {
        var isOpened = m_PopupDic[_id].IsOpened;
        m_PopupDic[_id].OnOpen(_option);

        if (isOpened)
            return;

        m_OpenList.Add(m_PopupDic[_id]);
        sort();
    }
    /// <summary>ID로 팝업 닫기</summary>
    public void Close(string _id, object _option = null)
    {
        if (!m_PopupDic[_id].IsOpened)
            return;

        m_OpenList.Remove(m_PopupDic[_id]);
        sort();

        m_PopupDic[_id].OnStartClose(_option);
    }
    /// <summary>ID로 팝업 표시/숨김 설정</summary>
    public void SetShow(string _id, bool _isShow)
    {
        m_PopupDic[_id].OnSetShow(_isShow);
    }
    /// <summary>ID로 팝업 조회</summary>
    public PopupBase Get(string _id)
    {
        return m_PopupDic[_id];
    }
    #endregion
}
