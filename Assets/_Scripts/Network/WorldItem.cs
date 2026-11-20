// 파일 경로: Assets/_Scripts/Network/WorldItem.cs
using System;
using System.Collections.Generic;
using Photon.Pun;
using ProjectD.Core;
using ProjectD.Data;
using UnityEngine;

namespace ProjectD.Network
{
    /// <summary>
    /// 땅에 떨어진 아이템. 플레이어가 버리면 마스터가 방 오브젝트로 만들고(생성 데이터: 아이템 ID, 개수),
    /// 누가 주우면 마스터가 승인한 뒤 지운다.
    /// 방 오브젝트라 나중에 들어온 사람도 생성 정보로 자동으로 받고, 이미 지워진 것은 받지 않는다.
    /// 모양은 생성 후 각 화면에서 아이템 데이터의 월드 프리팹을 자식으로 붙여 만든다 (네트워크로 보내지 않음).
    /// 프리팹은 Assets/Resources/WorldItem.prefab (PhotonView, Interactable 레이어 트리거 콜라이더 포함).
    /// </summary>
    public class WorldItem : MonoBehaviourPun, IInteractable
    {
        [Header("데이터")]
        [Tooltip("생성 데이터의 아이템 ID로 아이템을 찾는다")]
        [SerializeField] private ItemDatabase _itemDatabase;

        [Header("모양")]
        [Tooltip("아이템 모양을 붙일 위치")]
        [SerializeField] private Transform _visualRoot;

        [Tooltip("아이템에 월드 프리팹이 없을 때 보일 기본 모양")]
        [SerializeField] private GameObject _fallbackVisual;

        [Header("줍기")]
        [Tooltip("이 거리(m)보다 먼 사람의 줍기 요청은 거절한다")]
        [Min(0.5f)]
        [SerializeField] private float _maxPickupDistance = 4f;

        // 내가 보낸 줍기 요청의 결과를 기다리는 곳. 요청 번호별 결과 처리 함수
        private readonly Dictionary<int, Action<bool>> _pendingRequests = new Dictionary<int, Action<bool>>();

        private PlayerItemLedger _ledger;
        private int _nextRequestId;
        private bool _isTaken;

        /// <summary>아이템 데이터. 생성 데이터를 받기 전이나 못 찾았으면 null</summary>
        public ItemData Item { get; private set; }

        /// <summary>아이템 묶음 (ID, 개수)</summary>
        public ItemStack Stack { get; private set; }

        /// <summary>우선순위. 아이템은 가장 낮다</summary>
        public int Priority => 0;

        /// <summary>줍기는 누르는 즉시</summary>
        public float HoldDuration => 0f;

        /// <summary>거리 비교 위치</summary>
        public Vector3 Position => transform.position;

        /// <summary>안내 문구</summary>
        public string Prompt => Item == null ? string.Empty : $"줍기: {Item.DisplayName} x{Stack.Count}";

        /// <summary>아이템을 알고 있고 아직 줍지 않았을 때만 주울 수 있다</summary>
        public bool CanInteract => Item != null && !_isTaken;

        /// <summary>
        /// 아이템 목록 사본을 씬에서 찾는다.
        /// </summary>
        private void Awake()
        {
            _ledger = FindFirstObjectByType<PlayerItemLedger>();

            if (_itemDatabase == null)
            {
                Debug.LogWarning($"[{nameof(WorldItem)}] ItemDatabase가 비어 있습니다. 떨어진 아이템을 알 수 없습니다.", this);
            }
        }

        /// <summary>
        /// 생성 데이터에서 아이템 ID와 개수를 읽어 모양을 만든다.
        /// </summary>
        private void Start()
        {
            object[] data = photonView.InstantiationData;
            if (data == null || data.Length < 2)
            {
                Debug.LogWarning($"[{nameof(WorldItem)}] 생성 데이터(아이템 ID, 개수)가 없습니다.", this);
                return;
            }

            Setup((int)data[0], (int)data[1]);
        }

        /// <summary>
        /// 줍기를 요청한 플레이어 쪽에서 호출한다. 인벤토리 자리 예약은 호출한 쪽이 먼저 한다.
        /// </summary>
        /// <param name="interactor">상호작용한 플레이어 캐릭터</param>
        public void Interact(GameObject interactor)
        {
            if (interactor.TryGetComponent(out PlayerItemTransfer transfer))
            {
                transfer.RequestPickup(this);
            }
        }

