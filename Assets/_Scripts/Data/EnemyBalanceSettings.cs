// 파일 경로: Assets/_Scripts/Data/EnemyBalanceSettings.cs
using UnityEngine;

namespace ProjectD.Data
{
    /// <summary>
    /// 모든 적에 공통으로 적용하는 밸런스 수치. 지금은 인원별 체력 배율만 있다
    /// 인원이 바뀌면 살아 있는 적의 최대 체력을 이 배율로 다시 계산한다
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyBalanceSettings", menuName = "ProjectD/Enemy Balance Settings")]
    public class EnemyBalanceSettings : ScriptableObject
    {
        [Tooltip("인원별 적 체력 배율. 0번 칸이 1인, 1번 칸이 2인, 2번 칸이 3인, 3번 칸이 4인")]
        [SerializeField] private float[] _healthMultiplierByPlayerCount = { 1f, 1.3f, 1.6f, 1.9f };

        /// <summary>
        /// 현재 인원에 맞는 체력 배율을 돌려준다. 칸 수보다 인원이 많으면 마지막 칸 값을 쓴다.
        /// </summary>
        /// <param name="playerCount">방 안의 플레이어 수 (1 이상)</param>
        /// <returns>체력 배율. 설정이 비어 있으면 1</returns>
        public float GetHealthMultiplier(int playerCount)
        {
            if (_healthMultiplierByPlayerCount == null || _healthMultiplierByPlayerCount.Length == 0)
            {
                return 1f;
            }

            int index = Mathf.Clamp(playerCount - 1, 0, _healthMultiplierByPlayerCount.Length - 1);
            return _healthMultiplierByPlayerCount[index];
        }
    }
}
