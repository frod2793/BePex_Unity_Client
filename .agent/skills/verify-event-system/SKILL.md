---
name: verify-event-system
description: EventSystem 뷰모델의 메모리 관리(IDisposable) 및 어셈블리 정의 파일(.asmdef) 존재 여부 검증. EventSystem의 ViewModel 수정 또는 테스트 환경 구축 후 사용.
---

# EventSystem 검증

## Purpose
1. **메모리 누수 방지 검증**: EventSystem의 ViewModel들이 `IDisposable` 인터페이스를 상속하고, `OnDestroy` 등의 시점에 이벤트를 안전하게 해제하는지 확인합니다.
2. **어셈블리 정의 검증**: PlayMode 및 Editor 테스트를 정상적으로 실행하기 위해 메인 스크립트와 테스트 폴더 내에 `.asmdef` 파일이 올바르게 존재하는지 확인합니다.
3. **이벤트 등록(Action) 안전성 검증**: 이벤트를 구독(`+=`)한 ViewModel은 반드시 해제(`-=`)하도록 규정합니다.

## When to Run
- EventSystem의 ViewModel(`*ViewModel.cs`)을 생성하거나 수정했을 때
- EventSystem 관련 새로운 테스트 케이스(`*Tests.cs`)를 작성하거나 PlayMode 테스트 환경을 구성했을 때
- EventSystem의 메모리 누수나 이벤트 중복 트리거 버그가 의심될 때

## Related Files

| File | Purpose |
|------|---------|
| `Assets/_Game/Scripts/EventSystem/ViewModels/EventListViewModel.cs` | 이벤트 구독 및 IDisposable 검증 대상 |
| `Assets/_Game/Scripts/EventSystem/ViewModels/EventDetailViewModel.cs` | 이벤트 구독 및 IDisposable 검증 대상 |
| `Assets/_Game/Scripts/BePex.EventSystem.asmdef` | 메인 코드 어셈블리 정의 파일 검증 대상 |
| `Assets/_Game/Tests/PlayMode/PlayModeTests.asmdef` | PlayMode 테스트 어셈블리 정의 파일 검증 대상 |

## Workflow

### Step 1: ViewModel의 IDisposable 구현 확인
**도구:** `grep`
**경로:** `Assets/_Game/Scripts/EventSystem/ViewModels/*ViewModel.cs`
**검사:** ViewModel 클래스가 IDisposable을 구현하고, 내부에 `Dispose()` 메서드를 포함하는지 확인합니다.
```bash
grep -n "IDisposable" Assets/_Game/Scripts/EventSystem/ViewModels/*ViewModel.cs
grep -n "public void Dispose" Assets/_Game/Scripts/EventSystem/ViewModels/*ViewModel.cs
```
**PASS 기준:** 이벤트(Action 등)를 구독하는 모든 ViewModel 클래스에 `IDisposable` 인터페이스 선언과 `Dispose()` 구현부가 확인되어야 합니다.
**FAIL 기준:** `IDisposable` 구현이나 `Dispose` 메서드가 누락된 경우.
**수정 방법:** 해당 ViewModel 클래스 정의에 `: IDisposable`을 추가하고 `public void Dispose()`를 구현하여 구독한 이벤트를 `-=`로 안전하게 해제하도록 수정하세요.

### Step 2: 어셈블리 정의 파일(.asmdef) 존재 확인
**도구:** `ls`
**경로:** `Assets/_Game/Scripts/`, `Assets/_Game/Tests/PlayMode/`
**검사:** 유니티 Test Runner가 PlayMode 테스트를 정상 구동하기 위해 필요한 `.asmdef` 파일이 존재하는지 확인합니다.
```bash
ls Assets/_Game/Scripts/*.asmdef
ls Assets/_Game/Tests/PlayMode/*.asmdef
```
**PASS 기준:** `BePex.EventSystem.asmdef`와 `PlayModeTests.asmdef`가 정상적으로 존재해야 합니다.
**FAIL 기준:** `.asmdef` 파일이 하나라도 누락되거나 에러가 발생하는 경우.
**수정 방법:** Unity 에디터 상에서 Assembly Definition 파일을 생성하거나, 수동으로 규격에 맞는 JSON 형식의 `.asmdef` 파일을 작성하여 배치하세요.

## Output Format

| 검사 항목 | 검사 대상 | 결과 (PASS/FAIL) | 이슈 내용 |
|---|---|---|---|
| ViewModel IDisposable 검증 | `EventListViewModel.cs` 등 | PASS | - |
| asmdef 파일 검증 | `BePex.EventSystem.asmdef` 등 | PASS | - |

## Exceptions
1. **이벤트를 구독하지 않는 단순 ViewModel**: 외부 Model의 Action 이벤트를 구독하지 않고 오직 내부 상태만 가지고 있거나 단순 조회 역할만 수행하는 ViewModel이라면 `IDisposable` 구현이 생략되어도 무방할 수 있습니다.
2. **Editor 스크립트**: `Editor` 폴더에 속하는 `EventExtensionWindow.cs` 등은 런타임에 인스턴스화되는 ViewModel이 아니므로 이 검사 대상에서 제외됩니다.
