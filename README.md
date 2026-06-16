🎮 BePex Unity Client (이벤트 시스템 및 관리자 프로그램)

Unity Clean Code & Design Patterns 기반의 확장 가능한 퀘스트(이벤트) 센터 시스템 및 관리자 프로그램(EventAdminScene)

---

## 1. 📌 프로젝트 정보

- **Unity 버전**: 6.3.16f1
- **프로젝트 실행 방법**:
  - **① 인게임 이벤트 센터 씬**:
    - Unity 에디터에서 `Assets/_Game/Scenes/Inagme.unity`를 로드합니다.
    - 에디터 Play 버튼(▶)을 눌러 실행합니다.
    - *💡 테스트/디버그 모드*: `[System]/EventSceneInitializer` 오브젝트 인스펙터에서 `m_useDebugMode`를 활성화하고 실행하면, 우측 하단에 테스트 조작 패널(`EventDebugView`)이 활성화됩니다.
  - **② 이벤트 관리자 프로그램(어드민) 씬**:
    - Unity 에디터에서 `Assets/_Game/Scenes/EventAdminScene.unity` 씬을 로드합니다.
    - 에디터 Play 버튼(▶)을 클릭하면 `EventAdminSceneInitializer` 컴포지션 루트에 의해 데이터가 로드되고 UI 조작 패널이 구동됩니다.
    - *⚠️ 주의사항*: 좌측 `[+ 신규 이벤트]` 버튼으로 이벤트를 추가/편집한 후 `[로컬 파일 저장]`을 누르면 로컬 JSON 데이터가 변경되며, `[Firebase 서버 배포]`를 누르면 모의 서버로 데이터 업로드가 비동기(1.5초 대기 시뮬레이션)로 진행됩니다.
  - **③ Standalone 분리 빌드 방법 (빌드 자동화)**:
    - Unity 에디터 상단 메뉴 바의 **`BePex > Build`** 메뉴를 통해 각각의 씬을 단독 실행형 standalone 바이너리로 분리 빌드할 수 있습니다.
      - **`Build Admin Standalone`**: `Builds/Admin/` 폴더 아래 어드민 바이너리를 생성합니다.
      - **`Build Ingame Standalone`**: 어드레서블 자산을 최신화 빌드한 후 `Builds/Ingame/` 폴더 아래 인게임 바이너리를 생성합니다.
      - **`Build All Standalone`**: 어드민과 인게임 바이너리를 차례로 자동 순차 빌드합니다.
    - *⚠️ 연동 주의사항*: 두 개별 어플리케이션이 로컬 세이브 파일(`persistentDataPath`)을 완전 공유하며 상호 작용할 수 있도록 빌드 파이프라인 상에서 `Company Name`("BePex")과 `Product Name`("BePexEventClient") 설정을 강제 동기화시킵니다.

---

## 2. ⚙️ 시스템 설명

### 2.1 전체 시스템 구조 설명
본 프로젝트는 Pure DI (수동 의존성 주입) 및 MVVM 아키텍처를 기반으로 설계되어 전역 싱글톤을 배제하고 단방향 데이터 흐름을 준수합니다.

```mermaid
flowchart TD
    subgraph Unity_Admin_Scene ["1. 유니티 이벤트 관리자 씬 (EventAdminScene)"]
        A1[EventAdminView] -->|바인딩 및 명령| A2[EventAdminViewModel]
        A2 -->|전략 주입| A3[IFirebaseUploadService / MockFirebaseUploadService]
        A2 -->|로컬 I/O| A4[event_table.json 생성]
    end

    subgraph Unity_Runtime_Client ["2. 라이브 게임 클라이언트 (Inagme)"]
        C1[EventSceneInitializer] -->|Addressables: EventTableJson 로드| C2[event_table.json 역직렬화]
        C2 -->|생성자 주입| C3[EventModel 도메인 생성]
        C3 -->|데이터 바인딩| C4[EventListView / EventDetailView]
    end

    A4 -.->|Addressables 빌드/배포| C1
```

