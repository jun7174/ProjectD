// 파일 경로: Assets/_Scripts/AI/EnemyPerception.cs
using System;
using System.Collections.Generic;
using ProjectD.Data;
using UnityEngine;

namespace ProjectD.AI
{
    /// <summary>
    /// 적의 감지를 담당한다: 시야, 경계 게이지, 소음, 피격 반응.
    /// 시야 안에 플레이어가 보이면 경계 게이지가 차고(가까울수록 빠르게), 가득 차면 추적 대상으로 알린다.
    /// 소음을 들으면 게이지가 조금 오르고 소리 난 곳을 알린다. 소음만으로는 추적까지 가지 않는다.
    /// 맞으면 게이지를 바로 채우고 공격자를 추적 대상으로 알린다.
    /// 무엇을 할지(추적, 조사)는 EnemyBrain이 이벤트를 받아 정한다. 시야를 놓치면 마지막으로 본 위치도 알린다.
    /// AI라서 마스터에서만 동작한다. EnemyNetworkSetup의 Authority Only 목록에 넣는다.
    /// </summary>
    public class EnemyPerception : MonoBehaviour, INoiseListener
    {
        [Header("데이터")]
        [Tooltip("시야 거리·각도, 게이지 속도를 읽는다")]
        [SerializeField] private EnemyData _data;

        [Header("시야")]
        [Tooltip("눈 높이 (m). 적 발밑 기준")]
        [Min(0f)]
        [SerializeField] private float _eyeHeight = 1.5f;

        [Tooltip("시야 검사 간격 (초)")]
        [Min(0.02f)]
        [SerializeField] private float _sightCheckInterval = 0.2f;

        [Tooltip("볼 수 있는 대상의 레이어 (Player)")]
        [SerializeField] private LayerMask _targetMask;

        [Tooltip("시야를 가리는 레이어 (Environment)")]
        [SerializeField] private LayerMask _obstacleMask;

        [Header("경계 게이지")]
        [Tooltip("아주 가까이 있을 때 게이지 증가 배율. 시야 끝에서는 1배, 가까울수록 이 값에 가까워진다")]
        [Min(1f)]
        [SerializeField] private float _closeRangeGainMultiplier = 3f;

        [Header("소음")]
        [Tooltip("소음을 한 번 들을 때 오르는 게이지 양")]
        [Range(0f, 1f)]
        [SerializeField] private float _noiseAlertAmount = 0.3f;

        [Tooltip("소음만으로 오를 수 있는 게이지 최대값. 1보다 작게 두어 소음만으로는 추적하지 않게 한다")]
        [Range(0f, 0.99f)]
        [SerializeField] private float _noiseAlertCap = 0.8f;

