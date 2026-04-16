using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator anim;

    // 애니메이터 파라미터 캐싱
    private readonly int hashPillow = Animator.StringToHash("isPillow");
    private readonly int hashBubble = Animator.StringToHash("isBubble");
    private readonly int hashRecover = Animator.StringToHash("Recover");

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    // 장애물에 닿았을 때 애니메이션만 실행
    public void PlayHitReaction(string type)
    {
        if (anim != null)
        {
            if (type == "Pillow") anim.SetTrigger(hashPillow);
            else if (type == "Bubble") anim.SetTrigger(hashBubble);
        }
    }

    public void OnEscape()
    {
        if (anim != null) anim.SetTrigger(hashRecover);
    }
}