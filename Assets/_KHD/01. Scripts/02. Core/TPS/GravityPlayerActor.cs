/// <summary>
/// KHD 전용 구형 중력(Spherical Gravity) 및 원근법 쥐기(Perspective Grab) 입력을 탑재한 PlayerActor 확장 클래스입니다.
/// </summary>

using UnityEngine;
using UnityEngine.InputSystem;

namespace KHD
{
    public class GravityPlayerActor : PlayerActor
    {
        // 참고: PlayerInputHandler의 Action 액션을 그랩으로 사용
        public bool IsGrabPressed => m_InputHandler != null && m_InputHandler.ActionTriggered;
        public bool IsGrabHeld => m_InputHandler != null && m_InputHandler.IsActionPressed;

        private GravityCharacterPhysics _sphericalPhysics;

        private void Start()
        {
            // 구형 중력 환경에서는 캐릭터가 행성 밑바닥(Y가 마이너스인 곳)을 걸을 수 있으므로
            // 베이스 코드의 y축 고정 낙하 판정(m_FallThresholdY)을 강제로 비활성화(-99999) 합니다.
            var field = typeof(PlayerActor).GetField("m_FallThresholdY", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(this, -99999f);
            }

            // Init 과정 후 Physics 캐싱
            _sphericalPhysics = Physics as GravityCharacterPhysics;
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
        }

        /// <summary>
        /// 글로벌 Y축 기반의 카메라 전방 투영 대신, 현재 발 디딛고 있는 구면 법선(GroundNormal) 기반으로 투영합니다.
        /// </summary>
        public override Vector2 GetCameraMoveDir()
        {
            if (_sphericalPhysics == null)
                return base.GetCameraMoveDir();

            Transform camRot = LocalCameraManager.instance.RotRoot;
            Vector3 groundNormal = _sphericalPhysics.GroundNormal;

            // 카메라의 전방/우측 벡터를 현재 지면(GroundNormal)에 투영
            Vector3 forward = Vector3.ProjectOnPlane(camRot.forward, groundNormal).normalized;
            Vector3 right = Vector3.ProjectOnPlane(camRot.right, groundNormal).normalized;

            Vector3 moveDir = forward * MoveInput.y + right * MoveInput.x;

            // 물리부(CharacterPhysicsSpherical)는 Vector2를 받아서 (x, 0, y) 형태로 취급합니다.
            // 현재 내부적으로 KHD_CharacterPhysicsSpherical.HandleMovementKHD 에서 Vector3(_currentMoveTarget.x, 0, _currentMoveTarget.y)를 만든 후 다시 투영합니다.
            // 하지만 이렇게 하면 구형 좌표계에서 월드 3D 방향을 2D Vector로 축소할 때 큰 문제가 생깁니다.
            // 따라서 아예 3D 방향 그 자체를 전달하기 위해 x를 forward/right 투영 축에 맞춰서 주거나,
            // _currentMoveTarget 대신 CameraMoveDir을 직접 받는 새로운 인터페이스를 짜는것이 좋지만, 
            // 다형성을 위해 Vector2에 로컬 X, Z 속도를 매핑합니다.

            // 로컬 좌우/앞뒤 강도를 리턴
            return new Vector2(MoveInput.x, MoveInput.y);
        }

        /// <summary>
        /// 구형 중력 환경에서는 CharacterPhysicsSpherical 내부에서 이미 로컬 Up축 정렬(HandleBodyRotation)을 진행합니다.
        /// 따라서 여기서는 카메라 방향(월드 투영값)에 맞춰 수직 Y축 전용 회전만을 제어합니다.
        /// </summary>
        public override void RotateTowardMoveDir(Vector2 moveDir)
        {
            if (_sphericalPhysics == null)
            {
                base.RotateTowardMoveDir(moveDir);
                return;
            }

            if (moveDir == Vector2.zero) return;

            Transform camRot = LocalCameraManager.instance.RotRoot;
            Vector3 groundNormal = _sphericalPhysics.GroundNormal;

            Vector3 forward = Vector3.ProjectOnPlane(camRot.forward, groundNormal).normalized;
            Vector3 right = Vector3.ProjectOnPlane(camRot.right, groundNormal).normalized;

            Vector3 moveWorldDir = forward * moveDir.y + right * moveDir.x;

            // 현재 캐릭터의 Up(구면수직)을 유지한 채, 앞을 moveWorldDir 방향으로 맞추는 회전값 산출
            Quaternion targetRot = Quaternion.LookRotation(moveWorldDir, transform.up);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
        }
    }
}