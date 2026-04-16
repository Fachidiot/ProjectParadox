using System.Collections;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public static ObstacleSpawner instance;

    [Header("Obstacle Prefabs")]
    // 타입 A 변종들(베개, 비눗방울)을 넣는 배열
    public GameObject[] prefabA_Variants;
    // 타입 B(알람시계)
    public GameObject prefabB;

    [Header("Spawn Settings")]
    public float minInterval = 0.1f; // 가장 빠를 때
    public float maxInterval = 1.0f; // 가장 느릴 때 (시작)
    public float rangeX = 35f;         // 가로 범위

    [Header("Spawn Position")]
    public float spawnYPosition = -50f;

    private Coroutine spawnLoop;

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    void Start()
    {
        if (prefabA_Variants == null || prefabA_Variants.Length == 0 || prefabB == null)
        {
            Debug.LogError("프리팹을 인스펙터에서 연결해주세요!");
            return;
        }
        spawnLoop = StartCoroutine(SpawnRoutine());

    }

    IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            if (FallGameManager.instance != null &&
                        !FallGameManager.instance.isGameEnded &&
                        FallGameManager.instance.currentState == GameState.Playing)
            {
                Spawn();
            }
            // [핵심] 게이지(0~1)에 따라 스폰 간격 계산
            // 게이지가 0이면 1.2s, 게이지가 1이면 0.3s가 됩니다.
            float currentProgress = FallGameManager.instance.GetProgress(); // 게이지 값 가져오기 (0.0 ~ 1.0)
            float currentInterval = Mathf.Lerp(maxInterval, minInterval, currentProgress);

            yield return new WaitForSeconds(currentInterval);
        }
    }

    public void SpawnSniper(Transform targetPlayer)
    {
        if (targetPlayer == null) return;

        // [핵심] Y축 값을 따로 계산하지 않고 플레이어의 '현재 위치'를 그대로 가져옴
        // 플레이어의 몸 정중앙 좌표에 장애물이 생성됩니다.
        Vector3 spawnPos = targetPlayer.position;

        if (prefabA_Variants.Length > 0)
        {
            // 장애물 생성 (생성되자마자 플레이어와 충돌 판정이 일어납니다)
            GameObject tutorialObstacle = Instantiate(prefabA_Variants[0], spawnPos, Quaternion.identity);

            Debug.Log("<color=red>[직격 소환] 플레이어 좌표에 즉시 생성! 피하는 건 불가능합니다.</color>");
        }
    }
    void Spawn()
    {

        float randomX = Random.Range(-rangeX, rangeX);
        Vector3 spawnPos = new Vector3(randomX, spawnYPosition, 0f);

        GameObject selectedPrefab;
        float chance = Random.value;
        //"0.0에서 1.0 사이의 숫자 카드 중 하나를 무작위로 뽑아서 chance라는 변수에 저장해."

        if (chance < 0.8f) // 80% 확률로 타입 A
        {
            int randomIndex = Random.Range(0, prefabA_Variants.Length);
            selectedPrefab = prefabA_Variants[randomIndex];
        }
        else // 20% 확률로 타입 B
        {
            selectedPrefab = prefabB;
        }

        Instantiate(selectedPrefab, spawnPos, Quaternion.identity);
    }
}