- **객체 라이프사이클 관리 명세**: 외부 DI 프레임워크가 배제된 상태에서, 각 씬에 배치된 `EventSceneInitializer` 및 `EventAdminSceneInitializer`가 컴포지션 루트(Composition Root) 역할을 수행합니다. 씬 로드 시 도메인 모델(POCO), 세이브 시스템, 뷰모델, 서비스 인스턴스를 순수 C# 생성자 주입 방식으로 한 번만 초기화하여 결합도를 낮추고 수명을 제어합니다.

### 2.2 주요 클래스 역할 설명

사전 과제 평가 규정(`readme_rules.md`)에서 요구하는 주요 표준 도메인 클래스 책임과 본 프로젝트의 실제 구현 클래스 간의 매핑 및 역할 정의는 다음과 같습니다:

| 규정 표준 명칭 | 실제 구현 클래스 / 구조 | 설명 및 주요 책임 |
| :--- | :--- | :--- |
| **EventManager** | `EventModel` / `EventListViewModel` | 전체 이벤트 데이터의 로드, 진행 상태, 조건 체크 및 보상 수령 여부를 제어하고 상태 변화를 전파하는 핵심 중재 도메인 모델. |
| **EventTracker** | `BaseQuestCondition` 계열 (`IQuestCondition`) | 각 이벤트/퀘스트 인스턴스의 달성 조건을 관리하고 현재 진행 수치를 검증 및 추적하는 전략 구현체. |
| **RewardSystem** | `QuestRewardFactory` / `PlayerRewardModel` | 보상 획득 여부를 감지하고, 팩토리를 통해 보상 인스턴스를 생성한 후 플레이어 재화 및 자산 데이터를 적립시키는 시스템. |
| **AttendanceEvent** | `AttendanceQuestCondition` | 24시간 일일 날짜 대조 가드 로직을 내장하여 일일 1회만 카운트가 제한되도록 추적하는 출석 체크 전담 조건 전략. |
| **MissionEvent** | `StandardQuestCondition` | 적 처치(KillCount), 스테이지 클리어(StageClear) 등 행동 값의 단순 누적 및 한계치 비교를 담당하는 공용 범용 조건 전략. |
| **EventPointSystem** | `PlayerRewardModel` 내 `AddCurrency` API | 스테이지 클리어 및 광고 시청 등을 통해 이벤트 포인트를 획득하고, 이벤트 목록 내에서 보상과 교환하도록 HUD와 딕셔너리로 관리하는 재화 상태 시스템. |
| **SaveSystem** | `JsonSaveSystem` / `CachedSaveSystem` | 플레이어 진행 상황 및 획득한 보상 재화 DTO를 로컬 파일 디스크 I/O 기반으로 영속 저장 및 복구하는 입출력 장치. |
| **EventUI** | `EventListView` / `EventDetailView` / `CurrencyHUDView` | View-ViewModel 데이터 바인딩(Action 이벤트를 통한 UI 갱신 및 Command 메서드 호출)을 따르는 MVVM 기반 UI 뷰 컴포넌트군. |

---

본 프로젝트(`Assets/_Game/Scripts/` 폴더 내)의 모든 C# 스크립트는 구체적 역할에 따라 아래의 아키텍처 계층으로 분류됩니다:

#### 🗄️ A. Model / Data / DTO 계층
- **EventModel**: 개별 이벤트 데이터(ID, 타입, 기간)와 달성 상태를 결합하여 상태 변화를 관리하는 비즈니스 도메인 모델.
- **EventProgressModel**: 유저의 각 이벤트 달성 진행도를 디스크에 저장/복원하기 위한 래퍼 데이터 모델.
- **QuestProgressModel**: 개별 퀘스트(미션)의 진행 수치, 완료 상태, 보상 수령 시점 등을 관리하는 POCO 데이터 모델.
- **PlayerRewardModel**: 플레이어가 획득한 화폐/자산 데이터를 딕셔너리(`Dictionary<string, int>`) 구조로 캡슐화하고 안전한 가산/차감 API 및 Newtonsoft.Json의 `[OnDeserialized]` 역직렬화 수명주기를 활용한 구버전 세이브 호환 마이그레이션 기작을 탑재한 상태 모델.
- **EventTableSO / EventDefinitionSO / ConditionDefinitionSO / RewardDefinitionSO / ConditionTypeSO / RewardTypeSO / ConditionTypeRegistrySO / RewardTypeRegistrySO**: 데이터 정의 및 에디터 직렬화, Type Object를 위한 스크립터블 오브젝트.
- **EventTableDTO**: 전체 이벤트 테이블의 JSON 직렬화용 데이터 전송 객체.

