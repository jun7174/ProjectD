// 파일 경로: Assets/_Scripts/Inventory/InventoryGrid.cs
using System;
using System.Collections.Generic;
using ProjectD.Data;
using UnityEngine;

namespace ProjectD.Inventory
{
    /// <summary>
    /// 직사각형 아이템을 칸에 배치하는 그리드 계산. 화면이나 네트워크와 무관한 일반 클래스다.
    /// 칸마다 어떤 아이템이 차지하는지 기록해 두고, 놓기·옮기기·회전·합치기·빼기와 빈 자리 찾기를 한다.
    /// 줍기는 승인 전에 자리를 예약(TryReserve)하고, 승인되면 확정(Commit), 거절되면 취소(Cancel)한다.
    /// 좌표는 왼쪽 위가 (0, 0)이고 x는 오른쪽, y는 아래쪽으로 커진다.
    /// </summary>
    public class InventoryGrid
    {
        private readonly InventoryEntry[,] _cells;
        private readonly List<InventoryEntry> _entries = new List<InventoryEntry>();

        /// <summary>
        /// 빈 그리드를 만든다.
        /// </summary>
        /// <param name="width">가로 칸 수</param>
        /// <param name="height">세로 칸 수</param>
        public InventoryGrid(int width, int height)
        {
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);
            _cells = new InventoryEntry[Width, Height];
        }

        /// <summary>가로 칸 수</summary>
        public int Width { get; }

        /// <summary>세로 칸 수</summary>
        public int Height { get; }

        /// <summary>놓여 있는 모든 아이템</summary>
        public IReadOnlyList<InventoryEntry> Entries => _entries;

        /// <summary>
        /// 아이템이 놓이거나, 옮겨지거나, 개수가 바뀌거나, 빠지면 발생한다.
        /// </summary>
        public event Action OnChanged;

        /// <summary>
        /// 칸을 차지한 아이템을 찾는다.
        /// </summary>
        /// <param name="cell">칸 좌표</param>
        /// <returns>그 칸의 아이템. 비었거나 그리드 밖이면 null</returns>
        public InventoryEntry GetEntryAt(Vector2Int cell)
        {
            return IsInside(cell, Vector2Int.one) ? _cells[cell.x, cell.y] : null;
        }

