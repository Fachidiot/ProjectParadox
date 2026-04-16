/// <summary>
/// Rigidbody를 사용하여 캐릭터의 기본적인 이동, 회전, 점프 및 중력 방향 정렬을 처리하는 컴포넌트.
/// GravityWalker로부터 전달받은 지면 법선에 맞춰 캐릭터의 Up축을 정렬합니다.
/// </summary>

using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private float _jumpHeight = 2f;
    [SerializeField] private float _terminalVelocity = -50f;

    [Header("시야 및 판정 설정")]
    [SerializeField] private float _mouseSensitivity = 2f;
    [SerializeField] private float _upRotationLimit = -90f;
    [SerializeField] private float _downRotationLimit = 90f;
    [SerializeField] private float _groundCheckDistance = 0.2f;
    [SerializeField] private float _rotationSmoothing = 15f;
    [SerializeField] private LayerMask _groundLayer;

    private Rigidbody _rb;
    private CapsuleCollider _capsule;
    private PlayerInputHandler _inputHandler;
    private GravityWalker _gravityWalker;
    private Transform _playerCamera;

    private float _verticalVelocity;
    private float _cameraPitch; // 카메라의 수직 회전값 (상하)
    private float _yawInputAccumulator; // 고정 물리 업데이트 주기를 위한 수평 입력 누적기

    private void Awake()
    {
        _inputHandler = GetComponent<PlayerInputHandler>();
        _gravityWalker = GetComponent<GravityWalker>();
        _rb = GetComponent<Rigidbody>();
        _capsule = GetComponent<CapsuleCollider>();

        // 메인 카메라 캐싱 (매 프레임 호출 방지)
        if (Camera.main != null) _playerCamera = Camera.main.transform;

        InitializeRigidbody();

        if (_inputHandler == null)
            Debug.LogError("[PlayerMovement] PlayerInputHandler를 찾을 수 없습니다!");
    }

    /// <summary>
    /// 물리 기반 이동을 위한 Rigidbody 초기 설정
    /// </summary>
    private void InitializeRigidbody()
    {
        _rb.useGravity = false; // 커스텀 중력을 위해 비활성화
        _rb.freezeRotation = true; // 물리적 충돌에 의한 회전 방지
        _rb.interpolation = RigidbodyInterpolation.Interpolate; // 프레임 보간으로 움직임 부드럽게
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // 고속 이동 시 뚫림 방지
    }

    private void Update()
    {
        HandleCameraRotation();
    }

    private void FixedUpdate()
    {
        HandleBodyRotation();
        HandleMovement();
    }

    /// <summary>
    /// 마우스 입력에 따른 카메라 상하 회전(Pitch) 처리
    /// </summary>
    private void HandleCameraRotation()
    {
        if (_inputHandler == null || _playerCamera == null) return;

        Vector2 lookInput = _inputHandler.LookInput;

        _cameraPitch -= lookInput.y * _mouseSensitivity;
        _cameraPitch = Mathf.Clamp(_cameraPitch, _upRotationLimit, _downRotationLimit);

        // 카메라의 부모(플레이어 몸체) 회전을 고려한 로컬 회전만 적용
        _playerCamera.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);

        // 몸체 회전 입력을 고정 업데이트 주기에 맞춰 누적
        _yawInputAccumulator += lookInput.x * _mouseSensitivity;
    }

    /// <summary>
    /// 지면 법선에 따른 캐릭터 몸체의 수평 회전(Yaw) 및 중력 정렬(Gravity Align) 처리
    /// </summary>
    private void HandleBodyRotation()
    {
        // 1. 목표 Up 축 설정 (GravityWalker가 감지한 지면 법선 사용)
        Vector3 targetUp = _gravityWalker != null ? _gravityWalker.GroundNormal : Vector3.up;

        // 2. 마우스 입력에 따른 현재 몸체의 수평 회전값 계산
        Quaternion mouseRotation = Quaternion.AngleAxis(_yawInputAccumulator, transform.up);
        _yawInputAccumulator = 0f; // 초기화

        // 3. 현재 Up축에서 목표(지면 법선) Up축으로의 회전 델타 계산
        Quaternion tiltRotation = Quaternion.FromToRotation(transform.up, targetUp);

        // 4. 합산된 목표 회전값 계산 (기울기 * 마우스 입력 * 현재 회전)
        Quaternion targetRotation = tiltRotation * mouseRotation * transform.rotation;

        // 5. 급격한 회전 방지를 위해 부드럽게 보간하며 적용
        Quaternion nextRotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * _rotationSmoothing);
        _rb.MoveRotation(nextRotation.normalized);
    }

    /// <summary>
    /// 입력과 중력을 반영한 실제 좌표 이동 처리
    /// </summary>
    private void HandleMovement()
    {
        bool isGrounded = CheckGrounded();

        // 1. 착지 상태에서 수직 속도 보정
        if (isGrounded && _verticalVelocity < 0)
        {
            _verticalVelocity = -2f; // 지면에 완전히 밀착되도록 약한 하향 압력 유지
        }

        // 2. 이동 입력 처리 및 평면 투영
        Vector2 moveInput = _inputHandler.MoveInput;
        Vector3 rawMoveDirection = (transform.right * moveInput.x + transform.forward * moveInput.y);

        // 현재 서 있는 지면의 법선에 맞춰 이동 평면을 투영 (경사로 이동 보정)
        Vector3 groundNormal = _gravityWalker != null ? _gravityWalker.GroundNormal : transform.up;
        Vector3 projectedMove = Vector3.ProjectOnPlane(rawMoveDirection, groundNormal).normalized * rawMoveDirection.magnitude;

        Vector3 horizontalMove = projectedMove * _moveSpeed;

        // 3. 점프 및 중력 연산
        if (_inputHandler.IsJumpPressed && isGrounded)
        {
            _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
        }

        _verticalVelocity += _gravity * Time.fixedDeltaTime;
        if (_verticalVelocity < _terminalVelocity) _verticalVelocity = _terminalVelocity;

        // 4. 수평 이동과 수직(중력) 속도를 합산하여 최종 속도 적용
        // [수정] 점프대 등에서 중력을 반전시켰을 때, 시각적 회전(Slerp)으로 인해
        // transform.up이 대각선을 향해 있는 동안 점프력이 들어가면 옆으로 발사되는 현상을 막기 위해,
        // 절대적인 현재 지면 법선(groundNormal)의 축을 기준으로 수직 속성을 적용합니다.
        Vector3 finalVelocity = horizontalMove + groundNormal * _verticalVelocity;
        _rb.linearVelocity = finalVelocity;
    }

    /// <summary>
    /// 캡슐 하단에서 구체 판정을 수행하여 지면 착지 여부를 확인합니다.
    /// </summary>
    private bool CheckGrounded()
    {
        Vector3 capsuleCenter = transform.position + transform.up * _capsule.center.y;
        float radius = _capsule.radius * 0.9f;
        float distToBottom = _capsule.height / 2f;

        Vector3 castOrigin = capsuleCenter;
        float castDistance = distToBottom + _groundCheckDistance;

        // 플레이어 캐릭터와 환경 레이어를 대상으로 판정
        return Physics.SphereCast(castOrigin, radius, -transform.up, out _, castDistance, _groundLayer | _groundLayer);
    }

    /// <summary>
    /// 점프대 등 외부 요인에 의해 수직 속도를 강제로 변경합니다.
    /// </summary>
    /// <param name="verticalForce">적용할 수직 속도 값</param>
    public void Launch(float verticalForce)
    {
        _verticalVelocity = verticalForce;
    }

    /// <summary>
    /// 체공 중 중력이 반전될 때, 속력 벡터가 물리적으로 부드럽게 유지되도록 수직 속도 부호를 180도 뒤집습니다.
    /// </summary>
    public void InvertVerticalVelocity()
    {
        _verticalVelocity = -_verticalVelocity;
    }

    private void OnDrawGizmosSelected()
    {
        if (_capsule == null) return;

        Gizmos.color = Color.yellow;
        Vector3 capsuleCenter = transform.position + transform.up * _capsule.center.y;
        float distToBottom = _capsule.height / 2f;

        Vector3 endPoint = capsuleCenter - transform.up * (distToBottom + _groundCheckDistance);
        Gizmos.DrawLine(capsuleCenter, endPoint);
        Gizmos.DrawWireSphere(endPoint, _capsule.radius * 0.9f);
    }
}