#### 🧠 B. ViewModel 계층
- **EventListViewModel**: 이벤트 목록의 활성화 상태와 선택된 이벤트 정보를 View에 바인딩.
- **EventDetailViewModel**: 특정 이벤트 상세 속성 및 보상 획득 명령을 중개.
- **RewardPopupViewModel**: 획득 보상 리스트를 팝업 시각화를 위해 가공.
- **CurrencyHUDViewModel**: 상단 HUD의 재화(골드, 포인트 등) 상태를 리액티브하게 갱신.
- **EventAdminViewModel**: 신규 이벤트 실시간 등록, 동적 추가, 저장 및 서버 배포 기능 관장.
- **EventDebugViewModel**: 디버그 오버레이에서 실시간 조건 트리거 및 리로드 명령을 연결.

#### 🖥️ C. View (UI 컴포넌트) 계층
- **EventListView / EventDetailView**: 이벤트 목록 스크롤 뷰 및 상세 명세, 게이지바, 버튼 렌더링.
- **RewardPopupView**: 보상 획득 시 팝업 및 DOTween 애니메이션 연출.
- **CurrencyHUDView**: 재화 레이아웃 렌더링 및 카운팅 연출 시각화.
- **EventItemCell / EventAdminQuestRowView**: 개별 이벤트 셀 및 어드민 내 그리드 형태의 편집/노출 행 컴포넌트.
- **EventAdminView**: 어드민 설정 페이지 전체 레이아웃 바인딩 및 이벤트 라우팅.
- **EventDebugView**: 디버그용 치트 및 리셋 패널 조작 HUD (Awaitable 기반 프레임 애니메이션 장착).

#### 🎯 D. Condition (조건/행동 전략) 계층
- **BaseQuestCondition**: 모든 이벤트 조건 클래스들의 추상 기본 구현체.
- **StandardQuestCondition**: 별도의 예외 가드가 필요 없는 일반적인 퀘스트 조건(단순 값 누적 비교)을 전담하는 공용 범용 조건 클래스.
- **AttendanceQuestCondition**: 오늘 이미 출석했는지 날짜 대조 가드 로직을 내포하여 일일 1회 카운트 제한을 전담하는 특수 조건 클래스.
- **QuestConditionAttribute**: 조건 클래스와 `ConditionTypeSO`의 식별자(`TypeName`) 문자열을 런타임에 동적으로 매핑시키는 커스텀 특성.

#### 🎁 E. Reward (보상 지급 전략) 계층
- **BaseQuestReward**: 보상 지급 명령을 추상화한 공통 추상 클래스.
- **GeneralQuestReward**: 보상 타입 식별자 문자열 키를 직접 전달받아 플레이어 보상 모델에 가산하는 공용 범용 보상 클래스.
- **QuestRewardAttribute**: `RewardTypeSO`의 식별자(`TypeName`) 문자열과 런타임 구현 클래스를 연결해주는 커스텀 어트리뷰트.

#### 🔌 F. Interfaces (통일 규격) 계층
- **ITimeProvider**: 시간(UTC 등)을 주입하기 위한 인터페이스.
- **ISaveSystem**: 데이터 로컬/클라우드 입출력을 위한 세이브/로드 규격.
- **IQuestCondition / IQuestReward**: 조건 판정 전략 및 보상 지급 실행 인터페이스.
- **IFirebaseUploadService**: 클라우드 서버 배포 기능 추상화 인터페이스.

#### 🏗️ G. Infrastructure (조립 및 입출력) 계층
- **EventSceneInitializer / EventAdminSceneInitializer**: 각 씬의 의존성 결합을 담당하는 컴포지션 루트.
- **MockFirebaseUploadService**: 로컬 가상 파일 업로드 시뮬레이터.
- **JsonSaveSystem / InMemorySaveSystem / CachedSaveSystem / RetrySaveSystemDecorator**: 다양한 형태의 세이브/로드 인프라 및 지수 백오프 기반 재시도 최적화 데코레이터.
- **SystemTimeProvider / DebugTimeProvider**: 시스템 시간 및 조작 가능한 테스트용 시간 구현체.

