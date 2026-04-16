---
name: PlayerActor
description: |
    플레이어 캐릭터의 FSM 기반 이동/점프 및 3인칭 카메라 조작(줌/회전/좌우전환) 통합 관리
type: 공용
---

# `PlayerActor`

## 개요
플레이어 캐릭터의 물리 기반 이동과 3인칭 카메라 조작을 통합 처리한다. `FSM`으로 Idle/Move/Jump 상태를 관리하고, `CharacterPhysics3D`로 물리 이동/점프를 수행하며, `LocalCameraManager`로 카메라 줌/회전/좌우전환을 제어한다. 모든 입력은 `LocalInputManager`를 통해 수신한다.

## 참조 시스템
`Camera`: `LocalCameraManager.instance`를 통한 카메라 위치/회전/줌 제어
`Input`: `LocalInputManager.instance`를 통한 입력 콜백 등록
`CharacterPhysics`: `CharacterPhysics3D`를 통한 물리 기반 이동/점프
`FSM`: `FSM`을 통한 상태 관리

## 핵심 기능

### FSM 상태 관리
`Idle` → `Move` → `Jump` 상태를 FSM으로 전환한다. `PlayerActor`가 입력을 수신하여 `MoveInput`/`IsJumpRequested`를 갱신하고, 각 State가 이를 읽어 전환 및 동작을 수행한다.

### 카메라 방향 기반 이동
`GetCameraMoveDir`로 카메라 RotRoot의 forward/right 방향 기반 이동 벡터를 계산한다. Move/Jump 상태에서 `CharacterPhysics3D.Move`에 전달한다.

### 스크롤 줌
마우스 스크롤 휠로 카메라 줌 인/아웃을 수행한다. `ChangeZoom`으로 부드럽게 줌하며 `CameraClampBase`에 의해 자동 범위 제한된다.

### 마우스 회전
마우스 우클릭 드래그로 카메라를 회전한다. X→Yaw, Y→Pitch를 제어한다.

### 좌우 위치 전환
V키로 카메라 X축 위치를 좌/우 토글한다. `ChangePos`로 부드럽게 전환한다.

## 스크립트

### 매니저

#### `LocalPlayerActorManager`
플레이어 액터 관리 싱글톤 로컬 매니저. `PlayerActor`를 보유하고 초기화를 수행한다.

### 기타

#### `PlayerActor`
플레이어 캐릭터 본체. 입력 수신 및 카메라 조작을 담당하고, `MoveInput`/`IsJumpRequested`를 FSM 상태에 노출한다. `FSM.Init(this)`로 자신을 Owner로 전달한다.

#### `PlayerActorState_Idle`
대기 상태. 이동 입력 시 Move, 점프 요청 시 Jump로 전환한다.

#### `PlayerActorState_Move`
이동 상태. 카메라 방향 기반으로 `CharacterPhysics3D.Move`를 호출한다. 입력 없으면 Idle, 점프 요청 시 Jump로 전환한다.

#### `PlayerActorState_Jump`
점프 상태. `OnStart`에서 `CharacterPhysics3D.Jump`를 호출하고, 공중에서도 이동 입력을 처리한다. 착지 시 입력 여부에 따라 Move 또는 Idle로 전환한다.
