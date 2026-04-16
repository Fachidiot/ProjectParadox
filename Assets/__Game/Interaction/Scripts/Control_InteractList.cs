using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>상호작용 가능 대상 리스트 UI, 스크롤 선택 및 키 입력 상호작용</summary>
public class Control_InteractList : ControlBase
{
    #region Inspector
    [SerializeField, LabelText("템플릿")] private GameObject m_Template;
    [SerializeField, LabelText("풀 크기")] private int m_PoolSize = 5;
    [SerializeField, LabelText("아이템 간격")] private float m_ItemSpacing = 80f;
    [SerializeField, LabelText("스크롤 보간 속도")] private float m_LerpSpeed = 10f;
    [SerializeField, LabelText("키 설명 오브젝트")] private GameObject m_KeyGuide;
    #endregion
    #region Value
    private ObjectPool m_Pool;
    private List<RectTransform> m_ActiveItems = new();
    private int m_SelectedIndex;
    private float m_CurrentOffset;
    private float m_ScrollCooldown;
    #endregion

    #region Event
    public override void AfterInitGame()
    {
        m_Pool = new ObjectPool(m_Template, m_Template.transform.parent, m_PoolSize);

        LocalInteractionManager.instance.Interactables.AddChanged(this, _ => Refresh());
        LocalInteractionManager.instance.CanInteract.AddChanged(this, _ => Refresh());
        SettingManager.instance.UseInteractUI.AddChanged(this, _ => Refresh());

        var input = LocalInputManager.instance;
        input.Create(LocalInteractionManager.instance, "ScrollWheel", LocalInputManager.EPriority.UI, OnScrollInput);
        input.Create(LocalInteractionManager.instance, "Interact", LocalInputManager.EPriority.UI, OnInteractInput);

        Refresh();
        base.AfterInitGame();
    }

    private void Update()
    {
        if (m_ActiveItems.Count == 0) return;

        if (m_ScrollCooldown > 0f)
            m_ScrollCooldown -= Time.deltaTime;

        m_CurrentOffset = Mathf.Lerp(m_CurrentOffset, m_SelectedIndex, Time.deltaTime * m_LerpSpeed);
        UpdatePositions();
    }
    #endregion

    #region Input
    private bool OnScrollInput(InputAction.CallbackContext _context)
    {
        if (!_context.performed) return false;
        if (!SettingManager.instance.UseInteractUI.v) return false;
        if (m_ActiveItems.Count == 0) return false;
        if (!LocalInteractionManager.instance.CanInteract.v) return false;
        if (m_ScrollCooldown > 0f) return true;

        var scroll = _context.ReadValue<Vector2>();
        int prev = m_SelectedIndex;
        if (scroll.y > 0)
            m_SelectedIndex = Mathf.Max(0, m_SelectedIndex - 1);
        else if (scroll.y < 0)
            m_SelectedIndex = Mathf.Min(m_ActiveItems.Count - 1, m_SelectedIndex + 1);

        if (prev != m_SelectedIndex)
        {
            m_ScrollCooldown = 0.15f;
            UpdateHighlight();
        }

        return true;
    }

    private bool OnInteractInput(InputAction.CallbackContext _context)
    {
        if (!_context.performed) return false;
        if (!SettingManager.instance.UseInteractUI.v) return false;
        if (m_ActiveItems.Count == 0) return false;
        if (!LocalInteractionManager.instance.CanInteract.v) return false;

        var manager = LocalInteractionManager.instance;
        var keys = manager.Interactables.LocalKeys;
        if (m_SelectedIndex >= 0 && m_SelectedIndex < keys.Count)
        {
            var actor = LocalPlayerActorManager.instance.CurActor;
            manager.Interact(keys[m_SelectedIndex], actor);
            return true;
        }
        return false;
    }
    #endregion

    #region Function
    private void Refresh()
    {
        m_Pool.Clear();
        m_ActiveItems.Clear();

        var manager = LocalInteractionManager.instance;
        if (!SettingManager.instance.UseInteractUI.v || !manager.CanInteract.v)
        {
            if (m_KeyGuide != null)
                m_KeyGuide.SetActive(false);
            return;
        }

        var actor = LocalPlayerActorManager.instance.CurActor;
        foreach (var interactable in manager.Interactables.LocalKeys)
        {
            var obj = m_Pool.Get();
            if (obj == null) break;
            obj.GetComponent<Control_Button_Interact>().Set(interactable, actor);
            m_ActiveItems.Add(obj.GetComponent<RectTransform>());
        }

        if (m_ActiveItems.Count > 0)
            m_SelectedIndex = Mathf.Clamp(m_SelectedIndex, 0, m_ActiveItems.Count - 1);
        else
            m_SelectedIndex = 0;

        if (m_KeyGuide != null)
            m_KeyGuide.SetActive(m_ActiveItems.Count > 0);

        m_CurrentOffset = m_SelectedIndex;
        UpdatePositions();
        UpdateHighlight();
    }

    private void UpdateHighlight()
    {
        for (int i = 0; i < m_ActiveItems.Count; i++)
            m_ActiveItems[i].GetComponent<Control_Button_Interact>().SetHighlight(i == m_SelectedIndex);
    }

    private void UpdatePositions()
    {
        for (int i = 0; i < m_ActiveItems.Count; i++)
        {
            float offset = i - m_CurrentOffset;
            var rt = m_ActiveItems[i];
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -offset * m_ItemSpacing);
        }
    }
    #endregion
}
