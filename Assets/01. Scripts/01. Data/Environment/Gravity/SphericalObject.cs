/// <summary>
/// 구형 중력의 영향을 받아야 하는 오브젝트에 부착하는 컴포넌트.
/// Unity의 기본 중력을 끄고 GravityAttractor를 통해 커스텀 중력을 적용받습니다.
/// </summary>

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SphericalObject : MonoBehaviour
{
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        
        // 유니티 기본 중력 비활성화 (커스텀 중력 사용을 위함)
        _rb.useGravity = false;
        
        // 구형 맵에서 구르기보다는 똑바로 서 있는 것이 중요할 경우 회전 제약 추가 (선택사항)
        // _rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void FixedUpdate()
    {
        // 물리 연산 시마다 커스텀 중력 적용
        if (!_rb.isKinematic && GravityAttractor.Instance != null)
        {
            GravityAttractor.Instance.Attract(_rb);
        }
    }
}
