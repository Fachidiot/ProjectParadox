using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public enum GameState { TutorialMove, TutorialHit, Playing, GameOver}
public class FallGameManager : MonoBehaviour
{
    public static FallGameManager instance; // 싱글톤
    public GameState currentState = GameState.Playing;//기본값

    [Header("Wake Gauge Settings")]
    public float currentGauge = 0f;    // 0(깊은 잠) ~ 100(잠에서 깸)
    public float maxGauge = 100f;
    public float baseFillSpeed = 3f;   // 기본 초당 상승량
    private float currentFillSpeed;
    public Image gaugeImage;           // UI 이미지

    [Header("3D Spike Settings")] // 추가된 부분
    public Animator spikeAnimator;    // 3D 가시 모델의 애니메이터
    public string animationName = "Spike_Action"; // 애니메이터 내 상태(State) 이름

    [Header("UI Settings")]
    public GameObject gameOverUI;      // 실패 패널
    public GameObject gameClearUI;     // 성공 패널

    public bool isGameEnded = false;  // 게임 종료 여부
    public float Progress => Mathf.Clamp01(currentGauge / maxGauge);  // 0~1 값 반환

    void Awake()
    {
        if (instance == null) instance = this;
        currentFillSpeed = baseFillSpeed;
        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (gameClearUI != null) gameClearUI.SetActive(false);
    }

    void Start()
    {
        if (ConditionManager.instance != null && !ConditionManager.instance.HasSeenTutorial.v) {
            StartCoroutine(StartTutorialSequence());
        } else {
            currentState = GameState.Playing;
        }
    }

    IEnumerator StartTutorialSequence() 
    {
        currentState = GameState.TutorialMove;
        // [추가] 캐릭터가 내려온 후 1.8초 동안 대기
        yield return new WaitForSeconds(1.8f);

        // 1단계: 1.8초 뒤에 방향키 켜기!
        currentState = GameState.TutorialMove;
        
        if (FallTutorialManager.instance != null)
        {
            FallTutorialManager.instance.ShowMoveTutorial();
            Debug.Log("<color=cyan>[튜토리얼] 1.8초 지연 후 방향키 표시</color>");
        }
        else
        {
            Debug.LogError("FallTutorialManager 인스턴스를 찾을 수 없습니다!");
        }

        yield return new WaitForSeconds(5f);

        currentState = GameState.TutorialHit;
        // 2단계: 다 끄고 장애물 쏴라! (AllStop 호출 후 스폰)
        FallTutorialManager.instance.AllStop(); 
        
        // 플레이어 찾아서 스나이퍼 발사
        var player = FindFirstObjectByType<FallingPlayer>();
        if (player != null && ObstacleSpawner.instance != null)
        {
            ObstacleSpawner.instance.SpawnSniper(player.transform);
        }
    }

    // 장애물에 부딪혔을 때 호출될 함수
    public void TriggerETutorial()
    {
        // 3단계: E키 켜라!
        FallTutorialManager.instance.ShowETutorial();
    }
    public void FinishTutorial() 
    {
        // 1. 상태를 본 게임으로 변경
        currentState = GameState.Playing;
        if (Global.instance != null)
        {
            ConditionManager.instance.SetSeenTutorial(true);
        }

        // 2. 수행 비서(FallTutorialManager)에게 UI 정리 요청
        if (FallTutorialManager.instance != null)
        {
            FallTutorialManager.instance.AllStop();
        }
        
        Debug.Log("<color=green>[지휘] 튜토리얼 종료, 본 게임 시작!</color>");
    }

    void Update()
    {
        if (isGameEnded) return; // 게임이 끝났다면 실행 중지

        // [핵심] Playing 상태일 때만 게이지 상승!
        if (currentState == GameState.Playing) 
        {
            {
                currentGauge += currentFillSpeed * Time.deltaTime;
            }
        }

        UpdateGaugeUI();
        UpdateSpikeAnimation(); // 가시 애니메이션 업데이트 호출

        // 2. 클리어 판정
        if (currentGauge >= maxGauge)
        {
            currentGauge = maxGauge;
            GameClear();
        }

    }


    // 추가된 부분: 3D 가시 애니메이션 제어 로직
    private void UpdateSpikeAnimation()
    {
        if (spikeAnimator != null)
        {
            // 게이지 비율 계산 (0.0 ~ 1.0)
            
            // 애니메이션의 재생 시점을 progress 값으로 강제 고정
            // Speed가 0이어도 Play를 통해 특정 시점으로 이동할 수 있습니다.
            spikeAnimator.Play(animationName, 0, Progress);
        }
    }

    // 게이지 감소 (장애물 충돌 시 호출)
    public void DecreaseWakeGauge(float amount)
    {
        if (currentState != GameState.Playing) 
        {
            Debug.Log("<color=yellow>[지휘] 튜토리얼 중이라 게이지 감소를 무시합니다.</color>");
            return; 
        }
        if (isGameEnded) return;

        currentGauge -= amount;

        // 마이너스 값 방지 및 사망 판정
        if (currentGauge <= 0.0001f)
        {
            currentGauge = 0;
            GameOver();
        }

        UpdateGaugeUI();
        UpdateSpikeAnimation(); // 감소할 때도 즉시 반영
 
    }

    private void UpdateGaugeUI()
    {
        if (gaugeImage != null)
            gaugeImage.fillAmount = Progress;
    }

    // 알람 시계용 한시적 가속 로직
    public void SpeedUpWakeRateTemp(float multiplier, float duration)
    {
        if (isGameEnded) return;
        StartCoroutine(SpeedUpRoutine(multiplier, duration));
    }

    IEnumerator SpeedUpRoutine(float multiplier, float duration)
    {
        // 속도만 올림
        currentFillSpeed = baseFillSpeed * multiplier;

        yield return new WaitForSeconds(duration);

        // 다시 원래 속도로 복구
        currentFillSpeed = baseFillSpeed;
    }

    public float GetProgress()
    {
        return Progress;
    }
    public void GameOver()
    {
        if (isGameEnded) return;
        isGameEnded = true;
        if (gameOverUI != null) gameOverUI.SetActive(true);
    }

    public void GameClear()
    {
        if (isGameEnded) return;
        isGameEnded = true;
        if (gameClearUI != null) gameClearUI.SetActive(true);

    }

    public void OnClickGameOver()
    {
        SceneManager.LoadScene("FallingDream");
    }

    void OnTriggerEnter(Collider other)
    {    }

    public void OnClickClear()
    {
        Time.timeScale = 1.0f;
        if (Global.instance != null)
        {
            ConditionManager.instance.SetSwordUnlocked(true);
            SceneChangeHelper.Change("Room1", "You wake up from a dream... ");
        }
        else
        {
            Debug.LogError("씬에 [Global] 오브젝트가 없습니다!");
        }
    }
}