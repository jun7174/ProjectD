// 파일 경로: Assets/_Scripts/Inventory/InventoryReservation.cs
using System.Collections.Generic;
using ProjectD.Data;

namespace ProjectD.Inventory
{
    /// <summary>
    /// 마스터의 줍기 승인을 기다리는 동안 인벤토리에 미리 잡아 둔 자리.
    /// 기존 묶음에 더한 개수와 새로 놓은 칸을 기록해 두었다가,
    /// 승인되면 그대로 확정하고 거절되면 기록대로 되돌린다.
    /// 만들고 확정·취소하는 일은 InventoryGrid가 한다.
    /// </summary>
    public class InventoryReservation
    {
        /// <summary>
        /// 예약을 만든다.
        /// </summary>
        /// <param name="item">예약한 아이템</param>
        /// <param name="count">예약한 개수</param>
        public InventoryReservation(ItemData item, int count)
        {
            Item = item;
            Count = count;
        }

        /// <summary>예약한 아이템</summary>
        public ItemData Item { get; }

        /// <summary>예약한 개수</summary>
        public int Count { get; }

        /// <summary>확정이나 취소가 끝났는지 여부</summary>
        public bool IsFinished { get; set; }

        /// <summary>개수를 더한 기존 묶음들</summary>
        public List<InventoryEntry> MergedEntries { get; } = new List<InventoryEntry>();

        /// <summary>기존 묶음마다 더한 개수 (MergedEntries와 같은 순서)</summary>
        public List<int> MergedCounts { get; } = new List<int>();

        /// <summary>새로 놓은 칸. 기존 묶음에 모두 들어갔으면 null</summary>
        public InventoryEntry NewEntry { get; set; }
    }
}
