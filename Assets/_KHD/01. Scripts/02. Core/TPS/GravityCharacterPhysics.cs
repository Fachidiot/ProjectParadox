/// <summary>
/// Project Paradox 전용 구형 중력 물리 커스텀 클래스입니다.
/// CharacterPhysics3D를 상속하되, 기존 Y축 기반 중력을 무시하고 전천후 구면 법선(GroundNormal) 기반 수직/수평 이동을 처리합니다.
/// </summary>

using Sirenix.OdinInspector;
using UnityEngine;

namespace KHD
{
    public class GravityCharacterPhysics : CharacterPhysics3D
    {
        [Title("구형 중력 설정 (KHD)")]
        [SerializeField] private float _rayDistance = 3f;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _rotationSmoothing = 15f;

        // 점프/중력 제어용 (PlayerMovement에서 이관)
        [SerializeField] private float _jumpHeight = 2.0f;
        [SerializeField] private float _gravityForce = -9.81f;
        [SerializeField] private float _terminalVelocity = -50f;
        [Tooltip("착지 Ray 시작점 오프셋 (기본 위치에서 +위/-아래)")]
        [SerializeField] private float _groundCheckOrigin = 0f;
        [Tooltip("착지 Ray 여유분 (캡슐 바닥 아래로 추가 탐지)")]
        [SerializeField] private float _groundCheckSkin = 0.25f;

        [Tooltip("경사면 전환 쿨타임 (모서리에서 빠르게 전환되는 현상 방지)")]
        [SerializeField] private float _surfaceCooldown = 0.2f;

        private Vector3 _groundNormal = Vector3.up;
        private Vector3 _currentTargetNormal = Vector3.up;
        private float _ignoreSurfaceTime = 0f;
        private float _surfaceCooldownTimer = 0f;
        private float _verticalVelocity = 0f;

        // FSM 연동을 위해 임시 저장되는 이동/점프 벡터
        private Vector2 _currentMoveTarget = Vector2.zero;
        private bool _isMovedThisFrame = false;
        private bool _hasJumpedKHD = false;
        private bool _isFlyingKHD = false;
        private CapsuleCollider _capsule;

        public Vector3 GroundNormal => _groundNormal;
        public float VerticalVelocity => _verticalVelocity;

        protected override void FixedUpdate()
        {
            // 베이스 클래스의 2D 지향 Flat FixedUpdate를 막습니다 (의도된 덮어쓰기)

            DetectSurface();
            HandleBodyRotation();
            HandleMovementKHD();

            _isMovedThisFrame = false;
            _hasJumpedKHD = false;
        }

        public override EFlyState FlyState
        {
            get
            {
                if (_isFlyingKHD) return EFlyState.Fly;
                if (_verticalVelocity > 0.1f) return EFlyState.Jump;
                return CheckGrounded() ? EFlyState.None : EFlyState.Float;
            }
        }

        public override void StartFly()
        {
            _isFlyingKHD = true;
            _verticalVelocity = 0f;
            base.StartFly();
        }

        public override void StopFly()
        {
            _isFlyingKHD = false;
            base.StopFly();
        }

        /// <summary>
        /// FSM(PlayerActorState) 등에서 전달받은 이동명령을 버퍼에 넣습니다.
        /// </summary>
        public override void Move(Vector2 _vec, bool _isNow = false)
        {
            // 베이스와 동일한 인터페이스 유지, 내부 처리는 Spherical 구조 따름
            _currentMoveTarget = _vec;
            _isMovedThisFrame = true;
        }

        /// <summary>
        /// FSM(PlayerActorState) 등에서 전달받은 점프명령을 버퍼에 넣습니다.
        /// </summary>
        public override void Jump(float _power = 1.0f)
        {
            _hasJumpedKHD = true;
        }

        /// <summary>
        /// 외부 점프대(GravityFlipPad) 등에 의해 허공으로 수직 런치됩니다.
        /// </summary>
        public void Launch(float verticalForce)
        {
            _verticalVelocity = verticalForce;
        }

        /// <summary>
        /// 점프대 등에서 중력이 180도 반전되는 기믹 전용입니다.
        /// </summary>
        public void InvertGravity()
        {
            _groundNormal = -_groundNormal;
            _ignoreSurfaceTime = Time.time + 0.5f;
        }

        /// <summary>
        /// 포물선 궤도가 깨지지 않도록 속도의 기호를 뒤집어 줍니다.
        /// </summary>
        public void InvertVerticalVelocity()
        {
            _verticalVelocity = -_verticalVelocity;
        }

        #region 물리 내부 연산
        private void DetectSurface()
        {
            if (Time.time < _ignoreSurfaceTime) return;

            if (_surfaceCooldownTimer > 0f)
                _surfaceCooldownTimer -= Time.fixedDeltaTime;

            Vector3 rayOrigin = transform.position + transform.up * 0.5f + transform.forward * 0.2f;
            Ray forwardRay = new Ray(rayOrigin, transform.forward - transform.up);
            Ray downRay = new Ray(rayOrigin, -transform.up);

            bool hitForward = Physics.Raycast(forwardRay, out RaycastHit forwardHit, _rayDistance + 0.5f, _groundLayer);
            bool hitDown = Physics.Raycast(downRay, out RaycastHit downHit, _rayDistance + 0.5f, _groundLayer);

            Vector3 newTargetNormal = _groundNormal;

            if (hitForward) { newTargetNormal = forwardHit.normal; }
            else if (hitDown) { newTargetNormal = downHit.normal; }

            // 목표 노멀이 기존과 다를 때 (예: 5도 이상 차이)
            if (Vector3.Angle(_currentTargetNormal, newTargetNormal) > 5f)
            {
                // 쿨타임이 끝났다면 새로운 노멀을 채택하고 쿨타임 재시작
                if (_surfaceCooldownTimer <= 0f)
                {
                    _currentTargetNormal = newTargetNormal;
                    _surfaceCooldownTimer = _surfaceCooldown;
                }
            }
            else
            {
                // 같은 표면이거나 오차가 적은 경우는 바로 갱신
                _currentTargetNormal = newTargetNormal;
            }

            _groundNormal = Vector3.Slerp(_groundNormal, _currentTargetNormal, 15f * Time.fixedDeltaTime);
        }

