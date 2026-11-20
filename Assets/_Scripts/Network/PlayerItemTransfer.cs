// 파일 경로: Assets/_Scripts/Network/PlayerItemTransfer.cs
using System;
using System.Collections.Generic;
using Photon.Pun;
using ProjectD.Core.Define;
using ProjectD.Data;
using ProjectD.Inventory;
using UnityEngine;

namespace ProjectD.Network
{
    /// <summary>
    /// 내 인벤토리로 아이템이 들어오고 나가는 요청을 마스터에게 보낸다: 상자에서 꺼내기, 땅에서 줍기, 땅에 버리기.
    /// 들어오는 아이템은 요청 전에 인벤토리에 자리를 예약하고, 승인되면 확정, 거절되면 취소한다.
    /// 버리기는 내 인벤토리에서 바로 빼고, 마스터가 아이템 목록 사본으로 검증한 뒤 땅에 아이템을 만든다.
    /// 그리드 안에서 옮기는 것은 이 컴포넌트를 거치지 않는다 (로컬 전용).
    /// 플레이어 프리팹 루트에 붙이고, 모든 캐릭터에서 켜 둔다 (마스터 화면의 남의 캐릭터가 버리기 요청을 받아야 한다).
    /// </summary>
    public class PlayerItemTransfer : MonoBehaviourPun
    {
        [Header("데이터")]
        [Tooltip("아이템 ID로 아이템 데이터를 찾는다")]
        [SerializeField] private ItemDatabase _itemDatabase;

        [Header("버리기")]
        [Tooltip("캐릭터 앞쪽으로 이 거리(m)에 떨어뜨린다")]
        [Min(0f)]
        [SerializeField] private float _dropDistance = 1f;

        [Header("디버그")]
        [Tooltip("컨텍스트 메뉴로 받을 테스트 아이템. 각각 최대 스택 개수만큼 받는다")]
        [SerializeField] private ItemData[] _debugGrantItems;

