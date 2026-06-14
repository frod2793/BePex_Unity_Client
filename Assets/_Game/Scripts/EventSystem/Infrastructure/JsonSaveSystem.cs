using System.IO;
using UnityEngine;
using BePex.EventSystem.Interfaces;
using BePex.EventSystem.Models;

namespace BePex.EventSystem.Infrastructure
{
    /// <summary>
    /// [기능]: JSON 로컬 파일 시스템 기반 세이브 데이터 처리기.
    /// [작성자]: 윤승종
    /// </summary>
    public class JsonSaveSystem : ISaveSystem
    {
        #region 내부 필드
        private readonly string m_saveDir;
        #endregion

        #region 초기화
        /// <summary>
        /// [기능]: 생성자에서 persistentDataPath/save 디렉토리를 확인하고 없으면 신규 생성합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: 최초 작성
        /// </summary>
        public JsonSaveSystem()
        {
            m_saveDir = Path.Combine(Application.persistentDataPath, "save");
            if (Directory.Exists(m_saveDir) == false)
            {
                Directory.CreateDirectory(m_saveDir);
            }
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// [기능]: 특정 이벤트 ID에 해당하는 진행도 모델을 파일에서 비동기로 로드합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Awaitable 비동기 인터페이스로 갱신
        /// </summary>
        public async Awaitable<EventProgressModel> LoadProgressAsync(string eventId)
        {
            string path = Path.Combine(m_saveDir, $"event_progress_{eventId}.json");
            if (File.Exists(path) == false)
            {
                return new EventProgressModel { eventId = eventId };
            }
            
            await Awaitable.BackgroundThreadAsync(); // 파일 I/O 스레드 전환
            string json = File.ReadAllText(path);
            await Awaitable.MainThreadAsync();       // 메인 스레드 복귀
            
            return JsonUtility.FromJson<EventProgressModel>(json);
        }

        /// <summary>
        /// [기능]: 특정 이벤트의 진행 상태 정보를 JSON 포맷으로 비동기 저장합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Awaitable 비동기 인터페이스로 갱신
        /// </summary>
        public async Awaitable SaveProgressAsync(string eventId, EventProgressModel progress)
        {
            string path = Path.Combine(m_saveDir, $"event_progress_{eventId}.json");
            string json = JsonUtility.ToJson(progress, true);
            
            await Awaitable.BackgroundThreadAsync();
            File.WriteAllText(path, json);
            await Awaitable.MainThreadAsync();
        }

        /// <summary>
        /// [기능]: 플레이어의 누적 보상 데이터 정보를 비동기로 로드합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Awaitable 비동기 인터페이스로 갱신
        /// </summary>
        public async Awaitable<PlayerRewardModel> LoadRewardStateAsync()
        {
            string path = Path.Combine(m_saveDir, "player_rewards.json");
            if (File.Exists(path) == false)
            {
                return new PlayerRewardModel();
            }
            
            await Awaitable.BackgroundThreadAsync();
            string json = File.ReadAllText(path);
            await Awaitable.MainThreadAsync();
            
            return JsonUtility.FromJson<PlayerRewardModel>(json);
        }

        /// <summary>
        /// [기능]: 플레이어의 누적 보상 현황을 JSON 포맷으로 비동기 저장합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Awaitable 비동기 인터페이스로 갱신
        /// </summary>
        public async Awaitable SaveRewardStateAsync(PlayerRewardModel rewardState)
        {
            string path = Path.Combine(m_saveDir, "player_rewards.json");
            string json = JsonUtility.ToJson(rewardState, true);
            
            await Awaitable.BackgroundThreadAsync();
            File.WriteAllText(path, json);
            await Awaitable.MainThreadAsync();
        }

        /// <summary>
        /// [기능]: save 디렉토리 하위의 모든 세이브 파일을 비동기로 완전 삭제합니다.
        /// [작성자]: 윤승종
        /// [수정 날짜]: 2026-06-14
        /// [마지막 수정 작성자]: 윤승종
        /// [수정 내용]: Awaitable 비동기 인터페이스로 갱신
        /// </summary>
        public async Awaitable ClearAllAsync()
        {
            await Awaitable.BackgroundThreadAsync();
            if (Directory.Exists(m_saveDir))
            {
                string[] files = Directory.GetFiles(m_saveDir);
                for (int i = 0; i < files.Length; i++)
                {
                    File.Delete(files[i]);
                }
            }
            await Awaitable.MainThreadAsync();
        }
        #endregion
    }
}
