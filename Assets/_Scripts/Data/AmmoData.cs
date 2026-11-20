// 파일 경로: Assets/_Scripts/Data/AmmoData.cs
using ProjectD.Core.Define;
using UnityEngine;

namespace ProjectD.Data
{
    /// <summary>
    /// 탄약 아이템 데이터. 어떤 탄 종류인지를 더한다.
    /// 재장전할 때 무기의 탄 종류와 같은 탄약을 인벤토리에서 찾아 차감하는 데 쓴다.
    /// </summary>
    [CreateAssetMenu(fileName = "AmmoData", menuName = "ProjectD/Ammo Data")]
    public class AmmoData : ItemData
    {
        [Header("탄약")]
        [Tooltip("탄 종류. 같은 종류를 쓰는 무기에 장전된다")]
        [SerializeField] private AmmoType _ammoType = AmmoType.Rifle;

        /// <summary>탄 종류</summary>
        public AmmoType AmmoType => _ammoType;

        /// <summary>
        /// 공통 검사 후 분류를 탄약으로 고정한다.
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            SetCategory(ItemCategory.Ammo);
        }
    }
}
