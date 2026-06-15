🎮 BePex Unity Client (이벤트 시스템 및 관리자 프로그램)

Unity Clean Code & Design Patterns 기반의 확장 가능한 퀘스트(이벤트) 센터 시스템 및 관리자 프로그램(EventAdminScene)

1. 📌 프로젝트 정보

Unity 버전 : 6.3.16f1

프로젝트 실행 방법 :

① 인게임 이벤트 센터 씬

Unity 에디터에서 Assets/_Game/Scenes/SampleScene.unity를 로드합니다.

에디터 Play 버튼(▶)을 눌러 실행합니다.

💡 테스트/디버그 모드: [System]/EventSceneInitializer 오브젝트 인스펙터에서 m_useDebugMode를 활성화하고 실행하면, 우측 하단에 테스트 조작 패널(EventDebugView)이 활성화됩니다.

② 이벤트 관리자 프로그램(어드민) 씬

Unity 에디터에서 Assets/_Game/Scenes/EventAdminScene.unity 씬을 로드합니다.

에디터 Play 버튼(▶)을 클릭하면 EventAdminSceneInitializer 컴포지션 루트에 의해 데이터가 로드되고 UI 조작 패널이 구동됩니다.

⚠️ 주의사항: 좌측 [+ 신규 이벤트] 버튼으로 이벤트를 추가/편집한 후 [로컬 파일 저장]을 누르면 로컬 JSON 데이터가 변경되며, [Firebase 서버 배포]를 누르면 모의 서버로 데이터 업로드가 비동기(1.5초 대기 시뮬레이션)로 진행됩니다.

2. ⚙️ 시스템 설명

2.1 전체 시스템 구조 설명

본 프로젝트는 Pure DI (수동 의존성 주입) 및 MVVM 아키텍처를 기반으로 설계되어 전역 싱글톤을 배제하고 단방향 데이터 흐름을 준수합니다.

flowchart TD
    subgraph Unity_Admin_Scene ["1. 유니티 이벤트 관리자 씬 (EventAdminScene)"]
        A1[EventAdminView] -->|바인딩 및 명령| A2[EventAdminViewModel]
        A2 -->|전략 주입| A3[IFirebaseUploadService / MockFirebaseUploadService]
        A2 -->|로컬 I/O| A4[event_table.json 생성]
    end

    subgraph Unity_Runtime_Client ["2. 라이브 게임 클라이언트 (SampleScene)"]
        C1[EventSceneInitializer] -->|Addressables: EventTableJson 로드| C2[event_table.json 역직렬화]
        C2 -->|생성자 주입| C3[EventModel 도메인 생성]
        C3 -->|데이터 바인딩| C4[EventListView / EventDetailView]
    end

    A4 -.->|Addressables 빌드/배포| C1


객체 라이프사이클 관리 명세: 외부 DI 프레임워크가 배제된 상태에서, 각 씬에 배치된 EventSceneInitializer 및 EventAdminSceneInitializer가 컴포지션 루트(Composition Root) 역할을 수행합니다. 씬 로드 시 도메인 모델(POCO), 세이브 시스템, 뷰모델, 서비스 인스턴스를 순수 C# 생성자 주입 방식으로 한 번만 초기화하여 결합도를 낮추고 수명을 제어합니다.

2.2 주요 클래스 역할 설명

본 프로젝트(Assets/_Game/Scripts/ 폴더 내)의 모든 C# 스크립트는 역할에 따라 9개의 아키텍처 계층으로 분류됩니다.

🗄️ A. Model / Data / DTO 계층

EventModel: 개별 이벤트 데이터(ID, 타입, 기간)와 달성 상태를 결합하여 상태 변화를 관리하는 비즈니스 도메인 모델.

EventProgressModel: 유저의 각 이벤트 달성 진행도를 디스크에 저장/복원하기 위한 래퍼 데이터 모델.

QuestProgressModel: 개별 퀘스트(미션)의 진행 수치, 완료 상태, 보상 수령 시점 등을 관리하는 POCO 데이터 모델.

