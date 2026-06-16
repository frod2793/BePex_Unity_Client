# 이벤트 - 미션 계층형 구조 재설계 명세 (Event-Mission Hierarchy Design)

> **작성자**: 윤승종  
> **작성일**: 2026-06-16  

## 1. 개요 (Overview)
기존 "1 이벤트 = 1 달성 조건"의 단일 퀘스트 구조에서 발생하는 한계점(출석부, 다중 퀘스트 그룹핑 등의 표현 불가)을 극복하기 위해, **"1 이벤트 = N개의 독립적인 퀘스트(미션) 리스트"** 계층형 구조로 아키텍처를 전면 고도화합니다.

## 2. 도메인 모델 설계 (Data Models)

### AS-IS (기존 구조)
```csharp
public class EventDefinitionDTO {
    public string eventId;
    public string startDate;
    public string endDate;
    public ConditionDefinitionDTO condition; // 단일 퀘스트
    public List<RewardDefinitionDTO> rewards; // 단일 보상 묶음
}
```

### TO-BE (신규 계층형 구조)
이벤트는 기간(Period)과 메타데이터만 보유하며, 구체적인 조건과 보상은 개별 `MissionDefinitionDTO`로 이관됩니다.

```csharp
public class EventDefinitionDTO {
    public string eventId;
    public string eventTitle;
    public string eventDescription;
    public string eventIconAddress;
    public string startDate; // 기간 제한 공통
    public string endDate;
    
    // N개의 개별 미션 목록
    public List<MissionDefinitionDTO> missions = new List<MissionDefinitionDTO>(); 
}

public class MissionDefinitionDTO {
    public string missionId;
    public ConditionDefinitionDTO condition;      // 출석, 처치, 클리어 등 개별 조건
    public List<RewardDefinitionDTO> rewards;     // 해당 미션 달성 시의 개별 보상
}
```

## 3. 영속성 및 세이브 파일 구조 (Persistence)
세이브 파일(`EventProgressDTO`) 역시 개별 미션 단위로 진행도와 보상 수령 여부를 추적하도록 마이그레이션됩니다.

```csharp
public class EventSaveDataDTO {
    public string eventId;
    public List<MissionProgressDTO> missionProgresses;
}

public class MissionProgressDTO {
    public string missionId;
    public int currentProgress;
    public bool isCompleted;
    public bool isRewardClaimed;
}
```

## 4. 데이터 플로우 및 주요 변경점
1. **EventModel**: 이벤트의 진행도 가산 시, 해당 이벤트를 구성하는 모든 `Mission`들을 순회하며 타입이 일치하는 퀘스트에만 진행도를 배분합니다. (예: 킬수 퀘스트와 스테이지 클리어 퀘스트가 하나의 이벤트 안에 혼재 시 각각의 조건에 맞는 행동만 카운트 됨)
2. **RewardSystem**: 보상 수령 시, 이벤트 전체를 완료하는 것이 아니라 개별 `missionId` 단위로 보상 수령(`ClaimReward(eventId, missionId)`)이 이루어집니다.
3. **EventListView / EventDetailView**: UI에서는 최상단 이벤트 메타를 보여준 후, 아코디언 혹은 리스트 뷰 형태로 내부의 여러 미션 진행 바와 보상 수령 버튼을 복수로 노출해야 합니다.

## 5. 기존 설계서(`period_event.md`) 갱신 사항
- 기간 제한(`Start Date ~ End Date`)의 책임은 여전히 **최상위 이벤트(EventDefinitionDTO)**가 보유합니다.
- 기간이 만료될 경우, 해당 이벤트 산하에 있는 모든 미션(Mission)의 진행도 업데이트 및 보상 수령이 일괄적으로 차단됩니다.
