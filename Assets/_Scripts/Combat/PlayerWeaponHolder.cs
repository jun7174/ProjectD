// 파일 경로: Assets/_Scripts/Combat/PlayerWeaponHolder.cs
using System;
using ProjectD.Data;
using UnityEngine;

namespace ProjectD.Combat
{
    /// <summary>
    /// 플레이어가 가진 무기 목록과 현재 무기, 무기별 탄창 탄 수를 관리한다.
    /// 1주차에는 테스트용으로 무기 목록을 순서대로 바꿔 가며 쓴다. 장비 슬롯 규칙은 5주차에 적용한다.
    /// 예비탄은 임시로 무한이며, 4주차 인벤토리 연동 시 교체한다.
    /// </summary>
    public class PlayerWeaponHolder : MonoBehaviour
    {
        [Tooltip("사용할 무기 목록. 첫 번째 무기를 들고 시작한다")]
        [SerializeField] private WeaponData[] _weapons;

        private int _currentIndex;
        private int[] _ammoInMagazine;

        /// <summary>
        /// 현재 무기가 바뀌었을 때 호출된다.
        /// </summary>
        public event Action<WeaponData> OnWeaponChanged;

        /// <summary>
        /// 탄창 탄 수가 바뀌었을 때 호출된다. 첫 번째 값은 현재 탄 수, 두 번째 값은 탄창 크기다.
        /// </summary>
        public event Action<int, int> OnAmmoChanged;

        /// <summary>
        /// 현재 들고 있는 무기. 무기가 없으면 null
        /// </summary>
        public WeaponData CurrentWeapon => HasWeapons ? _weapons[_currentIndex] : null;

        /// <summary>
        /// 현재 무기의 탄창에 남은 탄 수
        /// </summary>
        public int CurrentAmmo => HasWeapons ? _ammoInMagazine[_currentIndex] : 0;

        /// <summary>
        /// 현재 무기의 탄창이 가득 찼는지 여부
        /// </summary>
        public bool IsMagazineFull => CurrentWeapon != null && CurrentAmmo >= CurrentWeapon.MagazineSize;

        /// <summary>
        /// 무기를 하나 이상 가지고 있는지 여부
        /// </summary>
        private bool HasWeapons => _weapons != null && _weapons.Length > 0;

        /// <summary>
        /// 무기 목록을 확인하고, 모든 무기의 탄창을 가득 채운다.
        /// </summary>
        private void Awake()
        {
            if (!HasWeapons)
            {
                Debug.LogWarning($"[{nameof(PlayerWeaponHolder)}] 무기 목록이 비어 있습니다. 인스펙터에서 WeaponData를 연결하세요.", this);
                _ammoInMagazine = new int[0];
                return;
            }

            _ammoInMagazine = new int[_weapons.Length];
            for (int i = 0; i < _weapons.Length; i++)
            {
                if (_weapons[i] == null)
                {
                    Debug.LogWarning($"[{nameof(PlayerWeaponHolder)}] 무기 목록 {i}번이 비어 있습니다.", this);
                    continue;
                }

                _ammoInMagazine[i] = _weapons[i].MagazineSize;
            }
        }

        /// <summary>
        /// 다른 컴포넌트가 이벤트를 구독한 뒤에 처음 무기와 탄 수를 알린다.
        /// </summary>
        private void Start()
        {
            NotifyWeaponChanged();
        }

        /// <summary>
        /// 무기 목록에서 다음 무기로 바꾼다. 마지막 무기 다음은 첫 번째 무기다.
        /// </summary>
        public void SelectNextWeapon()
        {
            if (!HasWeapons)
            {
                return;
            }

            SelectWeapon((_currentIndex + 1) % _weapons.Length);
        }

        /// <summary>
        /// 무기 목록에서 이전 무기로 바꾼다. 첫 번째 무기 이전은 마지막 무기다.
        /// </summary>
        public void SelectPreviousWeapon()
        {
            if (!HasWeapons)
            {
                return;
            }

            SelectWeapon((_currentIndex - 1 + _weapons.Length) % _weapons.Length);
        }

        /// <summary>
        /// 탄창에서 탄을 한 발 사용한다.
        /// </summary>
        /// <returns>탄이 있어서 사용했으면 true</returns>
        public bool TryConsumeAmmo()
        {
            if (CurrentWeapon == null || CurrentAmmo <= 0)
            {
                return false;
            }

            _ammoInMagazine[_currentIndex]--;
            NotifyAmmoChanged();
            return true;
        }

        /// <summary>
        /// 현재 무기의 탄창을 가득 채운다. 재장전이 끝났을 때 호출한다.
        /// </summary>
        public void RefillMagazine()
        {
            if (CurrentWeapon == null)
            {
                return;
            }

            _ammoInMagazine[_currentIndex] = CurrentWeapon.MagazineSize;
            NotifyAmmoChanged();
        }

        /// <summary>
        /// 지정한 번호의 무기로 바꾸고 변경을 알린다. 같은 무기면 아무것도 하지 않는다.
        /// </summary>
        /// <param name="index">바꿀 무기의 목록 번호</param>
        private void SelectWeapon(int index)
        {
            if (index == _currentIndex || _weapons[index] == null)
            {
                return;
            }

            _currentIndex = index;
            NotifyWeaponChanged();
        }

        /// <summary>
        /// 현재 무기와 탄 수를 함께 알린다.
        /// </summary>
        private void NotifyWeaponChanged()
        {
            OnWeaponChanged?.Invoke(CurrentWeapon);
            NotifyAmmoChanged();
        }

        /// <summary>
        /// 현재 탄 수를 알린다.
        /// </summary>
        private void NotifyAmmoChanged()
        {
            int magazineSize = CurrentWeapon != null ? CurrentWeapon.MagazineSize : 0;
            OnAmmoChanged?.Invoke(CurrentAmmo, magazineSize);
        }
    }
}
