---
name: inspecting-logic
description: Use when verifying architecture compliance in a Unity C# codebase, specifically validating MVVM boundaries, decoupling of conditions and rewards, and POCO data structures.
---

# Inspecting Logic

## Overview
이 스킬은 Unity 클라이언트 프로젝트에서 비즈니스 로직, 데이터 모델, UI 뷰 계층이 아키텍처 규칙(MVVM, Pure DI, POCO)을 올바르게 준수하고 있는지 검사하고 진단하기 위한 체계적인 절차를 제공합니다.

## When to Use
- Unity C# 프로젝트의 기능 구현 전후에 아키텍처 적합성을 평가할 때
- 의존성 그래프가 꼬이거나 View가 Model을 우회 참조하는지 점검할 때
- 새로운 이벤트 조건(Condition) 및 보상(Reward) 타입이 추가된 뒤 확장성(OCP)을 검증할 때

## Core Pattern

### 1. 계층 구분 검사 (Separation of Concerns)
모든 C# 클래스가 다음 4가지 역할 중 하나에만 충족되는지 검사합니다.

| 계층 | 기준 및 제약 | 허용되는 네임스페이스 및 참조 |
| :--- | :--- | :--- |
| **Model** | 도메인 데이터와 규칙만 포함. 유니티 엔진 및 View 참조 불가. | 순수 C# (POCO), `System` |
| **ViewModel** | 상태(State/Action)와 명령(Command) 제공. View를 몰라야 함. | `System`, `BePex.EventSystem.Models` |
| **View** | `MonoBehaviour` 상속. 시각화 및 입력 전달만 수행. | `UnityEngine.UI`, `TMPro`, `BePex.EventSystem.ViewModels` |
| **Strategy (Condition/Reward)** | 조건 판정(IEventCondition) 및 보상 지급(IEventReward)의 팩토리 위임 처리. | 인터페이스 기반 설계, `BePex.EventSystem.Interfaces` |

### 2. 위반 탐지 (Common Violations)
- **View → Model 직접 참조**: View 클래스 내부에서 Model 타입을 필드로 가지거나, 생성/캐스팅하거나, `using`으로 임포트하는 행위 (절대 금지).
- **상태의 양방향 흐름**: View가 직접 Model을 읽고 수정하는 행위. 모든 상호작용은 ViewModel의 Command를 경유해야 함.
- **데이터 클래스 내 로직 포함**: POCO 데이터 모델(`EventProgressModel`, `PlayerRewardModel`) 내부에 무거운 비즈니스 연산이 포함되는 경우.

## Implementation (검사 절차)
1. **대상 탐색**: `EventSystem` 디렉토리 내의 모든 C# 스크립트 리스트를 수집합니다. **누락 방지를 위해 최소 3번 이상 교차 탐색(예: list_dir, class 검색, interface 검색)을 수행해야 합니다.**
2. **역할 매핑**: 수집된 파일들을 Model, ViewModel, View, Strategy, DTO, Infra/Factory 계층으로 분류합니다.
3. **의존성 스캔**: 각 스크립트 상단의 `using` 지시문과 멤버 필드 타입을 스캔하여 의존성 규칙 위반을 색출합니다.
4. **결과 작성**: 아래의 세 부분으로 구성된 최종 로직 검사 리포트를 작성합니다.
   - **1. 검사한 스크립트 목록**: 역할군별로 완벽히 분류된 스크립트 전체 리스트.
   - **2. 누락된 스크립트 목록**: 계층 정의에 맞지 않거나, 아키텍처 위반(예: View가 Model 직접 참조) 등으로 누락/분류 불가능한 스크립트 목록 (없을 경우 '없음'으로 명시).
   - **3. 검사 결과 (판정 표)**: 아래 표 형식에 따라 각 스크립트의 데이터-로직 분리, UI-로직 분리, 조건-보상 분리 판정(Pass/Fail) 결과를 정리합니다.

```markdown
| 파일명 | 계층 (Role) | 데이터-로직 분리 | UI-로직 분리 | 조건-보상 분리 | 판정 (Pass/Fail) | 비고 |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `EventDetailView.cs` | View | N/A | Pass | N/A | Pass | ViewModel을 통해서만 통신 |
| `EventModel.cs` | Model | Pass | Pass | Pass | Pass | 비즈니스 로직 집중 |
```
