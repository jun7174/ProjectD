// 파일 경로: Assets/_Scripts/Network/EnemySpawner.cs
using System.Collections.Generic;
using Photon.Pun;
using ProjectD.AI;
using ProjectD.Data;
using UnityEngine;

namespace ProjectD.Network
{
    /// <summary>
    /// 레이드가 시작되면 마스터가 적 스폰 지점마다 종류를 무작위로 골라 적을 생성한다.
    /// 방 오브젝트(InstantiateRoomObject)로 만들어 생성 정보가 서버에 보관되므로, 중도 참가자도 같은 적을 자동으로 받는다.
    /// 생성한 적에게 순찰 경로를 넣어 준다: 스폰 지점 자신, 그 아래 자식들 순서. 자식이 없으면 스폰 지점에서 제자리 경계한다.
    /// 순찰 경로는 AI를 실행하는 마스터에게만 필요하므로 마스터에서만 넣는다.
    /// Raid 씬에 둔다.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("데이터")]
        [Tooltip("생성할 수 있는 모든 적 종류")]
        [SerializeField] private EnemyDatabase _enemyDatabase;

        [Header("스폰 지점")]
        [Tooltip("스폰 지점들의 부모 (Map/Points/EnemySpawns). 바로 아래 자식 하나가 스폰 지점 하나이고, 스폰 지점의 자식은 순찰 지점이다")]
        [SerializeField] private Transform _spawnPointRoot;

        // 순찰 지점을 모을 때 매번 새 목록을 만들지 않도록 같이 쓴다. EnemyPatrolRoute가 복사해서 보관한다
        private readonly List<Vector3> _patrolPointBuffer = new List<Vector3>();

        /// <summary>
        /// 데이터와 스폰 지점이 비어 있으면 경고한다.
        /// </summary>
        private void Awake()
        {
            if (_enemyDatabase == null || _enemyDatabase.Enemies == null || _enemyDatabase.Enemies.Count == 0)
            {
                Debug.LogWarning($"[{nameof(EnemySpawner)}] EnemyDatabase가 비어 있습니다. 적이 생성되지 않습니다.", this);
            }

            if (_spawnPointRoot == null)
            {
                Debug.LogWarning($"[{nameof(EnemySpawner)}] Spawn Point Root가 비어 있습니다. 적이 생성되지 않습니다.", this);
            }
        }

        /// <summary>
        /// 마스터만 적을 생성한다. 나중에 들어온 사람은 보관된 생성 정보로 자동으로 받으므로 생성하지 않는다.
        /// </summary>
        private void Start()
        {
            if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient)
            {
                return;
            }

            SpawnAll();
        }

        /// <summary>
        /// 모든 스폰 지점에 적을 하나씩 생성한다.
        /// </summary>
        private void SpawnAll()
        {
            if (_enemyDatabase == null || _spawnPointRoot == null || _enemyDatabase.Enemies.Count == 0)
            {
                return;
            }

            for (int i = 0; i < _spawnPointRoot.childCount; i++)
            {
                SpawnAt(i, _spawnPointRoot.GetChild(i));
            }
        }

        /// <summary>
        /// 스폰 지점 하나에 무작위 종류의 적을 방 오브젝트로 생성하고 순찰 경로를 넣어 준다.
        /// </summary>
        /// <param name="spawnIndex">스폰 지점 번호 (EnemySpawns 아래 자식 순서)</param>
        /// <param name="spawnPoint">스폰 지점</param>
        private void SpawnAt(int spawnIndex, Transform spawnPoint)
        {
            EnemyData enemy = ChooseRandomEnemy();
            if (enemy == null || string.IsNullOrEmpty(enemy.ResourcePrefabName))
            {
                return;
            }

            // 생성 데이터로 스폰 지점 번호를 넘긴다. 모든 클라이언트의 적이 photonView.InstantiationData로 읽을 수 있다
            object[] instantiationData = { spawnIndex };
            GameObject spawned = PhotonNetwork.InstantiateRoomObject(enemy.ResourcePrefabName, spawnPoint.position, spawnPoint.rotation, 0, instantiationData);

            if (spawned == null)
            {
                Debug.LogWarning($"[{nameof(EnemySpawner)}] {enemy.ResourcePrefabName}을 생성하지 못했습니다. Assets/Resources/{enemy.ResourcePrefabName}.prefab이 있는지 확인해 주세요.", this);
                return;
            }

            AssignPatrolRoute(spawned, spawnPoint);
        }

        /// <summary>
        /// 스폰 지점 자신과 그 자식들의 위치를 순서대로 적의 순찰 경로로 넣는다.
        /// </summary>
        /// <param name="enemy">생성한 적</param>
        /// <param name="spawnPoint">스폰 지점</param>
        private void AssignPatrolRoute(GameObject enemy, Transform spawnPoint)
        {
            if (!enemy.TryGetComponent(out EnemyPatrolRoute route))
            {
                Debug.LogWarning($"[{nameof(EnemySpawner)}] {enemy.name}에 EnemyPatrolRoute가 없습니다. 순찰하지 않습니다.", enemy);
                return;
            }

            _patrolPointBuffer.Clear();
            _patrolPointBuffer.Add(spawnPoint.position);

            for (int i = 0; i < spawnPoint.childCount; i++)
            {
                _patrolPointBuffer.Add(spawnPoint.GetChild(i).position);
            }

            route.SetPoints(_patrolPointBuffer);
        }

        /// <summary>
        /// 등록된 적 종류 중 하나를 무작위로 고른다.
        /// </summary>
        /// <returns>고른 적 데이터. 목록의 칸이 비어 있으면 null</returns>
        private EnemyData ChooseRandomEnemy()
        {
            int index = Random.Range(0, _enemyDatabase.Enemies.Count);
            return _enemyDatabase.Enemies[index];
        }
    }
}
