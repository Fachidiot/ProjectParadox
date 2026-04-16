using UnityEngine;

public class ObstacleRotator : MonoBehaviour
{
    [Header("Rotation Range (기준 각도 대비 가동 범위)")]
    public Vector3 angleRange = new Vector3(40f, 40f, 40f); // 각 축당 ±40도

    [Header("Rotation Speed (움직임 속도)")]
    public float minSpeed = 0.5f;
    public float maxSpeed = 2.0f;

    private Vector3 baseAngles;    // 시작 시점의 기준 각도
    private Vector3 randomOffsets; // 각 축의 노이즈 시작점 (군무 방지)
    private float moveSpeed;       // 이 오브젝트의 고유 속도
    private bool isPlayerCaptured = false; // 플레이어가 붙잡혔는지 확인 [추가]

    void Start()
    {
        // 1. 인스펙터에 설정된 초기 회전값을 기준점으로 잡습니다.
        baseAngles = transform.localEulerAngles;

        // 2. 모든 오브젝트가 다르게 움직이도록 랜덤 오프셋과 속도를 부여합니다.
        randomOffsets = new Vector3(Random.value * 10f, Random.value * 10f, Random.value * 10f);
        moveSpeed = Random.Range(minSpeed, maxSpeed);
    }

    void Update()
    {
        // 3. Mathf.PerlinNoise 또는 Sin을 이용해 부드러운 랜덤 움직임을 구현합니다.
        // Sin 함수를 사용하여 -1 ~ 1 사이를 부드럽게 오가도록 합니다.

        // [추가] 붙잡히면 속도를 40%로 줄입니다. (수치는 조절 가능)
        float currentMoveSpeed = isPlayerCaptured ? moveSpeed * 0.4f : moveSpeed;

        float time = Time.time * currentMoveSpeed; // 변경된 속도 적용

        float rotX = Mathf.Sin(time + randomOffsets.x) * angleRange.x;
        float rotY = Mathf.Cos(time + randomOffsets.y) * angleRange.y;
        float rotZ = Mathf.Sin(time * 0.7f + randomOffsets.z) * angleRange.z;

        // 4. 기준 각도에 계산된 오프셋을 더해 최종 회전 적용
        transform.localRotation = Quaternion.Euler(
            baseAngles.x + rotX,
            baseAngles.y + rotY,
            baseAngles.z + rotZ
        );
    }

    // [추가] 외부(ObstacleTypeA)에서 호출할 함수
    public void SetCaptured(bool captured)
    {
        isPlayerCaptured = captured;
    }
}