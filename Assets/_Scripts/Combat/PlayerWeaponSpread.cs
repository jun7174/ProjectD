// 파일 경로: Assets/_Scripts/Combat/PlayerWeaponSpread.cs
using ProjectD.Data;
using ProjectD.Player;
using UnityEngine;

namespace ProjectD.Combat
{
    /// <summary>
    /// 현재 탄퍼짐 각도를 계산한다.
    /// 최종 탄퍼짐 = (기본 + 누적) x 이동 배율 x 달리기 직후 배율 x 정밀 조준 배율
    /// 누적 탄퍼짐은 쏠 때마다 늘고, 쏘지 않으면 서서히 줄어든다.
    /// </summary>
    public class PlayerWeaponSpread : MonoBehaviour
    {
        [Header("참조 (비워 두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private PlayerWeaponHolder _weaponHolder;
        [SerializeField] private PlayerMover _mover;
        [SerializeField] private PlayerAimer _aimer;

        [Header("달리기 직후")]
        [Tooltip("달리기를 멈춘 뒤 달리기 직후 배율이 유지되는 시간 (초)")]
        [Min(0f)]
        [SerializeField] private float _afterSprintDuration = 0.4f;

        private float _accumulatedSpread;
        private float _lastSprintTime = float.NegativeInfinity;

        /// <summary>
        /// 현재 탄퍼짐 각도 (한쪽 최대 각도, 도)
        /// </summary>
        public float CurrentSpreadAngle { get; private set; }

        /// <summary>
        /// 참조를 찾고 비어 있으면 경고한다.
        /// </summary>
        private void Awake()
        {
            if (_weaponHolder == null)
            {
                _weaponHolder = GetComponent<PlayerWeaponHolder>();
            }

            if (_mover == null)
            {
                _mover = GetComponent<PlayerMover>();
            }

            if (_aimer == null)
            {
                _aimer = GetComponent<PlayerAimer>();
            }

            if (_weaponHolder == null)
            {
                Debug.LogWarning($"[{nameof(PlayerWeaponSpread)}] PlayerWeaponHolder를 찾지 못했습니다. 탄퍼짐이 계산되지 않습니다.", this);
            }

            if (_mover == null || _aimer == null)
            {
                Debug.LogWarning($"[{nameof(PlayerWeaponSpread)}] PlayerMover 또는 PlayerAimer를 찾지 못했습니다. 이동, 조준 배율이 적용되지 않습니다.", this);
            }
        }

        /// <summary>
        /// 무기 교체 이벤트를 구독한다.
        /// </summary>
        private void OnEnable()
        {
            if (_weaponHolder != null)
            {
                _weaponHolder.OnWeaponChanged += HandleWeaponChanged;
            }
        }

        /// <summary>
        /// 무기 교체 이벤트 구독을 해제한다.
        /// </summary>
        private void OnDisable()
        {
            if (_weaponHolder != null)
            {
                _weaponHolder.OnWeaponChanged -= HandleWeaponChanged;
            }
        }

        /// <summary>
        /// 매 프레임 달리기 시각을 기록하고, 누적 탄퍼짐을 회복시키고, 최종 탄퍼짐을 계산한다.
        /// </summary>
        private void Update()
        {
            WeaponData weapon = _weaponHolder != null ? _weaponHolder.CurrentWeapon : null;
            if (weapon == null)
            {
                CurrentSpreadAngle = 0f;
                return;
            }

            if (_mover != null && _mover.IsSprinting)
            {
                _lastSprintTime = Time.time;
            }

            _accumulatedSpread = Mathf.MoveTowards(_accumulatedSpread, 0f, weapon.SpreadRecoveryPerSecond * Time.deltaTime);
            CurrentSpreadAngle = CalculateSpread(weapon);
        }

        /// <summary>
        /// 한 번 쏠 때마다 호출해서 누적 탄퍼짐을 늘린다. 기본과 누적의 합이 최대 탄퍼짐을 넘지 않는다.
        /// </summary>
        public void AddShotSpread()
        {
            WeaponData weapon = _weaponHolder != null ? _weaponHolder.CurrentWeapon : null;
            if (weapon == null)
            {
                return;
            }

            float maxAccumulatedSpread = weapon.MaxSpread - weapon.BaseSpread;
            _accumulatedSpread = Mathf.Min(_accumulatedSpread + weapon.SpreadIncreasePerShot, maxAccumulatedSpread);
        }

        /// <summary>
        /// 현재 상태에 맞는 배율을 모두 곱해서 최종 탄퍼짐을 계산한다.
        /// </summary>
        /// <param name="weapon">현재 무기</param>
        /// <returns>최종 탄퍼짐 각도 (도)</returns>
        private float CalculateSpread(WeaponData weapon)
        {
            float spread = weapon.BaseSpread + _accumulatedSpread;

            if (_mover != null && _mover.IsMoving)
            {
                spread *= weapon.MoveSpreadMultiplier;
            }

            if (Time.time - _lastSprintTime < _afterSprintDuration)
            {
                spread *= weapon.AfterSprintSpreadMultiplier;
            }

            if (_aimer != null && _aimer.IsAimingPrecise)
            {
                spread *= weapon.PreciseAimSpreadMultiplier;
            }

            return spread;
        }

        /// <summary>
        /// 무기를 바꾸면 이전 무기의 누적 탄퍼짐을 지운다.
        /// </summary>
        /// <param name="weapon">새 무기</param>
        private void HandleWeaponChanged(WeaponData weapon)
        {
            _accumulatedSpread = 0f;
        }
    }
}
