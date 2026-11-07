// 파일 경로: Assets/_Scripts/Data/EnemyDatabase.cs
using System.Collections.Generic;
using UnityEngine;

namespace ProjectD.Data
{
    /// <summary>
    /// 게임에 있는 모든 적 데이터를 모아 두고, 적 ID로 찾거나 목록 전체를 준다.
    /// 적 스폰이 이 목록에서 종류를 무작위로 고르고, 네트워크로 받은 적 ID를 데이터로 바꿀 때도 쓴다.
    /// WeaponDatabase와 같은 구조다.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyDatabase", menuName = "ProjectD/Enemy Database")]
    public class EnemyDatabase : ScriptableObject
    {
        [Tooltip("게임에 있는 모든 적. 새 적 종류를 만들면 여기에 추가한다. 적 ID는 서로 겹치면 안 된다")]
        [SerializeField] private EnemyData[] _enemies;

        // 적 ID로 바로 찾기 위한 사전. 처음 찾을 때 만들고, 인스펙터에서 목록을 바꾸면 다시 만든다
        private Dictionary<int, EnemyData> _enemiesById;

        /// <summary>
        /// 등록된 모든 적 데이터
        /// </summary>
        public IReadOnlyList<EnemyData> Enemies => _enemies;

        /// <summary>
        /// 적 ID로 적 데이터를 찾는다.
        /// </summary>
        /// <param name="enemyId">찾을 적 ID</param>
        /// <param name="enemy">찾은 적 데이터. 없으면 null</param>
        /// <returns>찾았으면 true</returns>
        public bool TryGetEnemy(int enemyId, out EnemyData enemy)
        {
            if (_enemiesById == null)
            {
                BuildLookup();
            }

            return _enemiesById.TryGetValue(enemyId, out enemy);
        }

        /// <summary>
        /// 인스펙터에서 값이 바뀌면 찾기용 사전을 비우고, 비어 있는 칸과 잘못된 설정을 검사한다.
        /// </summary>
        private void OnValidate()
        {
            _enemiesById = null;
            ValidateEnemies();
        }

        /// <summary>
        /// 적 목록으로 ID 찾기용 사전을 만든다. 비어 있는 칸과 겹치는 ID는 건너뛴다.
        /// </summary>
        private void BuildLookup()
        {
            _enemiesById = new Dictionary<int, EnemyData>();

            if (_enemies == null)
            {
                return;
            }

            foreach (EnemyData enemy in _enemies)
            {
                if (enemy == null || _enemiesById.ContainsKey(enemy.EnemyId))
                {
                    continue;
                }

                _enemiesById.Add(enemy.EnemyId, enemy);
            }
        }

        /// <summary>
        /// 비어 있는 칸, ID가 0인 적, 겹치는 ID, 프리팹 이름이 비어 있는 적이 있으면 경고를 출력한다.
        /// </summary>
        private void ValidateEnemies()
        {
            if (_enemies == null)
            {
                return;
            }

            HashSet<int> usedIds = new HashSet<int>();

            foreach (EnemyData enemy in _enemies)
            {
                if (enemy == null)
                {
                    Debug.LogWarning($"[{nameof(EnemyDatabase)}] 적 목록에 비어 있는 칸이 있습니다.", this);
                    continue;
                }

                if (enemy.EnemyId == 0)
                {
                    Debug.LogWarning($"[{nameof(EnemyDatabase)}] {enemy.name}의 적 ID가 0입니다. 2000번대 번호를 지정해 주세요.", enemy);
                }

                if (!usedIds.Add(enemy.EnemyId))
                {
                    Debug.LogWarning($"[{nameof(EnemyDatabase)}] 적 ID {enemy.EnemyId}가 겹칩니다 ({enemy.name}). 적 ID는 서로 달라야 합니다.", enemy);
                }

                if (string.IsNullOrEmpty(enemy.ResourcePrefabName))
                {
                    Debug.LogWarning($"[{nameof(EnemyDatabase)}] {enemy.name}의 Resource Prefab Name이 비어 있습니다. 이 적은 생성되지 않습니다.", enemy);
                }
            }
        }
    }
}
