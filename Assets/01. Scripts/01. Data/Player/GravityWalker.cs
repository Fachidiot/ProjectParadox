/// <summary>
/// 캐릭터가 서 있는 지면의 법선(Normal)을 감지하여 벽 걷기 및 중력 정렬 정보를 제공하는 컴포넌트.
/// </summary>

using UnityEngine;
public class GravityWalker : MonoBehaviour
{
    [Header("탐색 설정")]
    [SerializeField] private LayerMask _environmentLayer;
    [SerializeField] private float _rayDistance = 3f;

    private Vector3 _groundNormal = Vector3.up;
    private float _ignoreSurfaceTime = 0f;
    
    /// <summary>
    /// 현재 감지된 지면의 법선 방향. (기본값: Vector3.up)
    /// </summary>
    public Vector3 GroundNormal => _groundNormal;

    /// <summary>
    /// 점프대 등에 의해 중력을 180도 반전시킵니다.
    /// </summary>
    public void InvertGravity()
    {
        _groundNormal = -_groundNormal;
        // 반전 후 0.5초 동안 기존 바닥/천장을 다시 탐지하지 않도록 레이캐스트를 무시합니다.
        _ignoreSurfaceTime = Time.time + 0.5f; 
    }

    private void FixedUpdate()
    {
        // 물리 연산 주기에 맞춰 지형 감지 수행
        DetectSurface();
    }

    /// <summary>
    /// 레이캐스트를 이용해 발밑과 전방 하단의 지형을 감지하고 새로운 법선을 계산합니다.
    /// </summary>
    private void DetectSurface()
    {
        // 쿨다운 중(반전 직후 공중에 체공 중)일 때는 지형 탐지를 생략하여 원래 바닥에 다시 붙는 현상을 방지
        if (Time.time < _ignoreSurfaceTime) return;

        // 레이 시작 지점을 캐릭터 중심에서 약간 앞/위로 설정하여 경사로나 벽면 진입 시 민감도를 높임
        Vector3 rayOrigin = transform.position + transform.up * 0.5f + transform.forward * 0.2f;

        // 1. 발바닥 직하단 레이 (기본 지면 유지용)
        Ray downRay = new Ray(rayOrigin, -transform.up);
        Debug.DrawRay(downRay.origin, downRay.direction * _rayDistance, Color.blue);

        // 2. 전방 하단 45도 레이 (벽면 진입 및 급격한 경사 감지용)
        Vector3 forwardDown = (transform.forward * 1.2f - transform.up).normalized;
        Ray forwardRay = new Ray(rayOrigin, forwardDown);
        Debug.DrawRay(forwardRay.origin, forwardRay.direction * _rayDistance, Color.cyan);

        bool hitForward = Physics.Raycast(forwardRay, out RaycastHit forwardHit, _rayDistance + 0.5f, _environmentLayer);
        bool hitDown = Physics.Raycast(downRay, out RaycastHit downHit, _rayDistance + 0.5f, _environmentLayer);

        Vector3 targetNormal = _groundNormal;

        // 우선순위: 전방 감지(벽 진입 시) -> 하단 감지(평지 유지 시)
        if (hitForward)
        {
            targetNormal = forwardHit.normal;
        }
        else if (hitDown)
        {
            targetNormal = downHit.normal;
        }

        // [수정] LowPoly 지형에서의 각진 법선 전환으로 인한 어지럼증(카메라 스냅) 방지
        // 새로운 법선 방향으로 즉시 회전하지 않고 부드럽게 스무딩하여 붙도록 처리합니다.
        _groundNormal = Vector3.Slerp(_groundNormal, targetNormal, 15f * Time.fixedDeltaTime);
    }
}