#### 🛠️ H. Factories / Utils (보조) 계층
- **QuestConditionFactory / QuestRewardFactory**: 리플렉션을 통해 구체 클래스를 인스턴싱하되, 매핑 클래스가 없을 경우 범용 클래스(`StandardQuestCondition`, `GeneralQuestReward`)로 자동 바인딩하는 폴백(Fallback)형 팩토리.
- **ItemSpriteMapper**: 보상 아이콘 드로잉을 위한 스프라이트 에셋 경로 맵퍼.

#### ✍️ I. Core / Editor (에디터 헬퍼) 계층
- **EventExtensionWindow**: 조건/보상 데이터 에셋을 생성 및 자동 등록하되, 필요한 경우에만 C# 클래스 보일러플레이트 파일을 생성하도록 조절하는 토글 토폴로지가 탑재된 에디터 윈도우.

---

## 3. 🚀 새로운 기능 추가 방법 (확장 가이드)

### 3.1 새로운 이벤트(조건) 타입 추가 방법
본 시스템은 데이터 기반(Data-driven) 팩토리 폴백 구조를 지니므로, **단순 수치 비교용 조건은 C# 코딩을 작성할 필요가 전혀 없습니다.**

#### [케이스 1] 단순 카운트 비교 조건 추가 (C# 작성 없음 - 권장 ⭐)
1. **에디터 도구 열기**: 상단 메뉴 `Tools > BePex > 이벤트 시스템 확장 도구`를 엽니다.
2. **속성 입력**: 확장 대상을 **이벤트 타입**으로 선택하고, 영문 식별자(예: `LoginCount`)와 표시 한글명(예: `로그인 횟수`)을 기재합니다.
3. **토글 끄기**: **"C# 클래스 파일 추가 생성 여부"** 토글을 **해제(False)** 상태로 둡니다.
4. **실행**: `[확장 파일 생성 및 등록]`을 클릭합니다.
   - `Assets/_Game/Data/ConditionTypes/LoginCount.asset` 데이터 에셋이 자동 생성되고 `ConditionTypeRegistry`에 즉시 등록됩니다.
   - 인게임 런타임 진입 시, 팩토리가 C# 스크립트 부재를 감지하고 범용 `StandardQuestCondition`으로 자동 변환해 조건 처리를 수납합니다.

#### [케이스 2] 특수한 비교 가드 로직이 요구되는 조건 추가 (C# 코드 필요)
1. **에디터 도구 열기**: 상단 메뉴 `Tools > BePex > 이벤트 시스템 확장 도구`를 엽니다.
2. **속성 입력**: 확장 대상을 **이벤트 타입**으로 선택하고, 식별자 영문명(예: `GuildMission`), 표시명 한글명(예: `길드 행동 미션`)을 기재합니다.
3. **토글 켜기**: **"C# 클래스 파일 추가 생성 여부"** 토글을 **체크(True)** 상태로 지정합니다.
4. **실행**: `[확장 파일 생성 및 등록]`을 클릭하면, SO 데이터 에셋 생성과 동시에 `Assets/_Game/Scripts/EventSystem/Conditions/GuildMissionQuestCondition.cs` C# 템플릿 파일이 생성됩니다.
5. **C# 구현**: 생성된 파일 내 `CanAddProgress` 등을 오버라이드하여 길드 시간 비교 등 특수 조건 가드를 작성합니다.

```csharp
[QuestCondition("GuildMission")]
public class GuildMissionQuestCondition : BaseQuestCondition
{
    public GuildMissionQuestCondition(int targetValue, ISaveSystem saveSystem, ITimeProvider timeProvider, string eventId, string questId)
        : base(targetValue, saveSystem, timeProvider, eventId, questId) { }

    public override bool CanAddProgress(Models.EventProgressModel progress)
    {
        // 커스텀 조건 가드 제어 연산 구현
        return true;
    }
}
```

