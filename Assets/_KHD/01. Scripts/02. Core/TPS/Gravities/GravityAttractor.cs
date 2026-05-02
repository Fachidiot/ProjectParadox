/// <summary>
/// KHD 전용 글로벌 구형(Spherical) 중력 중심체입니다.
/// </summary>

using UnityEngine;

namespace KHD
{
    public class GravityAttractor : MonoBehaviour
    {
        public static GravityAttractor Instance;

        [Header("중력 설정")]
        [SerializeField] private float _gravity = -9.81f;
        [SerializeField] private bool _isInsideSphere = false;

        private void Awake()
        {
            Instance = this;
        }

        public void ToggleGravityDirection()
        {
            _isInsideSphere = !_isInsideSphere;
        }

        public void Attract(Rigidbody body)
        {
            Vector3 dirToCenter = (transform.position - body.position).normalized;
            Vector3 gravityDir = _isInsideSphere ? -dirToCenter : dirToCenter;

            body.AddForce(gravityDir * Mathf.Abs(_gravity));

            Vector3 gravityUp = -gravityDir;
            Vector3 localUp = body.transform.up;

            Quaternion targetRot = Quaternion.FromToRotation(localUp, gravityUp) * body.rotation;
            body.MoveRotation(Quaternion.Slerp(body.rotation, targetRot, 5f * Time.fixedDeltaTime));
        }
    }
}
