// 파일 경로: Assets/_Scripts/Inventory/InventoryEntry.cs
using ProjectD.Data;
using UnityEngine;

namespace ProjectD.Inventory
{
    /// <summary>
    /// 인벤토리 그리드에 놓인 아이템 하나: 무엇이, 몇 개, 어느 칸에, 회전했는지.
    /// 값은 InventoryGrid만 바꾼다. 화면(UI)은 읽기만 한다.
    /// 줍기 승인을 기다리는 동안에는 예약 중(IsReserved)으로 표시되어 옮기거나 버릴 수 없다.
    /// </summary>
    public class InventoryEntry
    {
        /// <summary>
        /// 그리드에 놓인 아이템을 만든다.
        /// </summary>
        /// <param name="item">아이템 데이터</param>
        /// <param name="count">개수</param>
        /// <param name="position">왼쪽 위 칸 (x 오른쪽, y 아래쪽)</param>
        /// <param name="isRotated">90도 회전했는지 여부</param>
        public InventoryEntry(ItemData item, int count, Vector2Int position, bool isRotated)
        {
            Item = item;
            Count = count;
            Position = position;
            IsRotated = isRotated;
        }

        /// <summary>아이템 데이터</summary>
        public ItemData Item { get; }

        /// <summary>개수</summary>
        public int Count { get; private set; }

        /// <summary>왼쪽 위 칸 (x 오른쪽, y 아래쪽)</summary>
        public Vector2Int Position { get; private set; }

        /// <summary>90도 회전했는지 여부</summary>
        public bool IsRotated { get; private set; }

        /// <summary>이 칸을 쓰는 줍기 예약 수. 0보다 크면 승인을 기다리는 중</summary>
        public int ReservationCount { get; private set; }

        /// <summary>줍기 승인을 기다리는 중이라 옮기거나 버릴 수 없는지 여부</summary>
        public bool IsReserved => ReservationCount > 0;

        /// <summary>회전을 반영한 차지 칸 수</summary>
        public Vector2Int Size => GetSize(Item, IsRotated);

        /// <summary>최대 스택까지 더 넣을 수 있는 개수</summary>
        public int SpaceLeft => Item.MaxStack - Count;

        /// <summary>
        /// 아이템과 회전 여부로 차지 칸 수를 구한다. 회전하면 가로와 세로가 바뀐다.
        /// </summary>
        /// <param name="item">아이템 데이터</param>
        /// <param name="isRotated">회전 여부</param>
        /// <returns>차지 칸 수</returns>
        public static Vector2Int GetSize(ItemData item, bool isRotated)
        {
            Vector2Int size = item.GridSize;
            return isRotated ? new Vector2Int(size.y, size.x) : size;
        }

        /// <summary>
        /// 개수를 바꾼다. InventoryGrid만 호출한다.
        /// </summary>
        /// <param name="count">새 개수</param>
        public void SetCount(int count)
        {
            Count = count;
        }

        /// <summary>
        /// 위치와 회전을 바꾼다. InventoryGrid만 호출한다.
        /// </summary>
        /// <param name="position">새 왼쪽 위 칸</param>
        /// <param name="isRotated">새 회전 여부</param>
        public void SetPlacement(Vector2Int position, bool isRotated)
        {
            Position = position;
            IsRotated = isRotated;
        }

        /// <summary>
        /// 줍기 예약 수를 늘리거나 줄인다. InventoryGrid만 호출한다.
        /// </summary>
        /// <param name="delta">늘릴 수 (줄이려면 음수)</param>
        public void ChangeReservationCount(int delta)
        {
            ReservationCount = Mathf.Max(0, ReservationCount + delta);
        }
    }
}
