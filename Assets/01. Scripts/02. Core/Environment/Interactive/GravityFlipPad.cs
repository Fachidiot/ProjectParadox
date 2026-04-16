using System.Collections;
using UnityEngine;

/// <summary>
/// 밟는 순간 플레이어를 하늘로 쏘아 올리며, 시간차를 두고 중력의 방향을 반전시키는 점프대 기믹입니다.
/// </summary>
public class GravityFlipPad : MonoBehaviour
{
    [Header("점프대 설정")]
    [Tooltip("플레이어를 위로 쏘아 올릴 힘 (가속도 값)")]
    [SerializeField] private float _launchForce = 15f;
    [Tooltip("발사 후 중력이 반전되기까지의 체공 시간 (초) - 최고점에서 뒤집히게 조절하세요.")]
    [SerializeField] private float _flipDelay = 0.5f;
    [Tooltip("무한 점프 루프 방지를 위한 쿨다운 (초)")]
    [SerializeField] private float _cooldownTime = 1.0f;
    
    [Header("글로벌 반전 옵션")]
    [Tooltip("체크 시 맵 전체의 GravityAttractor 모드도 함께 반전시킵니다. (큐브나 다른 오브젝트들도 같이 떨어짐)")]
    [SerializeField] private bool _affectGlobalGravity = false;

    private float _lastUsedTime = -1f;

    private void OnTriggerEnter(Collider other)
    {
        // 쿨다운 체크
        if (Time.time < _lastUsedTime + _cooldownTime) return;

        bool isPlayerHit = false;
        PlayerMovement pm = null;
        GravityWalker gw = null;

        if (other.TryGetComponent(out pm)) isPlayerHit = true;
        if (other.TryGetComponent(out gw)) isPlayerHit = true;

        if (isPlayerHit)
        {
            _lastUsedTime = Time.time;
            StartCoroutine(HandleGravityFlipRoutine(pm, gw));
            CustomLog.Log($"[GravityFlipPad] 점프대 발동! (Launch Force: {_launchForce})");
        }
    }

    private IEnumerator HandleGravityFlipRoutine(PlayerMovement pm, GravityWalker gw)
    {
        // 1. 발사! (위로 정상적인 점프 가속도를 줍니다)
        if (pm != null)
        {
            pm.Launch(_launchForce);
        }

        // 2. 최고점에 다다를 때 무렵까지 대기
        yield return new WaitForSeconds(_flipDelay);

        // 3. 중력 및 속도 방향 반전
        if (gw != null) gw.InvertGravity();
        
        // 궤도(포물선)가 깨지지 않도록 플레이어의 상승 속도 기호도 함께 반전시킵니다.
        if (pm != null) pm.InvertVerticalVelocity();

        // 4. (옵션) 맵 전체 중력 시스템 반전 (다른 프랍들도 같이 떨어짐)
        if (_affectGlobalGravity && GravityAttractor.Instance != null)
        {
            GravityAttractor.Instance.ToggleGravityDirection();
            CustomLog.Log("[GravityFlipPad] 공간의 글로벌 중력이 함께 반전되었습니다!");
        }
    }
}
