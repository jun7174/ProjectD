// 파일 경로: Assets/_Scripts/UI/ItemIconPalette.cs
using ProjectD.Core.Define;
using ProjectD.Data;
using UnityEngine;

namespace ProjectD.UI
{
    /// <summary>
    /// 아이콘 그림이 없는 아이템을 분류별 색 사각형으로 보여 주기 위한 색 모음.
    /// 인벤토리 창과 상자 창이 같은 에셋을 써서 같은 아이템이 같은 색으로 보인다.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemIconPalette", menuName = "ProjectD/Item Icon Palette")]
    public class ItemIconPalette : ScriptableObject
    {
        [SerializeField] private Color _weapon = new Color(0.55f, 0.55f, 0.6f);
        [SerializeField] private Color _ammo = new Color(0.85f, 0.7f, 0.25f);
        [SerializeField] private Color _armor = new Color(0.3f, 0.45f, 0.7f);
        [SerializeField] private Color _bag = new Color(0.45f, 0.35f, 0.25f);
        [SerializeField] private Color _consumable = new Color(0.35f, 0.7f, 0.4f);
        [SerializeField] private Color _junk = new Color(0.5f, 0.45f, 0.4f);
        [SerializeField] private Color _currency = new Color(0.7f, 0.4f, 0.75f);
        [SerializeField] private Color _key = new Color(0.9f, 0.85f, 0.4f);

        /// <summary>
        /// 아이템 분류에 맞는 색을 돌려준다.
        /// </summary>
        /// <param name="item">아이템</param>
        /// <returns>분류 색</returns>
        public Color GetColor(ItemData item)
        {
            if (item == null)
            {
                return Color.gray;
            }

            switch (item.Category)
            {
                case ItemCategory.Weapon:
                    return _weapon;
                case ItemCategory.Ammo:
                    return _ammo;
                case ItemCategory.Armor:
                    return _armor;
                case ItemCategory.Bag:
                    return _bag;
                case ItemCategory.Consumable:
                    return _consumable;
                case ItemCategory.Currency:
                    return _currency;
                case ItemCategory.Key:
                    return _key;
                default:
                    return _junk;
            }
        }
    }
}
