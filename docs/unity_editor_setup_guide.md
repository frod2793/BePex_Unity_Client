# Unity Editor Setup Guide (이벤트 시스템 에디터 설정 가이드)

> **작성자**: 윤승종  
> **작성일**: 2026-06-14  
> **대상 프로젝트**: BePex Unity Client  

본 문서는 `Pure DI`와 `MVVM` 아키텍처가 적용된 이벤트 센터 시스템을 Unity 씬에 배치하고, 오브젝트 계층(Hierarchy) 및 필수 컴포넌트 컴포지션을 안전하게 설정하기 위한 공식 가이드라인입니다.

---

## 1. 오브젝트 계층 구조 (Hierarchy Structure)

씬 내부의 UI 캔버스 구성과 초기화 컨트롤러의 계층적 배치는 다음과 같이 구성합니다.

```
Scene Root
 ├── [System] (GameObject)
 │    └── EventSceneInitializer (Component)
 │
 ├── EventSystem_Canvas (Canvas)
 │    ├── EventListView (GameObject / RectTransform)
 │    │    └── ScrollView
 │    │         └── Viewport
 │    │              └── Content (Transform -> 셀 생성 컨테이너)
 │    │
 │    ├── EventDetailView (GameObject / RectTransform)
 │    │    ├── Title_Text (TextMeshProUGUI)
 │    │    ├── Desc_Text (TextMeshProUGUI)
 │    │    ├── Progress_Text (TextMeshProUGUI)
 │    │    ├── Progress_Slider (Slider)
 │    │    └── Claim_Button (Button)
 │    │
 │    └── RewardPopupView (GameObject / RectTransform)
 │         └── Popup_Root (GameObject -> 보상 연출 팝업 루트)
 │              ├── Exp_Text (TextMeshProUGUI)
 │              ├── Ticket_Text (TextMeshProUGUI)
 │              ├── Point_Text (TextMeshProUGUI)
 │              └── Close_Button (Button)
 │
 └── [UI_Debug] (GameObject - 디버그 조작용 에디터 전용 그룹, 기본 비활성화 상태로 배치)
      └── EventDebugView (GameObject / RectTransform)
           ├── EventId_InputField (InputField -> 시뮬레이션 타겟 ID 입력)
           ├── Amount_InputField (InputField -> 가산할 수치 입력)
           ├── AddProgress_Button (Button -> 진행도 가산 실행)
           └── ResetSave_Button (Button -> 세이브 데이터 초기화 실행)
```

*테스트 및 디버깅 검증 시*, 메인 씬에 기본 비활성화 상태로 배치된 디버그 패널(`EventDebugView`)을 `EventSceneInitializer` 컴포지션 루트의 `m_debugView` 슬롯에 연결하고 `m_useDebugMode` 옵션을 활성화합니다. 런타임 시 자동으로 오브젝트가 액티브(Active)화되어 수동 DI 조립을 수행합니다.

---

## 2. 필수 컴포넌트 및 SerializeField 설정 명세

각 컴포넌트의 인스펙터(Inspector)에서 반드시 할당해야 하는 참조 필드 세부 항목입니다.

### 2.1 EventSceneInitializer (Composition Root)
*   **역할**: 씬 로딩 시 수동으로 의존성을 해소하고 MVVM 레이어를 조립합니다. 디버그 모드가 켜져 있을 때 모의 저장소 주입 및 디버그 바인딩을 겸임합니다.
*   **인스펙터 바인딩 테이블**:
    | 필드명 | 대상 타입 | 설명 |
    | :--- | :--- | :--- |
    | `m_eventListView` | `EventListView` | 씬 내의 EventListView 컴포넌트 할당 |
    | `m_eventDetailView` | `EventDetailView` | 씬 내의 EventDetailView 컴포넌트 할당 |
    | `m_rewardPopupView` | `RewardPopupView` | 씬 내의 RewardPopupView 컴포넌트 할당 |
    | `m_debugView` | `EventDebugView` | (선택 사항) 디버그용 패널 조작 뷰 연결 |
    | `m_useDebugMode` | `bool` | 체크 시 테스트용 격리 인메모리 저장소 가동 및 디버그 패널 노출 활성화 |
    | `m_eventTable` | `EventTableSO` | 인게임에 로드될 활성 이벤트 데이터 테이블 에셋 지정 |

---

### 2.2 EventListView
*   **역할**: 활성화된 이벤트들의 목록을 뷰모델로부터 수집하여 스크롤 뷰 내에 프리팹 셀로 동적 생성합니다.
*   **인스펙터 바인딩 테이블**:
    | 필드명 | 대상 타입 | 설명 |
    | :--- | :--- | :--- |
    | `m_cellContainer` | `Transform` | 동적 생성된 셀들이 차일드로 정렬될 Content 트랜스폼 지정 |
    | `m_cellPrefab` | `GameObject` | 개별 셀의 화면 디자인이 담긴 `EventItemCell` 프리팹 에셋 연결 |

---

### 2.3 EventItemCell (Prefab)
*   **역할**: 리스트를 구성하는 단일 아이템 항목의 텍스트와 아이콘을 표출하고, 선택 이벤트를 중개합니다.
*   **인스펙터 바인딩 테이블**:
    | 필드명 | 대상 타입 | 설명 |
    | :--- | :--- | :--- |
    | `m_titleText` | `TMPro.TextMeshProUGUI` | 이벤트 제목을 표시할 텍스트 컴포넌트 연결 |
    | `m_iconImage` | `UnityEngine.UI.Image` | 이벤트 썸네일 이미지를 그릴 이미지 컴포넌트 연결 |
    | `m_selectButton` | `UnityEngine.UI.Button` | 해당 셀의 전체 영역을 감싸는 클릭 버튼 컴포넌트 연결 |

