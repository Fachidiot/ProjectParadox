---
name: Sound
description: |
    BGM과 SE의 볼륨 설정을 중앙에서 관리하고 영속화하는 오디오 볼륨 시스템
type: 공용
---

# `Sound`

## 개요
게임의 오디오 볼륨 레벨을 위한 중앙 집중식 설정 허브이다. BGM과 SE의 볼륨 값을 `FloatValue`로 관리하며, `PlayerPrefsSaveManager`를 통해 로컬에 영속화한다. `DealManager`, `NumberManager`, `LanguageManager`에 볼륨 데이터를 등록하여 다른 시스템에서 참조 및 거래가 가능하다.

## 참조 시스템
`Deal`: BGM/SE 볼륨의 거래(Set/Change) 등록
`Number`: BGM/SE 볼륨 값의 범용 숫자 참조 등록
`Language`: BGM/SE 볼륨의 퍼센트 텍스트 번역 등록
`PlayerPrefsSave`: BGM/SE 볼륨 값의 로컬 저장

## 핵심 기능

### 볼륨 관리
BGM과 SE 각각의 볼륨을 0~1 범위의 `FloatValue`로 관리한다. `ConstraintChanged`를 통해 값이 항상 0~1 범위로 제한된다.

### 거래 연동
`DealManager`에 "BGMVolume", "SEVolume" 거래를 등록하여 Need/Set/Change 연산을 지원한다.

## 스크립트

### 매니저

#### `SoundManager`
BGM/SE 볼륨을 관리하는 글로벌 싱글톤 매니저이다. `BGMVolume`과 `SEVolume` 프로퍼티를 통해 `IReadOnlyFloatValue`로 읽기 전용 접근을 제공한다. `SetBGMVolume`, `SetSEVolume` 함수로 볼륨을 설정한다.

