using UnityEngine;

public abstract class ObstacleBase : MonoBehaviour
//추상 클래스: "이건 아직 진짜 장애물이 아니라, 공통적인 특징만 모아둔 뼈대야!"라는 뜻
//특징: abstract가 붙으면 이 클래스 자체를 게임 세상에 소환(Instantiate)할 수 없음
{
    public float speed = 20f; // 장애물 이동 속도

    private void Start()
    {
        // 7초 뒤 자동 파괴 (메모리 관리)
        Destroy(gameObject, 7f);
    }

    protected virtual void Update()
    //protected: private과 public의 중간입니다.
    //"남들은 못 만지지만, 내 자식들(상속받은 클래스)한테는 이 변수/함수를 물려줄게"라는 뜻
    //virtual (가상): "내가 기본적으로 위로 움직이는 코드를 짜두긴 할 건데,
    //혹시 자식이 다르게 움직이고 싶으면 내 코드를 덮어써도 돼"라고 허락
    {
        // 기본 로직: 아래에서 위로 이동
        // 수정: Space.World를 추가하여 오브젝트가 회전해도 무조건 '세상 기준 위(Y축)'로만 올라가게함.
        // 이렇게 해야 회전 때문에 Z값이 틀어져 플레이어를 지나치는 현상을 막을 수 있음
        transform.Translate(Vector3.up * speed * Time.deltaTime, Space.World);
    }

    // 자식 클래스들이 각자의 특성에 맞게 구현할 충돌 함수
    public abstract void OnPlayerHit(FallingPlayer player);
    //추상 메서드: 개념: "장애물이라면 플레이어랑 부딪혔을 때 무슨 일이든 일어나야 해.
    //근데 그게 뭔지는 자식들(베개, 알람)이 각자 알아서 정해!"라는 예약
    //특징: 몸통({ })이 없음
    //대신 이걸 상속받은 자식들은 반드시 이 함수를 만들어야만 함
    //안 만들면 유니티가 빨간 줄을 띄우며 화를 냄 ㅜ
}