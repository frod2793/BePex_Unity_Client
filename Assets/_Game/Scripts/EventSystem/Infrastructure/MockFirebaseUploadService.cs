using UnityEngine;
using BePex.EventSystem.Interfaces;
using BePex.EventSystem.DTOs;

namespace BePex.EventSystem.Infrastructure
{
    /// <summary>
    /// [기능]: 테스트 환경 및 로컬 개발용 Firebase 업로드 모사(Mock) 구현 클래스.
    /// [작성자]: 윤승종
    /// </summary>
    public class MockFirebaseUploadService : IFirebaseUploadService
    {
        #region 공개 메서드
        /// <summary>
        /// [기능]: JSON 데이터를 인코딩하여 Firebase 가상 엔드포인트 업로드 로그를 출력합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-16
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: EditMode 유닛 테스트 정지 현상 방지를 위해 플레이모드 상에서만 딜레이 대기 실행하도록 가드 적용
        /// </summary>
        public async Awaitable<bool> UploadEventTableAsync(EventTableDTO tableDTO)
        {
            Debug.Log("[MockFirebaseUploadService] Firebase 업로드 요청 수락됨.");
            
            if (tableDTO == null)
            {
                Debug.LogError("[MockFirebaseUploadService] 테이블 DTO가 널입니다.");
                return false;
            }

            string jsonText = JsonUtility.ToJson(tableDTO, true);
            Debug.Log($"[MockFirebaseUploadService] 변환된 JSON 파일 본문:\n{jsonText}");

            // 1.5초 대기 시뮬레이션 (플레이모드 구동 중에만 실행하며, 에디터 및 EditMode 유닛 테스트 환경에서는 대기를 생략하여 정지 현상 방지)
            if (Application.isPlaying)
            {
                await Awaitable.WaitForSecondsAsync(1.5f);
            }

            Debug.Log("[MockFirebaseUploadService] Firebase 서버 업로드 완료! (가상 성공)");
            return true;
        }
        #endregion
    }
}
