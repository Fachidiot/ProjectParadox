---
name: CameraControl
description: |
    카메라 입력 조작 (줌/회전/좌우전환), PlayerActor 추적
type: 로컬
---

# `CameraControl`

## 개요
카메라 줌(스크롤), 회전(우클릭 드래그), 좌우 전환(V키) 입력을 처리하고, 매 프레임 카메라를 PlayerActor 위치에 추적시킨다.

## 참조 시스템
`Camera`: `LocalCameraManager.instance`로 줌/회전/위치 제어
`Input`: `LocalInputManager.instance.Create`로 입력 등록
`PlayerActor`: `LocalPlayerActorManager.instance.CurActor`로 추적 대상 참조

## 핵심 기능

### 카메라 줌
스크롤 입력으로 `LocalCameraManager.ChangeZoom` 호출.

### 카메라 회전
우클릭 드래그로 카메라 Yaw/Pitch 회전.

### 카메라 좌우 전환
V키로 카메라를 중심 기준 좌/우 오프셋 전환. `ESide` enum으로 상태 관리.

### PlayerActor 추적
LateUpdate에서 `LocalCameraManager.transform.position`을 PlayerActor 위치로 설정.

## 스크립트

### 매니저

#### `LocalCameraControlManager`
카메라 조작 싱글톤 로컬 매니저. 줌/회전/좌우전환 입력 콜백과 PlayerActor 추적을 담당한다.
