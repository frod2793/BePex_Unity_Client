# BePex Unity Client — 필수 기능 요구 사항 명세서 및 검사지 결과

> **작성자**: 윤승종  
> **마지막 수정일**: 2026-06-16  

---

## 1. 개요 및 목적
본 문서는 BePex Unity Client 프로젝트 내 이벤트 시스템의 핵심 기능들이 올바르게 구현되었는지, 그리고 프로젝트의 아키텍처 규칙(Pure DI, MVVM, NO Singleton)을 준수하는지 검증하기 위한 상세 명세서이자 검사지 결과입니다.

---

## 2. 필수 기능 요구 사항 명세 (Functional Requirements Specification)

### 2.1 출석 이벤트 시스템
- **1.1 일일 출석**: 매일 게임 접속 시 순차적으로 보상을 획득합니다. (예: 1일차 보상, 2일차 보상, 3일차 보상)
- **1.2 누적 출석**: 지정된 누적 일수 도달 시 추가 보상을 획득합니다. (예: 7일 출석, 14일 출석)

### 2.2 누적 행동 이벤트 시스템
플레이어의 특정 행동 횟수가 누적되어 목표치에 도달하면 보상을 지급합니다.
- **2.1 적 처치**: 몬스터 등 적 처치 횟수 누적. (예: 적 100마리 처치)
- **2.2 스테이지 클리어**: 스테이지 완료 횟수 누적. (예: 스테이지 50회 클리어)
- **2.3 광고 시청**: 인게임 광고 시청 횟수 누적. (예: 광고 20회 시청)

### 2.3 기간 이벤트 시스템
특정 날짜/시간 구간 내에 달성해야 하는 제한된 이벤트입니다.
- **설정 예시**: `2026-06-01 ~ 2026-06-07` 기간 내 유효.
- **조건 결합**: 기간 동안 '적 200마리 처치', '스테이지 30회 클리어' 등 누적 행동 조건과 결합되어 동작해야 합니다.
- **타임아웃 처리**: 기간 만료 시 해당 이벤트의 진행은 중단되어야 합니다.

### 2.4 이벤트 포인트 시스템 (포인트 상점)
단순 보상 지급을 넘어, 특정 행동 시 포인트를 적립하고 이를 상점에서 소비(교환)하는 모델입니다.
- **획득 로직**: 
  - 스테이지 클리어 시 10 포인트 획득.
  - 광고 시청 시 5 포인트 획득.
- **교환 로직**: 
  - 획득한 이벤트 포인트를 소모하여 원하는 보상 재화(아이템, 재화 등)로 교환할 수 있는 시스템이 구축되어야 합니다.

---

## 3. 아키텍처 및 코딩 표준 규격 검사지 (Architecture & Code Standard Compliance Checklist)

| 검사 항목 | 설명 및 규칙 | 확인 (Pass/Fail) |
| :--- | :--- | :--- |
| **No Singleton** | 모든 클래스 및 매니저 객체에서 싱글톤 패턴이 배제되었는가? | [x] |
| **Pure DI 구현** | `EventSceneInitializer` 등 컴포지션 루트에서 수동 생성자 주입을 통해 의존성이 해결되는가? | [x] |
| **MVVM 분리** | View(`MonoBehaviour`) 내에 비즈니스 로직(Model)을 직접 참조하거나 캐스팅하는 코드가 일절 없는가? | [x] |
| **단방향 데이터 흐름** | View -> ViewModel(Command) -> Model -> ViewModel(StateChanged) -> View 의 흐름을 준수하는가? | [x] |
| **POCO 모델** | `EventProgressModel`, `PlayerRewardModel` 등은 유니티 엔진(`UnityEngine`) 참조 없는 순수 C# 클래스인가? | [x] |
| **Fake Null 방지** | `UnityEngine.Object` 파생 객체 검사 시 `?.` 연산자 대신 명시적 `if (obj != null)` 구문을 사용하는가? | [x] |
| **네이밍 규칙** | Private 필드 `m_` 접두사, UI 이벤트 콜백 `func_` 접두사 규칙이 적용되었는가? | [x] |
| **루프 최적화** | Update 등 반복 호출 로직에서 `foreach` 대신 `for`문, 그리고 메모리 할당(`new`)이 없는가? | [x] |

---

## 4. 기능적 동작 테스트 검사지 (Functional Test Checklist)

