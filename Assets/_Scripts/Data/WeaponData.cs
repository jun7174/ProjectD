// 파일 경로: Assets/_Scripts/Data/WeaponData.cs
using ProjectD.Core.Define;
using UnityEngine;

namespace ProjectD.Data
{
    /// <summary>
    /// 무기 하나의 수치를 정의하는 데이터.
    /// 무기를 추가할 때는 코드를 고치지 않고 이 에셋만 새로 만든다.
    /// 탄퍼짐 각도는 모두 조준 방향에서 한쪽으로 최대 몇 도까지 벗어나는지를 뜻한다.
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponData", menuName = "ProjectD/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("기본 정보")]
        [Tooltip("화면에 표시할 무기 이름")]
        [SerializeField] private string _weaponName = "새 무기";

        [Tooltip("사용하는 탄약 종류")]
        [SerializeField] private AmmoType _ammoType = AmmoType.Rifle;

        [Header("사격")]
        [Tooltip("투사체 한 발의 데미지")]
        [Min(0f)]
        [SerializeField] private float _damage = 15f;

        [Tooltip("분당 발사 수")]
        [Min(1f)]
        [SerializeField] private float _roundsPerMinute = 600f;

        [Tooltip("체크하면 누르고 있는 동안 연사, 해제하면 누를 때마다 한 발")]
        [SerializeField] private bool _isAutomatic = true;

        [Tooltip("한 번 쏠 때 나가는 투사체 수 (산탄총은 여러 발)")]
        [Min(1)]
        [SerializeField] private int _pelletsPerShot = 1;

        [Header("투사체")]
        [Tooltip("투사체 속도 (m/s)")]
        [Min(0.1f)]
        [SerializeField] private float _projectileSpeed = 60f;

        [Tooltip("투사체 최대 사거리 (m)")]
        [Min(0.1f)]
        [SerializeField] private float _maxRange = 35f;

        [Header("탄퍼짐 (한쪽 최대 각도, 도)")]
        [Tooltip("가만히 서서 처음 쏠 때의 탄퍼짐")]
        [Min(0f)]
        [SerializeField] private float _baseSpread = 3f;

        [Tooltip("한 번 쏠 때마다 누적되는 탄퍼짐")]
        [Min(0f)]
        [SerializeField] private float _spreadIncreasePerShot = 0.5f;

        [Tooltip("기본 탄퍼짐과 누적 탄퍼짐을 합한 최대값 (이동, 조준 배율 적용 전)")]
        [Min(0f)]
        [SerializeField] private float _maxSpread = 8f;

        [Tooltip("쏘지 않을 때 누적 탄퍼짐이 초당 줄어드는 양")]
        [Min(0f)]
        [SerializeField] private float _spreadRecoveryPerSecond = 6f;

        [Tooltip("이동 중 탄퍼짐 배율")]
        [Min(0f)]
        [SerializeField] private float _moveSpreadMultiplier = 1.5f;

        [Tooltip("달리기 직후 탄퍼짐 배율")]
        [Min(0f)]
        [SerializeField] private float _afterSprintSpreadMultiplier = 2f;

        [Tooltip("정밀 조준 중 탄퍼짐 배율")]
        [Min(0f)]
        [SerializeField] private float _preciseAimSpreadMultiplier = 0.5f;

        [Header("탄창")]
        [Tooltip("탄창에 들어가는 탄 수")]
        [Min(1)]
        [SerializeField] private int _magazineSize = 30;

        [Tooltip("재장전 시간 (초)")]
        [Min(0f)]
        [SerializeField] private float _reloadTime = 2f;

        [Header("연출")]
        [Tooltip("발사 시 화면 흔들림 세기. 0이면 흔들리지 않는다")]
        [Min(0f)]
        [SerializeField] private float _shakeForce = 0.15f;

        /// <summary>
        /// 화면에 표시할 무기 이름
        /// </summary>
        public string WeaponName => _weaponName;

        /// <summary>
        /// 사용하는 탄약 종류
        /// </summary>
        public AmmoType AmmoType => _ammoType;

        /// <summary>
        /// 투사체 한 발의 데미지
        /// </summary>
        public float Damage => _damage;

        /// <summary>
        /// 연사 여부
        /// </summary>
        public bool IsAutomatic => _isAutomatic;

        /// <summary>
        /// 한 번 쏠 때 나가는 투사체 수
        /// </summary>
        public int PelletsPerShot => _pelletsPerShot;

        /// <summary>
        /// 발사 사이의 최소 간격 (초). 분당 발사 수로 계산한다.
        /// </summary>
        public float FireInterval => 60f / _roundsPerMinute;

        /// <summary>
        /// 투사체 속도 (m/s)
        /// </summary>
        public float ProjectileSpeed => _projectileSpeed;

        /// <summary>
        /// 투사체 최대 사거리 (m)
        /// </summary>
        public float MaxRange => _maxRange;

        /// <summary>
        /// 기본 탄퍼짐 (도)
        /// </summary>
        public float BaseSpread => _baseSpread;

        /// <summary>
        /// 한 번 쏠 때마다 누적되는 탄퍼짐 (도)
        /// </summary>
        public float SpreadIncreasePerShot => _spreadIncreasePerShot;

        /// <summary>
        /// 배율 적용 전 탄퍼짐 최대값 (도)
        /// </summary>
        public float MaxSpread => _maxSpread;

        /// <summary>
        /// 누적 탄퍼짐의 초당 회복량 (도)
        /// </summary>
        public float SpreadRecoveryPerSecond => _spreadRecoveryPerSecond;

        /// <summary>
        /// 이동 중 탄퍼짐 배율
        /// </summary>
        public float MoveSpreadMultiplier => _moveSpreadMultiplier;

        /// <summary>
        /// 달리기 직후 탄퍼짐 배율
        /// </summary>
        public float AfterSprintSpreadMultiplier => _afterSprintSpreadMultiplier;

        /// <summary>
        /// 정밀 조준 중 탄퍼짐 배율
        /// </summary>
        public float PreciseAimSpreadMultiplier => _preciseAimSpreadMultiplier;

        /// <summary>
        /// 탄창 크기
        /// </summary>
        public int MagazineSize => _magazineSize;

        /// <summary>
        /// 재장전 시간 (초)
        /// </summary>
        public float ReloadTime => _reloadTime;

        /// <summary>
        /// 발사 시 화면 흔들림 세기
        /// </summary>
        public float ShakeForce => _shakeForce;

        /// <summary>
        /// 인스펙터 값이 바뀌면 최대 탄퍼짐이 기본 탄퍼짐보다 작아지지 않도록 맞춘다.
        /// </summary>
        private void OnValidate()
        {
            _maxSpread = Mathf.Max(_maxSpread, _baseSpread);
        }
    }
}