---

### 3.2 새로운 보상 타입 추가 방법

조건 시스템과 동일하게, 플레이어의 자산 딕셔너리에 단순 가산 적립되는 보상은 **C# 코딩 없이 즉각 추가**할 수 있습니다.

#### [케이스 1] 단순 가산형 재화 보상 추가 (C# 작성 없음 - 권장 ⭐)
1. **에디터 도구 열기**: 상단 메뉴 `Tools > BePex > 이벤트 시스템 확장 도구`를 엽니다.
2. **속성 입력**: 확장 대상을 **보상 타입**으로 선택하고, 영문 식별자(예: `Ruby`)와 표시 한글명(예: `루비`)을 기재합니다.
3. **토글 끄기**: **"C# 클래스 파일 추가 생성 여부"** 토글을 **해제(False)** 상태로 둡니다.
4. **실행**: `[확장 파일 생성 및 등록]`을 클릭합니다.
   - `Assets/_Game/Data/RewardTypes/Ruby.asset` 데이터 에셋이 자동 생성되고 `RewardTypeRegistry`에 즉시 등록됩니다.
   - 런타임 보상 지급 시 팩토리는 범용 `GeneralQuestReward` 인스턴스를 동적으로 바인딩하여, `PlayerRewardModel.AddCurrency("Ruby", 수량)`을 실행해 자산 딕셔너리에 루비 재화를 안전하게 적립시킵니다.

#### [케이스 2] 특수한 연출이나 3rd Party 연동 등이 필요한 보상 추가 (C# 코드 필요)
1. **에디터 도구 열기**: 상단 메뉴 `Tools > BePex > 이벤트 시스템 확장 도구`를 엽니다.
2. **속성 및 토글 설정**: 확장 대상을 **보상 타입**으로 선택하고 식별자 기입 후 **"C# 클래스 파일 추가 생성 여부"**를 **체크(True)**하고 실행합니다.
3. **C# 구현**: 생성된 `*QuestReward.cs` 파일 내 `Grant` 메서드에서 우편함 REST API 호출이나 전용 가상 시뮬레이션을 작성합니다.

```csharp
[QuestReward("SpecialPackage")]
public class SpecialPackageQuestReward : BaseQuestReward
{
    public SpecialPackageQuestReward(int amount, string displayName) : base(amount, displayName) { }
    
    public override void Grant(PlayerRewardModel playerReward)
    {
        if (playerReward != null) 
        { 
            playerReward.AddCurrency("SpecialPackage", m_amount); 
            // 커스텀 특수 연출 및 외부 우편 서버 발송 연동 트리거
        }
    }
}
```

---

### 3.3 새로운 이벤트 포인트 획득 방식 추가 방법
이벤트 포인트(`EventPoint`)는 유저의 플레이 행동에 반응하여 `PlayerRewardModel`에 적립됩니다. 새로운 포인트 획득 경로(예: 스테이지 클리어 시 15포인트 적립 등)를 추가하려면 아래 흐름을 따릅니다:

#### 1 단계: 포인트 적립 인터페이스 정의 및 구현
유저 행동이 발생하는 인게임 컨트롤러 혹은 매니저 클래스에서 `PlayerRewardModel`의 인스턴스를 주입받은 뒤, 행동 보상으로서 포인트를 가산합니다.

```csharp
public class StageManager : MonoBehaviour
{
    private PlayerRewardModel m_playerRewardModel;

    // 생성자 혹은 DI 프레임워크(VContainer 등)를 통한 의존성 주입
    public void Initialize(PlayerRewardModel playerRewardModel)
    {
        m_playerRewardModel = playerRewardModel;
    }

    /// <summary>
    /// [기능]: 스테이지 클리어 시 호출되어 유저에게 이벤트 포인트를 지급합니다.
    /// [작성자]: 윤승종
    /// </summary>
    public void func_OnStageClear(int stageId)
    {
        // 1. 획득할 포인트 산출 (예: 스테이지당 15 포인트)
        int rewardPoint = 15;
        
        // 2. 플레이어 재화 모델에 직접 가산 (EventPoint 키)
        if (m_playerRewardModel != null)
        {
            m_playerRewardModel.AddCurrency("EventPoint", rewardPoint);
            Debug.Log($"[StageManager] 스테이지 클리어 보상 포인트 지급: {rewardPoint}");
        }
    }
}
```

