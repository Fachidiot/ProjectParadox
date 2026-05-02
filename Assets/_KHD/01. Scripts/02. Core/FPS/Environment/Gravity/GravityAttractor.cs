/// <summary>
/// 구형(Spherical) 맵의 중심에서 중력을 발생시키는 컴포넌트.
/// 물체를 구의 중심방향으로 당기거나 밀어내며, 지면 방향으로 정렬시킵니다.
/// </summary>

using UnityEngine;

public class GravityAttractor : MonoBehaviour
{
    public static GravityAttractor Instance;

    [Header("중력 설정")]
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private bool _isInsideSphere = false;

    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        Instance = this;
    }

    /// <summary>
    /// 구의 내부/외부 중력 모드를 반전시킵니다. (모든 SphericalObject에 영향)
    /// </summary>
    public void ToggleGravityDirection()
    {
        _isInsideSphere = !_isInsideSphere;
    }

    /// <summary>
    /// 대상 Rigidbody에 구형 중력을 적용하고 로컬 Up축을 지면 법선에 정렬시킵니다.
    /// </summary>
    /// <param name="body">중력을 적용할 리지드바디</param>
    public void Attract(Rigidbody body)
    {
        // 1. 중력 방향 계산
        Vector3 dirToCenter = (transform.position - body.position).normalized;
        // 내부(Hollow Sphere)면 중심 반대 방향이 중력, 외부(Planet)면 중심 방향이 중력
        Vector3 gravityDir = _isInsideSphere ? -dirToCenter : dirToCenter;

        // 2. 중력 가속도 적용
        body.AddForce(gravityDir * Mathf.Abs(_gravity));

        // 3. 지면 기준 Upright 정렬 (구 표면 법선과 물체의 up축 일치)
        Vector3 gravityUp = -gravityDir;
        Vector3 localUp = body.transform.up;

        // 현재 up에서 중력 up으로 가는 회전 델타를 계산하여 적용
        Quaternion targetRot = Quaternion.FromToRotation(localUp, gravityUp) * body.rotation;
        
        // [수정] 50f는 거의 1프레임만에 회전을 끝내버리므로 각진(로우폴리) 메쉬에서는 뚝뚝 끊겨 보입니다.
        // 계수를 5f로 낮추어 물체가 바닥에 부드럽게 스며들듯 정렬되게 합니다.
        body.MoveRotation(Quaternion.Slerp(body.rotation, targetRot, 5f * Time.fixedDeltaTime));
    }
}
