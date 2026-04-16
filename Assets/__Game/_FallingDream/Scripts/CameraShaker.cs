using UnityEngine;
using System.Collections;

public class CameraShaker : MonoBehaviour
{
    // [추가] FallingPlayer.cs에서 static으로 이 인스턴스를 불러와서 흔들 겁니다.
    public static CameraShaker instance;

    // 흔들림이 끝나고 돌아갈 카메라의 원래 로컬 위치
    private Vector3 originalLocalPosition;

    void Awake()
    {
        // 1. 싱글톤 인스턴스 설정
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 2. 게임 시작 시 카메라의 초기 로컬 위치를 저장합니다.
        // FallingDream 카메라의 초기 위치는 (0, 8, -20)일 겁니다.
        originalLocalPosition = transform.localPosition;
    }

    // 3. 외부에서 호출할 함수 (장애물 충돌용)
    // - duration: 흔들리는 시간 (초)
    // - magnitude: 흔들리는 세기 (숫자가 클수록 크게 흔들림)
    public void Shake(float duration, float magnitude)
    {
        // 이미 흔들리고 있다면 중복 실행을 막기 위해 멈추고 새로 시작합니다.
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    // 4. [핵심] 실제로 흔드는 코루틴 로직
    IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float timer = 0f;

        while (timer < duration)
        {
            // Random.insideUnitCircle을 사용해 (x, y) 평면 기준으로 랜덤한 좌표를 얻습니다.
            // z축은 고정해두거나 미세하게 흔들 수 있습니다.
            Vector2 randomOffset2D = Random.insideUnitCircle * magnitude;

            // 5. 원래 로컬 위치에 랜덤 오프셋을 더해서 카메라 위치를 강제 변경
            transform.localPosition = new Vector3(
                originalLocalPosition.x + randomOffset2D.x,
                originalLocalPosition.y + randomOffset2D.y,
                originalLocalPosition.z // z축은 흔들지 않음 (필요시 randomOffset2D.x / 10 등으로 미세하게 추가 가능)
            );

            timer += Time.deltaTime;

            // 매 프레임 위치를 바꿉니다.
            yield return null;
        }

        // 6. 흔들림이 끝나면 반드시 카메라 위치를 원래대로 돌려놔야 합니다.
        transform.localPosition = originalLocalPosition;
    }

    public void StopShake()
    {
        // 진행 중인 모든 흔들림 코루틴을 즉시 정지합니다.
        StopAllCoroutines();

        // 카메라 위치를 원래 위치로 깔끔하게 되돌려 놓습니다.
        transform.localPosition = originalLocalPosition;
    }
}