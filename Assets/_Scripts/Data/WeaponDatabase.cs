// 파일 경로: Assets/_Scripts/Data/WeaponDatabase.cs
using System.Collections.Generic;
using UnityEngine;

namespace ProjectD.Data
{
    /// <summary>
    /// 게임에 있는 모든 무기 데이터를 모아 두고, 무기 ID로 무기 데이터를 찾아 준다.
    /// ScriptableObject는 네트워크로 직접 보낼 수 없으므로, 보낼 때는 무기 ID(int)를 보내고 받는 쪽이 여기서 찾는다.
    /// 인벤토리, 상점과 저장에서도 같은 ID로 무기를 찾는다.
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponDatabase", menuName = "ProjectD/Weapon Database")]
    public class WeaponDatabase : ScriptableObject
    {
        [Tooltip("게임에 있는 모든 무기. 새 무기를 만들면 여기에 추가한다. 무기 ID는 서로 겹치면 안 된다")]
        [SerializeField] private WeaponData[] _weapons;

        // 무기 ID로 바로 찾기 위한 사전. 처음 찾을 때 만들고, 인스펙터에서 목록을 바꾸면 다시 만든다
        private Dictionary<int, WeaponData> _weaponsById;

        /// <summary>
        /// 무기 ID로 무기 데이터를 찾는다.
        /// </summary>
        /// <param name="weaponId">찾을 무기 ID</param>
        /// <param name="weapon">찾은 무기 데이터. 없으면 null</param>
        /// <returns>찾았으면 true</returns>
        public bool TryGetWeapon(int weaponId, out WeaponData weapon)
        {
            if (_weaponsById == null)
            {
                BuildLookup();
            }

            return _weaponsById.TryGetValue(weaponId, out weapon);
        }

        /// <summary>
        /// 인스펙터에서 값이 바뀌면 찾기용 사전을 비우고, 비어 있는 칸과 겹치는 ID를 검사한다.
        /// </summary>
        private void OnValidate()
        {
            _weaponsById = null;
            ValidateWeapons();
        }

        /// <summary>
        /// 무기 목록으로 ID 찾기용 사전을 만든다. 비어 있는 칸과 겹치는 ID는 건너뛴다.
        /// </summary>
        private void BuildLookup()
        {
            _weaponsById = new Dictionary<int, WeaponData>();

            if (_weapons == null)
            {
                return;
            }

            foreach (WeaponData weapon in _weapons)
            {
                if (weapon == null || _weaponsById.ContainsKey(weapon.WeaponId))
                {
                    continue;
                }

                _weaponsById.Add(weapon.WeaponId, weapon);
            }
        }

        /// <summary>
        /// 비어 있는 칸, ID가 0인 무기, ID가 겹치는 무기가 있으면 경고를 출력한다.
        /// </summary>
        private void ValidateWeapons()
        {
            if (_weapons == null)
            {
                return;
            }

            HashSet<int> usedIds = new HashSet<int>();

            foreach (WeaponData weapon in _weapons)
            {
                if (weapon == null)
                {
                    Debug.LogWarning($"[{nameof(WeaponDatabase)}] 무기 목록에 비어 있는 칸이 있습니다.", this);
                    continue;
                }

                if (weapon.WeaponId == 0)
                {
                    Debug.LogWarning($"[{nameof(WeaponDatabase)}] {weapon.name}의 무기 ID가 0입니다. 1000번대 번호를 지정해 주세요.", weapon);
                }

                if (!usedIds.Add(weapon.WeaponId))
                {
                    Debug.LogWarning($"[{nameof(WeaponDatabase)}] 무기 ID {weapon.WeaponId}가 겹칩니다 ({weapon.name}). 무기 ID는 서로 달라야 합니다.", weapon);
                }
            }
        }
    }
}
