// 파일 경로: Assets/_Scripts/Combat/WeaponAnimationBridge.cs
using ProjectD.Data;
using ProjectD.Player;
using UnityEngine;

namespace ProjectD.Combat
{
    /// <summary>
    /// 사격과 재장전 상태를 PlayerAnimationController에 전달한다. Animator는 직접 건드리지 않는다.
    /// Player 모듈이 Combat 모듈을 모르게 하려고, Combat 쪽에서 Player 쪽 함수를 호출하는 방향으로 연결한다.
    /// 사격 자세는 마지막 발사 후 일정 시간 동안 유지해서, 단발 무기를 쏠 때 자세가 깜빡이지 않게 한다.
    /// </summary>
    public class WeaponAnimationBridge : MonoBehaviour
    {
        [Header("참조 (비워 두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private PlayerShooter _shooter;
        [SerializeField] private PlayerReloader _reloader;
        [SerializeField] private PlayerAnimationController _animationController;

        [Header("사격 자세")]
        [Tooltip("마지막 발사 후 사격 자세를 유지하는 시간 (초)")]
        [Min(0f)]
        [SerializeField] private float _shootStanceDuration = 0.5f;

        private float _lastFireTime = float.NegativeInfinity;

        /// <summary>
        /// 참조를 찾고 비어 있으면 경고한다.
        /// </summary>
        private void Awake()
        {
            if (_shooter == null)
            {
                _shooter = GetComponent<PlayerShooter>();
            }

            if (_reloader == null)
            {
                _reloader = GetComponent<PlayerReloader>();
            }

            if (_animationController == null)
            {
                _animationController = GetComponent<PlayerAnimationController>();
            }

            if (_animationController == null)
            {
                Debug.LogWarning($"[{nameof(WeaponAnimationBridge)}] PlayerAnimationController를 찾지 못했습니다. 사격, 재장전 애니메이션이 전달되지 않습니다.", this);
            }

            if (_shooter == null || _reloader == null)
            {
                Debug.LogWarning($"[{nameof(WeaponAnimationBridge)}] PlayerShooter 또는 PlayerReloader를 찾지 못했습니다. 사격, 재장전 애니메이션 일부가 동작하지 않습니다.", this);
            }
        }

        /// <summary>
        /// 발사와 재장전 이벤트를 구독한다.
        /// </summary>
        private void OnEnable()
        {
            if (_shooter != null)
            {
                _shooter.OnFired += HandleFired;
            }

            if (_reloader != null)
            {
                _reloader.OnReloadStarted += HandleReloadStarted;
                _reloader.OnReloadFinished += HandleReloadEnded;
                _reloader.OnReloadCancelled += HandleReloadEnded;
            }
        }

        /// <summary>
        /// 이벤트 구독을 해제하고, 사격과 재장전 상태를 꺼 둔다.
        /// </summary>
        private void OnDisable()
        {
            if (_shooter != null)
            {
                _shooter.OnFired -= HandleFired;
            }

            if (_reloader != null)
            {
                _reloader.OnReloadStarted -= HandleReloadStarted;
                _reloader.OnReloadFinished -= HandleReloadEnded;
                _reloader.OnReloadCancelled -= HandleReloadEnded;
            }

            if (_animationController != null)
            {
                _animationController.SetShooting(false);
                _animationController.SetReloading(false);
            }
        }

        /// <summary>
        /// 매 프레임 마지막 발사 후 유지 시간이 지났는지 보고 사격 자세 여부를 전달한다.
        /// </summary>
        private void Update()
        {
            if (_animationController == null)
            {
                return;
            }

            bool isShooting = Time.time - _lastFireTime < _shootStanceDuration;
            _animationController.SetShooting(isShooting);
        }

        /// <summary>
        /// 발사할 때마다 발사 시각을 기록한다.
        /// </summary>
        /// <param name="weapon">발사한 무기</param>
        private void HandleFired(WeaponData weapon)
        {
            _lastFireTime = Time.time;
        }

        /// <summary>
        /// 재장전이 시작되면 재장전 중임을 전달한다.
        /// </summary>
        /// <param name="reloadTime">재장전 시간 (초)</param>
        private void HandleReloadStarted(float reloadTime)
        {
            if (_animationController != null)
            {
                _animationController.SetReloading(true);
            }
        }

        /// <summary>
        /// 재장전이 끝나거나 취소되면 재장전 중이 아님을 전달한다.
        /// </summary>
        private void HandleReloadEnded()
        {
            if (_animationController != null)
            {
                _animationController.SetReloading(false);
            }
        }
    }
}
