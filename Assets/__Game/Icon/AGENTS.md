---
name: Icon
description: |
    문자열 ID 기반 Sprite 에셋 등록 및 조회를 제공하는 아이콘 관리 시스템
type: 공용
---

# `Icon`

## 개요
Sprite 에셋의 등록 및 검색을 위한 중앙 집중식 서비스이다. 문자열 ID를 사용하여 아이콘을 요청할 수 있어 UI 컴포넌트와 에셋 파일 구조를 분리한다. 동적 아이콘은 `ValueBase`와 연동하여 값 변경 시 자동으로 스프라이트가 갱신된다. 등록된 아이콘 -> `Table` 데이터 -> `Resources` 폴더 순으로 폴백 조회를 수행한다.

## 참조 시스템
`Value`: 동적 아이콘의 `ValueBase` 연동으로 값 변경 시 스프라이트 자동 갱신
`Table`: 등록되지 않은 아이콘의 `Table` 데이터 폴백 조회

## 핵심 기능

### 동적 아이콘 등록
`Create`로 ID에 `ValueBase`와 변환 함수를 연동 등록한다. 값이 변경되면 변환 함수가 호출되어 스프라이트가 자동 갱신된다.

### 다단계 폴백 조회
`Get`으로 아이콘을 조회할 때 등록된 아이콘 -> `Table` 데이터(ID 또는 "Icon" 컬럼) -> `Resources/Icon/Icon_{id}` 순으로 폴백한다.

## 스크립트

### 매니저

#### `IconManager`
아이콘 관리를 총괄하는 `GlobalManager` 싱글톤이다. 동적 아이콘 등록(`Create`), 스프라이트 조회(`Get`), 연동 `ValueBase` 조회(`GetValue`/`GetValues`)를 제공한다.

