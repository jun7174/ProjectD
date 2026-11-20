// 파일 경로: Assets/_Scripts/Data/LootTable.cs
using System.Collections.Generic;
using UnityEngine;

namespace ProjectD.Data
{
    /// <summary>
    /// 상자나 적 시체에서 나올 아이템을 정하는 표.
    /// 뽑는 횟수(최소~최대)만큼 가중치로 한 줄씩 골라, 그 줄의 개수 범위에서 개수를 정한다.
    /// 상자 등급(일반, 군용, 희귀)과 적 종류마다 에셋을 하나씩 만든다.
    /// 마스터만 뽑고, 결과는 아이템 묶음(ItemStack)으로 모두에게 보낸다.
    /// </summary>
    [CreateAssetMenu(fileName = "LootTable", menuName = "ProjectD/Loot Table")]
    public class LootTable : ScriptableObject
    {
        [Tooltip("나올 수 있는 아이템 목록")]
        [SerializeField] private LootTableEntry[] _entries;

        [Tooltip("최소로 뽑는 횟수. 0이면 빈 상자가 나올 수 있다")]
        [Min(0)]
        [SerializeField] private int _minRolls = 1;

        [Tooltip("최대로 뽑는 횟수")]
        [Min(0)]
        [SerializeField] private int _maxRolls = 3;

        /// <summary>
        /// 내용물을 뽑아 목록에 담는다. 목록은 먼저 비운다. 같은 아이템이 여러 번 나오면 따로 담는다.
        /// </summary>
        /// <param name="results">뽑은 아이템 묶음을 담을 목록</param>
        public void Roll(List<ItemStack> results)
        {
            results.Clear();

            int totalWeight = GetTotalWeight();
            if (totalWeight <= 0)
            {
                return;
            }

            int rollCount = Random.Range(_minRolls, _maxRolls + 1);
            for (int i = 0; i < rollCount; i++)
            {
                LootTableEntry entry = PickEntry(totalWeight);
                if (entry == null)
                {
                    continue;
                }

                int count = Random.Range(entry.MinCount, entry.MaxCount + 1);
                count = Mathf.Clamp(count, 1, entry.Item.MaxStack);
                results.Add(new ItemStack(entry.Item.ItemId, count));
            }
        }

        /// <summary>
        /// 아이템이 있는 줄의 가중치 합을 구한다.
        /// </summary>
        /// <returns>가중치 합</returns>
        private int GetTotalWeight()
        {
            int total = 0;

            if (_entries == null)
            {
                return total;
            }

            foreach (LootTableEntry entry in _entries)
            {
                if (entry != null && entry.Item != null)
                {
                    total += entry.Weight;
                }
            }

            return total;
        }

        /// <summary>
        /// 가중치에 비례하는 확률로 한 줄을 고른다.
        /// </summary>
        /// <param name="totalWeight">가중치 합</param>
        /// <returns>고른 줄. 없으면 null</returns>
        private LootTableEntry PickEntry(int totalWeight)
        {
            // 0 이상 합계 미만의 수를 뽑고, 줄마다 가중치를 빼 가며 그 수가 들어가는 줄을 찾는다
            int pick = Random.Range(0, totalWeight);

            foreach (LootTableEntry entry in _entries)
            {
                if (entry == null || entry.Item == null)
                {
                    continue;
                }

                if (pick < entry.Weight)
                {
                    return entry;
                }

                pick -= entry.Weight;
            }

            return null;
        }

        /// <summary>
        /// 인스펙터 값이 바뀌면 최대 횟수가 최소 횟수보다 작아지지 않게 맞추고, 빈 줄을 경고한다.
        /// </summary>
        private void OnValidate()
        {
            _maxRolls = Mathf.Max(_maxRolls, _minRolls);

            if (_entries == null)
            {
                return;
            }

            foreach (LootTableEntry entry in _entries)
            {
                if (entry != null && entry.Item == null)
                {
                    Debug.LogWarning($"[{nameof(LootTable)}] {name}에 아이템이 비어 있는 줄이 있습니다. 그 줄은 뽑히지 않습니다.", this);
                }
            }
        }
    }
}