PlayerRewardModel: 플레이어가 획득한 화폐/자산 데이터를 캡슐화하고 안전한 가산/차감 API를 제공하는 상태 모델.

SeasonPassModel: 시즌패스의 기간, 포인트, 보상 테이블을 추적하는 시즌 패스 전용 데이터 모델.

EventTableSO / EventDefinitionSO / ConditionDefinitionSO / RewardDefinitionSO / SeasonPassDefinitionSO: 데이터 정의 및 에디터 직렬화를 위한 스크립터블 오브젝트.

EventTableDTO: 전체 이벤트 테이블의 JSON 직렬화용 데이터 전송 객체.

🧠 B. ViewModel 계층

EventListViewModel: 이벤트 목록의 활성화 상태와 선택된 이벤트 정보를 View에 바인딩.

EventDetailViewModel: 특정 이벤트 상세 속성 및 보상 획득 명령을 중개.

RewardPopupViewModel: 획득 보상 리스트를 팝업 시각화를 위해 가공.

CurrencyHUDViewModel: 상단 HUD의 재화(골드, 포인트 등) 상태를 리액티브하게 갱신.

EventAdminViewModel: 신규 이벤트 실시간 등록, 동적 추가, 저장 및 서버 배포 기능 관장.

EventDebugViewModel: 디버그 오버레이에서 실시간 조건 트리거 및 리로드 명령을 연결.

🖥️ C. View (UI 컴포넌트) 계층

EventListView / EventDetailView: 이벤트 목록 스크롤 뷰 및 상세 명세, 게이지바, 버튼 렌더링.

RewardPopupView: 보상 획득 시 팝업 및 DOTween 애니메이션 연출.

CurrencyHUDView: 재화 레이아웃 렌더링 및 카운팅 연출 시각화.

EventItemCell / EventAdminQuestRowView: 개별 이벤트 셀 및 어드민 내 그리드 형태의 편집/노출 행 컴포넌트.

EventAdminView: 어드민 설정 페이지 전체 레이아웃 바인딩 및 이벤트 라우팅.

EventDebugView: 디버그용 치트 및 리셋 패널 조작 HUD.

🎯 D. Condition (조건/행동 전략) 계층

BaseQuestCondition: 모든 이벤트 조건 클래스들의 추상 기본 구현체.

KillCountQuestCondition / StageClearQuestCondition / AttendanceQuestCondition: 적 처치, 스테이지 클리어, 출석 누적 조건 전략.

GuildQuestCondition / MonthQuestCondition / RankingQuestCondition / ADMobQuestCondition: 길드, 월간, 랭킹, 광고 시청 누적 조건 전략.

QuestConditionAttribute: 조건 클래스와 ConditionType 이넘을 런타임에 동적으로 매핑시키는 커스텀 특성.

🎁 E. Reward (보상 지급 전략) 계층

BaseQuestReward: 보상 지급 명령을 추상화한 공통 추상 클래스.

ExpQuestReward / TicketQuestReward / PointQuestReward / SeasonPointQuestReward / CreditQuestReward: 각종 재화 및 경험치, 티켓 지급 구현체.

QuestRewardAttribute: 보상 타입과 런타임 구현 클래스를 연결해주는 커스텀 어트리뷰트.

🔌 F. Interfaces (통일 규격) 계층

ITimeProvider: 시간(UTC 등)을 주입하기 위한 인터페이스.

ISaveSystem: 데이터 로컬/클라우드 입출력을 위한 세이브/로드 규격.

IQuestCondition / IQuestReward: 조건 판정 전략 및 보상 지급 실행 인터페이스.

IFirebaseUploadService: 클라우드 서버 배포 기능 추상화 인터페이스.

🏗️ G. Infrastructure (조립 및 입출력) 계층

EventSceneInitializer / EventAdminSceneInitializer: 각 씬의 의존성 결합을 담당하는 컴포지션 루트.

MockFirebaseUploadService: 로컬 가상 파일 업로드 시뮬레이터.