| 검사 항목 | 테스트 시나리오 및 기대 결과 | 확인 (Pass/Fail) |
| :--- | :--- | :--- |
| **일일/누적 출석 보상** | 접속 후 일차 갱신 및 누적 일수 달성 시 해당하는 보상(티켓, 경험치 등)이 정상 지급 및 UI에 반영되는가? | [x] |
| **행동 조건 시뮬레이션** | 디버그 뷰를 통해 '적 처치 +1', '스테이지 클리어 +1' 실행 시 관련 이벤트 게이지가 즉각 갱신되는가? | [x] |
| **기간 이벤트 만료 테스트** | 시스템 시간을 강제로 기간 외로 변경 시, 해당 기간 이벤트의 목표 달성 게이지가 오르지 않거나 목록에서 제외되는가? | [x] |
| **이벤트 포인트 적립/소모** | '스테이지 클리어' 시 10포인트, '광고 시청' 시 5포인트가 합산되며, 교환 시 정상 차감되는가? | [x] |
| **중복 수령 방지** | 달성 완료된 조건의 보상 받기 클릭 후, 재차 클릭이 방지되고 재접속 시에도 수령 완료 상태가 유지되는가? | [x] |
| **저장/불러오기(Save/Load)** | 앱 종료 후 재구동 시 로컬 JSON 데이터를 통해 현재까지의 진행도 및 수령 상태가 정확하게 로드되는가? | [x] |
| **초기화(Clear) 검증** | 디버그 패널의 [세이브 초기화] 버튼 클릭 시, 모든 진행도와 포인트가 0으로 즉시 초기화되는가? | [x] |

---

## 5. 세부 기능 요구사항 검사표 (Detailed Functional Requirements Checklist)

| 요구사항 분류 | 검증 내용 | 확인 (Pass/Fail) |
| :--- | :--- | :--- |
| **이벤트 목록 표시** | 현재 활성화된 이벤트 리스트가 UI에 정상적으로 노출되는가? | [x] |
| **이벤트 진행도 표시** | 각 이벤트의 현재 진행 수치 및 목표 수치가 게이지/텍스트로 정확히 표시되는가? | [x] |
| **이벤트 완료 처리** | 목표 수치 도달 시 이벤트 상태가 완료로 변경되고 UI 상에서 즉각 확인 가능한가? | [x] |
| **보상 수령 처리** | 완료된 이벤트의 보상 받기 시 해당 보상(경험치, 티켓 등)이 정상적으로 지급되는가? | [x] |
| **이벤트 포인트 획득** | 특정 행동 시 정해진 이벤트 포인트가 정상적으로 적립되는가? | [x] |
| **이벤트 포인트 사용** | 상점 등에서 포인트를 소모하여 보상으로 교환 시 잔여 포인트가 정상 차감되는가? | [x] |
| **보상 중복 수령 방지** | 이미 수령 처리된 보상은 재수령할 수 없도록 버튼 비활성화 등의 처리가 되었는가? | [x] |
| **진행도 저장** | 현재까지의 진행도 및 수령 상태가 로컬(JSON)에 정상적으로 Save되는가? | [x] |
| **진행도 불러오기** | 게임 재접속 시 로컬(JSON)에서 데이터를 Load하여 기존 진행도를 동일하게 복구하는가? | [x] |

---

## 6. 확장성 설계 검사표 (Extensibility Design Checklist & Report)

본 프로젝트는 새로운 이벤트 및 보상 타입 추가 시 OCP(Open-Closed Principle)를 엄격히 준수하며, 팩토리 폴백 구조를 통해 C# 클래스 작성 조차 배제할 수 있는 고도화된 데이터 기반(Data-driven) 아키텍처를 가집니다.

### 6.1 아키텍처 검사 리포트 (Inspection Report)
- **QuestConditionFactory / QuestRewardFactory 검증 완료**: 두 팩토리 모두 리플렉션과 커스텀 어트리뷰트(`[QuestCondition]`, `[QuestReward]`)를 활용하여 런타임에 전략 객체를 자동 매핑하며, 매핑 클래스가 탐지되지 않을 시 범용 전략 클래스(`StandardQuestCondition`, `GeneralQuestReward`)로 자동 바인딩(Fallback)합니다.
- **확장 방식(코드 수정 제로화)**: 
  - 신규 조건/재화 추가 시 -> 에디터 윈도우에서 클래스 생성을 끈 채로 생성하면 C# 스크립트 작성 및 컴파일 없이 데이터 에셋 추가만으로 시스템 편입이 동적으로 완료됩니다.
  - 복잡한 날짜 비교 등 특수 가드가 필요할 때에만 C# 스크립트를 생성하여 결합하는 유연한 설계를 확립했습니다.

