🎮 BePex Unity Client

Unity Clean Code & Design Patterns 기반의 확장 가능한 퀘스트(이벤트) 센터 시스템 및 관리자 프로그램

본 프로젝트는 이벤트 데이터의 생성, 추적, 보상 지급, 저장에 이르는 전체 라이프사이클을 단방향 데이터 흐름(MVVM)과 순수 의존성 주입(Pure DI)으로 제어하는 클라이언트 시스템입니다.

📑 목차

1. 프로젝트 정보
2. 시스템 설명
3. 새로운 기능 추가 방법 (확장 가이드)
4. 설계 설명
5. 작업 시간 (Time Log)
6. AI 사용 내역 (AI Usage)
7. 성능 최적화 포인트

## 1. 프로젝트 정보

- **Unity 버전**: 6.3.16f1

로컬 세이브 연동 : 어드민과 인게임 앱은 persistentDataPath를 공유하여 로컬 파일 기반으로 데이터를 상호 교환합니다.

🏃‍♂️ 씬(Scene) 실행 방법

① 인게임 이벤트 센터 (Inagme.unity)

Assets/_Game/Scenes/Inagme.unity 씬을 열고 Play(▶) 합니다.

💡 테스트/디버그 모드: [System]/EventSceneInitializer 오브젝트 인스펙터에서 m_useDebugMode를 체크하면 우측 하단에 치트 조작 패널(EventDebugView)이 활성화됩니다.

② 이벤트 관리자 어드민 (EventAdminScene.unity)

Assets/_Game/Scenes/EventAdminScene.unity 씬을 열고 Play(▶) 합니다.

⚠️ 주의사항: 좌측 [+ 신규 이벤트]에서 이벤트를 추가/편집 후 [로컬 파일 저장]을 누르면 로컬 JSON이 즉시 변경됩니다. [Firebase 서버 배포] 클릭 시 비동기 업로드(1.5초 지연 시뮬레이션)가 진행됩니다.

📦 Standalone 분리 빌드 (빌드 자동화)

Unity 에디터 상단 메뉴 BePex > Build 를 통해 씬별 단독 실행형 바이너리를 자동 빌드할 수 있습니다.

Build Admin Standalone : Builds/Admin/ 폴더에 어드민 바이너리 생성

Build Ingame Standalone : 어드레서블 자산 최신화 후 Builds/Ingame/ 폴더에 인게임 바이너리 생성

Build All Standalone : 어드민 ➔ 인게임 순차 자동 빌드
(※ 빌드 파이프라인에서 Company Name과 Product Name을 강제 동기화하여 세이브 파일 공유를 보장합니다.)

## 2. 시스템 설명

### 2.1 전체 시스템 구조 설명

전역 싱글톤을 배제하고, 각 씬의 Composition Root(Initializer)가 씬 로드 시 도메인, 뷰모델, 서비스를 한 번만 생성자 주입하여 결합도를 낮춥니다.

flowchart TD
    subgraph Unity_Admin_Scene ["1. 이벤트 관리자 씬 (EventAdminScene)"]
        A1[EventAdminView] -->|바인딩/명령| A2[EventAdminViewModel]
        A2 -->|전략 주입| A3[MockFirebaseUploadService]
        A2 -->|로컬 I/O| A4[event_table.json 생성]
    end

    subgraph Unity_Runtime_Client ["2. 라이브 게임 클라이언트 (Inagme)"]
        C1[EventSceneInitializer] -->|Addressables 로드| C2[event_table.json 역직렬화]
        C2 -->|생성자 주입| C3[EventModel 도메인]
        C3 -->|Action 바인딩| C4[EventListView / EventDetailView]
    end

    A4 -.->|Addressables 배포| C1


### 2.2 주요 클래스 역할 설명

과제 평가 규정(`readme_rules.md`)에서 요구하는 핵심 책임 요소들이 본 프로젝트에서 어떻게 구현되었는지 매핑한 명세 표입니다:

