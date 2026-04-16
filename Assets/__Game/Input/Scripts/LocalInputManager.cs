using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>입력 관리 매니저, PlayerInput 콜백 우선순위(UI→Game) 분배</summary>
public class LocalInputManager : LocalManagerBase
{
    /// <summary>입력 콜백 우선순위</summary>
    public enum EPriority
    {
        UI,
        Game,
        End
    }

    public static LocalInputManager instance { get; private set; }

    #region Value
    private PlayerInput m_PlayerInput;
    private Dictionary<string, List<Func<InputAction.CallbackContext, bool>>[]> m_OnInputMap = new();
    /// <summary>바인딩 변경 시 발생하는 이벤트</summary>
    public event Action OnBindingsChanged;
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }

    public override void Init()
    {
        m_PlayerInput = transform.GetComponent<PlayerInput>();
        m_PlayerInput.onActionTriggered += OnInput;

        base.Init();
    }

    /// <summary>입력 수신 및 우선순위별 콜백 분배</summary>
    public void OnInput(InputAction.CallbackContext _context)
    {
        if (m_OnInputMap.TryGetValue(_context.action.name, out var priorityArray))
            for (int i = 0; i < (int)EPriority.End; i++)
            {
                if (i == (int)EPriority.Game && _context.performed
                    && _context.control.device is Pointer && IsPointerOverPopup())
                    return;

                var list = priorityArray[i];
                if (list == null) continue;
                for (int j = list.Count - 1; j >= 0; j--)
                    if (list[j].Invoke(_context))
                        return;
            }
    }
    #endregion

    #region Function
    /// <summary>입력 액션에 우선순위별 콜백 등록</summary>
    public void Create(LocalManagerBase _callBy, string _name, EPriority _priority, Func<InputAction.CallbackContext, bool> _callback)
    {
        if (!m_OnInputMap.TryGetValue(_name, out var array))
        {
            array = new List<Func<InputAction.CallbackContext, bool>>[(int)EPriority.End];
            m_OnInputMap.Add(_name, array);
        }

        int index = (int)_priority;
        if (array[index] == null)
            array[index] = new List<Func<InputAction.CallbackContext, bool>>();

        if (!array[index].Contains(_callback))
            array[index].Add(_callback);
    }

    /// <summary>바인딩 변경 알림</summary>
    public void NotifyBindingsChanged()
    {
        OnBindingsChanged?.Invoke();
    }

    /// <summary>현재 컨트롤 스킴에 맞는 바인딩 표시 문자열 반환</summary>
    public string GetBindingDisplayString(string _actionName)
    {
        if (m_PlayerInput == null)
            return "";

        var action = m_PlayerInput.actions.FindAction(_actionName);
        if (action == null) return _actionName;

        var scheme = m_PlayerInput.currentControlScheme;
        var bindings = action.bindings;
        string result = null;

        for (int i = 0; i < bindings.Count; i++)
        {
            var binding = bindings[i];
            if (binding.isPartOfComposite) continue;

            string display;
            if (binding.isComposite)
            {
                display = GetCompositeDisplayString(action, i, scheme);
            }
            else
            {
                if (!string.IsNullOrEmpty(scheme) && !binding.groups.Contains(scheme))
                    continue;
                display = action.GetBindingDisplayString(i);
            }

            if (string.IsNullOrEmpty(display)) continue;
            result = result == null ? display : result + " / " + display;
        }

        return result ?? _actionName;
    }

    /// <summary>Composite 바인딩에서 파트별 첫 번째 키만 표시</summary>
    private string GetCompositeDisplayString(InputAction _action, int _compositeIndex, string _scheme)
    {
        var bindings = _action.bindings;
        var usedParts = new HashSet<string>();
        var displays = new List<string>();

        for (int j = _compositeIndex + 1; j < bindings.Count && bindings[j].isPartOfComposite; j++)
        {
            var part = bindings[j];
            if (!string.IsNullOrEmpty(_scheme) && !part.groups.Contains(_scheme))
                continue;
            if (!usedParts.Add(part.name)) continue;

            var display = _action.GetBindingDisplayString(j);
            if (!string.IsNullOrEmpty(display))
                displays.Add(display);
        }

        return displays.Count > 0 ? string.Join("/", displays) : null;
    }

    /// <summary>포인터가 팝업 UI 위에 있는지 여부</summary>
    private bool IsPointerOverPopup()
    {
        var pointer = Pointer.current;
        return pointer != null && !CanProcessTouch(pointer.position.ReadValue());
    }

    /// <summary>터치/클릭 처리 가능 여부 반환, 팝업 UI 위이면 false</summary>
    public bool CanProcessTouch(Vector2 _screenPos)
    {
        if (LocalPopupManager.instance == null)
            return true;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = _screenPos;

        foreach (var id in LocalPopupManager.instance.IDs)
        {
            var popup = LocalPopupManager.instance.Get(id);
            if (!popup.IsOpened) continue;

            List<RaycastResult> result = new List<RaycastResult>();
            var raycaster = popup.PopupGraphicRaycaster;
            if (raycaster)
            {
                raycaster.Raycast(eventData, result);
                if (0 < result.Count)
                    return false;
            }
        }

        return true;
    }
    #endregion
}
