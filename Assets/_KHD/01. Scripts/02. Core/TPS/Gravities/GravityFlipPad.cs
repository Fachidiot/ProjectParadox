/// <summary>
/// KHD 전용 중력 반전 점프대 기믹입니다.
/// CharacterPhysicsSpherical 기반으로 수직 상승 및 중력 점프/반전을 위임합니다.
/// </summary>

using System.Collections;
using UnityEngine;

namespace KHD
{
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
            if (Time.time < _lastUsedTime + _cooldownTime) return;

            bool isPlayerHit = false;

            // 새로 작성된 KHD_CharacterPhysicsSpherical에 대한 접근
            if (other.TryGetComponent(out GravityCharacterPhysics phys))
            {
                isPlayerHit = true;
                _lastUsedTime = Time.time;
                StartCoroutine(HandleGravityFlipRoutine(phys));
                CustomLog.Log($"[KHD_GravityFlipPad] 점프대 발동! (Launch Force: {_launchForce})");
            }
        }

        private IEnumerator HandleGravityFlipRoutine(GravityCharacterPhysics phys)
        {
            // 1. 발사!
            phys.Launch(_launchForce);

            // 2. 최고점에 다다를 때 무렵까지 대기
            yield return new WaitForSeconds(_flipDelay);

            // 3. 중력 및 속도 방향 반전
            phys.InvertGravity();
            phys.InvertVerticalVelocity();

            // 4. 글로벌 중력 시스템 반전 (다른 프랍들도 같이 떨어짐)
            if (_affectGlobalGravity && GravityAttractor.Instance != null)
            {
                GravityAttractor.Instance.ToggleGravityDirection();
                CustomLog.Log("[KHD_GravityFlipPad] 공간의 글로벌 중력이 함께 반전되었습니다!");
            }
        }
    }
}