| 요구 명칭 | 실제 구현 클래스 | 주요 역할 및 설명 |
| :--- | :--- | :--- |
| **EventManager** | `EventModel` / `EventListViewModel` | 전체 이벤트 데이터의 로드, 진행 상태, 조건 체크 및 보상 수령 여부를 제어하고 상태 변화를 전파하는 핵심 중재 도메인 모델. |
| **EventTracker** | `BaseQuestCondition` 계열 (`IQuestCondition`) | 각 이벤트/퀘스트 인스턴스의 달성 조건을 관리하고 현재 진행 수치를 검증 및 추적하는 전략 구현체. |
| **RewardSystem** | `QuestRewardFactory` / `PlayerRewardModel` | 보상 획득 여부를 감지하고, 팩토리를 통해 보상 인스턴스를 생성한 후 플레이어 재화 및 자산 데이터를 적립시키는 시스템. |
| **AttendanceEvent** | `AttendanceQuestCondition` | 24시간 일일 날짜 대조 가드 로직을 내장하여 일일 1회만 카운트가 제한되도록 추적하는 출석 체크 전담 조건 전략. |
| **MissionEvent** | `StandardQuestCondition` | 적 처치(KillCount), 스테이지 클리어(StageClear) 등 행동 값의 단순 누적 및 한계치 비교를 담당하는 공용 범용 조건 전략. |
| **EventPointSystem** | `PlayerRewardModel` 내 `AddCurrency` API | 스테이지 클리어 및 광고 시청 등을 통해 이벤트 포인트를 획득하고, 이벤트 목록 내에서 보상과 교환하도록 HUD와 딕셔너리로 관리하는 재화 상태 시스템. |
| **SaveSystem** | `JsonSaveSystem` / `CachedSaveSystem` | 플레이어 진행 상황 및 획득한 보상 재화 DTO를 로컬 파일 디스크 I/O 기반으로 영속 저장 및 복구하는 입출력 장치. |
| **EventUI** | `EventListView` / `EventDetailView` / `CurrencyHUDView` | View-ViewModel 데이터 바인딩(Action 이벤트를 통한 UI 갱신 및 Command 메서드 호출)을 따르는 MVVM 기반 UI 뷰 컴포넌트군. |

### 2.3 계층별 주요 클래스 역할

🗄️ Model / DTO: EventModel, EventProgressModel, PlayerRewardModel(딕셔너리 POCO 캡슐화), EventTableSO 등 순수 데이터와 비즈니스 로직.

🧠 ViewModel: EventListViewModel, CurrencyHUDViewModel 등 뷰와 모델 사이의 명령/상태 중개자.

🖥️ View: EventListView, RewardPopupView 등 UI 렌더링 및 입력 처리.

🎯 Condition (조건 전략): BaseQuestCondition, StandardQuestCondition 등 목표 달성 판정 로직.

🎁 Reward (보상 전략): BaseQuestReward, GeneralQuestReward 등 보상 지급 실행 로직.

🔌 Interfaces (규격): ISaveSystem, ITimeProvider, IFirebaseUploadService 등 결합도를 낮추는 통일 규격.

🏗️ Infrastructure: EventSceneInitializer(컴포지션 루트), CachedSaveSystem(비동기 IO 락 제어).

🛠️ Factories / Utils: QuestConditionFactory, ItemSpriteMapper 등 동적 객체 생성(리플렉션 및 폴백 매핑) 지원.

✍️ Editor (에디터 툴): EventExtensionWindow 등 개발 생산성을 돕는 데이터 자동 생성 툴바 윈도우.

## 3. 새로운 기능 추가 방법 (확장 가이드)

본 시스템은 데이터 기반(Data-driven) 팩토리 폴백 구조를 채택하여, 단순 수치 비교 로직이나 가산형 보상은 C# 코딩 없이 에디터 조작만으로 확장이 가능합니다.

3.1 새로운 이벤트(조건) 타입 추가

에디터 툴 위치: 상단 메뉴 Tools > BePex > 이벤트 시스템 확장 도구

