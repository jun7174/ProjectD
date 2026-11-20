// 파일 경로: Assets/_Scripts/Inventory/LootContainer.cs
using System;
using System.Collections.Generic;
using ProjectD.Data;
using UnityEngine;

namespace ProjectD.Inventory
{
    /// <summary>
    /// 루팅 상자(상자, 적 시체)의 내용물 목록. 항목은 번호(목록 순서)로 가리킨다.
    /// 꺼낸 항목은 지우지 않고 개수를 0으로 바꿔, 다른 항목의 번호가 바뀌지 않게 한다.
    /// 내용물을 정하고 바꾸는 일은 네트워크 쪽(LootContainerSync)이 이 컴포넌트의 함수로 한다.
    /// 상자와 적 프리팹 루트에 붙이고, 모든 클라이언트에서 켜 둔다.
    /// </summary>
    public class LootContainer : MonoBehaviour
    {
        [Tooltip("화면에 표시할 이름 (예: 공구 상자, 돌격형 시체)")]
        [SerializeField] private string _displayName = "상자";

        private readonly List<ItemStack> _entries = new List<ItemStack>();

        /// <summary>화면에 표시할 이름</summary>
        public string DisplayName => _displayName;

        /// <summary>내용물을 받았는지 여부. 받기 전에는 열 수 없다</summary>
        public bool HasContents { get; private set; }

        /// <summary>항목 목록 (꺼낸 항목은 개수 0)</summary>
        public IReadOnlyList<ItemStack> Entries => _entries;

        /// <summary>남은 아이템이 하나도 없는지 여부</summary>
        public bool IsEmpty
        {
            get
            {
                foreach (ItemStack entry in _entries)
                {
                    if (!entry.IsEmpty)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// 내용물이 정해지거나 항목이 빠지면 발생한다.
        /// </summary>
        public event Action OnChanged;

        /// <summary>
        /// 내용물을 통째로 정한다.
        /// </summary>
        /// <param name="entries">항목 목록 (꺼낸 항목은 개수 0)</param>
        public void SetContents(IReadOnlyList<ItemStack> entries)
        {
            _entries.Clear();
            for (int i = 0; i < entries.Count; i++)
            {
                _entries.Add(entries[i]);
            }

            HasContents = true;
            OnChanged?.Invoke();
        }

        /// <summary>
        /// 남아 있는 항목을 찾는다.
        /// </summary>
        /// <param name="index">항목 번호</param>
        /// <param name="entry">찾은 항목</param>
        /// <returns>번호가 맞고 개수가 1 이상이면 true</returns>
        public bool TryGetEntry(int index, out ItemStack entry)
        {
            if (index < 0 || index >= _entries.Count || _entries[index].IsEmpty)
            {
                entry = default;
                return false;
            }

            entry = _entries[index];
            return true;
        }

        /// <summary>
        /// 항목을 꺼낸 것으로 표시한다. 번호를 유지하기 위해 개수만 0으로 바꾼다.
        /// </summary>
        /// <param name="index">항목 번호</param>
        public void RemoveEntry(int index)
        {
            if (index < 0 || index >= _entries.Count)
            {
                return;
            }

            _entries[index] = _entries[index].WithCount(0);
            OnChanged?.Invoke();
        }
    }
}
