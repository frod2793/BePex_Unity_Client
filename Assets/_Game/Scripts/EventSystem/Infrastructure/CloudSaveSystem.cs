using UnityEngine;
using UnityEngine.Networking;
using BePex.EventSystem.Interfaces;
using BePex.EventSystem.Models;
using System.Text;

namespace BePex.EventSystem.Infrastructure
{
    /// <summary>
    /// [기능]: REST API 통신망을 통해 백엔드 서버(PlayFab/Firebase)와 세이브 데이터를 비동기로 교환하는 저장소.
    /// [작성자]: 윤승종
    /// </summary>
    public class CloudSaveSystem : ISaveSystem
    {
        #region 내부 필드
        // 실제 운영 환경에서는 초기화 시점에 외부에서 주입받거나 Config에서 읽어야 합니다.
        private readonly string m_endpointUrl = "https://api.bepex-server.com/v1";
        private readonly string m_userId;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 서버 연동을 위한 식별자(UserId)를 입력받아 초기화합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public CloudSaveSystem(string userId = "guest_001")
        {
            m_userId = userId;
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 특정 이벤트 ID의 진행 상태를 서버에서 비동기로 조회합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public async Awaitable<EventProgressModel> LoadProgressAsync(string eventId)
        {
            string url = $"{m_endpointUrl}/progress/{m_userId}/{eventId}";
            using (UnityWebRequest req = UnityWebRequest.Get(url))
            {
                await req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    return JsonUtility.FromJson<EventProgressModel>(req.downloadHandler.text);
                }
                
                Debug.LogWarning($"[CloudSaveSystem] 이벤트 {eventId} 진행도 로드 실패. 빈 인스턴스를 반환합니다. Error: {req.error}");
                return new EventProgressModel { eventId = eventId };
            }
        }

        /// <summary>
        /// [기능]: 특정 이벤트의 진행 상태를 서버에 비동기로 전송(저장)합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public async Awaitable SaveProgressAsync(string eventId, EventProgressModel progress)
        {
            string url = $"{m_endpointUrl}/progress/{m_userId}/{eventId}";
            string json = JsonUtility.ToJson(progress);
            
            using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");

                await req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[CloudSaveSystem] 이벤트 {eventId} 저장 실패. Error: {req.error}");
                }
            }
        }

        /// <summary>
        /// [기능]: 플레이어의 누적 보상 현황을 서버에서 비동기로 조회합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public async Awaitable<PlayerRewardModel> LoadRewardStateAsync()
        {
            string url = $"{m_endpointUrl}/rewards/{m_userId}";
            using (UnityWebRequest req = UnityWebRequest.Get(url))
            {
                await req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    return JsonUtility.FromJson<PlayerRewardModel>(req.downloadHandler.text);
                }

                Debug.LogWarning($"[CloudSaveSystem] 플레이어 보상 상태 로드 실패. 빈 인스턴스를 반환합니다. Error: {req.error}");
                return new PlayerRewardModel();
            }
        }

        /// <summary>
        /// [기능]: 플레이어의 누적 보상 현황을 서버에 비동기로 전송(저장)합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public async Awaitable SaveRewardStateAsync(PlayerRewardModel rewardState)
        {
            string url = $"{m_endpointUrl}/rewards/{m_userId}";
            string json = JsonUtility.ToJson(rewardState);
            
            using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");

                await req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[CloudSaveSystem] 보상 상태 저장 실패. Error: {req.error}");
                }
            }
        }

        /// <summary>
        /// [기능]: 서버의 모든 사용자 관련 세이브 데이터를 초기화합니다. (테스트용)
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public async Awaitable ClearAllAsync()
        {
            string url = $"{m_endpointUrl}/clear/{m_userId}";
            using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
            {
                req.downloadHandler = new DownloadHandlerBuffer();
                await req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[CloudSaveSystem] 초기화 실패. Error: {req.error}");
                }
                else
                {
                    Debug.Log($"[CloudSaveSystem] 유저 {m_userId} 데이터 초기화 완료.");
                }
            }
        }
        #endregion
    }
}