✅ [권장] 단순 카운트 조건 (C# 코딩 ❌)

확장 대상 이벤트 타입 선택 ➔ 식별자 영문명(예: LoginCount) 입력.

"C# 파일 추가 생성" 토글 OFF 후 실행.

결과: 데이터 에셋만 자동 등록되며, 런타임에 팩토리가 범용 StandardQuestCondition으로 자동 라우팅하여 처리합니다.

⚠️ 특수 가드 로직 필요 시 (C# 코딩 ⭕️)

위 설정에서 "C# 파일 추가 생성" 토글 ON 후 실행.

자동 생성된 *QuestCondition.cs의 CanAddProgress를 오버라이드하여 시간 비교 등의 특수 로직을 직접 구현합니다.

3.2 새로운 보상 타입 추가

✅ [권장] 단순 가산형 재화 추가 (C# 코딩 ❌)

확장 대상 보상 타입 선택 ➔ 식별자 영문명(예: Ruby) 입력.

"C# 파일 추가 생성" 토글 OFF 후 실행.

결과: 팩토리가 범용 GeneralQuestReward를 매핑하여 PlayerRewardModel.AddCurrency("Ruby", 수량)을 안전하게 실행합니다.

⚠️ 특수 연출 / 우편함 연동 필요 시 (C# 코딩 ⭕️)

토글 ON으로 C# 파일을 생성한 후 Grant() 메서드 내부에 외부 API 호출이나 커스텀 연출 로직을 작성합니다.

3.3 인게임 행동에 따른 '이벤트 포인트' 획득 연동 (예시)

인게임 컨트롤러(예: StageManager)에서 주입받은 PlayerRewardModel을 호출하기만 하면, ViewModel이 이를 감지하여 상단 HUD UI까지 자동으로 리액티브하게 갱신됩니다.

public class StageManager : MonoBehaviour
{
    private PlayerRewardModel m_playerRewardModel; // DI 주입됨

    public void func_OnStageClear(int stageId)
    {
        int rewardPoint = 15; // 획득 포인트 산출
        
        // 모델에 재화 가산 (ViewModel을 통해 CurrencyHUDView 자동 갱신됨)
        if (m_playerRewardModel != null)
        {
            m_playerRewardModel.AddCurrency("EventPoint", rewardPoint);
        }
    }
}


## 4. 설계 설명

### 4.1 설계 시 고려 사항
- **확장성 및 OCP (개방 폐쇄 원칙)**:
  - 새로운 이벤트 조건 및 보상 타입이 계속해서 추가되더라도 기존 비즈니스 코드가 수정되지 않도록 리플렉션 기반의 팩토리 시스템(`QuestConditionFactory`, `QuestRewardFactory`)을 설계했습니다.
  - 특수 가드 로직이 필요 없는 단순 수치 비교의 경우, 추가 파일 생성 없이 에셋 파일(`*TypeSO`) 등록만으로 팩토리가 폴백 매핑(`StandardQuestCondition`, `GeneralQuestReward`)하도록 구현했습니다.
- **관심사 분리 (SoC) 및 MVVM 준수**:
  - UI 뷰(`MonoBehaviour`)는 오직 데이터 바인딩(UI 갱신) 및 유저 클릭 전달 역할만 수행합니다. 비즈니스 도메인(`EventModel`, `PlayerRewardModel`)과 세이브 I/O는 순수 C# POCO 클래스로 격리하여 게임 로직의 유지보수성을 극대화했습니다.
- **데이터와 로직의 엄격한 분리**:
  - 상태 데이터(`EventTableDTO`, `EventProgressModel`)는 어떤 판정 로직도 직접 내포하지 않는 순수 DTO 형태를 유지합니다. 판정 및 실행에 대한 구동 로직은 전략 패턴의 조건/보상 핸들러(`BaseQuestCondition` 등)로 분리되어 독립적으로 관리됩니다.
- **서버 기능 확장성 고려**:
  - 로컬 I/O 인터페이스(`ISaveSystem`)와 클라우드 업로드 인터페이스(`IFirebaseUploadService`)를 공용 규격으로 정의함으로써, 추후 실제 백엔드 서버 DB(Firebase Realtime DB, REST API 서버 등)로 연동 대상을 손쉽게 교체할 수 있도록 지연 결합(DIP) 설계되었습니다.

### 4.2 현재 구조의 한계와 개선 방향
- **씬 전환 시 의존성 전달의 한계 (Pure DI)**:
  - 현재는 각 씬에 배치된 `EventSceneInitializer`와 `EventAdminSceneInitializer`가 각기 독자적인 DI 컴포지션 루트 역할을 수행합니다. 이에 따라 씬을 넘나들며 인스턴스(예: 플레이어 재화 정보)를 전달할 때 전역 클래스(싱글톤)가 배제된 구조에서는 인스턴스 전파에 제약이 발생합니다.
  - **개선 방향**: `VContainer`를 도입하여 글로벌 수명 주기(`ProjectLifetimeScope`) 상에 영구 인스턴스를 바인딩하고, 씬 전환 시 `LifetimeScope`를 참조 주입받아 의존성을 유연하게 상속받을 수 있도록 변경할 예정입니다.
- **Addressables 리소스 적용 시점의 제약**:
  - 현재는 어드민 갱신 후 에디터 빌드 시점에 에셋을 내장하고 있습니다. 런타임 상에서 실시간으로 데이터 테이블을 서버로부터 핫리로드하거나 다운로드받는 동적 패치가 배제되어 있습니다.
  - **개선 방향**: 인게임 로드 시점에 Addressables 로컬 에셋을 강제 호출하는 흐름에서 탈피하고, `Addressables.UpdateCatalogAsync` 및 원격 서버 버킷(AWS S3 등)을 런타임에 동적으로 탐색하여 실제 라이브 서비스에서 가능한 핫픽스 패치 구조로 변경을 제안합니다.

---

## 5. 작업 시간 (Time Log)

실제 각 단계별 작업 소요 시간을 명세합니다:
- **총 작업 시간**: 48.5시간
  - 설계 및 문서화: 12.0시간
  - 이벤트 시스템 로직 구현: 15.5시간
  - UI 및 연동: 10.0시간
  - README 및 최종 보완: 11.0시간 (Newtonsoft.Json 현대화, 비동기 세이브 및 핫패치 검증 포함)

---

## 6. AI 사용 내역 (AI Usage)

- **사용한 AI 도구**: Antigravity Agent (Opus 4.8 / Gemini / Claude 계열)
- **사용 범위**:
  - Newtonsoft.Json 도입에 따른 `PlayerRewardModel` 딕셔너리 직접 직렬화 전환 및 `[OnDeserialized]` 하위 호환 복구 마이그레이션 설계.
  - `CachedSaveSystem` 내 Awaitable detached state 크래시 버그 방지를 위한 HashSet 프레임 대기 기작 아이디어 차용.
  - C# 보일러플레이트 클래스를 줄이는 팩토리 폴백(Fallback) 라우팅 로직 정교화.
  - 지수 백오프 기반 `RetrySaveSystemDecorator` 설계 및 DI 바인딩 구조 검토.
- **검증 방법**:
  - AI가 제안한 로직은 즉시 반영하지 않고, 유니티 테스트 러너(Unity Test Runner)의 EditMode(24종) 및 PlayMode(6종) 총 30개의 단위/통합 테스트 시나리오를 100% Passed 완료하여 논리적 무결성을 교차 검증했습니다.

---

## 7. 성능 최적화 포인트

- **Zero-Allocation (GC 억제 최적화)**:
  - 매 프레임/갱신 시 리스트를 신규 할당하던 구조를 폐기하고, 캐시 버퍼(IReadOnlyList)를 재사용하는 `GetActiveEventsNonAlloc` 구조를 전면 도입하여 런타임 GC 스파이크를 원천 차단했습니다.
  - 잦은 `DateTime.TryParse` 파싱 오버헤드를 막기 위해 DTO 내부에 Nullable 캐싱 필드를 적용했습니다.
- **비동기 동시성 (Awaitable Detached State) 크래시 해결**:
  - Unity 6 Awaitable의 단 1회 await 제약(Detached State 예외)을 방지하기 위해, HashSet 기반 락(Lock) 상태 관리와 프레임 슬롯 폴링 방식을 고안하여 비동기 세이브의 안정성을 확보했습니다.
  - UI의 `async void` 진입점에 `try-catch(OperationCanceledException)` 래핑을 적용해 씬 소멸 시 발생하는 메모리 누수 및 예외 출력을 차단했습니다.
- **원격 예외 회복력 확보 (Decorator Pattern)**:
  - `ISaveSystem`을 데코레이터 패턴으로 감싼 `RetrySaveSystemDecorator`를 DI 조립하여, 저장 실패 시 지수 백오프(Exponential Backoff) 재시도를 통해 입출력 안정성을 높였습니다.