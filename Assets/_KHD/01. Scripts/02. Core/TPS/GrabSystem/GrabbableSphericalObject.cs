/// <summary>
/// KHD 전용 구형 중력의 영향을 받아야 하는 오브젝트.
/// Unity의 기본 중력을 끄고 KHD_GravityAttractor를 통해 커스텀 중력을 적용받습니다.
/// </summary>

using UnityEngine;

namespace KHD
{
    [RequireComponent(typeof(Rigidbody))]
    public class GrabbableSphericalObject : MonoBehaviour
    {
        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
        }

        private void FixedUpdate()
        {
            if (!_rb.isKinematic && GravityAttractor.Instance != null)
            {
                GravityAttractor.Instance.Attract(_rb);
            }
        }
    }
}