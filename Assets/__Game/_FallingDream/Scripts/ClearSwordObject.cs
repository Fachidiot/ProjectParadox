using UnityEngine;
using UnityEngine.EventSystems;

public class ClearSwordObject : MonoBehaviour
{
    [Header("회전 속도")]
    public float rotationSpeed = 30f;

    private Vector3 randomAxis;

    void Start()
    {
        // 시작할 때 랜덤한 회전축을 하나 정해줍니다. 
        randomAxis = new Vector3(
            Random.Range(0.1f, 0.5f), // W(X)축 느낌
            Random.Range(0.5f, 1.0f), // Y축 (주회전)
            Random.Range(0.1f, 0.5f)  // Z축
        ).normalized;
    }

    void Update()
    {
        // 매 프레임 정해진 랜덤 축을 바탕으로 천천히 회전
        transform.Rotate(randomAxis * rotationSpeed * Time.deltaTime);
        // 둥둥 뜨는 연출
        transform.localPosition += new Vector3(0, Mathf.Sin(Time.time * 2f) * 0.001f, 0);
    }

    // 마우스로 칼을 클릭했을 때 실행 (버튼 역할)
    private void OnMouseDown()
    {
        Debug.Log("<color=green>[클리어] 신비로운 검을 획득했습니다!</color>");

        // 기존 FallGameManager에 있던 클리어 버튼 함수를 호출합니다.
        if (FallGameManager.instance != null)
        {
            FallGameManager.instance.OnClickClear();
        }
    }
}