#### 2 단계: UI 리액티브 갱신 바인딩
인게임 UI(`CurrencyHUDView`)는 `CurrencyHUDViewModel`을 통해 `PlayerRewardModel.OnBalancesChanged` 이벤트를 리액티브하게 구독 중이므로, 위 1단계에서 포인트를 가산하는 즉시 화면 상단 HUD의 포인트 보유량이 실시간으로 자동 갱신됩니다.

```csharp
// CurrencyHUDViewModel.cs의 일부
public CurrencyHUDViewModel(PlayerRewardModel rewardModel)
{
    m_rewardModel = rewardModel;
    // 포인트 변경 이벤트 구독 연동
    m_rewardModel.OnBalancesChanged += RefreshBalances;
}
```

---

## 4. 📐 설계 설명

### 설계 시 고려 사항 (Design Intentions)
- **확장성 및 OCP (개방 폐쇄 원칙)**:
  - 새로운 이벤트 조건 및 보상 타입이 계속해서 동적 추가되더라도 기존 비즈니스 코드가 수정되지 않도록 리플렉션 기반의 팩토리 시스템(`QuestConditionFactory`, `QuestRewardFactory`)을 갖추었습니다.
  - 특수 조건 가드가 필요 없는 경우, 추가 파일 생성 없이 에셋 파일(`*TypeSO`) 등록만으로 팩토리가 폴백 매핑(`StandardQuestCondition`, `GeneralQuestReward`)하도록 유연하게 구현하여 확장성을 높였습니다.
- **관심사 분리 (SoC) 및 MVVM 준수**:
  - 뷰(`MonoBehaviour`)는 데이터 바인딩(UI 갱신) 및 유저 클릭 전달 역할만 수행합니다. 비즈니스 도메인(`EventModel`, `PlayerRewardModel`)과 세이브 I/O는 순수 C# POCO 클래스로 격리하여 게임 로직의 유지보수성을 극대화하고 유니티 프레임워크 종속성을 완전히 낮췄습니다.
- **데이터와 로직의 엄격한 분리**:
  - 상태 데이터(`EventTableDTO`, `EventProgressModel`)는 어떤 인게임 판정 로직도 직접 내포하지 않는 순수 DTO 형태를 유지합니다. 판정 및 실행에 대한 구동 로직은 전략 패턴의 조건/보상 핸들러(`BaseQuestCondition` 등)로 명확히 이격시켰습니다.
- **서버 기능 확장성 고려**:
  - 로컬 I/O 인터페이스(`ISaveSystem`)와 클라우드 업로드 인터페이스(`IFirebaseUploadService`)를 공용 규격으로 정의함으로써, 추후 실제 백엔드 서버 DB(Firebase Realtime DB, REST API 서버 등)로 연동 대상을 손쉽게 교체할 수 있도록 지연 결합(DIP) 설계되었습니다.
- **Zero-Allocation 성능 최적화 전면 적용**:
  - `EventModel`에서 매 프레임 혹은 갱신 시마다 리스트를 신규 할당하던 `GetActiveEvents()`를 완전히 삭제(폐기)하고, 캐시 버퍼를 재사용하여 가비지 컬렉션(GC) 할당을 원천 차단했습니다.
- **날짜 문자열 파싱 오버헤드 제거 (DateTime Caching)**:
  - 이벤트 활성화 판정 시 매 프레임 혹은 이벤트 갱신 시점마다 발생하던 `DateTime.TryParse` 파싱 연산을 DTO 클래스 내의 Nullable 캐싱 필드로 대체하여 연산 부하를 차단했습니다.
- **CachedSaveSystem의 Awaitable Detached State 크래시 해결**:
  - 유니티 6.0+에서 풀링되는 `UnityEngine.Awaitable`의 단 1회 await 제약(detached state 예외)을 우회하기 위해, HashSet 기반의 비동기 락 상태 관리와 프레임 단위 슬롯 폴링 방식을 도입하여 비동기 동시성 크래시를 방지했습니다.

