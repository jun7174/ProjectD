// 파일 경로: Assets/_Scripts/Combat/PlayerReloader.cs
using System;
using System.Collections;
using ProjectD.Data;
using ProjectD.Player;
using UnityEngine;

namespace ProjectD.Combat
{
    /// <summary>
    /// 재장전을 처리한다. 무기의 재장전 시간이 지나면 탄창을 채운다.
    /// 회피 중에는 재장전을 시작할 수 없고, 재장전 중에 회피하거나 무기를 바꾸면 재장전이 취소된다.
    /// </summary>
    public class PlayerReloader : MonoBehaviour
    {
        [Header("참조 (비워 두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private PlayerWeaponHolder _weaponHolder;
        [SerializeField] private PlayerDodger _dodger;

        private Coroutine _reloadRoutine;

        /// <summary>
        /// 재장전이 시작될 때 호출된다. 값은 재장전 시간(초)이다.
        /// </summary>
        public event Action<float> OnReloadStarted;

        /// <summary>
        /// 재장전이 끝나 탄창이 채워졌을 때 호출된다.
        /// </summary>
        public event Action OnReloadFinished;

        /// <summary>
        /// 재장전이 중간에 취소됐을 때 호출된다.
        /// </summary>
        public event Action OnReloadCancelled;

        /// <summary>
        /// 현재 재장전 중인지 여부
        /// </summary>
        public bool IsReloading => _reloadRoutine != null;

        /// <summary>
        /// 현재 회피 중인지 여부
        /// </summary>
        private bool IsDodging => _dodger != null && _dodger.IsDodging;

        /// <summary>
        /// 참조를 찾고 비어 있으면 경고한다.
        /// </summary>
        private void Awake()
        {
            if (_weaponHolder == null)
            {
                _weaponHolder = GetComponent<PlayerWeaponHolder>();
            }

            if (_dodger == null)
            {
                _dodger = GetComponent<PlayerDodger>();
            }

            if (_weaponHolder == null)
            {
                Debug.LogWarning($"[{nameof(PlayerReloader)}] PlayerWeaponHolder를 찾지 못했습니다. 재장전이 동작하지 않습니다.", this);
            }

            if (_dodger == null)
            {
                Debug.LogWarning($"[{nameof(PlayerReloader)}] PlayerDodger를 찾지 못했습니다. 회피에 의한 재장전 취소가 동작하지 않습니다.", this);
            }
        }

        /// <summary>
        /// 회피 시작과 무기 교체 이벤트를 구독한다.
        /// </summary>
        private void OnEnable()
        {
            if (_dodger != null)
            {
                _dodger.OnDodgeStarted += CancelReload;
            }

            if (_weaponHolder != null)
            {
                _weaponHolder.OnWeaponChanged += HandleWeaponChanged;
            }
        }

        /// <summary>
        /// 이벤트 구독을 해제하고, 진행 중인 재장전을 취소한다.
        /// </summary>
        private void OnDisable()
        {
            if (_dodger != null)
            {
                _dodger.OnDodgeStarted -= CancelReload;
            }

            if (_weaponHolder != null)
            {
                _weaponHolder.OnWeaponChanged -= HandleWeaponChanged;
            }

            CancelReload();
        }

        /// <summary>
        /// 재장전을 시도한다. 이미 재장전 중이거나, 탄창이 가득 찼거나, 회피 중이면 실패한다.
        /// </summary>
        /// <returns>재장전을 시작했으면 true</returns>
        public bool TryStartReload()
        {
            if (IsReloading || IsDodging || _weaponHolder == null)
            {
                return false;
            }

            WeaponData weapon = _weaponHolder.CurrentWeapon;
            if (weapon == null || _weaponHolder.IsMagazineFull)
            {
                return false;
            }

            _reloadRoutine = StartCoroutine(ReloadRoutine(weapon.ReloadTime));
            OnReloadStarted?.Invoke(weapon.ReloadTime);
            return true;
        }

        /// <summary>
        /// 진행 중인 재장전을 취소한다. 재장전 중이 아니면 아무것도 하지 않는다.
        /// </summary>
        public void CancelReload()
        {
            if (!IsReloading)
            {
                return;
            }

            StopCoroutine(_reloadRoutine);
            _reloadRoutine = null;
            OnReloadCancelled?.Invoke();
        }

        /// <summary>
        /// 재장전 시간만큼 기다린 뒤 탄창을 채운다.
        /// </summary>
        /// <param name="reloadTime">재장전 시간 (초)</param>
        /// <returns>코루틴 진행용 열거자</returns>
        private IEnumerator ReloadRoutine(float reloadTime)
        {
            yield return new WaitForSeconds(reloadTime);

            _reloadRoutine = null;
            _weaponHolder.RefillMagazine();
            OnReloadFinished?.Invoke();
        }

        /// <summary>
        /// 무기를 바꾸면 이전 무기의 재장전을 취소한다.
        /// </summary>
        /// <param name="weapon">새 무기</param>
        private void HandleWeaponChanged(WeaponData weapon)
        {
            CancelReload();
        }
    }
}