        /// <summary>
        /// 이 아이템을 주워도 되는지 마스터에게 요청한다. 결과는 onResult로 받는다.
        /// </summary>
        /// <param name="requesterViewId">요청한 캐릭터의 PhotonView ID (거리 검사용)</param>
        /// <param name="onResult">결과를 받을 함수. 승인이면 true</param>
        public void RequestPickup(int requesterViewId, Action<bool> onResult)
        {
            _nextRequestId++;
            _pendingRequests[_nextRequestId] = onResult;
            photonView.RPC(nameof(RpcRequestPickup), RpcTarget.MasterClient, requesterViewId, _nextRequestId);
        }

        /// <summary>
        /// 줍기 요청을 받는다 (마스터). 실제 처리는 HandlePickupRequest에서 한다.
        /// </summary>
        /// <param name="requesterViewId">요청한 캐릭터의 PhotonView ID</param>
        /// <param name="requestId">요청 번호</param>
        /// <param name="info">보낸 사람 등 메시지 정보 (PUN이 자동으로 채운다)</param>
        [PunRPC]
        private void RpcRequestPickup(int requesterViewId, int requestId, PhotonMessageInfo info)
        {
            HandlePickupRequest(requesterViewId, requestId, info.Sender);
        }

        /// <summary>
        /// 내 줍기 요청의 결과를 받는다.
        /// </summary>
        /// <param name="requestId">요청 번호</param>
        /// <param name="isApproved">승인이면 true</param>
        [PunRPC]
        private void RpcPickupResult(int requestId, bool isApproved)
        {
            if (!_pendingRequests.TryGetValue(requestId, out Action<bool> onResult))
            {
                return;
            }

            _pendingRequests.Remove(requestId);
            onResult?.Invoke(isApproved);
        }

        /// <summary>
        /// 줍기 요청을 검사한다 (마스터). 승인하면 요청자에게 알리고, 사본에 더하고, 이 오브젝트를 지운다.
        /// 승인 결과를 먼저 보내고 지우므로, 요청자는 결과를 받은 뒤에 오브젝트가 사라진다.
        /// </summary>
        /// <param name="requesterViewId">요청한 캐릭터의 PhotonView ID</param>
        /// <param name="requestId">요청 번호</param>
        /// <param name="sender">요청한 플레이어</param>
        private void HandlePickupRequest(int requesterViewId, int requestId, Photon.Realtime.Player sender)
        {
            if (!PhotonNetwork.IsMasterClient || sender == null)
            {
                return;
            }

            bool isApproved = CanPickup(requesterViewId, sender);
            photonView.RPC(nameof(RpcPickupResult), sender, requestId, isApproved);

            if (!isApproved)
            {
                return;
            }

            _isTaken = true;

            if (_ledger != null)
            {
                _ledger.Add(sender.ActorNumber, Stack.ItemId, Stack.Count);
            }

            PhotonNetwork.Destroy(gameObject);
        }

        /// <summary>
        /// 줍기 요청이 유효한지 검사한다: 아직 줍지 않았는지, 요청자가 자기 캐릭터로 요청했는지, 가까운지.
        /// </summary>
        /// <param name="requesterViewId">요청한 캐릭터의 PhotonView ID</param>
        /// <param name="sender">요청한 플레이어</param>
        /// <returns>유효하면 true</returns>
        private bool CanPickup(int requesterViewId, Photon.Realtime.Player sender)
        {
            if (_isTaken || Item == null)
            {
                return false;
            }

            PhotonView requesterView = PhotonView.Find(requesterViewId);
            if (requesterView == null || requesterView.Owner != sender)
            {
                return false;
            }

            return Vector3.Distance(requesterView.transform.position, transform.position) <= _maxPickupDistance;
        }

        /// <summary>
        /// 아이템을 찾고 모양을 붙인다.
        /// </summary>
        /// <param name="itemId">아이템 ID</param>
        /// <param name="count">개수</param>
        private void Setup(int itemId, int count)
        {
            Stack = new ItemStack(itemId, count);

            if (_itemDatabase == null || !_itemDatabase.TryGetItem(itemId, out ItemData item))
            {
                Debug.LogWarning($"[{nameof(WorldItem)}] 아이템 ID {itemId}를 ItemDatabase에서 찾지 못했습니다.", this);
                return;
            }

            Item = item;

            bool hasWorldPrefab = item.WorldPrefab != null;
            if (hasWorldPrefab)
            {
                Transform parent = _visualRoot != null ? _visualRoot : transform;
                Instantiate(item.WorldPrefab, parent);
            }

            if (_fallbackVisual != null)
            {
                _fallbackVisual.SetActive(!hasWorldPrefab);
            }
        }
    }
}
