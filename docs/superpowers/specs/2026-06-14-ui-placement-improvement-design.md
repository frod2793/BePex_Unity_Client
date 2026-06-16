# EventAdminScene UI 배치 개선 설계 문서 (UI Placement Design Spec)

> **작성자**: 윤승종
> **작성일**: 2026-06-14
> **대상**: Assets/_Game/Scenes/EventAdminScene.unity
> **목적**: EventAdminScene의 UI 요소들이 씬 내에서 어긋나고 겹치는 문제를 분석하고, 직관적이고 미려한 상용 수준의 반응형 2열 레이아웃(또는 스크롤 뷰 레이아웃)을 구성하기 위한 디자인 상세를 명세합니다.

---

## 1. 현재 UI 배치 현황 및 문제 분석

현재 `EventAdminScene` 하이어라키에서 UI 구성요소는 다음과 같이 부모 자식 관계를 맺고 있으나, 일부 그룹이 적절치 못한 부모 밑에 들어가 앵커/피벗 조작으로 씬 상에 억지로 배치되어 있습니다.

```
UI_Canvas (Canvas)
└── EventAdminView (RectTransform)
    ├── Sidebar_Panel (RectTransform) - 이벤트 리스트 (가로 360, Stretch Height, Left 정렬)
    ├── Detail_Panel (RectTransform) - 우측 편집 패널 (Stretch Width/Height, Offset Left 410)
    │   └── Meta_Group (Image) - 이벤트 정보 편집 (ID, Title, Description 등)
    ├── Condition_Group (Image) - 달성 조건 편집 (달성 조건 추가/수정) (잘못된 부모: EventAdminView)
    ├── Reward_Group (RectTransform) - 보상 설정 편집 (보상 추가/수정) (잘못된 부모: EventAdminView)
    └── ControlBar_Panel (HorizontalLayoutGroup) - 하단 버튼 바 (X: 200, Y: 40)
```

### 1.1 주요 문제점
1. **잘못된 부모 배치로 인한 겹침 현상**: `Condition_Group`과 `Reward_Group`이 우측 상세 영역을 담당하는 `Detail_Panel`이 아닌, 최상위 `EventAdminView` 바로 밑에 위치하고 있습니다. 이로 인해 앵커가 중앙(0.5, 0.5)으로 고정된 채 오프셋(X: 701.5, Y: -130.66)으로 강제 이동되어 있어 화면 크기 변화 시 두 패널이 서로 겹치거나 화면 밖으로 탈출하는 치명적인 반응형 결함이 존재합니다.
2. **세로 길이 오버플로우 문제**: 
   - `Meta_Group` (높이 390px)
   - `Condition_Group` (높이 261px)
   - `Reward_Group` (높이 427px)
   - 이들을 우측 영역에 세로로 모두 직렬 배치할 경우 총 높이가 **1,078px**에 달하여, 일반적인 16:9 1080p 화면 세로 높이(1040px 가용 영역)를 초과하게 됩니다. 스크롤 뷰가 부재하여 하단부 UI가 잘릴 가능성이 큽니다.
3. **가로 영역 낭비**: `Detail_Panel`은 가로 1,500px에 달하는 매우 넓은 영역입니다. 이 거대한 공간에 세로로 한 열로만 UI를 길게 배치하는 것은 시각적으로 불균형하며 가로 공간을 비효율적으로 낭비하게 됩니다.

---

## 2. 레이아웃 개선 제안

### 방안 A: 반응형 2열 분할 레이아웃 (강력 추천)
넓은 가로 영역(1,500px)을 적극적으로 활용하여 겹침을 해결하고 스크롤 없이도 한눈에 모든 편집 요소를 가시화하는 방식입니다.

```
[ Detail_Panel (가로 1,500px, 세로 1,040px) ]
┌───────────────────────────────┬──────────────────────────────────┐
│  좌측 열 (가로 520px)           │  우측 열 (가로 920px)            │
│  - Meta_Group (이벤트 메타)     │  - Reward_Group (보상 목록)      │
│    (가로 517px, 세로 390px)     │    (가로 903px, 세로 427px)      │
│                               │                                  │
│  - Condition_Group (조건)      │                                  │
│    (가로 517px, 세로 261px)     │                                  │
└───────────────────────────────┴──────────────────────────────────┘
```

- **구조 설계**:
  - `Condition_Group`과 `Reward_Group`을 `Detail_Panel` 하위로 이동(Reparenting).
  - `Detail_Panel` 하위에 좌측 컨테이너(`Left_Column`)와 우측 컨테이너(`Right_Column`) 생성.
  - `Left_Column`: 앵커 Left-Stretch, Width 520, X Pivot 0. `Meta_Group`과 `Condition_Group`을 세로로 차례로 배치 (Vertical Layout Group 활용).
  - `Right_Column`: 앵커 Right-Stretch, Width 920, X Pivot 1. `Reward_Group`을 배치하고 상단에 맞춤.
- **장점**: 스크롤이 불필요하며 관리자 화면의 시인성이 대폭 강화됩니다. 16:9 와이드 화면 최적화.

### 방안 B: 세로 스크롤 뷰 레이아웃
모든 기기 해상도 및 다양한 세로 비율에서도 완벽하게 UI가 잘리지 않고 보존되도록 1열 스크롤 뷰를 장착하는 방식입니다.

- **구조 설계**:
  - `Detail_Panel` 내부에 `Scroll View` 오브젝트를 생성 (Stretch Anchor).
  - `Scroll View/Viewport/Content` 자식으로 `Meta_Group`, `Condition_Group`, `Reward_Group`을 순서대로 이동.
  - `Content`에 `Vertical Layout Group` (Spacing 20, Padding 20) 및 `Content Size Fitter` (Vertical Fit: Preferred Size)를 추가하여 세 개의 패널이 세로로 자동 스태킹 및 스크롤되도록 조절.
- **장점**: 보상 목록이나 조건 목록의 행 수가 대폭 늘어나도 완벽하게 스크롤되어 레이아웃이 절대 깨지지 않습니다.

---

## 3. 구현 방식 (Editor Automation Script)

씬 파일(`.unity`)을 텍스트로 직접 수정하면 GUID 충돌이나 직렬화 손상 위험이 높으므로, 유니티 에디터 상에서 실시간으로 RectTransform 컴포넌트를 정확하게 재구성할 수 있도록 **C# Editor Script 유틸리티**(`EventAdminUISetup.cs`)를 제작하여 런타임/에디터 모두에서 안전하게 정렬을 적용할 수 있도록 설계합니다.

### 3.1 Editor Script 구현 내용 (`EventAdminUISetup.cs`)
- **기능**: 에디터 메뉴 `Tools/Event System/Align UI Layout`을 클릭하거나 컴포넌트의 Context Menu 버튼을 눌러 실행.
- **동작**:
  1. `EventAdminView` 하위에서 `Sidebar_Panel`, `Detail_Panel`, `Condition_Group`, `Reward_Group`, `ControlBar_Panel`을 탐색.
  2. `Condition_Group`과 `Reward_Group`을 `Detail_Panel` 하위로 Reparenting 처리.
  3. **방안 A (2열 레이아웃)**를 기준으로 하위 서브 패널들의 RectTransform(Anchors, Pivot, sizeDelta, anchoredPosition)을 수학적으로 엄격하게 재조정하여 고해상도 반응형 레이아웃 구현.
  4. 수정된 씬 상태를 `EditorUtility.SetDirty` 및 `Undo.RegisterFullObjectHierarchyUndo`를 통해 기록하여 언제든 Undo가 가능하도록 안정성 제공.
