---
name: Equip
description: |
    장비 장착 상태 관리, SlotSave 연동 저장/로드
type: 공용
---

# `Equip`

## 개요
`EItemKind` 슬롯(Shoes, Sword, Hat, Wing)별로 장착된 아이템 ID를 `StringValue`로 관리한다. `SlotSaveManager`에 등록하여 슬롯 저장/로드 시 자동으로 포함된다.

## 참조 시스템
`Table`: `TableManager.instance.Get<EItemKind>("Item.{id}.Kind")`로 아이템 종류 조회
`SlotSave`: `SlotSaveManager.instance.Create`로 값 등록
`Value`: `StringValue`로 장착 아이템 ID 관리

## 핵심 기능

### 장착/해제
`Equip(itemId)`로 아이템 종류를 자동 판별하여 해당 슬롯에 장착. `Unequip(kind)`로 슬롯 해제.

### 비주얼 반영
`LocalPlayerActorManager`에서 `StringValue.AddChanged`를 통해 장착 변경을 감지하고, `Resources/Item/{itemId}` 프리팹을 `PlayerActor`의 슬롯 Transform에 생성/삭제한다. Shoes는 `_Left`/`_Right` 프리팹을 각각 로드한다.

## 스크립트

### 매니저

#### `EquipManager`
장비 장착 상태 싱글톤 글로벌 매니저. 슬롯별 `StringValue`를 보유하며 `Equip`/`Unequip`/`GetEquipped`/`IsEquipped` API를 제공한다.
