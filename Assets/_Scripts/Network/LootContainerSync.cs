// 파일 경로: Assets/_Scripts/Network/LootContainerSync.cs
using System;
using System.Collections.Generic;
using Photon.Pun;
using ProjectD.Data;
using ProjectD.Inventory;
using UnityEngine;

namespace ProjectD.Network
{
    /// <summary>
    /// 루팅 상자(상자, 적 시체)의 내용물을 모든 클라이언트에서 같게 맞추고, 꺼내기를 마스터가 승인한다.
    /// 1. 마스터가 루팅 테이블로 내용물을 정해 모두에게 보낸다 (상자는 시작할 때, 시체는 죽을 때).
    /// 2. 꺼내려는 사람은 자기 인벤토리에 자리를 예약한 뒤 마스터에게 요청한다.
    /// 3. 마스터는 항목이 아직 있는지와 거리를 검사해, 통과하면 모두에게 "그 항목이 빠짐"을 보내고 요청자에게 승인을 보낸다.
    ///    빠짐 알림은 마스터에서 바로 실행되므로, 같은 항목에 대한 다음 요청은 이미 빠진 것으로 보고 거절한다.
    /// 나중에 들어온 사람에게는 마스터가 현재 내용물(빠진 항목 포함)을 한 번 보낸다.
    /// 상자와 적 프리팹 루트에 LootContainer, PhotonView와 함께 붙이고, 모든 클라이언트에서 켜 둔다.
    /// </summary>
    public class LootContainerSync : MonoBehaviourPunCallbacks
    {
        [Tooltip("시작할 때 마스터가 내용물을 정할 루팅 테이블 (상자). 적 시체는 비워 둔다")]
        [SerializeField] private LootTable _startingLootTable;

        [Tooltip("이 거리(m)보다 먼 사람의 꺼내기 요청은 거절한다")]
        [Min(0.5f)]
        [SerializeField] private float _maxTakeDistance = 4f;

