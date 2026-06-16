# Spec: Agent Folder and Rules Setup

## 1. 개요
이 문서는 BePex Unity Client 프로젝트에 독립된 에이전트 전용 규칙 베이스 및 문서 폴더(`/.agent/`)를 설정하기 위한 기획 사양서입니다. 에이전트가 향후 개발 작업을 수행할 때 이 규칙들을 인지하고 일관된 코드 품질을 유지할 수 있도록 하는 것을 목표로 합니다.

## 2. 요구사항 및 목표
- 에이전트 전용의 감춰진 폴더 구조인 `/.agent/`를 생성합니다.
- `/.agent/rules/` 하위에 도메인별 규칙 마크다운 파일을 작성합니다.
- `/.agent/docs/` 하위에 프로젝트 아키텍처 및 씬 전환 데이터 전달 가이드를 작성합니다.
- VContainer, MVVM, Unity Safety, Clean Code 철학을 명확히 명세합니다.

## 3. 디렉토리 구조 설계
프로젝트 루트 디렉토리 최상위에 다음과 같이 구성합니다:
```
/.agent/
├── rules/
│   ├── architecture-rules.md  # Clean Code, POCO, MVVM 규칙
│   ├── vcontainer-rules.md    # VContainer 사용 및 등록 규칙
│   └── safety-rules.md        # Naming, Unity Safety, 루프 최적화 규칙
└── docs/
    └── architecture-guide.md  # 디렉토리 구조 및 씬 전환 DTO 가이드
```

## 4. 상세 파일 설계

### 4.1. rules/architecture-rules.md
- **Clean Code & POCO**: 비즈니스 로직은 `MonoBehaviour`를 상속받지 않는 순수 C# 클래스로 작성. 싱글톤 사용 전면 금지.
- **MVVM 구조**:
  - **Model**: 순수 데이터 클래스, View나 Unity 엔진에 의존하지 않음.
  - **ViewModel**: Model 데이터를 가공 및 제공. View가 구독할 상태(State)와 명령(Command) 구현.
  - **View**: MonoBehaviour 상속, 데이터 바인딩 및 입력 전달 수행. Model 직접 참조 절대 금지.

### 4.2. rules/vcontainer-rules.md
- **LifetimeScope**: 씬 당 하나 배치, 기능별로 `ConfigureXxx` private 메서드로 분리하여 구성.
- **View 등록**: `RegisterComponentInHierarchy<T>()`를 우선 사용. 동적 생성 시 `RegisterComponentOnNewGameObject<T>()` 사용. `SerializeField`로 LifetimeScope에서 직접 참조하여 `RegisterInstance` 등록하는 구조 금지.

### 4.3. rules/safety-rules.md
- **Naming Conventions**: 
  - Private 필드: `m_` 접두사 + camelCase.
  - Public 프로퍼티/메서드: PascalCase.
  - UI 이벤트 콜백: `func_` 접두사.
- **Unity Safety**: `UnityEngine.Object` 파생 타입에 널 조건부 연산자(`?.`, `??`) 사용 절대 금지 (Fake Null 방지).
- **Optimization**: 빈번하게 호출되는 루프(`Update` 등)에서는 `foreach` 대신 `for` 사용. `new` 할당, Boxing, LINQ 사용 금지.
- **Async & Tween**: 코루틴 대신 `UniTask` 사용, 트윈 처리는 `DOTween` 사용.

### 4.4. docs/architecture-guide.md
- **Directory Layout**: Assets/Scripts 하위의 아키텍처 레이어 역할 설명.
- **DTO Scene Transition**: 씬 전환 시 유지해야 할 데이터는 DTO(`~DTO` 접미사)로 캡슐화하고 다음 씬의 Initializer에 주입하는 규격 명세.

## 5. 검증 계획
- `/.agent/` 디렉토리 하위의 파일들이 올바르게 생성되었는지 확인합니다.
- 마크다운 파일들의 내용이 누락 없이 가이드를 포함하는지 검사합니다.
