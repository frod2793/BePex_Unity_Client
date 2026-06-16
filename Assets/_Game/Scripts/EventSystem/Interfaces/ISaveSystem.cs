using UnityEngine;
using BePex.EventSystem.Models;
using System.Threading;

namespace BePex.EventSystem.Interfaces
{
    /// <summary>
    /// [기능]: 이벤트 진척도 정보 및 플레이어 보상 수령 상태를 로드/세이브하는 데이터 영속성 제어 인터페이스.
    /// [작성자]: 윤승종
    /// </summary>
    public interface ISaveSystem
    {
        /// <summary>
        /// [기능]: 특정 이벤트의 진행 상태를 파일/저장소로부터 비동기로 로드합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 취소 제어를 위한 CancellationToken 추가
        /// </summary>
        Awaitable<EventProgressModel> LoadProgressAsync(string eventId, CancellationToken cancellationToken = default);

        /// <summary>
        /// [기능]: 특정 이벤트의 진행 상태를 파일/저장소에 비동기로 저장합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 취소 제어를 위한 CancellationToken 추가
        /// </summary>
        Awaitable SaveProgressAsync(string eventId, EventProgressModel progress, CancellationToken cancellationToken = default);

        /// <summary>
        /// [기능]: 플레이어의 누적 보상 현황 및 획득 상태를 비동기로 로드합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 취소 제어를 위한 CancellationToken 추가
        /// </summary>
        Awaitable<PlayerRewardModel> LoadRewardStateAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// [기능]: 플레이어의 누적 보상 현황 및 획득 상태를 비동기로 저장합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 취소 제어를 위한 CancellationToken 추가
        /// </summary>
        Awaitable SaveRewardStateAsync(PlayerRewardModel rewardState, CancellationToken cancellationToken = default);

        /// <summary>
        /// [기능]: 저장소의 모든 세이브 데이터를 완전히 리셋(초기화)합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 취소 제어를 위한 CancellationToken 추가
        /// </summary>
        Awaitable ClearAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// [기능]: 이벤트의 진행 상태와 플레이어 자산 상태를 트랜잭션 단위로 일괄(Batch) 비동기 저장합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 취소 제어를 위한 CancellationToken 추가
        /// </summary>
        Awaitable SaveBatchAsync(string eventId, EventProgressModel progress, PlayerRewardModel rewardState, CancellationToken cancellationToken = default);
    }
}
