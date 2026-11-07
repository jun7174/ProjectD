// 파일 경로: Assets/_Scripts/AI/EnemyBrain.cs
using ProjectD.AI.States;
using ProjectD.Core.Define;
using ProjectD.Data;
using UnityEngine;

namespace ProjectD.AI
{
    /// <summary>
    /// 적의 판단을 담당한다.
    /// 상태들(순찰, 조사, 추적, 공격, 복귀)을 만들어 상태 머신에 넣고, 감지(EnemyPerception) 이벤트를 받아 상태를 바꾼다.
    /// 공격 상태는 EnemyData의 공격 방식(돌진, 점사)에 맞는 것을 만든다.
    /// 추적 대상도 여기서 정한다: 가장 가까운 보이는 플레이어, 맞으면 공격자. 한 번 바꾸면 일정 시간 유지한다.
    /// 상태 안의 세부 동작은 각 상태 클래스가 맡고, 이 클래스는 "언제 어느 상태로 가는가"만 다룬다.
    /// AI라서 마스터에서만 동작한다. EnemyNetworkSetup의 Authority Only 목록에 넣는다.
    /// 적 프리팹 루트에 붙인다.
    /// </summary>
    public class EnemyBrain : MonoBehaviour
    {
        [Header("데이터")]
        [Tooltip("이동 속도, 공격 사거리, 놓친 뒤 복귀 시간을 읽는다")]
        [SerializeField] private EnemyData _data;

        [Header("순찰")]
        [Tooltip("순찰 지점에 도착해서 멈춰 있는 시간 (초)")]
        [Min(0f)]
        [SerializeField] private float _patrolWaitSeconds = 2f;

        [Header("조사")]
        [Tooltip("잠깐 보고 놓쳤을 때 이 값 이상이면 마지막으로 본 곳을 조사하러 간다 (경계 게이지 0~1)")]
        [Range(0f, 1f)]
        [SerializeField] private float _suspicionAlertThreshold = 0.3f;

        [Tooltip("조사 위치에 도착해서 둘러보는 시간 (초)")]
        [Min(0f)]
        [SerializeField] private float _lookAroundSeconds = 4f;

        [Tooltip("둘러볼 때 좌우로 도는 최대 각도 (도)")]
        [Range(0f, 180f)]
        [SerializeField] private float _lookAroundAngle = 60f;

        [Tooltip("좌우를 한 번 다 둘러보는 데 걸리는 시간 (초)")]
        [Min(0.1f)]
        [SerializeField] private float _lookAroundCycleSeconds = 2f;

        [Header("추적과 공격")]
        [Tooltip("추적 대상을 바꾼 뒤 다시 바꾸지 않고 유지하는 시간 (초). 2~3초 권장")]
        [Min(0f)]
        [SerializeField] private float _targetHoldSeconds = 2.5f;

        [Tooltip("공격 중 대상이 공격 사거리의 이 배수보다 멀어지면 다시 추적한다. 사거리 경계에서 상태가 깜빡이는 것을 막는다")]
        [Min(1f)]
        [SerializeField] private float _attackExitRangeMultiplier = 1.2f;

