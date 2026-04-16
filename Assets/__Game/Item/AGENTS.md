---
name: Item
description: |
    아이템 보유 상태 관리, SlotSave 연동 저장/로드
type: 공용
---

# `Item`

## 개요
아이템 테이블(`TableManager.Item`)의 각 아이템에 대해 보유 수량을 `IntValue`로 관리한다. `SlotSaveManager`에 등록하여 슬롯 저장/로드 시 자동으로 포함된다.

## 참조 시스템
`Table`: `TableManager.instance.Item`에서 아이템 ID 목록 조회
`SlotSave`: `SlotSaveManager.instance.Create`로 값 등록
`Value`: `IntValue`로 아이템 수량 관리

## 핵심 기능

### 아이템 수량 관리
`Add`/`Remove`/`SetCount`로 아이템 수량을 변경하고, `GetCount`/`Has`로 조회한다.

### SlotSave 연동
Init 시 모든 아이템 ID에 대해 `IntValue`를 생성하고 `SlotSaveManager`에 등록한다. 슬롯 저장/로드 시 자동 포함된다.

## 스크립트

### 매니저

#### `ItemManager`
아이템 보유 상태 싱글톤 글로벌 매니저. 아이템 ID별 `IntValue`를 보유하며 `Add`/`Remove`/`SetCount`/`GetCount`/`Has` API를 제공한다.
