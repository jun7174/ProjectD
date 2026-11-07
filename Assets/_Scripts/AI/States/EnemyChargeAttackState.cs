// 파일 경로: Assets/_Scripts/AI/States/EnemyChargeAttackState.cs
using System.Collections.Generic;
using ProjectD.Core.Define;
using ProjectD.Data;
using UnityEngine;

namespace ProjectD.AI.States
{
    /// <summary>
    /// 돌격형의 공격 상태. 예고 → 돌진 → 빈틈 순서로 한 번 공격하고 추적으로 돌아간다.
    /// 예고 동안 대상을 바라보다가, 예고가 끝나는 순간의 방향으로 정해진 거리만큼 곧게 돌진한다.
    /// 돌진 중 몸에 닿은 대상은 한 번씩만 맞는다. 벽에 막히면 돌진을 멈춘다.
    /// 예고가 시작되면 대상이 피하더라도 끝까지 진행해서, 플레이어가 예고를 보고 피할 수 있게 한다.
    /// </summary>
    public class EnemyChargeAttackState : IEnemyState
    {
        // 한 프레임에 움직이려던 거리 중 이 비율보다 적게 움직였으면 벽에 막힌 것으로 본다
        private const float BlockedMoveRatio = 0.3f;

        private readonly EnemyBrain _brain;
        private readonly EnemyMovement _movement;
        private readonly EnemyStatus _status;
        private readonly EnemyWeapon _weapon;
        private readonly EnemyData _data;

        private readonly List<EnemyTarget> _touchingTargets = new List<EnemyTarget>();
        private readonly List<EnemyTarget> _alreadyHitTargets = new List<EnemyTarget>();

        private EnemyAttackPhase _phase;
        private float _phaseTimer;
        private Vector3 _chargeDirection;
        private float _chargedDistance;

        /// <summary>
        /// 돌진 공격 상태를 만든다.
        /// </summary>
        /// <param name="brain">상태 전환 요청과 대상을 맡는다</param>
        /// <param name="movement">이동 담당</param>
        /// <param name="status">예고 여부를 기록해 다른 화면에 보여 준다</param>
        /// <param name="weapon">몸 접촉 검사와 명중 알림</param>
        /// <param name="data">예고 시간, 돌진 거리·속도, 빈틈 시간을 읽는다</param>
        public EnemyChargeAttackState(EnemyBrain brain, EnemyMovement movement, EnemyStatus status, EnemyWeapon weapon, EnemyData data)
        {
            _brain = brain;
            _movement = movement;
            _status = status;
            _weapon = weapon;
            _data = data;
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
            _movement.Stop();
            StartTelegraph();
        }

        /// <summary>
        /// 지금 단계를 진행한다.
        /// </summary>
        /// <param name="deltaTime">지난 프레임 이후 시간 (초)</param>
        public void Tick(float deltaTime)
        {
            switch (_phase)
            {
                case EnemyAttackPhase.Telegraph:
                    TickTelegraph(deltaTime);
                    break;
                case EnemyAttackPhase.Strike:
                    TickCharge(deltaTime);
                    break;
                case EnemyAttackPhase.Recover:
                    TickRecover(deltaTime);
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
        /// 예고 단계를 시작한다.
        /// </summary>
        private void StartTelegraph()
        {
            _phase = EnemyAttackPhase.Telegraph;
            _phaseTimer = _data.TelegraphDuration;
            _status.SetTelegraphing(true);
        }

        /// <summary>
        /// 대상을 바라보며 예고 시간을 기다리고, 끝나면 돌진을 시작한다.
        /// </summary>
        /// <param name="deltaTime">지난 프레임 이후 시간 (초)</param>
        private void TickTelegraph(float deltaTime)
        {
            EnemyTarget target = _brain.CurrentTarget;
            if (target != null)
            {
                _movement.FaceTowards(target.transform.position, deltaTime);
            }

            _phaseTimer -= deltaTime;
            if (_phaseTimer <= 0f)
            {
                StartCharge(target);
            }
        }

        /// <summary>
        /// 돌진 방향을 정하고 돌진 단계를 시작한다. 대상이 없으면 바라보는 방향으로 돌진한다.
        /// </summary>
        /// <param name="target">돌진할 대상</param>
        private void StartCharge(EnemyTarget target)
        {
            Vector3 direction = _brain.transform.forward;
            if (target != null)
            {
                direction = target.transform.position - _brain.transform.position;
            }

            direction.y = 0f;
            _chargeDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : _brain.transform.forward;
            _brain.transform.rotation = Quaternion.LookRotation(_chargeDirection);

            _chargedDistance = 0f;
            _alreadyHitTargets.Clear();
            _status.SetTelegraphing(false);
            _phase = EnemyAttackPhase.Strike;
        }

        /// <summary>
        /// 정해진 방향으로 밀고 나가며 닿은 대상을 맞힌다. 거리를 다 가거나 벽에 막히면 빈틈 단계로 간다.
        /// </summary>
        /// <param name="deltaTime">지난 프레임 이후 시간 (초)</param>
        private void TickCharge(float deltaTime)
        {
            float step = Mathf.Min(_data.ChargeSpeed * deltaTime, _data.ChargeDistance - _chargedDistance);
            float moved = _movement.MoveDirectly(_chargeDirection * step);
            _chargedDistance += step;

            HitTouchingTargets();

            bool isBlocked = moved < step * BlockedMoveRatio;
            if (isBlocked || _chargedDistance >= _data.ChargeDistance)
            {
                StartRecover();
            }
        }

        /// <summary>
        /// 지금 몸에 닿은 대상 중 이번 돌진에서 아직 안 맞은 대상에게 명중을 알린다.
        /// </summary>
        private void HitTouchingTargets()
        {
            _weapon.CollectTouchingTargets(_touchingTargets);

            for (int i = 0; i < _touchingTargets.Count; i++)
            {
                EnemyTarget target = _touchingTargets[i];
                if (_alreadyHitTargets.Contains(target))
                {
                    continue;
                }

                _alreadyHitTargets.Add(target);
                _weapon.ReportMeleeHit(target);
            }
        }

        /// <summary>
        /// 돌진 후 빈틈 단계를 시작한다.
        /// </summary>
        private void StartRecover()
        {
            _movement.Stop();
            _phase = EnemyAttackPhase.Recover;
            _phaseTimer = _data.ChargeRecoverSeconds;
        }

        /// <summary>
        /// 빈틈 시간을 기다린 뒤 추적으로 돌아간다. 추적 상태가 사거리 안이면 다시 공격을 시작한다.
        /// </summary>
        /// <param name="deltaTime">지난 프레임 이후 시간 (초)</param>
        private void TickRecover(float deltaTime)
        {
            _phaseTimer -= deltaTime;
            if (_phaseTimer <= 0f)
            {
                _brain.GoToChase();
            }
        }
    }
}
