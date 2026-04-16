---
name: Deal
description: |
    모든 형태의 거래(조건 확인, 비용 지불, 보상 지급)를 처리하는 중앙 집중식 시스템
type: 공용
---

# Deal

## 개요
게임 내 모든 자원/재화/상태의 소비·획득·조건확인을 통합 처리하는 중앙 거래 시스템이다.
각 `개발시스템`이 자신의 거래 대상을 Key로 등록하면, `SDeal`의 "Key"와 "Count"만으로 어떤 재화든 동일한 방식으로 거래할 수 있다.

## 참조 시스템
`Table`: 거래 대상을 테이블 데이터에서 자동 조회
`Value`: 재화 보유량 변화를 실시간 감시하여 UI에 자동 반영
`Icon`: 거래 대상의 아이콘 자동 표시
`Popup`: 거래 결과 `팝업` 표시
`ObjectPool`: `팝업` 내 아이템 목록 풀링

## 핵심 기능

### `SDeal`
모든 거래의 기본 데이터 단위. `ITableType`을 구현하며 테이블에서 비용/보상 정의 시 이 타입을 사용한다.
- "Key" (string): 거래 대상 (예: "Currency_Iron", "Resource_Stone")
- "Action" (string): 행동 수식어 (예: "", "Not")
- "Count" (double): 수량

### 거래 종류
- Need: 조건 확인 (소비 없음)
- Set: 값을 지정한 수치로 덮어쓰기
- Change: 값을 지정한 수치만큼 증감
- Pay: Need 확인 후 통과하면 차감, 실패 시 null 반환

### 복수 거래
- NeedAll: 여러 조건을 모두 확인, 하나라도 실패하면 전체 실패
- PayAll: 모든 조건 확인 후 일괄 지불 (부분 차감 없음)
- SetAll/ChangeAll: 여러 거래 일괄 처리

### 등록 구조
각 `개발시스템`이 Init 단계에서 `DealManager.Create()`로 자신이 관리하는 Key/Table을 등록한다.
등록 시 Need/NeedValue/Set/Change/Pay 콜백을 지정한다.

## 스크립트

### 매니저

#### `DealManager`
글로벌 싱글톤. Key/Table 기반으로 거래 처리 함수를 관리하며, Need/Set/Change/Pay 연산을 위임한다.

### 기타
`SDeal`: 거래 데이터 구조체 (Key, Action, Count). `ITableType` 구현.

## UI

### `팝업`
`Popup_ChangeResult`: 거래로 획득한 아이템 목록을 아이콘+수량으로 표시, 아이템이 많으면 페이지로 나눠 표시

### `컨트롤`
`Control_Button_Deal`: 비용/보상을 설정하면 자동으로 조건 확인 → 비용 차감 → 보상 지급 처리, 비용 부족 시 버튼 잠금
`Control_Active_Deal`: 특정 재화 조건에 따라 게임오브젝트를 자동 활성화/비활성화
`Control_Button_ChangeIcon`: 거래 아이템의 아이콘, 수량, 액션을 표시하는 버튼