        [Header("참조 (비워 두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private EnemyStatus _status;

        // 시야 검사에서 찾은 대상을 담는 배열. 한 번에 이 수보다 많은 플레이어가 반경 안에 있을 일은 없다
        private const int MaxTargetsInRange = 8;

        private readonly Collider[] _overlapBuffer = new Collider[MaxTargetsInRange];

        // 콜라이더마다 붙어 있는 EnemyTarget을 한 번만 찾고 기억한다
        private readonly Dictionary<Collider, EnemyTarget> _targetByCollider = new Dictionary<Collider, EnemyTarget>();

        // 마지막 시야 검사에서 보인 대상 전부. 추적 중인 대상이 지금 보이는지 확인할 때 쓴다
        private readonly List<EnemyTarget> _visibleTargets = new List<EnemyTarget>();

        private float _nextSightCheckTime;

        /// <summary>
        /// 시야 안에 보이는 가장 가까운 대상. 안 보이면 null
        /// </summary>
        public EnemyTarget VisibleTarget { get; private set; }

        /// <summary>
        /// 추적할 대상. 게이지가 가득 찼을 때 보이던 대상이거나, 마지막으로 공격해 온 대상
        /// </summary>
        public EnemyTarget AlertTarget { get; private set; }

        /// <summary>
        /// 대상을 마지막으로 본(또는 공격받은) 위치
        /// </summary>
        public Vector3 LastKnownTargetPosition { get; private set; }

        /// <summary>
        /// 마지막으로 소음을 들은 위치
        /// </summary>
        public Vector3 LastHeardNoisePosition { get; private set; }

        /// <summary>
        /// 경계 게이지 (0~1)
        /// </summary>
        public float AlertLevel => _status != null ? _status.AlertLevel : 0f;

        /// <summary>
        /// 게이지가 가득 차 추적 중인지 여부
        /// </summary>
        public bool IsFullyAlerted => AlertLevel >= 1f;

        /// <summary>
        /// 게이지가 가득 찼을 때 발생한다. 인자는 추적할 대상이다.
        /// </summary>
        public event Action<EnemyTarget> OnFullyAlerted;

        /// <summary>
        /// 소음을 들었을 때 발생한다. 인자는 소리 난 위치다.
        /// </summary>
        public event Action<Vector3> OnNoiseHeardAt;

        /// <summary>
        /// 공격받았을 때 발생한다. 인자는 공격한 대상이다.
        /// </summary>
        public event Action<EnemyTarget> OnAttacked;

        /// <summary>
        /// 보이던 대상이 모두 안 보이게 되었을 때 발생한다. 인자는 마지막으로 본 위치다.
        /// </summary>
        public event Action<Vector3> OnSightLost;

        /// <summary>
        /// 참조를 찾고 비어 있으면 경고한다.
        /// </summary>
        private void Awake()
        {
            if (_status == null && !TryGetComponent(out _status))
            {
                Debug.LogWarning($"[{nameof(EnemyPerception)}] EnemyStatus를 찾지 못했습니다. 경계 게이지가 저장되지 않습니다.", this);
            }

            if (_data == null)
            {
                Debug.LogWarning($"[{nameof(EnemyPerception)}] EnemyData가 비어 있습니다. 감지가 동작하지 않습니다.", this);
            }
        }

        /// <summary>
        /// 일정 간격으로 보이는 대상을 다시 찾고, 매 프레임 경계 게이지를 갱신한다.
        /// 보이던 대상이 이번 검사에서 모두 사라지면 마지막으로 본 위치를 알린다.
        /// </summary>
        private void Update()
        {
            if (_data == null || _status == null)
            {
                return;
            }

            if (Time.time >= _nextSightCheckTime)
            {
                _nextSightCheckTime = Time.time + _sightCheckInterval;

                bool wasSeeing = VisibleTarget != null;
                VisibleTarget = FindClosestVisibleTarget();

                if (wasSeeing && VisibleTarget == null)
                {
                    OnSightLost?.Invoke(LastKnownTargetPosition);
                }
            }

            UpdateAlertLevel(Time.deltaTime);
        }

        /// <summary>
        /// 대상이 마지막 시야 검사에서 보였는지 확인한다.
        /// </summary>
        /// <param name="target">확인할 대상</param>
        /// <returns>보였으면 true</returns>
        public bool IsTargetVisible(EnemyTarget target)
        {
            return target != null && _visibleTargets.Contains(target);
        }

        /// <summary>
        /// 소음을 들었을 때 NoiseSystem이 호출한다. 게이지를 조금 올리고 소리 난 곳을 알린다.
        /// 게이지는 소음 최대값까지만 오르므로 소음만으로는 추적하지 않는다.
        /// </summary>
        /// <param name="noisePosition">소음이 난 위치</param>
        public void OnNoiseHeard(Vector3 noisePosition)
        {
            // 컴포넌트가 꺼져 있어도 같은 오브젝트의 콜라이더로 호출될 수 있으므로 직접 확인한다
            if (!enabled || _status == null)
            {
                return;
            }

            LastHeardNoisePosition = noisePosition;

            if (!IsFullyAlerted)
            {
                float raised = Mathf.Min(AlertLevel + _noiseAlertAmount, _noiseAlertCap);
                _status.SetAlertLevel(Mathf.Max(AlertLevel, raised));
            }

            OnNoiseHeardAt?.Invoke(noisePosition);
        }

        /// <summary>
        /// 공격받았을 때 호출한다. 게이지를 바로 채우고 공격자를 추적 대상으로 정한다.
        /// 마스터의 명중 검증(PlayerHitReporter)이 데미지를 적용한 뒤 호출한다.
        /// </summary>
        /// <param name="attacker">공격한 대상</param>
        public void ReactToAttack(EnemyTarget attacker)
        {
            if (!enabled || _status == null || attacker == null || !attacker.IsTargetable)
            {
                return;
            }

            bool wasFullyAlerted = IsFullyAlerted;

            _status.SetAlertLevel(1f);
            AlertTarget = attacker;
            LastKnownTargetPosition = attacker.transform.position;

            OnAttacked?.Invoke(attacker);

            if (!wasFullyAlerted)
            {
                OnFullyAlerted?.Invoke(attacker);
            }
        }

        /// <summary>
        /// 보이는 대상이 있으면 게이지를 올리고(가까울수록 빠르게), 없으면 내린다. 가득 차는 순간 추적 대상을 알린다.
        /// </summary>
        /// <param name="deltaTime">지난 프레임 이후 시간 (초)</param>
        private void UpdateAlertLevel(float deltaTime)
        {
            bool wasFullyAlerted = IsFullyAlerted;

            if (VisibleTarget != null)
            {
                float distance = Vector3.Distance(GetEyePosition(), VisibleTarget.AimPoint);

                // 시야 끝(비율 1)에서는 1배, 바로 앞(비율 0)에서는 근거리 배율만큼 빨리 찬다
                float closeness = Mathf.Clamp01(distance / _data.SightDistance);
                float multiplier = Mathf.Lerp(_closeRangeGainMultiplier, 1f, closeness);

                _status.SetAlertLevel(AlertLevel + _data.AlertGainPerSecond * multiplier * deltaTime);
                LastKnownTargetPosition = VisibleTarget.transform.position;
            }
            else
            {
                _status.SetAlertLevel(AlertLevel - _data.AlertDecayPerSecond * deltaTime);
            }

            if (!wasFullyAlerted && IsFullyAlerted && VisibleTarget != null)
            {
                AlertTarget = VisibleTarget;
                OnFullyAlerted?.Invoke(VisibleTarget);
            }
        }

        /// <summary>
        /// 시야 반경 안의 대상 중 각도 안에 있고 벽에 가리지 않은 대상을 모두 기록하고, 그중 가장 가까운 대상을 고른다.
        /// </summary>
        /// <returns>가장 가까운 보이는 대상. 없으면 null</returns>
        private EnemyTarget FindClosestVisibleTarget()
        {
            Vector3 eye = GetEyePosition();
            int count = Physics.OverlapSphereNonAlloc(eye, _data.SightDistance, _overlapBuffer, _targetMask, QueryTriggerInteraction.Ignore);

            _visibleTargets.Clear();
            EnemyTarget closest = null;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                EnemyTarget target = GetTarget(_overlapBuffer[i]);

                // 한 캐릭터에 콜라이더가 여러 개면 같은 대상이 여러 번 걸릴 수 있다
                if (target == null || !target.IsTargetable || _visibleTargets.Contains(target) || !CanSee(eye, target))
                {
                    continue;
                }

                _visibleTargets.Add(target);

                float distance = Vector3.Distance(eye, target.AimPoint);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = target;
                }
            }

            return closest;
        }

