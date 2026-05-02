using Sirenix.OdinInspector;
using UnityEngine;

namespace KHD
{
    /// <summary>
    /// 구형 중력 환경에서 카메라 리그(LocalCameraManager)의 up 방향을
    /// 플레이어의 표면 법선에 맞춰 회전시키는 컴포넌트.
    /// 기존 LocalCameraControlManager와 함께 사용합니다.
    /// 
    /// 카메라 계층:
    ///   CameraRig (LocalCameraManager.transform) — 이 스크립트가 world rotation 설정
    ///     └ RotRoot — localRotation = 마우스 입력 (Yaw/Pitch)
    ///         └ PosRoot — localPosition = 오프셋/줌
    ///             └ Camera
    /// </summary>
    public class GravityCameraController : MonoBehaviour
    {
        #region Inspector
        [Title("구형 중력 카메라")]
        [SerializeField, LabelText("정렬 속도")]
        private float _alignSpeed = 10f;

        [SerializeField, LabelText("물리 참조")]
        private GravityCharacterPhysics _physics;
        #endregion

        #region Event
        private void LateUpdate()
        {
            var camManager = LocalCameraManager.instance;
            if (camManager == null) return;

            var actorManager = LocalPlayerActorManager.instance;
            if (actorManager == null || actorManager.CurActor == null) return;

            Transform camRig = camManager.transform;
            Transform player = actorManager.CurActor.transform;

            // ① 위치: 플레이어 추적 (기존 LocalCameraControlManager와 동일)
            camRig.position = player.position;

            // ② 회전: 카메라 리그의 up을 플레이어의 up(표면 법선)에 정렬
            Vector3 playerUp = player.up;
            AlignCameraRig(camRig, playerUp);
        }
        #endregion

        #region Function
        /// <summary>
        /// 카메라 리그의 up 방향을 targetUp에 부드럽게 정렬합니다.
        /// 기존 forward 방향을 최대한 보존하면서 up만 변경합니다.
        /// </summary>
        private void AlignCameraRig(Transform camRig, Vector3 targetUp)
        {
            // 현재 카메라의 forward를 새로운 up 평면에 투영
            Vector3 projectedForward = Vector3.ProjectOnPlane(camRig.forward, targetUp);

            // forward가 targetUp과 거의 평행한 경우 (위/아래를 보고 있을 때) → right 기준 복구
            if (projectedForward.sqrMagnitude < 0.001f)
                projectedForward = Vector3.ProjectOnPlane(camRig.right, targetUp);

            if (projectedForward.sqrMagnitude < 0.001f)
                return; // 안전장치

            projectedForward.Normalize();

            Quaternion targetRot = Quaternion.LookRotation(projectedForward, targetUp);
            camRig.rotation = Quaternion.Slerp(camRig.rotation, targetRot, Time.deltaTime * _alignSpeed);
        }
        #endregion
    }
}
