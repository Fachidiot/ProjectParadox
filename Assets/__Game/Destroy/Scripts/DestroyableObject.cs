using UnityEngine;

/// <summary>파괴 가능한 오브젝트, 공격 시 콜라이더/비주얼 비활성화 및 저장</summary>
public class DestroyableObject : MonoBehaviour, IAttackable
{
    #region Inspector
    [SerializeField] private GameObject m_Visual;
    [SerializeField] private Collider m_Collider;
    [SerializeField] private ParticleSystem m_DestroyParticle;
    [SerializeField] private AudioClip m_DestroySE;
    #endregion

    #region Get,Set
    public string ID => $"{gameObject.scene.name}_{gameObject.name}";
    #endregion

    #region Event
    private void Start()
    {
        LocalDestroyManager.instance.Register(this);
    }
    #endregion

    #region Function
    void IAttackable.OnAttacked(PlayerActor _attacker)
    {
        LocalDestroyManager.instance.Destroy(ID);
    }

    /// <summary>파괴 상태 적용. playEffect=true일 때만 파티클 재생</summary>
    public void ApplyDestroyedState(bool _isDestroyed, bool _playEffect = false)
    {
        if (m_Visual != null)   m_Visual.SetActive(!_isDestroyed);
        if (m_Collider != null) m_Collider.enabled = !_isDestroyed;

        if (_isDestroyed && _playEffect)
        {
            if (m_DestroyParticle != null) m_DestroyParticle.Play();
            SoundManager.instance.PlaySE(m_DestroySE, transform.position);
        }
    }
    #endregion
}