### 현재 구조의 한계와 개선 방향
- **씬 전환 시 의존성 전달의 한계 (Pure DI)**:
  - 현재는 각 씬에 배치된 `EventSceneInitializer`와 `EventAdminSceneInitializer`가 각기 독자적인 DI 컴포지션 루트 역할을 수행합니다. 이에 따라 씬을 넘나들며 인스턴스(예: 플레이어 재화 정보)를 전달할 때 전역 클래스(싱글톤)가 배제된 구조에서는 인스턴스 전파에 제약이 발생합니다.
  - **개선 제안**: `VContainer`를 도입하여 글로벌 생명 주기(`ProjectLifetimeScope`) 상에 영구 인스턴스를 바인딩하고, 씬 전환 시 `LifetimeScope`를 참조 주입받아 의존성을 유연하게 상속받을 수 있도록 변경할 예정입니다.
- **Addressables 리소스 적용 시점의 제약**:
  - 현재는 어드민에서 갱신한 로컬 JSON 파일 변경본을 인게임 씬 실행 전 에디터 빌드 시점에 어드레서블로 강제 빌드 및 내장하고 있습니다. 런타임 상에서 실시간으로 데이터 테이블을 서버로부터 핫리로드하거나 다운로드받는 동적 패치가 배제되어 있습니다.
  - **개선 제안**: 인게임 로드 시점에 Addressables 로컬 에셋을 강제 호출하는 흐름에서 탈피하고, `Addressables.UpdateCatalogAsync` 및 원격 서버 버킷(AWS S3 등)을 런타임에 동적으로 탐색하여 실제 라이브 서비스에서 가능한 핫픽스 패치 구조로 변경을 제안합니다.

---

## 5. ⏳ 작업 시간 (Time Log)

- **총 작업 시간**: 48.5시간
  - 설계 및 아키텍처 문서화: 12.0시간
  - 이벤트 시스템 로직 및 OCP 개편: 15.5시간
  - UI 연동 및 Awaitable 성능 개선: 10.0시간
  - Newtonsoft.Json 현대화 및 비동기 크래시 보완 테스트: 11.0시간

---

## 6. 🤖 AI 사용 내역 (AI Usage)

- **사용한 AI 도구**: Antigravity Agent (Opus 4.8 / Gemini / Claude 계열)
- **사용 범위**:
  - Newtonsoft.Json 도입에 따른 `PlayerRewardModel` 딕셔너리 직접 직렬화 전환 및 `[OnDeserialized]` 하위 호환 복구 마이그레이션 적용.
  - `EventProgressModel`의 questId 중복 검색 캡슐화(TryGetQuestProgress) 설계.
  - `overrideReferences: true` 환경의 테스트 어셈블리 `.asmdef` 내 `Newtonsoft.Json.dll` precompiledReferences 참조 오류(CS0246) 해소.
  - `CachedSaveSystem` 내 Awaitable detached state 크래시 버그 방지를 위한 HashSet 기반 프레임 대기 기작 적용.
  - `PlayerRewardModel` 딕셔너리 데이터 구조 OCP 리팩토링 및 `ISerializationCallbackReceiver` 직렬화 동기화 설계.
  - C# 깡통 클래스 12개 삭제에 대응하는 팩토리 폴백(Fallback) 라우팅 로직 개발.
  - `EventExtensionWindow` 소스 파일 선택적 빌드 제어 토글 추가.
  - 비동기 라이프사이클 누수 방지를 위한 CancellationToken 전파 및 Awaitable 뷰 예외 가드 구현.
  - `EventAdminViewModel` 클래스의 partial 구조 분할 설계.
  - 지수 백오프 기반 `RetrySaveSystemDecorator` 설계 및 DI 바인딩.
  - DTO 내부의 DateTime 파싱 캐싱 및 5종 재화 HUD 리액티브 동기화 연동.
- **검증 방법**:
  - 유니티 테스트 러너(Unity Test Runner) EditMode 내 24종 유닛 테스트 및 PlayMode 내 6종 통합 시나리오 테스트(총 30종) 100% Passed 완료 검증.