        private void HandleBodyRotation()
        {
            Vector3 targetUp = _groundNormal;
            Quaternion tiltRotation = Quaternion.FromToRotation(transform.up, targetUp);

            // 이동 입력(moveDir)에 따른 회전은 별도의 카메라 방향을 계산하여 넣거나,
            // PlayerActor.RotateTowardMoveDir 로직이 이제 이 클래스의 위젯이나 부모 Transform을 돌리는 방식으로 연계될 수 있습니다.
            // 현재는 지면(구체) 밀착 회전만 담당합니다. (Z, X축 보정)
            Quaternion targetRotation = tiltRotation * transform.rotation;
            Quaternion nextRotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * _rotationSmoothing);
            Rig.MoveRotation(nextRotation.normalized);
        }

        private void HandleMovementKHD()
        {
            bool isGrounded = CheckGrounded();

            // ① 착지 상태: 중력 누적을 먼저 0으로 슬럭 후 중력 연산 (=같은 프레임에 충돌)
            if (isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = 0f;

            // ② 점프 입력 at 착지 상태
            if (_hasJumpedKHD && isGrounded)
                _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravityForce);

            // ③ 비행 모드가 아닌 경우에만 중력 누적
            if (!_isFlyingKHD)
            {
                _verticalVelocity += _gravityForce * Time.fixedDeltaTime;
                if (_verticalVelocity < _terminalVelocity)
                    _verticalVelocity = _terminalVelocity;
            }

            if (LocalCameraManager.instance == null) return;
            Transform camRot = LocalCameraManager.instance.RotRoot;

            // ④ 카메라 방향을 지면법선에 평행하게 투영
            Vector3 forward = Vector3.ProjectOnPlane(camRot.forward, _groundNormal).normalized;
            Vector3 right = Vector3.ProjectOnPlane(camRot.right, _groundNormal).normalized;

            // ⑤ 이동 벡터: 조작이 없으면 0, 있으면 카메라 기준 3D방향 산출
            Vector3 horizontalMove = Vector3.zero;
            if (_isMovedThisFrame)
            {
                Vector3 moveDir = (forward * _currentMoveTarget.y + right * _currentMoveTarget.x).normalized;
                horizontalMove = moveDir * MoveSpeed.v;
            }

            // ⑥ 수직 속도 = ground Normal 방향으로 _verticalVelocity 적용
            Vector3 finalVelocity = horizontalMove + _groundNormal * _verticalVelocity;
            Rig.linearVelocity = finalVelocity;
        }

        /// <summary> 착지 판정용 값 계산 </summary>
        private void CalcGroundCheckParams(out Vector3 origin, out float castDist)
        {
            if (_capsule == null) _capsule = GetComponentInChildren<CapsuleCollider>();

            Vector3 capsuleWorldCenter = _capsule != null
                ? _capsule.transform.TransformPoint(_capsule.center)
                : transform.position;
            float halfHeight = _capsule != null ? _capsule.height * 0.5f : 1.0f;

            // 원점 = 캡슐 중심 + 인스펙터 오프셋
            origin = capsuleWorldCenter + transform.up * _groundCheckOrigin;
            // 거리 = 반높이 + 오프셋 + 발 밑 여유분
            castDist = halfHeight + _groundCheckOrigin + _groundCheckSkin;
        }

        private bool CheckGrounded()
        {
            if (_verticalVelocity > 0.1f) return false;
            if (_capsule == null) _capsule = GetComponentInChildren<CapsuleCollider>();
            if (_capsule == null) return false;

            CalcGroundCheckParams(out Vector3 origin, out float castDist);
            return Physics.Raycast(origin, -transform.up, castDist, _groundLayer);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_capsule == null) _capsule = GetComponentInChildren<CapsuleCollider>();
            if (_capsule == null) return;

            CalcGroundCheckParams(out Vector3 origin, out float castDist);

            Vector3 capsuleWorldCenter = _capsule.transform.TransformPoint(_capsule.center);
            float halfHeight = _capsule.height * 0.5f;
            Vector3 down = -transform.up;

            Vector3 headPos = capsuleWorldCenter - down * halfHeight; // 머리
            Vector3 feetPos = capsuleWorldCenter + down * halfHeight; // 발
            Vector3 endPos = origin + down * castDist;
            bool grounded = Application.isPlaying && CheckGrounded();

            // ① 머리 (노란)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(headPos, 0.1f);

            // ② 발 (시안)
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(feetPos, 0.08f);

            // ③ Ray 시작 = origin (흰)
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(origin, 0.06f);

            // ④ Ray 끝 = 착지 한계 (마젠타)
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(endPos, 0.06f);

            // ⑤ Ray 선
            Gizmos.color = grounded ? Color.green : Color.red;
            Gizmos.DrawLine(origin, endPos);

            // ⑥ 라벨
            if (Application.isPlaying)
            {
                UnityEditor.Handles.Label(
                    headPos + transform.up * 0.3f,
                    $"vVel: {_verticalVelocity:F2}  Grounded: {grounded}\n" +
                    $"height: {_capsule.height:F2}  castDist: {castDist:F2}"
                );
            }
        }
#endif
        #endregion
    }
}