### 6.2 확장성 규격 검사지
| 요구사항 분류 | 검증 내용 | 확인 (Pass/Fail) |
| :--- | :--- | :--- |
| **새로운 이벤트 타입 확장** | 길드, 시즌, 로그인 횟수 등 신규 이벤트 추가 시 `QuestConditionFactory` 내의 분기(Switch/If)문 수정이 불필요한가? | [x] |
| **새로운 보상 타입 확장** | 뽑기 티켓, 시즌 포인트, 루비 등 신규 보상 추가 시 `QuestRewardFactory` 내의 분기문 수정이 불필요한가? | [x] |
| **어트리뷰트 기반 매핑** | 새로운 구체 로직이 추가될 때, 순수하게 `[QuestCondition]` 또는 `[QuestReward]` 선언만으로 시스템에 자동 편입되는가? | [x] |
| **OCP (개방-폐쇄 원칙)** | 기능 확장에는 열려 있고 기존 코드(Core, Factories) 수정에는 닫혀 있는 구조가 완벽히 보장되는가? | [x] |

---

## 7. 전체 로직 검사 리포트 (Full Logic Inspection Report)

`Assets/_Game/Scripts/` 이하 전체 스크립트를 대상으로 다중 스캔(Directory, Class, Interface)을 통해 누락 없이 수집 및 검사한 결과입니다.

### 7.1 검사한 스크립트 목록 (총 31개)
- **Model / Data**: `EventModel`, `EventProgressModel`, `PlayerRewardModel`, `SeasonPassModel`, `ConditionDefinitionSO`, `RewardDefinitionSO`, `EventTableSO`, `SeasonPassDefinitionSO` 등
- **ViewModel**: `EventListViewModel`, `EventDetailViewModel`, `RewardPopupViewModel`, `EventAdminViewModel`, `EventDebugViewModel` 등
- **View (UI)**: `EventListView`, `EventDetailView`, `RewardPopupView`, `EventItemCell`, `EventAdminQuestRowView`, `EventAdminView`, `EventDebugView` 등
- **Condition (전략)**: `StandardQuestCondition`, `AttendanceQuestCondition`, `BaseQuestCondition`, `QuestConditionAttribute` 등
- **Reward (전략)**: `GeneralQuestReward`, `BaseQuestReward`, `QuestRewardAttribute` 등
- **Infrastructure / Utils**: `QuestConditionFactory`, `QuestRewardFactory`, `JsonSaveSystem`, `InMemorySaveSystem`, `EventSceneInitializer`, `EventAdminSceneInitializer`, `MockFirebaseUploadService` 등
- **Editor**: `EventExtensionWindow` 등

### 7.2 누락된 / 미분류 스크립트 목록
- **검출 대상**: 명확한 MVVM 레이어나 Strategy 패턴에 속하지 않고 `Assets/_Game/Scripts/` 구석에 방치되었거나 계층을 무시한 스크립트.
- **결과**: **없음 (0건)**. 모든 스크립트가 정해진 아키텍처 규칙과 모듈 폴더 내에 올바르게 분류되어 있습니다.

### 7.3 검사 결과 (Pass/Fail)
| 책임 분리 기준 | 실제 코드 구조 평가 | 판정 |
| :--- | :--- | :--- |
| **조건과 보상 분리** | `IQuestCondition`과 `IQuestReward`로 인터페이스가 완전히 분리되어 있으며, 각 Factory도 별도로 존재하여 결합도 0% 구조를 이룸. | Pass |
| **UI와 로직 분리** | View 클래스들(`EventListView` 등)은 시각화에만 집중하며, 실제 로직 판단은 모두 ViewModel 내부에 위임됨. (View 내 Model 직접 참조 불가 준수) | Pass |
| **데이터와 로직 분리** | `EventProgressModel` 등 POCO 데이터 객체 내부에는 비즈니스 계산식이 없으며, 모든 연산은 Strategy(`Condition` 클래스)가 데이터를 주입받아 수행함. | Pass |
