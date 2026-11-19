// 파일 경로: Assets/_Scripts/Network/EnemyDeath.cs
using Photon.Pun;
using ProjectD.AI;
using ProjectD.Combat;
using ProjectD.Core.Define;
using UnityEngine;

namespace ProjectD.Network
{
    /// <summary>
    /// 적이 죽으면 시체로 남긴다. 오브젝트는 지우지 않는다.
    /// AI, 이동, 위치 동기화, 탄 판정 콜라이더를 끄고, 쓰러진 모습(옆으로 눕힘)으로 바꾼다.
    /// 체력 0은 NetworkHealth의 RPC로 모든 화면에서 같이 일어나므로, 시체 처리도 각 화면에서 직접 한다.
    /// 시체는 더 움직이지 않으므로 마스터는 위치 전송을 끈다. 나중에 들어온 사람에게는 마스터가 시체 위치를 한 번 보낸다.
    /// 적 프리팹 루트에 붙이고, 모든 클라이언트에서 켜 둔다.
    /// </summary>
    public class EnemyDeath : MonoBehaviourPunCallbacks
    {
        [Header("죽으면 끌 것")]
        [Tooltip("EnemyBrain, EnemyPerception, EnemyDormancy, NavMeshAgent, PhotonTransformView, EnemyTelegraphView를 넣는다")]
        [SerializeField] private Behaviour[] _behavioursToDisable;

        [Tooltip("탄 판정과 이동을 막는 콜라이더. 비워 두면 자신과 자식의 콜라이더를 모두 끈다")]
        [SerializeField] private Collider[] _collidersToDisable;

        [Header("시체 모습")]
        [Tooltip("옆으로 눕히는 각도 (도)")]
        [SerializeField] private float _corpseTiltAngle = 90f;

        [Tooltip("눕힌 뒤 아래로 내리는 높이 (m). 캡슐이 바닥에 닿도록 반지름 정도")]
        [Min(0f)]
        [SerializeField] private float _corpseHeightDrop = 0.5f;

        [Header("참조 (비워 두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private Health _health;
        [SerializeField] private EnemyStatus _status;

        /// <summary>
        /// 시체가 되었는지 여부
        /// </summary>
        public bool IsCorpse { get; private set; }

        /// <summary>
        /// 참조를 찾고, 끌 콜라이더가 비어 있으면 자신과 자식의 콜라이더를 모은다.
        /// </summary>
        private void Awake()
        {
            if (_health == null && !TryGetComponent(out _health))
            {
                Debug.LogWarning($"[{nameof(EnemyDeath)}] Health를 찾지 못했습니다. 적이 죽어도 시체가 되지 않습니다.", this);
            }

            if (_status == null && !TryGetComponent(out _status))
            {
                Debug.LogWarning($"[{nameof(EnemyDeath)}] EnemyStatus를 찾지 못했습니다. 사망 상태가 표시되지 않습니다.", this);
            }

            if (_collidersToDisable == null || _collidersToDisable.Length == 0)
            {
                _collidersToDisable = GetComponentsInChildren<Collider>();
            }
        }

        /// <summary>
        /// 방 콜백을 등록하고 사망 이벤트를 구독한다.
        /// </summary>
        public override void OnEnable()
        {
            base.OnEnable();

            if (_health != null)
            {
                _health.OnDied += HandleDied;
            }
        }

        /// <summary>
        /// 방 콜백 등록과 구독을 해제한다.
        /// </summary>
        public override void OnDisable()
        {
            base.OnDisable();

            if (_health != null)
            {
                _health.OnDied -= HandleDied;
            }
        }

        /// <summary>
        /// 나중에 들어온 사람에게 마스터가 시체의 위치와 회전을 보낸다.
        /// 위치 전송이 꺼져 있어 정기 전송으로는 시체 위치가 가지 않기 때문이다.
        /// </summary>
        /// <param name="newPlayer">들어온 플레이어</param>
        public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            if (IsCorpse && PhotonNetwork.IsMasterClient)
            {
                photonView.RPC(nameof(RpcPlaceCorpse), newPlayer, transform.position, transform.rotation);
            }
        }

        /// <summary>
        /// 체력이 0이 되면 시체가 되고, 마스터는 위치 전송을 끈다.
        /// </summary>
        private void HandleDied()
        {
            BecomeCorpse();

            if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
            {
                photonView.Synchronization = ViewSynchronization.Off;
            }
        }

        /// <summary>
        /// 중도 참가 시 마스터가 보낸 시체 위치를 받는다. 실제 처리는 PlaceCorpse에서 한다.
        /// </summary>
        /// <param name="position">마스터 화면의 시체 위치</param>
        /// <param name="rotation">마스터 화면의 시체 회전 (눕힌 뒤)</param>
        [PunRPC]
        private void RpcPlaceCorpse(Vector3 position, Quaternion rotation)
        {
            PlaceCorpse(position, rotation);
        }

        /// <summary>
        /// 시체로 만든 뒤 마스터의 위치와 회전으로 맞춘다.
        /// 체력 동기화와 이 메시지 중 어느 쪽이 먼저 와도 결과가 같다.
        /// 시체 처리는 한 번만 하고, 위치는 항상 마스터 값으로 덮어쓰기 때문이다.
        /// </summary>
        /// <param name="position">시체 위치</param>
        /// <param name="rotation">시체 회전</param>
        private void PlaceCorpse(Vector3 position, Quaternion rotation)
        {
            BecomeCorpse();
            transform.SetPositionAndRotation(position, rotation);
        }

        /// <summary>
        /// 컴포넌트와 콜라이더를 끄고, 상태를 사망으로 바꾸고, 쓰러진 모습으로 만든다. 한 번만 실행된다.
        /// 위치를 바꾸기 전에 NavMeshAgent를 꺼야 NavMesh 위로 다시 끌려 올라가지 않는다.
        /// </summary>
        private void BecomeCorpse()
        {
            if (IsCorpse)
            {
                return;
            }

            IsCorpse = true;

            foreach (Behaviour behaviour in _behavioursToDisable)
            {
                if (behaviour != null)
                {
                    behaviour.enabled = false;
                }
            }

            foreach (Collider bodyCollider in _collidersToDisable)
            {
                if (bodyCollider != null)
                {
                    bodyCollider.enabled = false;
                }
            }

            if (_status != null)
            {
                _status.SetState(EnemyState.Dead);
                _status.SetTelegraphing(false);
            }

            transform.position += Vector3.down * _corpseHeightDrop;
            transform.rotation *= Quaternion.Euler(_corpseTiltAngle, 0f, 0f);
        }
    }
}
