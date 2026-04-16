---
name: SlotSave
description: |
    슬롯 기반 파일 저장/로드 시스템, 문서 폴더에 JSON 파일로 관리
type: 공용
---

# `SlotSave`

## 개요
`ValueBase` 데이터를 슬롯 번호 기반 JSON 파일로 저장/로드한다. `PlayerPrefsSave`와 달리 자동 저장이 아닌 수동 저장/로드 방식이며, `MyDocuments/MyGames/Nbing/DontWakeup/Save_{슬롯번호}.json` 경로에 파일을 관리한다.

## 참조 시스템
`Value`: `ValueBase`의 `OnSaveString`/`OnLoadString`을 통한 직렬화/역직렬화

## 핵심 기능

### 값 등록
`Create`로 `ValueBase` 인스턴스를 저장 대상으로 등록한다. `PlayerPrefsSaveManager`와 동일한 패턴이나 자동 저장/로드는 하지 않는다.

### 수동 저장
`Save(slot)`으로 등록된 모든 값을 슬롯 파일에 JSON으로 저장한다. `Save()`로 현재 슬롯에 저장한다.

### 수동 로드
`Load(slot)`으로 슬롯 파일에서 값을 로드한다. 파일이 없으면 `false`를 반환한다.

### 슬롯 관리
`Exists(slot)`으로 파일 존재 여부, `Delete(slot)`으로 슬롯 파일 삭제, `CurrentSlot`으로 현재 로드된 슬롯 번호를 확인한다.

## 스크립트

### 매니저

#### `SlotSaveManager`
슬롯 기반 파일 저장/로드 싱글톤 글로벌 매니저. `Create`로 `ValueBase`를 등록하고, `Save`/`Load`/`Delete`/`Exists`로 슬롯 파일을 관리한다. LitJson을 사용하여 JSON 직렬화를 수행한다.