---

### 2.4 EventDetailView
*   **역할**: 선택된 특정 이벤트의 달성 목표, 현재 상태 게이지, 보상 획득 가능 유무 시각화 및 수령 명령을 전달합니다.
*   **인스펙터 바인딩 테이블**:
    | 필드명 | 대상 타입 | 설명 |
    | :--- | :--- | :--- |
    | `m_titleText` | `TMPro.TextMeshProUGUI` | 이벤트 타이틀용 텍스트 지정 |
    | `m_descText` | `TMPro.TextMeshProUGUI` | 상세 설명글을 표출할 텍스트 지정 |
    | `m_progressText` | `TMPro.TextMeshProUGUI` | 현재 수치 / 목표치 수치 (예: 5 / 10)를 보여줄 텍스트 지정 |
    | `m_progressSlider` | `UnityEngine.UI.Slider` | 진척 비율(0.0~1.0)에 따라 채워질 슬라이더 바 지정 |
    | `m_claimButton` | `UnityEngine.UI.Button` | 달성 시 활성화되는 보상 청구 버튼 연결 |

---

### 2.5 RewardPopupView
*   **역할**: 보상 획득 완료 사실을 팝업 형태로 안내하고 플레이어의 총 자산 누적치를 드로우합니다.
*   **인스펙터 바인딩 테이블**:
    | 필드명 | 대상 타입 | 설명 |
    | :--- | :--- | :--- |
    | `m_popupRoot` | `GameObject` | 팝업 전체 창의 Active 상태를 온/오프할 최상위 Parent 오브젝트 연결 |
    | `m_expText` | `TMPro.TextMeshProUGUI` | 플레이어의 누적 획득 경험치(totalExp) 노출용 텍스트 |
    | `m_ticketText` | `TMPro.TextMeshProUGUI` | 플레이어의 누적 획득 티켓(totalTickets) 노출용 텍스트 |
    | `m_pointText` | `TMPro.TextMeshProUGUI` | 플레이어의 누적 획득 포인트(totalPoints) 노출용 텍스트 |
    | `m_closeButton` | `UnityEngine.UI.Button` | 팝업 창을 비활성화하는 [닫기] 버튼 연결 |

---

## 3. 주의사항 및 컴파일 안전 규칙
1.  **Unity Fake Null 회피**:
    본 시스템의 모든 View 컴포넌트는 `UnityEngine.Object` 파생 클래스의 널 검사 시 `?.` 연산자를 일절 사용하지 않고 `if (m_titleText != null)` 과 같은 명시적 조건 체크를 활용합니다. 인스펙터에서 참조 할당이 누락되었을 경우에도 씬 실행 중 `NullReferenceException`이 터지지 않고 경고 로그만 남도록 안전 조치되어 있습니다.
2.  **자동 이벤트 바인딩**:
    에디터의 UI Button 컴포넌트 내에 존재하는 `OnClick()` 인스펙터 슬롯에 개발자가 수동으로 스크립트 메서드를 등록할 필요가 없습니다. 각 View의 `Bind()` 시점에 C# 리스너 코드가 실행되어 뷰모델의 명령을 자동 바인딩합니다.

---

## 4. 통합 디버그 시뮬레이션 설정 가이드 (Integration Debug Guide)

인게임 단일 씬 내에서 이벤트 시스템을 100% 독립 시뮬레이션하기 위한 에디터 바인딩 및 작동 가이드라인입니다.

### 4.1 에셋 할당 및 디버그 모드 작동 단계
1.  **디버그 데이터 설정**:
    `EventSceneInitializer` 컴포넌트 인스펙터의 `m_eventTable` 슬롯에 상용 테이블 에셋 대신 모의 테이블 데이터인 **Mock_EventTable.asset**을 할당합니다.
2.  **디버그 모드 플래그 활성화**:
    `m_useDebugMode` 토글을 `True`로 체크합니다.
3.  **디버그 UI 활성화 및 드래그 바인딩**:
    씬 내 비활성화되어 있는 `[UI_Debug]` 그룹 오브젝트 하단의 `EventDebugView` 컴포넌트를 `EventSceneInitializer` 인스펙터의 `m_debugView` 슬롯에 연결합니다. 씬 시작 시 디버그 UI가 자동으로 활성화(Activate)되고 메모리 저장 장치(`InMemorySaveSystem`)가 수동 DI 주입됩니다.

### 4.2 EventDebugView 필수 컴포넌트 바인딩 테이블
*   **역할**: 테스트 상황에서 목표치 가산 입력 및 인게임 데이터 강제 초기화를 시뮬레이션합니다.
*   **인스펙터 바인딩 테이블**:
    | 필드명 | 대상 타입 | 설명 |
    | :--- | :--- | :--- |
    | `m_eventIdInput` | `TMPro.TMP_InputField` | 시뮬레이션 대상 이벤트의 고유 문자 ID(예: `test_event_001`)를 입력할 InputField |
    | `m_amountInput` | `TMPro.TMP_InputField` | 더해줄 진척도 양(예: `1`, `5` 등)을 입력할 InputField |
    | `m_addProgressButton` | `UnityEngine.UI.Button` | 클릭 시 수치를 인가하는 [진행도 가산] 버튼 연결 |
    | `m_resetButton` | `UnityEngine.UI.Button` | 클릭 시 세이브 데이터 딕셔너리를 공백으로 리셋하는 [세이브 초기화] 버튼 연결 |