        /// <summary>
        /// 대상이 시야 각도 안에 있고, 눈에서 대상까지 벽이 없으면 보인다고 판단한다.
        /// 각도는 위아래를 빼고 수평으로만 비교한다.
        /// </summary>
        /// <param name="eye">눈 위치</param>
        /// <param name="target">검사할 대상</param>
        /// <returns>보이면 true</returns>
        private bool CanSee(Vector3 eye, EnemyTarget target)
        {
            Vector3 toTarget = target.AimPoint - eye;
            Vector3 flatToTarget = new Vector3(toTarget.x, 0f, toTarget.z);
            Vector3 flatForward = new Vector3(transform.forward.x, 0f, transform.forward.z);

            if (Vector3.Angle(flatForward, flatToTarget) > _data.SightAngle * 0.5f)
            {
                return false;
            }

            return !Physics.Linecast(eye, target.AimPoint, _obstacleMask, QueryTriggerInteraction.Ignore);
        }

        /// <summary>
        /// 콜라이더에 붙은 EnemyTarget을 찾는다. 콜라이더마다 처음 한 번만 찾고 기억한다.
        /// </summary>
        /// <param name="targetCollider">검사에 걸린 콜라이더</param>
        /// <returns>EnemyTarget. 없으면 null</returns>
        private EnemyTarget GetTarget(Collider targetCollider)
        {
            if (!_targetByCollider.TryGetValue(targetCollider, out EnemyTarget target))
            {
                target = targetCollider.GetComponentInParent<EnemyTarget>();
                _targetByCollider[targetCollider] = target;
            }

            return target;
        }

        /// <summary>
        /// 눈 위치를 계산한다.
        /// </summary>
        /// <returns>발밑에서 눈 높이만큼 올린 위치</returns>
        private Vector3 GetEyePosition()
        {
            return transform.position + Vector3.up * _eyeHeight;
        }

        /// <summary>
        /// 선택했을 때 Scene 뷰에 시야 거리와 시야 각도 경계를 그린다.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (_data == null)
            {
                return;
            }

            Vector3 eye = GetEyePosition();
            float halfAngle = _data.SightAngle * 0.5f;
            Vector3 leftEdge = Quaternion.Euler(0f, -halfAngle, 0f) * transform.forward * _data.SightDistance;
            Vector3 rightEdge = Quaternion.Euler(0f, halfAngle, 0f) * transform.forward * _data.SightDistance;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(eye, _data.SightDistance);
            Gizmos.DrawLine(eye, eye + leftEdge);
            Gizmos.DrawLine(eye, eye + rightEdge);

            if (VisibleTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(eye, VisibleTarget.AimPoint);
            }
        }
    }
}
