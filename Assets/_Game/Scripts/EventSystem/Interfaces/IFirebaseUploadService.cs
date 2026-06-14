using UnityEngine;
using BePex.EventSystem.DTOs;

namespace BePex.EventSystem.Interfaces
{
    /// <summary>
    /// [기능]: 직렬화된 이벤트 JSON 데이터를 Firebase 서버로 안전하게 업로드하는 통신 인터페이스.
    /// [작성자]: 윤승종
    /// </summary>
    public interface IFirebaseUploadService
    {
        /// <summary>
        /// [기능]: 이벤트 DTO 데이터를 JSON 문자열로 변환하여 Firebase Storage 또는 Realtime DB에 비동기 업로드합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 정의
        /// </summary>
        Awaitable<bool> UploadEventTableAsync(EventTableDTO tableDTO);
    }
}
