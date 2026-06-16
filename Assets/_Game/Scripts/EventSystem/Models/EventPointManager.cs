/// <summary>
/// [기능]: PlayerRewardModel 내의 이벤트 포인트("Point") 문자열 키 조작부를 안전하게 캡슐화하여 
///         오타 및 데이터 결합성을 방지하는 순수 C# 비즈니스 레이어 포인트 관리 클래스.
/// [작성자]: 윤승종
/// </summary>

using BePex.EventSystem.Models;

namespace BePex.EventSystem.Models
{
    public class EventPointManager
    {
        #region 내부 상수
        public const string POINT_KEY = "Point";
        #endregion

        #region 내부 필드
        private readonly PlayerRewardModel m_playerReward;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 포인트 관리를 대행할 플레이어 보상 데이터 모델을 주입받아 초기화합니다.
        /// [작성자]: 윤승종
        /// </summary>
        public EventPointManager(PlayerRewardModel playerReward)
        {
            m_playerReward = playerReward;
        }
        #endregion

        #region 공개 비즈니스 메서드
        /// <summary>
        /// [기능]: 지정 수량만큼 안전하게 포인트를 획득 및 가산합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        public void AddPoint(int amount)
        {
            if (m_playerReward != null && amount > 0)
            {
                m_playerReward.AddCurrency(POINT_KEY, amount);
            }
        }

        /// <summary>
        /// [기능]: 잔액을 검증한 뒤 지정 수량만큼 포인트를 차감 소모합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        public bool TrySpendPoint(int amount)
        {
            if (m_playerReward != null && amount > 0)
            {
                return m_playerReward.TrySpendCurrency(POINT_KEY, amount);
            }
            return false;
        }

        /// <summary>
        /// [기능]: 현재 적립되어 있는 잔여 포인트를 조회합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        public int GetPointBalance()
        {
            if (m_playerReward != null)
            {
                return m_playerReward.totalPoints;
            }
            return 0;
        }
        #endregion
    }
}
