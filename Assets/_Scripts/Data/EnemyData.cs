// 파일 경로: Assets/_Scripts/Data/EnemyData.cs
using ProjectD.Core.Define;
using UnityEngine;

namespace ProjectD.Data
{
    /// <summary>
    /// 적 한 종류의 수치를 정의한다. 돌격형, 사수형마다 에셋을 하나씩 만든다.
    /// 적 AI 코드에는 수치를 쓰지 않고 이 에셋의 값을 읽는다.
    /// </summary>
    [CreateAssetMenu(fileName = "ED_NewEnemy", menuName = "ProjectD/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("기본 정보")]
        [Tooltip("적 종류 고유 ID. 네트워크 전송에 쓴다. 적은 2000번대를 쓰고 서로 겹치면 안 된다")]
        [SerializeField] private int _enemyId;

        [Tooltip("표시 이름")]
        [SerializeField] private string _displayName;

        [Tooltip("Resources 폴더 안의 프리팹 이름(확장자 제외). 네트워크 생성에 쓴다")]
        [SerializeField] private string _resourcePrefabName;

        [Header("체력")]
        [Tooltip("1인 기준 최대 체력. 인원별 배율은 EnemyBalanceSettings에서 곱한다")]
        [Min(1f)]
        [SerializeField] private float _maxHealth = 100f;

        [Header("이동")]
        [Tooltip("순찰 속도 (m/s)")]
        [Min(0f)]
        [SerializeField] private float _patrolSpeed = 2f;

        [Tooltip("추적 속도 (m/s)")]
        [Min(0f)]
        [SerializeField] private float _chaseSpeed = 4f;

        [Header("시야")]
        [Tooltip("시야 거리 (m)")]
        [Min(0f)]
        [SerializeField] private float _sightDistance = 15f;

        [Tooltip("시야 각도 (도). 정면 기준 전체 각도")]
        [Range(1f, 360f)]
        [SerializeField] private float _sightAngle = 100f;

        [Header("경계 게이지 (0~1)")]
        [Tooltip("시야 안에 있을 때 초당 차는 양 (가장 먼 거리 기준. 가까울수록 더 빨리 참)")]
        [Min(0f)]
        [SerializeField] private float _alertGainPerSecond = 0.5f;

        [Tooltip("시야 밖일 때 초당 줄어드는 양")]
        [Min(0f)]
        [SerializeField] private float _alertDecayPerSecond = 0.2f;

        [Header("공격")]
        [Tooltip("공격을 시작하는 거리 (m)")]
        [Min(0f)]
        [SerializeField] private float _attackRange = 3f;

        [Tooltip("공격 예고 동작 시간 (초). 0.3~0.6초 권장")]
        [Min(0f)]
        [SerializeField] private float _telegraphDuration = 0.5f;

        [Tooltip("공격 한 번의 데미지. 돌진은 한 번 닿을 때, 점사는 탄 한 발마다")]
        [Min(0f)]
        [SerializeField] private float _attackDamage = 10f;

        [Tooltip("공격 방식. 돌격형은 Charge, 사수형은 Burst")]
        [SerializeField] private EnemyAttackType _attackType = EnemyAttackType.Charge;

        [Header("돌진 공격 (Charge)")]
        [Tooltip("돌진 거리 (m)")]
        [Min(0f)]
        [SerializeField] private float _chargeDistance = 6f;

        [Tooltip("돌진 속도 (m/s)")]
        [Min(0.1f)]
        [SerializeField] private float _chargeSpeed = 14f;

        [Tooltip("돌진이 끝난 뒤 멈춰 있는 빈틈 (초)")]
        [Min(0f)]
        [SerializeField] private float _chargeRecoverSeconds = 0.8f;

        [Header("점사 공격 (Burst)")]
        [Tooltip("적 탄의 비행 수치(탄속, 사거리)를 읽는 무기 데이터. 탄속은 플레이어 탄의 1/3~1/2 권장")]
        [SerializeField] private WeaponData _projectileWeapon;

        [Tooltip("한 번 점사에 쏘는 탄 수")]
        [Min(1)]
        [SerializeField] private int _burstCount = 3;

        [Tooltip("점사 안에서 탄 사이 간격 (초)")]
        [Min(0f)]
        [SerializeField] private float _burstInterval = 0.15f;

        [Tooltip("점사가 끝나고 다음 예고까지 쉬는 시간 (초)")]
        [Min(0f)]
        [SerializeField] private float _burstCooldownSeconds = 1.2f;

        [Tooltip("대상이 이 거리(m)보다 가까우면 쉬는 동안 뒤로 물러난다")]
        [Min(0f)]
        [SerializeField] private float _retreatDistance = 5f;

        [Header("복귀")]
        [Tooltip("타겟을 놓친 뒤 이 시간(초)이 지나면 순찰 경로로 돌아간다")]
        [Min(0f)]
        [SerializeField] private float _returnAfterLostSeconds = 5f;

        /// <summary>적 종류 고유 ID (2000번대)</summary>
        public int EnemyId => _enemyId;

        /// <summary>표시 이름</summary>
        public string DisplayName => _displayName;

        /// <summary>Resources 폴더 안의 프리팹 이름</summary>
        public string ResourcePrefabName => _resourcePrefabName;

        /// <summary>1인 기준 최대 체력</summary>
        public float MaxHealth => _maxHealth;

        /// <summary>순찰 속도</summary>
        public float PatrolSpeed => _patrolSpeed;

        /// <summary>추적 속도</summary>
        public float ChaseSpeed => _chaseSpeed;

        /// <summary>시야 거리</summary>
        public float SightDistance => _sightDistance;

        /// <summary>시야 각도 (정면 기준 전체 각도)</summary>
        public float SightAngle => _sightAngle;

        /// <summary>시야 안에서 초당 경계 게이지 증가량</summary>
        public float AlertGainPerSecond => _alertGainPerSecond;

        /// <summary>시야 밖에서 초당 경계 게이지 감소량</summary>
        public float AlertDecayPerSecond => _alertDecayPerSecond;

        /// <summary>공격 시작 거리</summary>
        public float AttackRange => _attackRange;

        /// <summary>공격 예고 동작 시간</summary>
        public float TelegraphDuration => _telegraphDuration;

        /// <summary>공격 한 번의 데미지</summary>
        public float AttackDamage => _attackDamage;

        /// <summary>공격 방식</summary>
        public EnemyAttackType AttackType => _attackType;

        /// <summary>돌진 거리</summary>
        public float ChargeDistance => _chargeDistance;

        /// <summary>돌진 속도</summary>
        public float ChargeSpeed => _chargeSpeed;

        /// <summary>돌진 후 빈틈 시간</summary>
        public float ChargeRecoverSeconds => _chargeRecoverSeconds;

        /// <summary>적 탄의 비행 수치를 읽는 무기 데이터</summary>
        public WeaponData ProjectileWeapon => _projectileWeapon;

        /// <summary>한 번 점사의 탄 수</summary>
        public int BurstCount => _burstCount;

        /// <summary>점사 안의 탄 간격</summary>
        public float BurstInterval => _burstInterval;

        /// <summary>점사 후 쉬는 시간</summary>
        public float BurstCooldownSeconds => _burstCooldownSeconds;

        /// <summary>뒤로 물러나기 시작하는 거리</summary>
        public float RetreatDistance => _retreatDistance;

        /// <summary>타겟을 놓친 뒤 복귀까지 걸리는 시간</summary>
        public float ReturnAfterLostSeconds => _returnAfterLostSeconds;
    }
}
