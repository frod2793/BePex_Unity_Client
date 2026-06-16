# Coding Standards & Safety Rules

## 1. Naming & Formatting
- **Private Field**: `m_` 접두사 + camelCase (예: `m_playerData`)
- **Public Property/Method**: PascalCase (예: `PlayerLevel`)
- **Interface**: `I` 접두사 (예: `IViewModel`)
- **DTO Class**: `~DTO` 접미사 (예: `PlayerStatsDTO`)
- **UI Event Callbacks**: UI 버튼의 OnClick 이벤트 등에 연결되는 public 메서드는 반드시 `func_` 접두사를 사용합니다. (예: `public void func_OnStartButtonClick()`)
- **Brace & Indent**: 중괄호는 항상 줄 바꿈 후 작성하는 Allman Style을 엄격히 준수하며, 들여쓰기는 4개의 공백(Space)을 사용하십시오.
- **중괄호 생략 금지**: 모든 `if`, `else if`, `else`, `for`, `while` 문에는 로직이 단 한 줄이더라도 예외 없이 중괄호를 사용해야 합니다.

## 2. Unity Safety (Strict)
- **Fake Null 방지**: `UnityEngine.Object` 파생 타입에는 널 조건부 연산자(`?.`, `??`)를 절대 사용하지 마십시오. 널 체크는 반드시 `if (obj != null)` 구문을 명시적으로 사용하십시오.
- 직렬화 필드명 변경 시에는 데이터 유실을 막기 위해 `[FormerlySerializedAs("기존이름")]`을 추가하십시오.

## 3. Using Directives (using 지시문)
- 파일 최상단에 `using` 지시문을 명시하고, 본문 코드 내에서는 클래스 이름 등을 최대한 간결하게 호출하십시오.
- 자주 사용되는 정적 클래스는 `using static`을 활용하여 호출을 단축하십시오. (예: `using static UnityEngine.Mathf;`)
- 긴 제네릭 타입이나 중복되는 타입명은 `using` 별칭을 사용하여 가독성을 높이십시오.

## 4. 주석 및 리전 (#region)
- 모든 스크립트 최상단, 클래스 정의 바로 위에 작성자와 기능을 명시하는 XML 주석을 작성합니다.
  ```csharp
  /// <summary>
  /// [기능]: 이 클래스가 수행하는 주요 역할에 대해 간략히 기술합니다.
  /// [작성자]: 윤승종
  /// </summary>
  ```
- 모든 메서드에 대해 동작 설명과 작성/수정 이력을 포함하는 XML 주석을 작성합니다.
  ```csharp
  /// <summary>
  /// [기능]: 메서드의 동작을 상세히 설명합니다.
  /// [작성자]: 윤승종
  /// [수정 날짜]: YYYY-MM-DD
  /// [마지막 수정 작성자]: 윤승종
  /// [수정 내용]: 구체적인 수정 사항 요약
  /// </summary>
  ```
- `#region`을 적극적으로 사용하여 코드 섹션을 명확히 구분하고 구조화하십시오. 리전 이름은 한글로 작성합니다.
- 기존 코드에 이미 존재하는 주석이나 `#region` 블록은 절대 삭제하거나 훼손하지 말고 그대로 유지하십시오.

## 5. 로깅 및 성능 최적화
- **한글 로그 강제**: `Debug.Log` 등 모든 로그 메시지는 `[클래스명]`을 포함하여 한글로 작성하십시오.
- **루프 성능 최적화**: 빈번하게 호출되는 로직(`Update` 등)에서는 `foreach` 대신 반드시 `for` 루프를 사용하십시오.
- **Zero Allocation**: `Update` 루프 내에서 `new` 키워드, Boxing, LINQ 사용을 엄격히 금지합니다.
- **비동기 최적화**: 코루틴이나 외부 라이브러리(UniTask 등)의 사용을 금지하며, 반드시 Unity 6 네이티브 `Awaitable`을 활용하여 비동기를 구현하십시오.