        [Header("참조 (비워 두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private LootContainer _container;

        private readonly List<ItemStack> _rollBuffer = new List<ItemStack>();

        // 내가 보낸 꺼내기 요청의 결과를 기다리는 곳. 요청 번호별 결과 처리 함수
        private readonly Dictionary<int, Action<bool>> _pendingRequests = new Dictionary<int, Action<bool>>();

        private PlayerItemLedger _ledger;
        private int _nextRequestId;

        /// <summary>
        /// 참조를 찾고 비어 있으면 경고한다. 아이템 목록 사본은 씬에서 찾는다.
        /// </summary>
        private void Awake()
        {
            if (_container == null && !TryGetComponent(out _container))
            {
                Debug.LogWarning($"[{nameof(LootContainerSync)}] LootContainer를 찾지 못했습니다. 내용물이 동기화되지 않습니다.", this);
            }

            _ledger = FindFirstObjectByType<PlayerItemLedger>();
        }

        /// <summary>
        /// 시작 루팅 테이블이 있으면 마스터(방 밖이면 이 화면)가 내용물을 정한다.
        /// </summary>
        private void Start()
        {
            if (_startingLootTable != null && IsAuthority())
            {
                GenerateContents(_startingLootTable);
            }
        }

        /// <summary>
        /// 루팅 테이블로 내용물을 정해 모두에게 보낸다. 마스터(방 밖이면 이 화면)만 실행한다.
        /// </summary>
        /// <param name="lootTable">사용할 루팅 테이블</param>
        public void GenerateContents(LootTable lootTable)
        {
            if (!IsAuthority() || lootTable == null || _container == null)
            {
                return;
            }

            lootTable.Roll(_rollBuffer);

            if (PhotonNetwork.InRoom)
            {
                ToArrays(_rollBuffer, out int[] itemIds, out int[] counts);
                photonView.RPC(nameof(RpcSetContents), RpcTarget.All, itemIds, counts);
            }
            else
            {
                _container.SetContents(_rollBuffer);
            }
        }

        /// <summary>
        /// 항목 하나를 꺼내 달라고 마스터에게 요청한다. 결과는 onResult로 받는다.
        /// 요청 전에 요청한 쪽이 인벤토리에 자리를 예약해 두어야 한다.
        /// </summary>
        /// <param name="entryIndex">꺼낼 항목 번호</param>
        /// <param name="requesterViewId">요청한 플레이어 캐릭터의 PhotonView ID (거리 검사용)</param>
        /// <param name="onResult">결과를 받을 함수. 승인이면 true</param>
        public void RequestTake(int entryIndex, int requesterViewId, Action<bool> onResult)
        {
            if (!PhotonNetwork.InRoom)
            {
                bool hasEntry = _container != null && _container.TryGetEntry(entryIndex, out _);
                if (hasEntry)
                {
                    _container.RemoveEntry(entryIndex);
                }

                onResult?.Invoke(hasEntry);
                return;
            }

            _nextRequestId++;
            _pendingRequests[_nextRequestId] = onResult;
            photonView.RPC(nameof(RpcRequestTake), RpcTarget.MasterClient, entryIndex, requesterViewId, _nextRequestId);
        }

        /// <summary>
        /// 새 사람이 들어오면 마스터가 그 사람에게만 현재 내용물을 보낸다.
        /// </summary>
        /// <param name="newPlayer">들어온 플레이어</param>
        public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            if (!PhotonNetwork.IsMasterClient || _container == null || !_container.HasContents)
            {
                return;
            }

            ToArrays(_container.Entries, out int[] itemIds, out int[] counts);
            photonView.RPC(nameof(RpcSetContents), newPlayer, itemIds, counts);
        }

        /// <summary>
        /// 마스터가 정한 내용물을 받는다.
        /// </summary>
        /// <param name="itemIds">항목별 아이템 ID</param>
        /// <param name="counts">항목별 개수 (꺼낸 항목은 0)</param>
        [PunRPC]
        private void RpcSetContents(int[] itemIds, int[] counts)
        {
            ApplyContents(itemIds, counts);
        }

        /// <summary>
        /// 꺼내기 요청을 받는다 (마스터). 실제 처리는 HandleTakeRequest에서 한다.
        /// </summary>
        /// <param name="entryIndex">꺼낼 항목 번호</param>
        /// <param name="requesterViewId">요청한 캐릭터의 PhotonView ID</param>
        /// <param name="requestId">요청 번호</param>
        /// <param name="info">보낸 사람 등 메시지 정보 (PUN이 자동으로 채운다)</param>
        [PunRPC]
        private void RpcRequestTake(int entryIndex, int requesterViewId, int requestId, PhotonMessageInfo info)
        {
            HandleTakeRequest(entryIndex, requesterViewId, requestId, info.Sender);
        }

        /// <summary>
        /// 마스터가 꺼낸 항목을 받는다.
        /// </summary>
        /// <param name="entryIndex">빠진 항목 번호</param>
        [PunRPC]
        private void RpcRemoveEntry(int entryIndex)
        {
            if (_container != null)
            {
                _container.RemoveEntry(entryIndex);
            }
        }

        /// <summary>
        /// 내 꺼내기 요청의 결과를 받는다.
        /// </summary>
        /// <param name="requestId">요청 번호</param>
        /// <param name="isApproved">승인이면 true</param>
        [PunRPC]
        private void RpcTakeResult(int requestId, bool isApproved)
        {
            ResolveRequest(requestId, isApproved);
        }

        /// <summary>
        /// 받은 배열로 내용물을 정한다.
        /// </summary>
        /// <param name="itemIds">항목별 아이템 ID</param>
        /// <param name="counts">항목별 개수</param>
        private void ApplyContents(int[] itemIds, int[] counts)
        {
            if (_container == null || itemIds == null || counts == null)
            {
                return;
            }

            _rollBuffer.Clear();
            int length = Mathf.Min(itemIds.Length, counts.Length);
            for (int i = 0; i < length; i++)
            {
                _rollBuffer.Add(new ItemStack(itemIds[i], counts[i]));
            }

            _container.SetContents(_rollBuffer);
        }

        /// <summary>
        /// 꺼내기 요청을 검사해 승인하거나 거절한다 (마스터).
        /// 승인하면 모두에게 항목이 빠졌다고 알리고, 요청자의 아이템 목록 사본에 더한다.
        /// </summary>
        /// <param name="entryIndex">꺼낼 항목 번호</param>
        /// <param name="requesterViewId">요청한 캐릭터의 PhotonView ID</param>
        /// <param name="requestId">요청 번호</param>
        /// <param name="sender">요청한 플레이어</param>
        private void HandleTakeRequest(int entryIndex, int requesterViewId, int requestId, Photon.Realtime.Player sender)
        {
            if (!PhotonNetwork.IsMasterClient || sender == null)
            {
                return;
            }

            bool isApproved = CanTake(entryIndex, requesterViewId, sender, out ItemStack entry);

            if (isApproved)
            {
                photonView.RPC(nameof(RpcRemoveEntry), RpcTarget.All, entryIndex);

                if (_ledger != null)
                {
                    _ledger.Add(sender.ActorNumber, entry.ItemId, entry.Count);
                }
            }

            photonView.RPC(nameof(RpcTakeResult), sender, requestId, isApproved);
        }

        /// <summary>
        /// 꺼내기 요청이 유효한지 검사한다: 항목이 남아 있는지, 요청자가 자기 캐릭터로 요청했는지, 가까운지.
        /// </summary>
        /// <param name="entryIndex">항목 번호</param>
        /// <param name="requesterViewId">요청한 캐릭터의 PhotonView ID</param>
        /// <param name="sender">요청한 플레이어</param>
        /// <param name="entry">남아 있는 항목</param>
        /// <returns>유효하면 true</returns>
        private bool CanTake(int entryIndex, int requesterViewId, Photon.Realtime.Player sender, out ItemStack entry)
        {
            entry = default;
            if (_container == null || !_container.TryGetEntry(entryIndex, out entry))
            {
                return false;
            }

            PhotonView requesterView = PhotonView.Find(requesterViewId);
            if (requesterView == null || requesterView.Owner != sender)
            {
                return false;
            }

            return Vector3.Distance(requesterView.transform.position, transform.position) <= _maxTakeDistance;
        }

        /// <summary>
        /// 기다리던 요청의 결과 처리 함수를 실행하고 지운다.
        /// </summary>
        /// <param name="requestId">요청 번호</param>
        /// <param name="isApproved">승인이면 true</param>
        private void ResolveRequest(int requestId, bool isApproved)
        {
            if (!_pendingRequests.TryGetValue(requestId, out Action<bool> onResult))
            {
                return;
            }

            _pendingRequests.Remove(requestId);
            onResult?.Invoke(isApproved);
        }

        /// <summary>
        /// 내용물을 결정하는 쪽인지 확인한다. 방 안이면 마스터, 방 밖(Sandbox)이면 이 화면.
        /// </summary>
        /// <returns>결정하는 쪽이면 true</returns>
        private static bool IsAuthority()
        {
            return !PhotonNetwork.InRoom || PhotonNetwork.IsMasterClient;
        }

        /// <summary>
        /// 아이템 묶음 목록을 RPC로 보낼 수 있는 ID 배열과 개수 배열로 바꾼다.
        /// </summary>
        /// <param name="stacks">아이템 묶음 목록</param>
        /// <param name="itemIds">아이템 ID 배열</param>
        /// <param name="counts">개수 배열</param>
        private static void ToArrays(IReadOnlyList<ItemStack> stacks, out int[] itemIds, out int[] counts)
        {
            itemIds = new int[stacks.Count];
            counts = new int[stacks.Count];

            for (int i = 0; i < stacks.Count; i++)
            {
                itemIds[i] = stacks[i].ItemId;
                counts[i] = stacks[i].Count;
            }
        }
    }
}
