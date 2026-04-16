/// <summary>
/// 플레이어의 카메라 시선(Viewport) 내에 특정 오브젝트가 포착되었는지 감지하고 이벤트를 발생시키는 컴포넌트.
/// </summary>

using System;
using UnityEngine;
using UnityEngine.Events;

public class ViewportManager : MonoBehaviour
{
    [Serializable]
    public class ViewpointEvent
    {
        [Tooltip("이벤트 식별을 위한 이름")]
        public string Name;
        [Tooltip("감지할 포인트 혹은 오브젝트")]
        public Transform Target;
        [Tooltip("중앙 시선으로부터 허용되는 각도 (작을수록 정확히 바라봐야 함)")]
        public float MaxAngle = 5f;
        [Tooltip("감지 가능한 최대 거리")]
        public float MaxDistance = 20f;
        [Tooltip("조건 충족 시 실행할 이벤트")]
        public UnityEvent OnViewed;
        
        /// <summary>
        /// 이미 이벤트가 실행되었는지 여부
        /// </summary>
        [HideInInspector] public bool IsTriggered;
    }

    [Header("시선 감지 설정")]
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private ViewpointEvent[] _events;

    private void Update()
    {
        // 최적화를 위해 시선 체크는 매 프레임 수행하되, 카메라 유무 확인
        CheckViewpoints();
    }

    /// <summary>
    /// 등록된 모든 관심 지점(Viewpoints)들을 순회하며 관측 여부를 체크합니다.
    /// </summary>
    private void CheckViewpoints()
    {
        if (_playerCamera == null) return;

        foreach (var ev in _events)
        {
            // 이미 감지된 이벤트는 건너뜀 (일회성 실행 방지)
            if (ev.IsTriggered || ev.Target == null) continue;

            // 1. 방향 및 각도 계산
            Vector3 directionToTarget = (ev.Target.position - _playerCamera.transform.position).normalized;
            float angle = Vector3.Angle(_playerCamera.transform.forward, directionToTarget);
            
            // 2. 거리 계산
            float distance = Vector3.Distance(_playerCamera.transform.position, ev.Target.position);

            // 3. 조건 판정 (각도와 거리 범위 내에 있는지)
            if (angle < ev.MaxAngle && distance < ev.MaxDistance)
            {
                // 4. 장애물 체크 (시야가 벽 등에 가려져 있는지 확인)
                if (Physics.Raycast(_playerCamera.transform.position, directionToTarget, out RaycastHit hit, ev.MaxDistance))
                {
                    // 레이캐스트가 목표물과 일치할 때만 최종적으로 트리거
                    if (hit.transform == ev.Target)
                    {
                        ev.IsTriggered = true;
                        ev.OnViewed?.Invoke();
                        CustomLog.Log($"[ViewportManager] Viewpoint '{ev.Name}' triggered!");
                    }
                }
            }
        }
    }

    /// <summary>
    /// 특정 이벤트를 다시 활성화해야 할 때 외부에서 호출합니다.
    /// </summary>
    /// <param name="eventName">초기화할 이벤트 이름</param>
    public void ResetEvent(string eventName)
    {
        foreach (var ev in _events)
        {
            if (ev.Name == eventName)
            {
                ev.IsTriggered = false;
                break;
            }
        }
    }
}
