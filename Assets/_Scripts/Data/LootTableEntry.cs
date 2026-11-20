// 파일 경로: Assets/_Scripts/Data/LootTableEntry.cs
using System;
using UnityEngine;

namespace ProjectD.Data
{
    /// <summary>
    /// 루팅 테이블의 한 줄: 어떤 아이템이 얼마나 자주(가중치), 몇 개(최소~최대) 나오는지.
    /// </summary>
    [Serializable]
    public class LootTableEntry
    {
        [Tooltip("나올 아이템")]
        [SerializeField] private ItemData _item;

        [Tooltip("뽑힐 가중치. 다른 줄보다 클수록 자주 나온다")]
        [Min(1)]
        [SerializeField] private int _weight = 1;

        [Tooltip("한 번 나올 때 최소 개수")]
        [Min(1)]
        [SerializeField] private int _minCount = 1;

        [Tooltip("한 번 나올 때 최대 개수. 아이템의 최대 스택을 넘지 않는다")]
        [Min(1)]
        [SerializeField] private int _maxCount = 1;

        /// <summary>나올 아이템</summary>
        public ItemData Item => _item;

        /// <summary>뽑힐 가중치</summary>
        public int Weight => _weight;

        /// <summary>최소 개수</summary>
        public int MinCount => _minCount;

        /// <summary>최대 개수</summary>
        public int MaxCount => Mathf.Max(_minCount, _maxCount);
    }
}
