using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FallTutorialManager : MonoBehaviour
{
    public static FallTutorialManager instance;

    [Header("UI 오브젝트")]
    public GameObject moveTutorialUI;
    public GameObject eTutorialUI;

    [Header("애니메이션 대상 Image (직접 연결 권장)")]
    public Image moveImage;
    public Image eImage;

    private Coroutine currentAnim;

    void Awake()
    {
        instance = this;
        // 초기화
        if (moveTutorialUI) moveTutorialUI.SetActive(false);
        if (eTutorialUI) eTutorialUI.SetActive(false);
    }

    void Update()
    {
        // LookAt 대신 카메라의 회전값만 그대로 복사 (이게 제일 정확합니다)
        if (Camera.main != null)
        {
            if (moveTutorialUI != null && moveTutorialUI.activeSelf)
                moveTutorialUI.transform.rotation = Camera.main.transform.rotation;
            
            if (eTutorialUI != null && eTutorialUI.activeSelf)
                eTutorialUI.transform.rotation = Camera.main.transform.rotation;
        }
    }


    public void ShowMoveTutorial()
    {
        // [수정] 금고(Global)를 확인해서 이미 봤다면 아무것도 안 하고 돌아갑니다(return).
        if (ConditionManager.instance != null && ConditionManager.instance.HasSeenTutorial.v)
        {
            Debug.Log("<color=white>이미 튜토리얼을 보셨으므로 이동 튜토리얼을 스킵합니다.</color>");
            return;
        }
        AllStop(); // 이전 꺼 확실히 끄기
        if (moveTutorialUI != null)
        {
            moveTutorialUI.SetActive(true);
            // [수정] 이미지가 있다면 애니메이션 무조건 시작
            Image target = moveImage != null ? moveImage : moveTutorialUI.GetComponentInChildren<Image>();
            if (target != null) currentAnim = StartCoroutine(ClickAnimation(target));
            
            Debug.Log("<color=cyan>[수행] 방향키 애니메이션 시작!</color>");
        }
    }

    public void ShowETutorial()
    {
        // [수정] 여기도 마찬가지로 확인 로직 추가
        if (ConditionManager.instance != null && ConditionManager.instance.HasSeenTutorial.v)
        {
            Debug.Log("<color=white>이미 튜토리얼을 보셨으므로 E키 튜토리얼을 스킵합니다.</color>");
            return;
        }
        AllStop();
        if (eTutorialUI != null)
        {
            eTutorialUI.SetActive(true);
            Image target = eImage != null ? eImage : eTutorialUI.GetComponentInChildren<Image>();
            if (target != null) currentAnim = StartCoroutine(ClickAnimation(target));

            Debug.Log("<color=cyan>[수행] E키 애니메이션 시작!</color>");
        }
    }

    public void AllStop()
    {
        // 1. 코루틴 먼저 확실히 정지
        if (currentAnim != null)
        {
            StopCoroutine(currentAnim);
            currentAnim = null;
        }

        // 2. UI 끄기
        if (moveTutorialUI) moveTutorialUI.SetActive(false);
        if (eTutorialUI) eTutorialUI.SetActive(false);

        // 3. 스케일 원복 (중요: 커진 채로 멈추는 것 방지)
        if (moveImage) moveImage.transform.localScale = Vector3.one;
        if (eImage) eImage.transform.localScale = Vector3.one;
    }

    // [핵심] 외부 변수 없이 'while(true)'로 무한 반복 (AllStop에서만 멈춤)
    IEnumerator ClickAnimation(Image targetImg)
    {
        Debug.Log($"<color=yellow>[애니메이션] {targetImg.name} 루프 진입</color>");
        while (true)
        {
            targetImg.transform.localScale = new Vector3(1.2f, 1.2f, 1.0f);
            yield return new WaitForSeconds(0.1f);
            targetImg.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            yield return new WaitForSeconds(0.15f);
        }
    }
}