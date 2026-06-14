using System;

namespace BePex.EventSystem.DTOs
{
    /// <summary>
    /// [기능]: 씬 전환 및 세션 이동 시 유저 데이터나 설정 상태 정보를 매핑하여 전송하기 위한 DTO 클래스.
    /// [작성자]: 윤승종
    /// [수정 날짜]: 2026-06-14
    /// [마지막 수정 작성자]: 윤승종
    /// [수정 내용]: [Serializable] 어트리뷰트와 XML 주석의 배치 순서 교정
    /// </summary>
    [Serializable]
    public class EventSceneDTO
    {
        #region 데이터 멤버
        public string lobbyUserName;
        public int initialSceneCode;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 기본값을 명시적으로 세팅하기 위한 기본 생성자.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public EventSceneDTO()
        {
            lobbyUserName = string.Empty;
            initialSceneCode = 0;
        }
        #endregion
    }
}
