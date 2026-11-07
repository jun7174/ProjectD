// 파일 경로: Assets/_Scripts/AI/States/EnemyBurstAttackState.cs
using ProjectD.Core.Define;
using ProjectD.Data;
using UnityEngine;

namespace ProjectD.AI.States
{
    /// <summary>
    /// 사수형의 공격 상태. 예고 → 점사 → 쉬기를 반복한다.
    /// 예고 동안 멈춰서 대상을 바라보고, 점사 때는 한 발마다 대상의 가슴 쪽으로 쏜다.
    /// 쉬는 동안 대상이 너무 가까우면 뒤로 물러나 거리를 유지한다.
    /// 쉬기가 끝났을 때 대상이 안 보이거나 사거리보다 멀면 추적으로 돌아간다.
    /// </summary>
    public class EnemyBurstAttackState : IEnemyState
    {
        // 한 번 물러날 때 목적지로 잡는 거리 (m). 목적지에 가까워지면 다시 잡는다
        private const float RetreatStepDistance = 3f;

        private readonly EnemyBrain _brain;
        private readonly EnemyMovement _movement;
        private readonly EnemyPerception _perception;
        private readonly EnemyStatus _status;
        private readonly EnemyWeapon _weapon;
        private readonly EnemyData _data;
        private readonly float _exitRangeMultiplier;

        private EnemyAttackPhase _phase;
        private float _phaseTimer;
        private int _shotsLeft;
        private bool _isRetreating;

        /// <summary>
        /// 점사 공격 상태를 만든다.
        /// </summary>
        /// <param name="brain">상태 전환 요청과 대상을 맡는다</param>
        /// <param name="movement">이동 담당</param>
        /// <param name="perception">대상이 보이는지 확인한다</param>
        /// <param name="status">예고 여부를 기록해 다른 화면에 보여 준다</param>
        /// <param name="weapon">발사 위치와 발사 알림</param>
        /// <param name="data">예고 시간, 점사 수치, 물러나는 거리를 읽는다</param>
        /// <param name="exitRangeMultiplier">쉬기가 끝났을 때 공격 사거리의 이 배수보다 멀면 추적으로 돌아간다</param>
        public EnemyBurstAttackState(EnemyBrain brain, EnemyMovement movement, EnemyPerception perception,
            EnemyStatus status, EnemyWeapon weapon, EnemyData data, float exitRangeMultiplier)
        {
            _brain = brain;
            _movement = movement;
            _perception = perception;
            _status = status;
            _weapon = weapon;
            _data = data;
            _exitRangeMultiplier = exitRangeMultiplier;
        }

        /// <summary>
        /// 이 상태의 번호
        /// </summary>
        public EnemyState Id => EnemyState.Attack;

        /// <summary>
        /// 멈춰서 예고를 시작한다.
        /// </summary>
        public void Enter()
        {
            StartTelegraph();
        }

        /// <summary>
        /// 대상이 없으면 복귀하고, 있으면 지금 단계를 진행한다.
        /// </summary>
        /// <param name="deltaTime">지난 프레임 이후 시간 (초)</param>
        public void Tick(float deltaTime)
        {
            _brain.RefreshTarget();

            EnemyTarget target = _brain.CurrentTarget;
            if (target == null)
            {
                _brain.GoToReturn();
                return;
            }

            switch (_phase)
            {
                case EnemyAttackPhase.Telegraph:
                    TickTelegraph(target, deltaTime);
                    break;
                case EnemyAttackPhase.Strike:
                    TickBurst(target, deltaTime);
                    break;
                case EnemyAttackPhase.Recover:
                    TickCooldown(target, deltaTime);
                    break;
            }
        }

        /// <summary>
        /// 예고 표시를 끈다. 예고 중에 다른 상태로 바뀌어도 표시가 남지 않게 한다.
        /// </summary>
        public void Exit()
        {
            _status.SetTelegraphing(false);
        }

        /// <summary>
        /// 멈춰서 예고 단계를 시작한다.
        /// </summary>
        private void StartTelegraph()
        {
            _movement.Stop();
            _isRetreating = false;
            _phase = EnemyAttackPhase.Telegraph;
            _phaseTimer = _data.TelegraphDuration;
            _status.SetTelegraphing(true);
        }

        /// <summary>
        /// 대상을 바라보며 예고 시간을 기다리고, 끝나면 점사를 시작한다.
        /// </summary>
        /// <param name="target">공격 대상</param>
        /// <param name="deltaTime">지난 프레임 이후 시간 (초)</param>
        private void TickTelegraph(EnemyTarget target, float deltaTime)
        {
            _movement.FaceTowards(target.transform.position, deltaTime);

            _phaseTimer -= deltaTime;
            if (_phaseTimer <= 0f)
            {
                _status.SetTelegraphing(false);
                _phase = EnemyAttackPhase.Strike;
                _shotsLeft = _data.BurstCount;
                _phaseTimer = 0f;
            }
        }

        /// <summary>
        /// 탄 간격마다 한 발씩 쏘고, 다 쏘면 쉬기 단계로 간다.
        /// </summary>
        /// <param name="target">공격 대상</param>
        /// <param name="deltaTime">지난 프레임 이후 시간 (초)</param>
        private void TickBurst(EnemyTarget target, float deltaTime)
        {
            _movement.FaceTowards(target.transform.position, deltaTime);

            _phaseTimer -= deltaTime;
            if (_phaseTimer > 0f)
            {
                return;
            }

            _weapon.Fire(target.AimPoint - _weapon.MuzzlePosition);
            _shotsLeft--;
            _phaseTimer = _data.BurstInterval;

            if (_shotsLeft <= 0)
            {
                _phase = EnemyAttackPhase.Recover;
                _phaseTimer = _data.BurstCooldownSeconds;
            }
        }

        /// <summary>
        /// 쉬는 동안 거리를 유지하고, 쉬기가 끝나면 다시 예고하거나 추적으로 돌아간다.
        /// </summary>
        /// <param name="target">공격 대상</param>
        /// <param name="deltaTime">지난 프레임 이후 시간 (초)</param>
        private void TickCooldown(EnemyTarget target, float deltaTime)
        {
            KeepDistance(target, deltaTime);

            _phaseTimer -= deltaTime;
            if (_phaseTimer > 0f)
            {
                return;
            }

            bool isTooFar = _brain.GetFlatDistanceTo(target) > _data.AttackRange * _exitRangeMultiplier;
            if (!_perception.IsTargetVisible(target) || isTooFar)
            {
                _brain.GoToChase();
                return;
            }

            StartTelegraph();
        }

        /// <summary>
        /// 대상이 물러나는 거리보다 가까우면 대상 반대쪽으로 물러나고, 충분히 멀면 멈춰서 대상을 바라본다.
        /// </summary>
        /// <param name="target">공격 대상</param>
        /// <param name="deltaTime">지난 프레임 이후 시간 (초)</param>
        private void KeepDistance(EnemyTarget target, float deltaTime)
        {
            if (_brain.GetFlatDistanceTo(target) < _data.RetreatDistance)
            {
                Vector3 away = _brain.transform.position - target.transform.position;
                away.y = 0f;

                if (away.sqrMagnitude > 0.0001f)
                {
                    _movement.MoveTo(_brain.transform.position + away.normalized * RetreatStepDistance, _data.ChaseSpeed);
                    _isRetreating = true;
                }

                return;
            }

            if (_isRetreating)
            {
                _movement.Stop();
                _isRetreating = false;
            }

            _movement.FaceTowards(target.transform.position, deltaTime);
        }
    }
}
