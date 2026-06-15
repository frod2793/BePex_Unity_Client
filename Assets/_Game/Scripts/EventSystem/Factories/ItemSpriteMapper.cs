using UnityEngine;
using UnityEngine.AddressableAssets;

namespace BePex.EventSystem.Factories
{
    /// <summary>
    /// [기능]: 어드레서블(Addressables) 시스템을 활용하여 item_Sheet 내에 잘려진 21개의 서브 스프라이트를 비동기 로드 및 캐싱하는 OCP 매퍼.
    /// [작성자]: 윤승종
    /// </summary>
    public static class ItemSpriteMapper
    {
        #region 내부 캐시 필드
        private static readonly Sprite[] m_cachedSprites = new Sprite[21];
        #endregion

        #region 공개 비동기 메서드
        /// <summary>
        /// [기능]: 인덱스(0 ~ 20)에 해당하는 아이템 슬라이스 스프라이트를 어드레서블을 통해 비동기 로드하고 캐싱하여 반환합니다.
        /// [작성자]: 윤승종
        /// </summary>
        public static async Awaitable<Sprite> GetItemSpriteAsync(int index)
        {
            if (index < 0 || index > 20)
            {
                Debug.LogWarning($"[ItemSpriteMapper] 범위 초과 슬라이스 인덱스 요청: {index}");
                return null;
            }

            if (m_cachedSprites[index] != null)
            {
                return m_cachedSprites[index];
            }

            try
            {
                // Key[SubAsset] 포맷을 활용하여 어드레서블 비동기 로딩 큐잉
                string addressKey = $"item_Sheet[item_Sheet_{index}]";
                var handle = Addressables.LoadAssetAsync<Sprite>(addressKey);
                
                // Addressables 핸들의 Task를 await하여 비동기 다운로드/로드 완료 대기
                Sprite sprite = await handle.Task;
                
                if (sprite != null)
                {
                    m_cachedSprites[index] = sprite;
                }
                return sprite;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ItemSpriteMapper] 어드레서블 스프라이트 로드 중 오류 발생: {ex.Message}");
                return null;
            }
        }
        #endregion
    }
}