JsonSaveSystem / InMemorySaveSystem / CloudSaveSystem / CachedSaveSystem: 다양한 형태의 세이브/로드 인프라 및 최적화 데코레이터.

SystemTimeProvider / DebugTimeProvider: 시스템 시간 및 조작 가능한 테스트용 시간 구현체.

🛠️ H. Factories / Utils (보조) 계층

QuestConditionFactory / QuestRewardFactory: 리플렉션을 통해 알맞은 조건/보상 객체를 동적 인스턴싱하는 팩토리.

ItemSpriteMapper: 보상 아이콘 드로잉을 위한 스프라이트 에셋 경로 맵퍼.

EnumDisplayHelper: 이넘 필드의 한글 표시명을 추출하는 유틸리티.

✍️ I. Core / Attributes / Editor (에디터 헬퍼) 계층

SeasonPassManager: 시즌패스 레벨 및 보상 관리 코어 매니저.

EventExtensionWindow: 조건/보상 파일을 템플릿 형태로 자동 생성해 주는 에디터 윈도우.

EventDisplayNameAttribute / EventDebugViewSetup / CurrencyHUDViewSetup / RewardPopupViewSetup: 레이아웃 셋업 및 인스펙터 시각화 헬퍼들.

3. 🚀 새로운 기능 추가 방법 (확장 가이드)

3.1 새로운 이벤트(조건) 타입 추가 방법 (예: 길드 이벤트)

[방법 A] 에디터 자동화 도구 사용 (권장 ⭐)

Unity 상단 메뉴에서 Tools > BePex > 이벤트 시스템 확장 도구를 클릭합니다.

확장 대상을 이벤트 타입으로 선택합니다.

식별자 영문명에 GuildEvent, 표시명 한글명에 길드 이벤트를 입력 후 확장 파일 생성 및 등록을 클릭합니다.

Assets/_Game/Scripts/EventSystem/Conditions/GuildEventQuestCondition.cs가 자동 생성되면 세부 로직만 구현합니다.

[방법 B] 수동 수정 방법

ConditionDefinitionSO.cs 내의 ConditionType 이넘에 신규 타입을 추가합니다.

public enum ConditionType
{
    // ... 기존 타입
    [EventDisplayName("길드 이벤트")]
    GuildEvent
}


GuildQuestCondition.cs를 생성하고 [QuestCondition] 어트리뷰트를 장식하여 로직을 구현합니다.

[QuestCondition(ConditionDefinitionSO.ConditionType.GuildEvent)]
public class GuildQuestCondition : BaseQuestCondition
{
    public GuildQuestCondition(int targetValue, ISaveSystem saveSystem, ITimeProvider timeProvider, string eventId, string questId)
        : base(targetValue, saveSystem, timeProvider, eventId, questId) { }
}


3.2 새로운 보상 타입 추가 방법 (예: 시즌 포인트)

[방법 A] 에디터 자동화 도구 사용 (권장 ⭐)

상단 메뉴 Tools > BePex > 이벤트 시스템 확장 도구를 엽니다.

확장 대상을 보상 타입으로 선택합니다.

식별자 영문명에 SeasonPoint, 표시명 한글명에 시즌 포인트를 입력하고 실행합니다.

생성된 파일의 Grant 메서드에 재화 가산 로직을 작성합니다.

[방법 B] 수동 수정 방법

RewardDefinitionSO.cs 내의 RewardType 이넘에 타입을 명시합니다.

public enum RewardType
{
    // ... 기존 타입
    [EventDisplayName("시즌 포인트")]
    SeasonPoint
}


SeasonPointQuestReward.cs 클래스를 생성한 뒤 Grant 메서드를 작성합니다.

[QuestReward(RewardDefinitionSO.RewardType.SeasonPoint)]
public class SeasonPointQuestReward : BaseQuestReward
{
    public SeasonPointQuestReward(int amount, string displayName) : base(amount, displayName) { }
    