        /// <summary>
        /// 주어진 위치에 주어진 크기를 놓을 수 있는지 확인한다.
        /// </summary>
        /// <param name="position">왼쪽 위 칸</param>
        /// <param name="size">차지 칸 수</param>
        /// <param name="ignore">겹쳐도 되는 아이템 (옮기는 자기 자신). 없으면 null</param>
        /// <returns>그리드 안이고 다른 아이템과 겹치지 않으면 true</returns>
        public bool CanPlace(Vector2Int position, Vector2Int size, InventoryEntry ignore)
        {
            if (!IsInside(position, size))
            {
                return false;
            }

            for (int x = position.x; x < position.x + size.x; x++)
            {
                for (int y = position.y; y < position.y + size.y; y++)
                {
                    InventoryEntry occupant = _cells[x, y];
                    if (occupant != null && occupant != ignore)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 아이템을 놓을 빈 자리를 위에서 아래, 왼쪽에서 오른쪽 순서로 찾는다. 그대로 안 되면 회전해서 찾는다.
        /// </summary>
        /// <param name="item">놓을 아이템</param>
        /// <param name="position">찾은 왼쪽 위 칸</param>
        /// <param name="isRotated">회전해서 찾았는지 여부</param>
        /// <returns>자리가 있으면 true</returns>
        public bool TryFindFreeSpot(ItemData item, out Vector2Int position, out bool isRotated)
        {
            isRotated = false;
            if (TryFindFreeSpot(InventoryEntry.GetSize(item, false), out position))
            {
                return true;
            }

            bool isSquare = item.GridSize.x == item.GridSize.y;
            if (!isSquare && TryFindFreeSpot(InventoryEntry.GetSize(item, true), out position))
            {
                isRotated = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 아이템을 넣을 자리를 예약한다. 같은 아이템의 덜 찬 묶음에 먼저 더하고, 남은 개수는 빈 자리에 새로 놓는다.
        /// 예약한 묶음은 확정이나 취소 전까지 옮기거나 버릴 수 없다. 다 넣을 수 없으면 아무것도 바꾸지 않는다.
        /// </summary>
        /// <param name="item">넣을 아이템</param>
        /// <param name="count">넣을 개수. 최대 스택 이하여야 한다</param>
        /// <param name="reservation">만든 예약. 실패하면 null</param>
        /// <returns>예약했으면 true</returns>
        public bool TryReserve(ItemData item, int count, out InventoryReservation reservation)
        {
            reservation = null;
            if (item == null || count <= 0 || count > item.MaxStack)
            {
                return false;
            }

            // 먼저 계산만 해서 다 들어가는지 확인하고, 들어갈 때만 실제로 바꾼다
            int remaining = count;
            foreach (InventoryEntry entry in _entries)
            {
                if (entry.Item == item)
                {
                    remaining -= Mathf.Min(entry.SpaceLeft, remaining);
                }
            }

            Vector2Int freePosition = Vector2Int.zero;
            bool isRotated = false;
            if (remaining > 0 && !TryFindFreeSpot(item, out freePosition, out isRotated))
            {
                return false;
            }

            reservation = new InventoryReservation(item, count);
            remaining = count;

            foreach (InventoryEntry entry in _entries)
            {
                if (remaining <= 0 || entry.Item != item || entry.SpaceLeft <= 0)
                {
                    continue;
                }

                int added = Mathf.Min(entry.SpaceLeft, remaining);
                entry.SetCount(entry.Count + added);
                entry.ChangeReservationCount(1);
                reservation.MergedEntries.Add(entry);
                reservation.MergedCounts.Add(added);
                remaining -= added;
            }

            if (remaining > 0)
            {
                InventoryEntry newEntry = Place(item, remaining, freePosition, isRotated);
                newEntry.ChangeReservationCount(1);
                reservation.NewEntry = newEntry;
            }

            OnChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// 예약을 확정한다. 묶음의 예약 표시만 풀고 개수는 그대로 둔다.
        /// </summary>
        /// <param name="reservation">확정할 예약</param>
        public void Commit(InventoryReservation reservation)
        {
            if (reservation == null || reservation.IsFinished)
            {
                return;
            }

            foreach (InventoryEntry entry in reservation.MergedEntries)
            {
                entry.ChangeReservationCount(-1);
            }

            if (reservation.NewEntry != null)
            {
                reservation.NewEntry.ChangeReservationCount(-1);
            }

            reservation.IsFinished = true;
            OnChanged?.Invoke();
        }

        /// <summary>
        /// 예약을 취소한다. 기존 묶음에 더한 개수를 빼고, 새로 놓은 칸을 비운다.
        /// </summary>
        /// <param name="reservation">취소할 예약</param>
        public void Cancel(InventoryReservation reservation)
        {
            if (reservation == null || reservation.IsFinished)
            {
                return;
            }

            for (int i = 0; i < reservation.MergedEntries.Count; i++)
            {
                InventoryEntry entry = reservation.MergedEntries[i];
                entry.SetCount(entry.Count - reservation.MergedCounts[i]);
                entry.ChangeReservationCount(-1);
            }

            if (reservation.NewEntry != null)
            {
                RemoveInternal(reservation.NewEntry);
            }

            reservation.IsFinished = true;
            OnChanged?.Invoke();
        }

        /// <summary>
        /// 아이템을 다른 칸으로 옮기거나 회전한다. 예약 중이거나 놓을 수 없으면 그대로 둔다.
        /// </summary>
        /// <param name="entry">옮길 아이템</param>
        /// <param name="position">새 왼쪽 위 칸</param>
        /// <param name="isRotated">새 회전 여부</param>
        /// <returns>옮겼으면 true</returns>
        public bool TryMove(InventoryEntry entry, Vector2Int position, bool isRotated)
        {
            if (entry == null || entry.IsReserved || !_entries.Contains(entry))
            {
                return false;
            }

            Vector2Int newSize = InventoryEntry.GetSize(entry.Item, isRotated);
            if (!CanPlace(position, newSize, entry))
            {
                return false;
            }

            SetCells(entry, null);
            entry.SetPlacement(position, isRotated);
            SetCells(entry, entry);
            OnChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// 같은 아이템 묶음끼리 합친다. 대상의 최대 스택까지만 옮기고, 원래 묶음이 비면 뺀다.
        /// </summary>
        /// <param name="source">옮길 묶음</param>
        /// <param name="target">받을 묶음</param>
        /// <returns>하나라도 옮겼으면 true</returns>
        public bool TryMerge(InventoryEntry source, InventoryEntry target)
        {
            if (source == null || target == null || source == target || source.Item != target.Item)
            {
                return false;
            }

            if (source.IsReserved || target.IsReserved || target.SpaceLeft <= 0)
            {
                return false;
            }

            int moved = Mathf.Min(target.SpaceLeft, source.Count);
            target.SetCount(target.Count + moved);
            source.SetCount(source.Count - moved);

            if (source.Count <= 0)
            {
                RemoveInternal(source);
            }

            OnChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// 아이템을 그리드에서 뺀다. 예약 중이면 빼지 않는다.
        /// </summary>
        /// <param name="entry">뺄 아이템</param>
        /// <returns>뺐으면 true</returns>
        public bool Remove(InventoryEntry entry)
        {
            if (entry == null || entry.IsReserved || !_entries.Contains(entry))
            {
                return false;
            }

            RemoveInternal(entry);
            OnChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// 주어진 크기가 들어갈 빈 자리를 위에서 아래, 왼쪽에서 오른쪽 순서로 찾는다.
        /// </summary>
        /// <param name="size">차지 칸 수</param>
        /// <param name="position">찾은 왼쪽 위 칸</param>
        /// <returns>자리가 있으면 true</returns>
        private bool TryFindFreeSpot(Vector2Int size, out Vector2Int position)
        {
            for (int y = 0; y <= Height - size.y; y++)
            {
                for (int x = 0; x <= Width - size.x; x++)
                {
                    position = new Vector2Int(x, y);
                    if (CanPlace(position, size, null))
                    {
                        return true;
                    }
                }
            }

            position = Vector2Int.zero;
            return false;
        }

        /// <summary>
        /// 아이템을 새로 놓는다. 자리가 비어 있는지는 호출하는 쪽이 확인한다.
        /// </summary>
        /// <param name="item">아이템</param>
        /// <param name="count">개수</param>
        /// <param name="position">왼쪽 위 칸</param>
        /// <param name="isRotated">회전 여부</param>
        /// <returns>놓은 아이템</returns>
        private InventoryEntry Place(ItemData item, int count, Vector2Int position, bool isRotated)
        {
            InventoryEntry entry = new InventoryEntry(item, count, position, isRotated);
            _entries.Add(entry);
            SetCells(entry, entry);
            return entry;
        }

        /// <summary>
        /// 아이템이 차지한 칸을 비우고 목록에서 뺀다.
        /// </summary>
        /// <param name="entry">뺄 아이템</param>
        private void RemoveInternal(InventoryEntry entry)
        {
            SetCells(entry, null);
            _entries.Remove(entry);
        }

        /// <summary>
        /// 아이템이 차지하는 칸들에 값을 쓴다.
        /// </summary>
        /// <param name="entry">위치와 크기를 읽을 아이템</param>
        /// <param name="value">칸에 쓸 값 (차지하면 그 아이템, 비우면 null)</param>
        private void SetCells(InventoryEntry entry, InventoryEntry value)
        {
            Vector2Int size = entry.Size;
            for (int x = entry.Position.x; x < entry.Position.x + size.x; x++)
            {
                for (int y = entry.Position.y; y < entry.Position.y + size.y; y++)
                {
                    _cells[x, y] = value;
                }
            }
        }

        /// <summary>
        /// 주어진 위치와 크기가 그리드 안에 들어가는지 확인한다.
        /// </summary>
        /// <param name="position">왼쪽 위 칸</param>
        /// <param name="size">차지 칸 수</param>
        /// <returns>그리드 안이면 true</returns>
        private bool IsInside(Vector2Int position, Vector2Int size)
        {
            return position.x >= 0 && position.y >= 0
                && position.x + size.x <= Width
                && position.y + size.y <= Height;
        }
    }
}
