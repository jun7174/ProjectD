// 파일 경로: Assets/_Scripts/Combat/PlayerShooter.cs
using System;
using ProjectD.Data;
using ProjectD.Player;
using UnityEngine;

namespace ProjectD.Combat
{
    /// <summary>
    /// 사격을 처리한다. 방아쇠 입력, 발사 간격, 연사 여부, 탄 소모를 확인하고,
    /// 탄퍼짐을 적용한 발사 정보를 만들어 ProjectileLauncher에 넘긴다.
    /// 회피 중이나 재장전 중에는 쏠 수 없고, 탄창이 비었을 때 쏘려고 하면 자동으로 재장전한다.
    /// </summary>
    public class PlayerShooter : MonoBehaviour
    {
        [Header("발사 위치")]
        [Tooltip("투사체가 나가는 위치. 플레이어 앞쪽에 둔 자식 오브젝트")]
        [SerializeField] private Transform _muzzle;

        [Header("참조 (비워 두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private PlayerWeaponHolder _weaponHolder;
        [SerializeField] private PlayerWeaponSpread _weaponSpread;
        [SerializeField] private PlayerReloader _reloader;
        [SerializeField] private ProjectileLauncher _launcher;
        [SerializeField] private PlayerAimer _aimer;
        [SerializeField] private PlayerDodger _dodger;

        private bool _isTriggerHeld;
        private bool _wasTriggerHeld;
        private float _nextFireTime;

        /// <summary>
        /// 한 번 발사했을 때 호출된다. 산탄총처럼 여러 발이 나가도 한 번만 호출된다.
        /// 화면 흔들림, 사운드 등 발사 연출이 구독한다.
        /// </summary>
        public event Action<WeaponData> OnFired;

        /// <summary>
        /// 참조를 찾고 비어 있으면 경고한다.
        /// </summary>
        private void Awake()
        {
            FindMissingReferences();
            WarnMissingReferences();
        }

        /// <summary>
        /// 방아쇠를 누르고 있는지 설정한다.
        /// </summary>
        /// <param name="isHeld">사격 키를 누르고 있으면 true</param>
        public void SetTriggerHeld(bool isHeld)
        {
            _isTriggerHeld = isHeld;
        }

        /// <summary>
        /// 매 프레임 방아쇠 상태와 무기 연사 여부를 보고 발사를 시도한다.
        /// </summary>
        private void Update()
        {
            bool isPressedThisFrame = _isTriggerHeld && !_wasTriggerHeld;
            _wasTriggerHeld = _isTriggerHeld;

            if (!_isTriggerHeld || _weaponHolder == null)
            {
                return;
            }

            WeaponData weapon = _weaponHolder.CurrentWeapon;
            if (weapon == null)
            {
                return;
            }

            // 연사 무기는 누르고 있는 동안, 단발 무기는 누른 순간에만 쏜다
            bool wantsToFire = weapon.IsAutomatic || isPressedThisFrame;
            if (wantsToFire)
            {
                TryFire(weapon);
            }
        }

        /// <summary>
        /// 발사 조건을 확인하고 발사한다. 탄이 없으면 재장전을 시작한다.
        /// </summary>
        /// <param name="weapon">현재 무기</param>
        private void TryFire(WeaponData weapon)
        {
            if (!CanFire())
            {
                return;
            }

            if (!_weaponHolder.TryConsumeAmmo())
            {
                if (_reloader != null)
                {
                    _reloader.TryStartReload();
                }

                return;
            }

            _nextFireTime = Time.time + weapon.FireInterval;
            FirePellets(weapon);

            if (_weaponSpread != null)
            {
                _weaponSpread.AddShotSpread();
            }

            OnFired?.Invoke(weapon);
        }

        /// <summary>
        /// 회피, 재장전, 발사 간격 조건을 확인한다.
        /// </summary>
        /// <returns>지금 발사할 수 있으면 true</returns>
        private bool CanFire()
        {
            if (_muzzle == null || _launcher == null || _aimer == null)
            {
                return false;
            }

            if (_dodger != null && _dodger.IsDodging)
            {
                return false;
            }

            if (_reloader != null && _reloader.IsReloading)
            {
                return false;
            }

            return Time.time >= _nextFireTime;
        }

        /// <summary>
        /// 무기의 한 발당 투사체 수만큼 탄퍼짐을 적용한 발사 정보를 만들어 발사한다.
        /// </summary>
        /// <param name="weapon">현재 무기</param>
        private void FirePellets(WeaponData weapon)
        {
            float spreadAngle = _weaponSpread != null ? _weaponSpread.CurrentSpreadAngle : 0f;

            for (int i = 0; i < weapon.PelletsPerShot; i++)
            {
                Vector3 direction = ApplySpread(_aimer.AimDirection, spreadAngle);
                _launcher.Launch(new FireInfo(_muzzle.position, direction, weapon));
            }
        }

        /// <summary>
        /// 조준 방향을 탄퍼짐 각도 안에서 무작위로 좌우로 틀어준다. 쿼터뷰라 수평 방향으로만 퍼진다.
        /// </summary>
        /// <param name="aimDirection">조준 방향</param>
        /// <param name="spreadAngle">한쪽 최대 각도 (도)</param>
        /// <returns>탄퍼짐이 적용된 방향</returns>
        private Vector3 ApplySpread(Vector3 aimDirection, float spreadAngle)
        {
            float randomAngle = UnityEngine.Random.Range(-spreadAngle, spreadAngle);
            return Quaternion.AngleAxis(randomAngle, Vector3.up) * aimDirection;
        }

        /// <summary>
        /// 인스펙터에서 비어 있는 참조를 같은 오브젝트에서 찾는다.
        /// </summary>
        private void FindMissingReferences()
        {
            if (_weaponHolder == null)
            {
                _weaponHolder = GetComponent<PlayerWeaponHolder>();
            }

            if (_weaponSpread == null)
            {
                _weaponSpread = GetComponent<PlayerWeaponSpread>();
            }

            if (_reloader == null)
            {
                _reloader = GetComponent<PlayerReloader>();
            }

            if (_launcher == null)
            {
                _launcher = GetComponent<ProjectileLauncher>();
            }

            if (_aimer == null)
            {
                _aimer = GetComponent<PlayerAimer>();
            }

            if (_dodger == null)
            {
                _dodger = GetComponent<PlayerDodger>();
            }
        }

        /// <summary>
        /// 찾지 못한 참조가 있으면 어떤 기능이 동작하지 않는지 경고를 출력한다.
        /// </summary>
        private void WarnMissingReferences()
        {
            if (_muzzle == null)
            {
                Debug.LogWarning($"[{nameof(PlayerShooter)}] Muzzle이 비어 있습니다. 플레이어 앞쪽에 자식 오브젝트를 만들어 연결하세요.", this);
            }

            if (_weaponHolder == null || _launcher == null || _aimer == null)
            {
                Debug.LogWarning($"[{nameof(PlayerShooter)}] PlayerWeaponHolder, ProjectileLauncher, PlayerAimer 중 찾지 못한 것이 있습니다. 사격이 동작하지 않습니다.", this);
            }

            if (_weaponSpread == null)
            {
                Debug.LogWarning($"[{nameof(PlayerShooter)}] PlayerWeaponSpread를 찾지 못했습니다. 탄퍼짐 없이 발사됩니다.", this);
            }

            if (_reloader == null)
            {
                Debug.LogWarning($"[{nameof(PlayerShooter)}] PlayerReloader를 찾지 못했습니다. 탄이 떨어져도 재장전되지 않습니다.", this);
            }

            if (_dodger == null)
            {
                Debug.LogWarning($"[{nameof(PlayerShooter)}] PlayerDodger를 찾지 못했습니다. 회피 중 사격 제한이 동작하지 않습니다.", this);
            }
        }
    }
}
