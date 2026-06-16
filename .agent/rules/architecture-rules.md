# Unity Client Architecture Rules

## 1. Clean Code & POCO
- **Decoupling**: 인게임 비즈니스 로직, 계산, 상태 관리는 `MonoBehaviour`를 상속받지 않는 일반 C# 클래스(POCO)로 작성합니다.
- **No Singleton**: 싱글톤(Singleton) 패턴은 절대 사용하지 마십시오.
- **ISP & DIP**: 구체적인 클래스 구현보다 인터페이스(예: `IMovable`)에 의존하도록 설계하십시오.

## 2. 수동 의존성 주입 (Pure DI)
- **생성자 주입 (Constructor Injection)**: 의존성을 필요로 하는 모든 순수 C# 클래스는 필드 주입이나 프로퍼티 주입 대신, 생성자를 통해 필요한 객체나 인터페이스를 명시적으로 전달받아야 합니다.
- **컴포지션 루트 (Composition Root)**: 씬이 시작될 때 단 하나의 MonoBehaviour 스크립트(예: `SceneInitializer` 혹은 `ProjectContext`)가 모든 매니저, 뷰모델, 컨트롤러의 인스턴스를 올바른 라이프사이클 순서에 맞게 생성하고 조립하여 의존성을 해소해 줍니다. 씬 내부의 다른 컴포넌트들이 `new`를 남발하거나 서로를 직접 찾지 않도록 제약합니다.

## 3. UI Architecture (MVVM)
- **Model**: View나 Unity 엔진에 대해 전혀 모르는 순수 데이터 클래스로 작성합니다. 비즈니스 도메인 데이터와 규칙만을 담으며, UI 표현 방식에 대한 어떠한 정보도 포함하지 않습니다.
- **ViewModel**: Model의 데이터를 View가 사용하기 좋은 형태로 가공하여 제공하며, View가 구독할 상태(State/Action)와 명령(Command)을 가집니다. ViewModel은 Model을 직접 참조하여 데이터를 읽고 가공할 수 있으나, View에 대해서는 전혀 알지 못합니다. View에서 발생한 사용자 입력은 ViewModel의 Command(public 메서드)를 호출하여 처리합니다.
- **View**: `MonoBehaviour`를 상속하며, 오직 데이터 바인딩(시각화)과 입력 전달만 수행해야 합니다.
- **[절대 금지] View → Model 직접 참조**: View는 어떠한 경우에도 Model 클래스를 직접 참조(import, 필드 선언, 캐스팅 등)해서는 안 됩니다. 모든 데이터와 명령은 오직 ViewModel을 통해서만 교환해야 합니다. View는 ViewModel의 상태 변경 이벤트(`Action`)를 구독하여 UI를 갱신합니다. View에서 사용자 입력이 발생하면 직접 로직을 실행하지 않고 ViewModel의 Command 메서드를 호출합니다.
- **데이터 흐름 (단방향)**:
  `[사용자 입력] → View → ViewModel.Command() → Model 갱신 → UI 갱신 ← View ← ViewModel.StateChanged`
  이 단방향 흐름을 반드시 준수하며, View가 Model을 직접 읽거나 수정하는 양방향 흐름은 엄격히 금지합니다.

## 4. 외부 아키텍처 라이브러리 및 에셋 사용 금지 (Strict)
- **VContainer, Zenject 등 DI 프레임워크 금지**: 외부 의존성 주입 프레임워크를 절대 사용하지 말고, 생성자를 통한 순수 수동 주입(Pure DI)만을 사용합니다.
- **UniTask 등 외부 비동기/유틸리티 라이브러리 금지**: `UniTask`나 외부 비동기 라이브러리를 배제하고, 반드시 Unity 6의 네이티브 `Awaitable`을 활용한 비동기 프로그래밍을 수행하십시오.
- **아키텍처 프레임워크 도입 금지**: MVVM, MVC 등을 돕는 타사 툴이나 아키텍처 프레임워크(예: UniRx, 타사 MVVM 라이브러리)의 의존성을 배제하고 순수 C# 기반의 자체 구조로 구현하십시오.
