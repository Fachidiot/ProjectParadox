using UnityEngine;

public class FallingPlayer : MonoBehaviour
{
    [Header("Inertia & Weight (이동 및 관성)")]
    public float acceleration = 40f;   // 가속도
    public float maxSpeed = 25f;       // 최대 속도
    public float drag = 3f;            // 마찰력 (입력 없을 때 멈추는 속도)

    [Header("Y-Axis Range (이동 범위 제어)")]
    public float minY = -15f;          // 하단 제한
    public float maxY = 5f;            // 상단 제한

    [Header("State")]
    // 외부 장애물 스크립트에서 조작하여 플레이어를 멈추게 함
    public bool isLocked = false;

    public System.Action OnEscapeInput;

    private Vector3 velocity;

    void Start()
    {
        // 초기 하강 속도 부여
        velocity = new Vector3(0, -60f, 0);
    }

    void Update()
    {
        // 구속 상태에서 E를 누를 때만 이벤트 발송
        if (isLocked && Input.GetKeyDown(KeyCode.E))
        {
            OnEscapeInput?.Invoke();
        }

        // 락 상태(장애물에 걸림)라면 입력과 이동을 완전히 무시
        if (isLocked) return;


        // 1. 입력 감지
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(h, v, 0).normalized;

        // 2. 가속도 및 관성 계산
        if (inputDir.magnitude > 0.1f)
            velocity += inputDir * acceleration * Time.deltaTime;
        else
            velocity = Vector3.Lerp(velocity, Vector3.zero, drag * Time.deltaTime);

        // 3. 속도 제한
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        // 4. 최종 이동 및 범위 제한 (Clamp)
        transform.position += velocity * Time.deltaTime;

        float clampedX = Mathf.Clamp(transform.position.x, -24f, 24f);
        float clampedY = Mathf.Clamp(transform.position.y, minY, maxY);

        // [수정] Z축을 0f로 강제 고정하여 카메라나 물리 충돌로 인한 이탈 방지
        transform.position = new Vector3(clampedX, clampedY, 0f);

        // 5. 시각적 디테일: 좌우 이동 시 캐릭터 몸체가 살짝 기움
        float targetTilt = -velocity.x * 1.5f;
        transform.rotation = Quaternion.Euler(0, 0, targetTilt);

    }

    private void OnTriggerEnter(Collider other)
    {
        // 이미 무엇인가에 갇혀있는 상태라면 추가 충돌을 무시 (중복 방지 1차)
        if (isLocked) return;

        ObstacleBase obstacle = other.GetComponent<ObstacleBase>();
        if (obstacle != null)
        {
            // 장애물에게 '나(this)'를 전달하며 충돌 로직 실행
            obstacle.OnPlayerHit(this);


        }
    }
}