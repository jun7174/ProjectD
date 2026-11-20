// 파일 경로: Assets/_Scripts/Inventory/PlayerInventory.cs
using System;
using ProjectD.Data;
using UnityEngine;

namespace ProjectD.Inventory
{
    /// <summary>
    /// 내 캐릭터의 인벤토리. 그리드 계산(InventoryGrid)을 들고, 화면과 네트워크 쪽에 필요한 기능만 보여 준다.
    /// 그리드 배치는 로컬 전용이고, 무엇을 가지고 있는지는 마스터가 따로 사본으로 관리한다.
    /// 플레이어 프리팹 루트에 붙이고 PlayerNetworkSetup의 Local Only 목록에 넣는다.
    /// </summary>
    public class PlayerInventory : MonoBehaviour
    {
        [Tooltip("인벤토리 크기 (가로, 세로)")]
        [SerializeField] private Vector2Int _gridSize = new Vector2Int(4, 4);

        /// <summary>
        /// 그리드. 화면은 읽기만 하고, 바꿀 때는 이 컴포넌트의 함수를 쓴다
        /// </summary>
        public InventoryGrid Grid { get; private set; }

        /// <summary>
        /// 내용이 바뀌면 발생한다.
        /// </summary>
        public event Action OnChanged;

        /// <summary>
        /// 그리드를 만든다.
        /// </summary>
        private void Awake()
        {
            Grid = new InventoryGrid(_gridSize.x, _gridSize.y);
            Grid.OnChanged += HandleGridChanged;
        }

        /// <summary>
        /// 그리드 이벤트 구독을 해제한다.
        /// </summary>
        private void OnDestroy()
        {
            if (Grid != null)
            {
                Grid.OnChanged -= HandleGridChanged;
            }
        }

        /// <summary>
        /// 아이템을 넣을 자리를 예약한다. 승인되면 Commit, 거절되면 Cancel을 호출해야 한다.
        /// </summary>
        /// <param name="item">넣을 아이템</param>
        /// <param name="count">개수</param>
        /// <param name="reservation">만든 예약</param>
        /// <returns>자리가 있어 예약했으면 true</returns>
        public bool TryReserve(ItemData item, int count, out InventoryReservation reservation)
        {
            return Grid.TryReserve(item, count, out reservation);
        }

        /// <summary>
        /// 예약을 확정한다.
        /// </summary>
        /// <param name="reservation">확정할 예약</param>
        public void Commit(InventoryReservation reservation)
        {
            Grid.Commit(reservation);
        }

        /// <summary>
        /// 예약을 취소한다.
        /// </summary>
        /// <param name="reservation">취소할 예약</param>
        public void Cancel(InventoryReservation reservation)
        {
            Grid.Cancel(reservation);
        }

        /// <summary>
        /// 아이템을 옮기거나 회전한다.
        /// </summary>
        /// <param name="entry">옮길 아이템</param>
        /// <param name="position">새 왼쪽 위 칸</param>
        /// <param name="isRotated">새 회전 여부</param>
        /// <returns>옮겼으면 true</returns>
        public bool TryMove(InventoryEntry entry, Vector2Int position, bool isRotated)
        {
            return Grid.TryMove(entry, position, isRotated);
        }

        /// <summary>
        /// 같은 아이템 묶음끼리 합친다.
        /// </summary>
        /// <param name="source">옮길 묶음</param>
        /// <param name="target">받을 묶음</param>
        /// <returns>합쳤으면 true</returns>
        public bool TryMerge(InventoryEntry source, InventoryEntry target)
        {
            return Grid.TryMerge(source, target);
        }

        /// <summary>
        /// 아이템을 뺀다 (버리기).
        /// </summary>
        /// <param name="entry">뺄 아이템</param>
        /// <returns>뺐으면 true</returns>
        public bool Remove(InventoryEntry entry)
        {
            return Grid.Remove(entry);
        }

        /// <summary>
        /// 그리드가 바뀌면 바깥에 알린다.
        /// </summary>
        private void HandleGridChanged()
        {
            OnChanged?.Invoke();
        }
    }
}