        [Header("참조 (비워 두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private EnemyPerception _perception;
        [SerializeField] private EnemyMovement _movement;
        [SerializeField] private EnemyPatrolRoute _patrolRoute;
        [SerializeField] private EnemyStatus _status;
        [SerializeField] private EnemyWeapon _weapon;

        private readonly EnemyStateMachine _stateMachine = new EnemyStateMachine();

        private EnemyPatrolState _patrolState;
        private EnemyInvestigateState _investigateState;
        private EnemyChaseState _chaseState;
        private IEnemyState _attackState;
        private EnemyReturnState _returnState;

        // 이 시각 전에는 추적 대상을 다른 플레이어로 바꾸지 않는다
        private float _targetLockedUntil;

        /// <summary>
        /// 지금 쫓는 대상. 추적이나 공격 중이 아니면 null
        /// </summary>
        public EnemyTarget CurrentTarget { get; private set; }

        /// <summary>
        /// 현재 상태 번호. 처음 상태를 정하기 전에는 순찰로 본다
        /// </summary>
        public EnemyState CurrentStateId => _stateMachine.CurrentState != null ? _stateMachine.CurrentState.Id : EnemyState.Patrol;

        /// <summary>
        /// 추적이나 공격 중인지 여부
        /// </summary>
        private bool IsEngaged => CurrentStateId == EnemyState.Chase || CurrentStateId == EnemyState.Attack;

        /// <summary>
        /// 참조를 찾고 상태들을 만든다. 필수 참조가 비어 있으면 경고 후 이 컴포넌트를 끈다.
        /// </summary>
        private void Awake()
        {
            if (_perception == null && !TryGetComponent(out _perception))
            {
                Debug.LogWarning($"[{nameof(EnemyBrain)}] EnemyPerception을 찾지 못했습니다.", this);
            }

            if (_movement == null && !TryGetComponent(out _movement))
            {
                Debug.LogWarning($"[{nameof(EnemyBrain)}] EnemyMovement를 찾지 못했습니다.", this);
            }

            if (_patrolRoute == null && !TryGetComponent(out _patrolRoute))
            {
                Debug.LogWarning($"[{nameof(EnemyBrain)}] EnemyPatrolRoute를 찾지 못했습니다.", this);
            }

            if (_status == null && !TryGetComponent(out _status))
            {
                Debug.LogWarning($"[{nameof(EnemyBrain)}] EnemyStatus를 찾지 못했습니다.", this);
            }

            if (_weapon == null && !TryGetComponent(out _weapon))
            {
                Debug.LogWarning($"[{nameof(EnemyBrain)}] EnemyWeapon을 찾지 못했습니다.", this);
            }

            if (_data == null)
            {
                Debug.LogWarning($"[{nameof(EnemyBrain)}] EnemyData가 비어 있습니다.", this);
            }

            if (_data == null || _perception == null || _movement == null || _patrolRoute == null || _status == null || _weapon == null)
            {
                Debug.LogWarning($"[{nameof(EnemyBrain)}] 필수 참조가 비어 있어 AI를 끕니다.", this);
                enabled = false;
                return;
            }

            CreateStates();
        }

        /// <summary>
        /// 감지 이벤트와 상태 변경 이벤트를 구독한다.
        /// </summary>
        private void OnEnable()
        {
            if (_perception != null)
            {
                _perception.OnFullyAlerted += HandleFullyAlerted;
                _perception.OnAttacked += HandleAttacked;
                _perception.OnNoiseHeardAt += HandleNoiseHeard;
                _perception.OnSightLost += HandleSightLost;
            }

            _stateMachine.OnStateChanged += HandleStateChanged;
        }

        /// <summary>
        /// 구독을 해제한다.
        /// </summary>
        private void OnDisable()
        {
            if (_perception != null)
            {
                _perception.OnFullyAlerted -= HandleFullyAlerted;
                _perception.OnAttacked -= HandleAttacked;
                _perception.OnNoiseHeardAt -= HandleNoiseHeard;
                _perception.OnSightLost -= HandleSightLost;
            }

            _stateMachine.OnStateChanged -= HandleStateChanged;
        }

        /// <summary>
        /// 순찰로 시작한다. 생성 직후 순찰 지점을 받을 수 있도록 Awake가 아니라 Start에서 시작한다.
        /// </summary>
        private void Start()
        {
            GoToPatrol();
        }

        /// <summary>
        /// 현재 상태를 한 프레임 실행한다.
        /// </summary>
        private void Update()
        {
            _stateMachine.Tick(Time.deltaTime);
        }

        /// <summary>
        /// 순찰 상태로 바꾼다.
        /// </summary>
        public void GoToPatrol()
        {
            _stateMachine.ChangeState(_patrolState);
        }

        /// <summary>
        /// 위치를 조사하러 가는 상태로 바꾼다. 이미 조사 중이면 새 위치로 처음부터 다시 조사한다.
        /// </summary>
        /// <param name="position">조사할 위치</param>
        public void GoToInvestigate(Vector3 position)
        {
            _investigateState.SetDestination(position);
            _stateMachine.ChangeState(_investigateState);
        }

        /// <summary>
        /// 추적 상태로 바꾼다. 대상이 정해져 있어야 한다.
        /// </summary>
        public void GoToChase()
        {
            _stateMachine.ChangeState(_chaseState);
        }

        /// <summary>
        /// 공격 상태로 바꾼다.
        /// </summary>
        public void GoToAttack()
        {
            _stateMachine.ChangeState(_attackState);
        }

        /// <summary>
        /// 대상을 놓고 순찰 경로로 돌아가는 상태로 바꾼다.
        /// </summary>
        public void GoToReturn()
        {
            CurrentTarget = null;
            _stateMachine.ChangeState(_returnState);
        }

        /// <summary>
        /// 추적 대상을 다시 고른다. 추적·공격 상태가 매 프레임 호출한다.
        /// 지금 대상이 안 보이면 보이는 다른 대상으로 바로 바꾸고,
        /// 지금 대상도 보이면 유지 시간이 지난 뒤에만 더 가까운 대상으로 바꾼다.
        /// </summary>
        public void RefreshTarget()
        {
            EnemyTarget closest = _perception.VisibleTarget;
            if (closest == null || closest == CurrentTarget)
            {
                return;
            }

            bool isCurrentVisible = _perception.IsTargetVisible(CurrentTarget);
            if (!isCurrentVisible || Time.time >= _targetLockedUntil)
            {
                SetTarget(closest);
            }
        }

        /// <summary>
        /// 대상까지 수평 거리를 계산한다. 높이 차이는 무시한다.
        /// </summary>
        /// <param name="target">거리를 잴 대상</param>
        /// <returns>수평 거리 (m)</returns>
        public float GetFlatDistanceTo(EnemyTarget target)
        {
            Vector3 offset = target.transform.position - transform.position;
            offset.y = 0f;
            return offset.magnitude;
        }

        /// <summary>
        /// 상태 클래스들을 만든다. 각 상태에는 필요한 것만 넘긴다.
        /// </summary>
        private void CreateStates()
        {
            _patrolState = new EnemyPatrolState(_movement, _patrolRoute, _data, _patrolWaitSeconds);
            _investigateState = new EnemyInvestigateState(this, _movement, _data, _lookAroundSeconds, _lookAroundAngle, _lookAroundCycleSeconds);
            _chaseState = new EnemyChaseState(this, _movement, _perception, _data);
            _attackState = CreateAttackState();
            _returnState = new EnemyReturnState(this, _movement, _patrolRoute, _data);
        }

        /// <summary>
        /// EnemyData의 공격 방식에 맞는 공격 상태를 만든다.
        /// </summary>
        /// <returns>돌진 공격 또는 점사 공격 상태</returns>
        private IEnemyState CreateAttackState()
        {
            switch (_data.AttackType)
            {
                case EnemyAttackType.Burst:
                    return new EnemyBurstAttackState(this, _movement, _perception, _status, _weapon, _data, _attackExitRangeMultiplier);
                default:
                    return new EnemyChargeAttackState(this, _movement, _status, _weapon, _data);
            }
        }

        /// <summary>
        /// 추적 대상을 바꾸고, 유지 시간 동안 다시 바꾸지 않게 한다.
        /// </summary>
        /// <param name="target">새 대상</param>
        private void SetTarget(EnemyTarget target)
        {
            CurrentTarget = target;
            _targetLockedUntil = Time.time + _targetHoldSeconds;
        }

        /// <summary>
        /// 조사할 수 있는 상태(순찰, 복귀, 조사 중)인지 확인한다. 추적·공격 중에는 소리나 흔적을 무시한다.
        /// </summary>
        /// <returns>조사를 시작할 수 있으면 true</returns>
        private bool CanInvestigate()
        {
            EnemyState state = CurrentStateId;
            return state == EnemyState.Patrol || state == EnemyState.Return || state == EnemyState.Investigate;
        }

        /// <summary>
        /// 경계 게이지가 가득 차면 그 대상을 쫓는다. 이미 쫓고 있으면 대상 선택은 RefreshTarget에 맡긴다.
        /// </summary>
        /// <param name="target">게이지를 채운 대상</param>
        private void HandleFullyAlerted(EnemyTarget target)
        {
            if (IsEngaged)
            {
                return;
            }

            SetTarget(target);
            GoToChase();
        }

        /// <summary>
        /// 공격받으면 공격자를 쫓는다. 이미 다른 대상을 쫓고 있으면 유지 시간이 지난 뒤에만 공격자로 바꾼다.
        /// </summary>
        /// <param name="attacker">공격한 대상</param>
        private void HandleAttacked(EnemyTarget attacker)
        {
            if (!IsEngaged)
            {
                SetTarget(attacker);
                GoToChase();
                return;
            }

            if (attacker != CurrentTarget && Time.time >= _targetLockedUntil)
            {
                SetTarget(attacker);
            }
        }

        /// <summary>
        /// 소음을 들으면 소리 난 곳을 조사하러 간다.
        /// </summary>
        /// <param name="noisePosition">소음이 난 위치</param>
        private void HandleNoiseHeard(Vector3 noisePosition)
        {
            if (CanInvestigate())
            {
                GoToInvestigate(noisePosition);
            }
        }

        /// <summary>
        /// 게이지가 다 차기 전에 대상을 놓쳤을 때, 게이지가 기준 이상이면 마지막으로 본 곳을 조사하러 간다.
        /// </summary>
        /// <param name="lastSeenPosition">마지막으로 본 위치</param>
        private void HandleSightLost(Vector3 lastSeenPosition)
        {
            if (CanInvestigate() && _perception.AlertLevel >= _suspicionAlertThreshold)
            {
                GoToInvestigate(lastSeenPosition);
            }
        }

        /// <summary>
        /// 상태가 바뀌면 동기화와 디버그 표시용 상태 값에 기록한다.
        /// </summary>
        /// <param name="state">새 상태</param>
        private void HandleStateChanged(IEnemyState state)
        {
            if (_status != null)
            {
                _status.SetState(state.Id);
            }
        }
    }
}
