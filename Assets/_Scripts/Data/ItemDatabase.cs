// 파일 경로: Assets/_Scripts/Data/ItemDatabase.cs
using System.Collections.Generic;
using UnityEngine;

namespace ProjectD.Data
{
    /// <summary>
    /// 게임에 있는 모든 아이템(무기 포함)을 모아 두고, 아이템 ID로 데이터를 찾아 준다.
    /// ScriptableObject는 네트워크로 직접 보낼 수 없으므로, 보낼 때는 ID(int)를 보내고 받는 쪽이 여기서 찾는다.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "ProjectD/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        [Tooltip("게임에 있는 모든 아이템. 새 아이템을 만들면 여기에 추가한다. ID는 서로 겹치면 안 된다")]
        [SerializeField] private ItemData[] _items;

        // ID로 바로 찾기 위한 사전. 처음 찾을 때 만들고, 인스펙터에서 목록을 바꾸면 다시 만든다
        private Dictionary<int, ItemData> _itemsById;

        /// <summary>
        /// 아이템 ID로 아이템 데이터를 찾는다.
        /// </summary>
        /// <param name="itemId">찾을 아이템 ID</param>
        /// <param name="item">찾은 아이템 데이터. 없으면 null</param>
        /// <returns>찾았으면 true</returns>
        public bool TryGetItem(int itemId, out ItemData item)
        {
            if (_itemsById == null)
            {
                BuildLookup();
            }

            return _itemsById.TryGetValue(itemId, out item);
        }

        /// <summary>
        /// 아이템 ID로 무기 데이터를 찾는다. ID가 있어도 무기가 아니면 실패한다.
        /// </summary>
        /// <param name="weaponId">찾을 무기 ID</param>
        /// <param name="weapon">찾은 무기 데이터. 없으면 null</param>
        /// <returns>찾았고 무기이면 true</returns>
        public bool TryGetWeapon(int weaponId, out WeaponData weapon)
        {
            weapon = null;

            if (TryGetItem(weaponId, out ItemData item))
            {
                weapon = item as WeaponData;
            }

            return weapon != null;
        }

        /// <summary>
        /// 인스펙터에서 값이 바뀌면 찾기용 사전을 비우고, 비어 있는 칸과 겹치는 ID를 검사한다.
        /// </summary>
        private void OnValidate()
        {
            _itemsById = null;
            ValidateItems();
        }

        /// <summary>
        /// 아이템 목록으로 ID 찾기용 사전을 만든다. 비어 있는 칸과 겹치는 ID는 건너뛴다.
        /// </summary>
        private void BuildLookup()
        {
            _itemsById = new Dictionary<int, ItemData>();

            if (_items == null)
            {
                return;
            }

            foreach (ItemData item in _items)
            {
                if (item == null || _itemsById.ContainsKey(item.ItemId))
                {
                    continue;
                }

                _itemsById.Add(item.ItemId, item);
            }
        }

        /// <summary>
        /// 비어 있는 칸, ID가 0인 아이템, ID가 겹치는 아이템이 있으면 경고를 출력한다.
        /// </summary>
        private void ValidateItems()
        {
            if (_items == null)
            {
                return;
            }

            HashSet<int> usedIds = new HashSet<int>();

            foreach (ItemData item in _items)
            {
                if (item == null)
                {
                    Debug.LogWarning($"[{nameof(ItemDatabase)}] 아이템 목록에 비어 있는 칸이 있습니다.", this);
                    continue;
                }

                if (item.ItemId == 0)
                {
                    Debug.LogWarning($"[{nameof(ItemDatabase)}] {item.name}의 아이템 ID가 0입니다. 분류에 맞는 번호를 지정해 주세요.", item);
                }

                if (!usedIds.Add(item.ItemId))
                {
                    Debug.LogWarning($"[{nameof(ItemDatabase)}] 아이템 ID {item.ItemId}가 겹칩니다 ({item.name}). ID는 서로 달라야 합니다.", item);
                }
            }
        }
    }
}