        [Header("참조 (비워 두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private PlayerInventory _inventory;

        // 디버그 지급 요청의 결과를 기다리는 예약. 요청 번호별
        private readonly Dictionary<int, InventoryReservation> _pendingGrants = new Dictionary<int, InventoryReservation>();

        private PlayerItemLedger _ledger;
        private int _nextGrantId;

        /// <summary>
        /// 요청이 실패하면 발생한다. 인자는 화면에 보여 줄 문구다.
        /// </summary>
        public event Action<string> OnTransferFailed;

        /// <summary>
        /// 참조를 찾는다. 인벤토리는 내 캐릭터에만 있으므로 남의 캐릭터에서는 비어 있어도 경고하지 않는다.
        /// </summary>
        private void Awake()
        {
            if (_inventory == null)
            {
                TryGetComponent(out _inventory);
            }

            if (_itemDatabase == null)
            {
                Debug.LogWarning($"[{nameof(PlayerItemTransfer)}] ItemDatabase가 비어 있습니다. 아이템을 받을 수 없습니다.", this);
            }

            _ledger = FindFirstObjectByType<PlayerItemLedger>();
        }

        /// <summary>
        /// 상자의 항목 하나를 꺼내 달라고 요청한다.
        /// </summary>
        /// <param name="container">상자</param>
        /// <param name="entryIndex">꺼낼 항목 번호</param>
        /// <returns>요청을 보냈으면 true. 자리가 없거나 항목이 없으면 false</returns>
        public bool RequestTake(LootContainer container, int entryIndex)
        {
            if (container == null || !container.TryGetEntry(entryIndex, out ItemStack entry))
            {
                return false;
            }

            if (!container.TryGetComponent(out LootContainerSync containerSync))
            {
                Debug.LogWarning($"[{nameof(PlayerItemTransfer)}] {container.name}에 LootContainerSync가 없습니다.", container);
                return false;
            }

            if (!TryReserve(entry, out InventoryReservation reservation))
            {
                return false;
            }

            containerSync.RequestTake(entryIndex, photonView.ViewID, isApproved => FinishReservation(reservation, isApproved));
            return true;
        }

        /// <summary>
        /// 땅의 아이템을 주워도 되는지 요청한다.
        /// </summary>
        /// <param name="worldItem">주울 아이템</param>
        /// <returns>요청을 보냈으면 true. 자리가 없으면 false</returns>
        public bool RequestPickup(WorldItem worldItem)
        {
            if (worldItem == null || !worldItem.CanInteract)
            {
                return false;
            }

            if (!TryReserve(worldItem.Stack, out InventoryReservation reservation))
            {
                return false;
            }

            worldItem.RequestPickup(photonView.ViewID, isApproved => FinishReservation(reservation, isApproved));
            return true;
        }

        /// <summary>
        /// 인벤토리의 아이템을 땅에 버린다. 내 인벤토리에서 바로 빼고 마스터에게 땅에 만들어 달라고 요청한다.
        /// </summary>
        /// <param name="entry">버릴 아이템</param>
        /// <returns>버렸으면 true</returns>
        public bool RequestDrop(InventoryEntry entry)
        {
            if (_inventory == null || entry == null || entry.IsReserved)
            {
                return false;
            }

            if (!PhotonNetwork.InRoom)
            {
                OnTransferFailed?.Invoke("방 밖에서는 버릴 수 없습니다");
                return false;
            }

            int itemId = entry.Item.ItemId;
            int count = entry.Count;
            if (!_inventory.Remove(entry))
            {
                return false;
            }

            Vector3 dropPosition = transform.position + transform.forward * _dropDistance;
            photonView.RPC(nameof(RpcRequestDrop), RpcTarget.MasterClient, itemId, count, dropPosition);
            return true;
        }

        /// <summary>
        /// 확인용: 테스트 아이템을 마스터를 거쳐 받는다. 마스터의 아이템 목록 사본에도 기록된다.
        /// </summary>
        [ContextMenu("디버그: 테스트 아이템 받기")]
        private void DebugGrantItems()
        {
            if (_debugGrantItems == null)
            {
                return;
            }

            foreach (ItemData item in _debugGrantItems)
            {
                if (item != null)
                {
                    RequestGrant(item, item.MaxStack);
                }
            }
        }

        /// <summary>
        /// 버리기 요청을 받는다 (마스터). 실제 처리는 SpawnDroppedItem에서 한다.
        /// </summary>
        /// <param name="itemId">아이템 ID</param>
        /// <param name="count">개수</param>
        /// <param name="position">떨어뜨릴 위치</param>
        /// <param name="info">보낸 사람 등 메시지 정보 (PUN이 자동으로 채운다)</param>
        [PunRPC]
        private void RpcRequestDrop(int itemId, int count, Vector3 position, PhotonMessageInfo info)
        {
            SpawnDroppedItem(itemId, count, position, info.Sender);
        }

        /// <summary>
        /// 디버그 지급 요청을 받는다 (마스터).
        /// </summary>
        /// <param name="itemId">아이템 ID</param>
        /// <param name="count">개수</param>
        /// <param name="requestId">요청 번호</param>
        /// <param name="info">보낸 사람 등 메시지 정보 (PUN이 자동으로 채운다)</param>
        [PunRPC]
        private void RpcRequestGrant(int itemId, int count, int requestId, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient || info.Sender == null || info.Sender != photonView.Owner)
            {
                return;
            }

            if (_ledger != null)
            {
                _ledger.Add(info.Sender.ActorNumber, itemId, count);
            }

            photonView.RPC(nameof(RpcGrantResult), info.Sender, requestId, true);
        }

        /// <summary>
        /// 내 디버그 지급 요청의 결과를 받는다.
        /// </summary>
        /// <param name="requestId">요청 번호</param>
        /// <param name="isApproved">승인이면 true</param>
        [PunRPC]
        private void RpcGrantResult(int requestId, bool isApproved)
        {
            if (!_pendingGrants.TryGetValue(requestId, out InventoryReservation reservation))
            {
                return;
            }

            _pendingGrants.Remove(requestId);
            FinishReservation(reservation, isApproved);
        }

        /// <summary>
        /// 버린 아이템을 땅에 만든다 (마스터). 요청자가 자기 캐릭터로 보냈는지, 사본에 그 아이템이 있는지 검사한다.
        /// </summary>
        /// <param name="itemId">아이템 ID</param>
        /// <param name="count">개수</param>
        /// <param name="position">떨어뜨릴 위치</param>
        /// <param name="sender">요청한 플레이어</param>
        private void SpawnDroppedItem(int itemId, int count, Vector3 position, Photon.Realtime.Player sender)
        {
            if (!PhotonNetwork.IsMasterClient || sender == null || sender != photonView.Owner)
            {
                return;
            }

            if (_ledger != null && !_ledger.TryRemove(sender.ActorNumber, itemId, count))
            {
                Debug.LogWarning($"[{nameof(PlayerItemTransfer)}] 플레이어 {sender.ActorNumber}의 사본에 아이템 {itemId} x{count}가 없어 버리기를 거절했습니다.", this);
                return;
            }

            object[] instantiationData = { itemId, count };
            PhotonNetwork.InstantiateRoomObject(PrefabPaths.WorldItem, position, Quaternion.identity, 0, instantiationData);
        }

        /// <summary>
        /// 아이템을 마스터를 거쳐 받는다. 방 밖이면 바로 넣는다.
        /// </summary>
        /// <param name="item">받을 아이템</param>
        /// <param name="count">개수</param>
        private void RequestGrant(ItemData item, int count)
        {
            if (!TryReserve(new ItemStack(item.ItemId, count), out InventoryReservation reservation))
            {
                return;
            }

            if (!PhotonNetwork.InRoom)
            {
                FinishReservation(reservation, true);
                return;
            }

            _nextGrantId++;
            _pendingGrants[_nextGrantId] = reservation;
            photonView.RPC(nameof(RpcRequestGrant), RpcTarget.MasterClient, item.ItemId, count, _nextGrantId);
        }

        /// <summary>
        /// 아이템 묶음이 들어갈 자리를 인벤토리에 예약한다. 자리가 없으면 실패 문구를 알린다.
        /// </summary>
        /// <param name="stack">넣을 아이템 묶음</param>
        /// <param name="reservation">만든 예약</param>
        /// <returns>예약했으면 true</returns>
        private bool TryReserve(ItemStack stack, out InventoryReservation reservation)
        {
            reservation = null;

            if (_inventory == null || _itemDatabase == null || !_itemDatabase.TryGetItem(stack.ItemId, out ItemData item))
            {
                return false;
            }

            if (!_inventory.TryReserve(item, stack.Count, out reservation))
            {
                OnTransferFailed?.Invoke("인벤토리 공간이 부족합니다");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 마스터의 결과에 따라 예약을 확정하거나 취소한다.
        /// </summary>
        /// <param name="reservation">예약</param>
        /// <param name="isApproved">승인이면 true</param>
        private void FinishReservation(InventoryReservation reservation, bool isApproved)
        {
            if (_inventory == null)
            {
                return;
            }

            if (isApproved)
            {
                _inventory.Commit(reservation);
                return;
            }

            _inventory.Cancel(reservation);
            OnTransferFailed?.Invoke("다른 사람이 먼저 가져갔거나 너무 멉니다");
        }
    }
}
