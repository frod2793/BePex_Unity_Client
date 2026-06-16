/// <summary>
/// [기능]: 플레이어 자산의 즉시 가산이 아닌 외부 시스템 연동을 시뮬레이션하기 위한 예시용 보상 전략 클래스.
/// [작성자]: 윤승종
/// </summary>

using UnityEngine;
using BePex.EventSystem.Interfaces;
using BePex.EventSystem.Models;

namespace BePex.EventSystem.Rewards
{
    [QuestReward("Sample")]
    public class SampleQuestReward : BaseQuestReward
    {
        #region 초기화
        /// <summary>
        /// [기능]: 지급할 보상 수량 및 표시 이름을 주입받아 초기화합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        public SampleQuestReward(int amount, string displayName) : base(amount, displayName)
        {
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 외부 플랫폼이나 연동 시스템에 보상 청구 요청을 가상으로 발행합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 시뮬레이션 로그 작성
        /// </summary>
        public override void Grant(PlayerRewardModel playerReward)
        {
            // 실제 PlayerRewardModel의 변수를 조작하지 않고 외부 시뮬레이션 로그 기록
            Debug.Log($"[SampleQuestReward] [외부 연동 성공] 보상명: {m_displayName}, 수량: {m_amount} 지급 완료.");
        }
        #endregion
    }
}
