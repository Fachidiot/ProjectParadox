using UnityEngine;
using System.Collections;

public class ObstacleTypeA : ObstacleBase
{
    public enum VisualStyle { Pillow, Bubble }
    public VisualStyle appearance; // 장애물 외형 설정 (베개 또는 거품)
    public float gaugePenalty = 30f; // 부딪혔을 때 감소할 게이지 양
    public float lockDuration = 3f; // 최대 구속 시간
    public int escapePressCount = 5; // 탈출을 위해 눌러야 하는 횟수
    private bool isHit = false;

    protected override void Update() { if (!isHit) base.Update(); }

    public override void OnPlayerHit(FallingPlayer player)
    {   
        // 현재 튜토리얼 충돌 대기 상태(TutorialHit)라면 E키 가이드를 띄우라고 지휘관에게 명령
        if (FallGameManager.instance != null && FallGameManager.instance.currentState == GameState.TutorialHit)
             FallGameManager.instance.TriggerETutorial();

        // 이미 맞았거나 플레이어가 이미 구속 상태라면 무시
        if (isHit || player.isLocked) return;
        isHit = true;

        // 카메라 흔들림 중지 (다른 오브젝트와 충돌 시 연출 꼬임 방지)
        if (CameraShaker.instance != null) 
            CameraShaker.instance.StopShake();

        // 게임 매니저를 통해 깨어남 게이지 감소 처리
        if (FallGameManager.instance != null)
            FallGameManager.instance.DecreaseWakeGauge(gaugePenalty);

        // 플레이어 구속 루틴 시작
        StartCoroutine(LockRoutine(player));
    }

    IEnumerator LockRoutine(FallingPlayer player)
    {
        var animCtrl = player.GetComponent<AnimationController>();
        Rigidbody rb = player.GetComponent<Rigidbody>();

        // 1. 회전 속도 제어 (장애물 회전 로직 가져오기)
        var rotator = GetComponent<ObstacleRotator>();
        if (rotator != null) rotator.SetCaptured(true);

        player.isLocked = true; // 플레이어 조작 잠금

        // 2. 물리적 속도 초기화 (그 자리에 멈춤)
        if (rb) rb.linearVelocity = Vector3.zero;

        // 3. 피격 애니메이션 재생 (외형 설정에 따라 다른 반응)
        if (animCtrl) animCtrl.PlayHitReaction(appearance.ToString());

        // 4. 장애물 위치에 플레이어 고정
        UpdatePlayerAttachment(player);

        // --- [연타 탈출 로직 시작] ---
        int currentPresses = 0;
        System.Action countPress = () => {
            currentPresses++;
            Debug.Log($"<color=yellow>[연타 중]</color> {currentPresses} / {escapePressCount}");
        };

        // 플레이어의 탈출 입력 이벤트에 카운트 함수 연결
        player.OnEscapeInput += countPress;

        float timer = 0f;
        // 제한 시간 이내이면서, 연타 횟수를 다 채우지 못했을 때 반복
        while (timer < lockDuration && currentPresses < escapePressCount)
        {
            timer += Time.deltaTime;

            // 매 프레임마다 장애물 위치에 플레이어를 밀착시킴
            UpdatePlayerAttachment(player);

            yield return null;
        }

        // LockRoutine 코루틴 안에서 연타 성공 시
        if (currentPresses >= escapePressCount) 
        {
            Debug.Log("<color=cyan>[탈출 성공!]</color>");
        }
            
        // 튜토리얼 히트 상태였다면 지휘관에게 보고
        if (FallGameManager.instance.currentState == GameState.TutorialHit) 
        {
            FallGameManager.instance.FinishTutorial(); // 여기서 UI 정리까지 한 번에 처리됨
        }        
              
         else Debug.Log("<color=red>[탈출 실패...]</color>");

        // 이벤트 연결 해제 (메모리 누수 및 중복 실행 방지)
        player.OnEscapeInput -= countPress;
        // ------------------------------------------

        // 애니메이션 컨트롤러에 탈출 상태 알림
        if (animCtrl) animCtrl.OnEscape();
        yield return new WaitForSeconds(0.15f);

        player.isLocked = false; // 플레이어 조작 해제
        Destroy(gameObject); // 장애물 제거
    }

    // 플레이어를 장애물 위치/회전에 맞춰 고정시키는 함수
    private void UpdatePlayerAttachment(FallingPlayer player)
    {
        Vector3 pos = transform.position;
        // 외형(베개/거품)에 따라 고정될 높이값 조절
        if (appearance == VisualStyle.Pillow) pos += transform.up * 1.0f;
        else if (appearance == VisualStyle.Bubble) pos += transform.up * -2.0f;

        player.transform.position = pos;
        player.transform.rotation = transform.rotation;
    }
}