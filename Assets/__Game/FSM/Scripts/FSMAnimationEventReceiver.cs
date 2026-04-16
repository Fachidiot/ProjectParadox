using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>Animator의 애니메이션 이벤트를 직접 처리</summary>
public class FSMAnimationEventReceiver : MonoBehaviour
{
    #region Inspector
    [SerializeField, TabGroup("Sound"), LabelText("발소리 클립")] private AudioClip[] m_FootstepClips;
    [SerializeField, TabGroup("Sound"), LabelText("착지 클립")] private AudioClip m_LandClip;
    #endregion

    #region Animation Event
    private void OnFootstep()
    {
        if (m_FootstepClips != null && m_FootstepClips.Length > 0)
        {
            var clip = m_FootstepClips[Random.Range(0, m_FootstepClips.Length)];
            SoundManager.instance.PlaySE(clip, transform.position);
        }
    }

    private void OnLand()
    {
        if (m_LandClip != null)
            SoundManager.instance.PlaySE(m_LandClip, transform.position);
    }
    #endregion
}
