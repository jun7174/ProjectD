// 파일 경로: Assets/_Scripts/AI/States/EnemyChaseState.cs
using ProjectD.Core.Define;
using ProjectD.Data;
using UnityEngine;

namespace ProjectD.AI.States
{
    /// <summary>
    /// 추적 상태.
    /// 대상이 보이면 대상을 쫓고, 공격 사거리에 들어오면 공격 상태로 간다.
    /// 대상이 안 보이면 마지막으로 본 곳으로 가고, 놓친 시간이 EnemyData의 복귀 시간을 넘으면 순찰 경로로 돌아간다.
    /// 대상 선택(가장 가까운 대상, 공격자 전환, 유지 시간)은 EnemyBrain.RefreshTarget이 한다.
    /// </summary>
    public class EnemyChaseState : IEnemyState
    {
        private readonly EnemyBrain _brain;
        private readonly EnemyMovement _movement;
        private readonly EnemyPerception _perception;
        private readonly EnemyData _data;

        private EnemyTarget _trackedTarget;
        private Vector3 _lastSeenPosition;
        private float _lostTimer;

        /// <summary>
        /// 추적 상태를 만든다.
        /// </summary>
        /// <param name="brain">상태 전환 요청과 대상 선택을 맡는다</param>
        /// <param name="movement">이동 담당</param>
        /// <param name="perception">대상이 보이는지 확인한다</param>
        /// <param name="data">추적 속도, 공격 사거리, 복귀 시간을 읽는다</param>
        public EnemyChaseState(EnemyBrain brain, EnemyMovement movement, EnemyPerception perception, EnemyData data)
        {
            _brain = brain;
            _movement = movement;
            _perception = perception;
            _data = data;
        }

        /// <summary>
        /// 이 상태의 번호
        /// </summary>
        public EnemyState Id => EnemyState.Chase;

        /// <summary>
        /// 놓친 시간을 초기화한다. 대상의 현재 위치는 첫 Tick에서 기록한다.
        /// </summary>
        public void Enter()
        {
            _trackedTarget = null;
            _lostTimer = 0f;
        }

        /// <summary>
        /// 대상을 다시 고르고, 보이면 쫓거나 공격으로, 안 보이면 마지막 위치로 가다가 시간이 지나면 복귀한다.
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

            // 대상이 바뀌었으면(처음 들어왔을 때 포함) 그 대상의 지금 위치부터 쫓는다. 안 보이는 공격자도 그 방향으로 간다
            if (target != _trackedTarget)
            {
                _trackedTarget = target;
                _lastSeenPosition = target.transform.position;
                _lostTimer = 0f;
            }

            if (_perception.IsTargetVisible(target))
            {
                _lostTimer = 0f;
                _lastSeenPosition = target.transform.position;

                if (_brain.GetFlatDistanceTo(target) <= _data.AttackRange)
                {
                    _brain.GoToAttack();
                    return;
                }
            }
            else
            {
                _lostTimer += deltaTime;
                if (_lostTimer >= _data.ReturnAfterLostSeconds)
                {
                    _brain.GoToReturn();
                    return;
                }
            }

            _movement.MoveTo(_lastSeenPosition, _data.ChaseSpeed);
        }

        /// <summary>
        /// 나갈 때 할 일은 없다. 다음 상태가 이동을 새로 정한다.
        /// </summary>
        public void Exit()
        {
        }
    }
}
