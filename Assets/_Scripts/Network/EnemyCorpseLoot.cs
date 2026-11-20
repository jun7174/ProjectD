// 파일 경로: Assets/_Scripts/Network/EnemyCorpseLoot.cs
using Photon.Pun;
using ProjectD.Combat;
using ProjectD.Data;
using UnityEngine;

namespace ProjectD.Network
{
    /// <summary>
    /// 적이 죽으면 시체를 루팅 상자로 만든다.
    /// 모든 화면에서 상호작용 콜라이더를 켜고, 마스터는 적 데이터의 루팅 테이블로 내용물을 정해 모두에게 보낸다.
    /// 나중에 들어온 사람은 체력 동기화로 사망 이벤트를 받아 콜라이더를 켜고, 내용물은 LootContainerSync가 보낸다.
    /// 상호작용 콜라이더 오브젝트는 프리팹에서 꺼 둔다. 꺼진 오브젝트라 EnemyDeath가 끌 콜라이더 목록에 넣지 않는다.
    /// 적 프리팹 루트에 붙이고, 모든 클라이언트에서 켜 둔다.
    /// </summary>
    public class EnemyCorpseLoot : MonoBehaviour
    {
        [Header("데이터")]
        [Tooltip("시체 루팅 테이블을 읽는다")]
        [SerializeField] private EnemyData _data;

        [Header("시체 상호작용")]
        [Tooltip("죽으면 켤 상호작용 콜라이더 오브젝트 (Interactable 레이어, 프리팹에서는 꺼 둠)")]
        [SerializeField] private GameObject _lootTrigger;

        [Header("참조 (비워 두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private Health _health;
        [SerializeField] private LootContainerSync _containerSync;

        /// <summary>
        /// 참조를 찾고 비어 있으면 경고한다.
        /// </summary>
        private void Awake()
        {
            if (_health == null && !TryGetComponent(out _health))
            {
                Debug.LogWarning($"[{nameof(EnemyCorpseLoot)}] Health를 찾지 못했습니다. 시체를 루팅할 수 없습니다.", this);
            }

            if (_containerSync == null && !TryGetComponent(out _containerSync))
            {
                Debug.LogWarning($"[{nameof(EnemyCorpseLoot)}] LootContainerSync를 찾지 못했습니다. 시체에 내용물이 생기지 않습니다.", this);
            }

            if (_lootTrigger == null)
            {
                Debug.LogWarning($"[{nameof(EnemyCorpseLoot)}] Loot Trigger가 비어 있습니다. 시체에 다가가도 열 수 없습니다.", this);
            }
            else
            {
                _lootTrigger.SetActive(false);
            }
        }

        /// <summary>
        /// 사망 이벤트를 구독한다.
        /// </summary>
        private void OnEnable()
        {
            if (_health != null)
            {
                _health.OnDied += HandleDied;
            }
        }

        /// <summary>
        /// 구독을 해제한다.
        /// </summary>
        private void OnDisable()
        {
            if (_health != null)
            {
                _health.OnDied -= HandleDied;
            }
        }

        /// <summary>
        /// 죽으면 상호작용 콜라이더를 켜고, 마스터(방 밖이면 이 화면)는 내용물을 정한다.
        /// </summary>
        private void HandleDied()
        {
            if (_lootTrigger != null)
            {
                _lootTrigger.SetActive(true);
            }

            bool decidesContents = !PhotonNetwork.InRoom || PhotonNetwork.IsMasterClient;
            if (decidesContents && _containerSync != null && _data != null && _data.LootTable != null)
            {
                _containerSync.GenerateContents(_data.LootTable);
            }
        }
    }
}
