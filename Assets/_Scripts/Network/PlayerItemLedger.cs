// 파일 경로: Assets/_Scripts/Network/PlayerItemLedger.cs
using System.Collections.Generic;
using Photon.Pun;
using ProjectD.Data;
using UnityEngine;

namespace ProjectD.Network
{
    /// <summary>
    /// 마스터가 들고 있는 플레이어별 아이템 목록 사본. 그리드 위치는 모르고 무엇을 몇 개 가졌는지만 안다.
    /// 마스터가 줍기를 승인하거나 버리기를 처리할 때만 바뀐다 (그리드 안에서 옮기는 것은 전송하지 않음).
    /// 버리기 요청이 실제로 가진 아이템인지 검증하고, 연결이 끊긴 플레이어의 시체를 만들 때 쓴다.
    /// 마스터가 아닌 클라이언트의 사본은 비어 있다.
    /// Raid 씬에 하나만 둔다.
    /// </summary>
    public class PlayerItemLedger : MonoBehaviourPunCallbacks
    {
        // 플레이어 번호(ActorNumber)별 아이템 묶음 목록. 같은 아이템은 하나의 묶음으로 합쳐 둔다
        private readonly Dictionary<int, List<ItemStack>> _itemsByActor = new Dictionary<int, List<ItemStack>>();

        /// <summary>
        /// 플레이어의 사본에 아이템을 더한다.
        /// </summary>
        /// <param name="actorNumber">플레이어 번호</param>
        /// <param name="itemId">아이템 ID</param>
        /// <param name="count">개수</param>
        public void Add(int actorNumber, int itemId, int count)
        {
            if (count <= 0)
            {
                return;
            }

            List<ItemStack> items = GetOrCreateList(actorNumber);
            int index = FindIndex(items, itemId);

            if (index >= 0)
            {
                items[index] = items[index].WithCount(items[index].Count + count);
            }
            else
            {
                items.Add(new ItemStack(itemId, count));
            }
        }

        /// <summary>
        /// 플레이어의 사본에서 아이템을 뺀다. 가진 개수보다 많이 빼려고 하면 아무것도 바꾸지 않는다.
        /// </summary>
        /// <param name="actorNumber">플레이어 번호</param>
        /// <param name="itemId">아이템 ID</param>
        /// <param name="count">개수</param>
        /// <returns>가지고 있어서 뺐으면 true</returns>
        public bool TryRemove(int actorNumber, int itemId, int count)
        {
            if (!_itemsByActor.TryGetValue(actorNumber, out List<ItemStack> items))
            {
                return false;
            }

            int index = FindIndex(items, itemId);
            if (index < 0 || items[index].Count < count)
            {
                return false;
            }

            int left = items[index].Count - count;
            if (left > 0)
            {
                items[index] = items[index].WithCount(left);
            }
            else
            {
                items.RemoveAt(index);
            }

            return true;
        }

        /// <summary>
        /// 플레이어가 가진 아이템 목록을 돌려준다.
        /// </summary>
        /// <param name="actorNumber">플레이어 번호</param>
        /// <returns>아이템 목록. 없으면 빈 목록</returns>
        public IReadOnlyList<ItemStack> GetItems(int actorNumber)
        {
            return GetOrCreateList(actorNumber);
        }

        /// <summary>
        /// 나간 플레이어의 사본을 지운다.
        /// </summary>
        /// <param name="otherPlayer">나간 플레이어</param>
        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            _itemsByActor.Remove(otherPlayer.ActorNumber);
        }

        /// <summary>
        /// 확인용: 모든 플레이어의 사본을 콘솔에 출력한다.
        /// </summary>
        [ContextMenu("디버그: 아이템 목록 사본 출력")]
        private void LogAll()
        {
            if (_itemsByActor.Count == 0)
            {
                Debug.Log($"[{nameof(PlayerItemLedger)}] 사본이 비어 있습니다. 마스터 화면에서 확인해 주세요.", this);
                return;
            }

            foreach (KeyValuePair<int, List<ItemStack>> pair in _itemsByActor)
            {
                List<string> lines = new List<string>();
                foreach (ItemStack stack in pair.Value)
                {
                    lines.Add($"{stack.ItemId} x{stack.Count}");
                }

                Debug.Log($"[{nameof(PlayerItemLedger)}] 플레이어 {pair.Key}: {string.Join(", ", lines)}", this);
            }
        }

        /// <summary>
        /// 플레이어의 목록을 찾고, 없으면 새로 만든다.
        /// </summary>
        /// <param name="actorNumber">플레이어 번호</param>
        /// <returns>그 플레이어의 목록</returns>
        private List<ItemStack> GetOrCreateList(int actorNumber)
        {
            if (!_itemsByActor.TryGetValue(actorNumber, out List<ItemStack> items))
            {
                items = new List<ItemStack>();
                _itemsByActor.Add(actorNumber, items);
            }

            return items;
        }

        /// <summary>
        /// 목록에서 아이템의 위치를 찾는다.
        /// </summary>
        /// <param name="items">목록</param>
        /// <param name="itemId">아이템 ID</param>
        /// <returns>위치. 없으면 -1</returns>
        private static int FindIndex(List<ItemStack> items, int itemId)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].ItemId == itemId)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
