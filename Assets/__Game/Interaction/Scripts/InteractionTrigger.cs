using UnityEngine;

/// <summary>트리거 범위 내 InteractionObject을 매니저에 등록/해제</summary>
public class InteractionTrigger : MonoBehaviour
{
    #region Event
    private void OnTriggerEnter(Collider other)
    {
        var interactable = other.GetComponent<InteractionObject>();
        if (interactable != null)
            LocalInteractionManager.instance.Add(interactable);
    }

    private void OnTriggerExit(Collider other)
    {
        var interactable = other.GetComponent<InteractionObject>();
        if (interactable != null)
            LocalInteractionManager.instance.Remove(interactable);
    }
    #endregion
}