    public override void Grant(PlayerRewardModel playerReward)
    {
        if (playerReward != null) 
        { 
            playerReward.AddCurrency(RewardDefinitionSO.RewardType.SeasonPoint, m_amount); 
        }
    }
}


3.3 새로운 이벤트 포인트 획득 방식 추가 방법 (예: 광고 시청 포인트 획득)

로직 상 광고 시청 완료를 트리거하는 시점에 아래와 같이 AddCurrency를 호출하여 안전하게 세이브 파일을 갱신합니다.

public async Awaitable AddAdWatchPointAsync(int amount, ISaveSystem saveSystem, PlayerRewardModel playerReward)
{
    if (playerReward != null)
    {
        playerReward.AddCurrency(RewardDefinitionSO.RewardType.Point, amount);
        await saveSystem.SaveRewardStateAsync(playerReward);
        Debug.Log($"[EventPointSystem] 광고 시청 완료 보상 포인트 지급: +{amount}P (현재: {playerReward.GetBalances()["Point"]}P)");
    }
}


3.4 씬 UI 미배치 시 에디터 자동화 헬퍼 활용법

UI 오브젝트 누락이나 인스펙터 직렬화 필드가 null일 때 도구를 활용해 복구할 수 있습니다.

상단 HUD 레이아웃 셋업

Tools > BePex > Setup CurrencyHUDView Layout 클릭.

기존 HUD 구조를 기반으로 필요 그룹(SeasonPoint, Credit 등)을 자동 복제 및 정렬하고 참조를 강제 할당합니다.

보상 팝업 레이아웃 셋업

Tools > BePex > Setup RewardPopupView Layout 클릭.

레이아웃 색상 변경 및 프리팹 구조 자동 빌드, 리플렉션 바인딩을 수행합니다.

4. 📐 설계 설명

설계 시 고려 사항

확장성 (OCP) 극대화: 새로운 이벤트 및 보상 타입 정의 시 기존 팩토리 코드를 수정하지 않도록 리플렉션(Reflection) 및 어트리뷰트 기반 매핑 방식을 채택했습니다.

책임 분리 (SoC): View는 데이터 바인딩과 사용자 입력에만 관여하며, 비즈니스 도메인 규칙에 직접 접근하지 못하도록 원천 차단하여 결합도를 최소화했습니다.

서버 확장성 고려: IFirebaseUploadService 인터페이스로 백엔드 통신을 추상화했습니다. 모킹 환경에서 개발을 진행하다가 추후 실제 SDK 환경으로 손쉽게 전환 가능합니다.

현재 구조의 한계와 개선 방향

수동 DI의 한계: 수동으로 모든 의존성을 씬 초기화 시점에 조립해야 하므로, 프로젝트가 방대해질 경우 조립 레이어(Composition Root)가 길어집니다.

개선 방향: 추후 VContainer와 같은 DI 라이브러리를 도입하여 수명 주기 및 컨테이너 바인딩을 자동 관리하도록 개선할 예정입니다.

5. ⏳ 작업 시간 (Time Log)

총 작업 시간 : 32.5시간

9.0시간 : 설계 및 아키텍처 문서화

10.5시간 : 이벤트 시스템 로직 및 확장 도구 구현

7.0시간 : UI 연동 및 MVVM 구조 개선

6.0시간 : README 작성 및 최종 결함 보완 테스트

6. 🤖 AI 사용 내역 (AI Usage)

사용한 AI 도구: Antigravity Agent (Gemini 3.5 Flash / Gemini 3.1 Pro 계열)

사용 범위:

Pure DI 아키텍처 가이드라인 적용 및 MVVM 단방향 결합 검증.

리플렉션 팩토리 자동화에 대응하는 EventExtensionWindow 에디터 도구 개발.

정규식을 이용한 SO Enum 코드 주입 및 템플릿 C# 제네레이터 작성.

검증 방법:

유니티 내 컴파일 검증 도구(validate_script)를 통한 문법 무결성 검사.

유니티 테스트 러너(Unity Test Runner) EditMode 내 14가지 유닛 테스트 시나리오 통과를 통한 논리적 검증 완료.