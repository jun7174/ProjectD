// 파일 경로: Assets/_Scripts/AI/States/EnemyPatrolState.cs
using ProjectD.Core.Define;
using ProjectD.Data;

namespace ProjectD.AI.States
{
    /// <summary>
    /// 순찰 상태. 순찰 지점을 순서대로 걷고, 지점에 도착하면 잠깐 멈췄다가 다음 지점으로 간다.
    /// 지점이 하나뿐이면 그 자리에 서서 경계한다.
    /// 다른 상태로 가는 판단(발견, 소음, 피격)은 EnemyBrain이 감지 이벤트로 한다.
    /// </summary>
    public class EnemyPatrolState : IEnemyState
    {
        private readonly EnemyMovement _movement;
        private readonly EnemyPatrolRoute _route;
        private readonly EnemyData _data;
        private readonly float _waitSeconds;

        private bool _isWaiting;
        private bool _isStandingGuard;
        private float _waitTimer;

        /// <summary>
        /// 순찰 상태를 만든다.
        /// </summary>
        /// <param name="movement">이동 담당</param>
        /// <param name="route">순찰 지점 목록</param>
        /// <param name="data">순찰 속도를 읽는다</param>
        /// <param name="waitSeconds">지점에서 멈춰 있는 시간 (초)</param>
        public EnemyPatrolState(EnemyMovement movement, EnemyPatrolRoute route, EnemyData data, float waitSeconds)
        {
            _movement = movement;
            _route = route;
            _data = data;
            _waitSeconds = waitSeconds;
        }

        /// <summary>
        /// 이 상태의 번호
        /// </summary>
        public EnemyState Id => EnemyState.Patrol;

        /// <summary>
        /// 지금 향하는 순찰 지점으로 걷기 시작한다.
        /// </summary>
        public void Enter()
        {
            _isWaiting = false;
            _isStandingGuard = false;
            MoveToCurrentPoint();
        }

        /// <summary>
        /// 도착하면 기다리고, 기다린 뒤 다음 지점으로 간다.
        /// </summary>
        /// <param name="deltaTime">지난 프레임 이후 시간 (초)</param>
        public void Tick(float deltaTime)
        {
            if (_isStandingGuard)
            {
                return;
            }

            if (_isWaiting)
            {
                _waitTimer -= deltaTime;
                if (_waitTimer <= 0f)
                {
                    _isWaiting = false;
                    _route.AdvanceToNext();
                    MoveToCurrentPoint();
                }

                return;
            }

            if (!_movement.HasArrived)
            {
                return;
            }

            _movement.Stop();

            if (_route.Count <= 1)
            {
                _isStandingGuard = true;
                return;
            }

            _isWaiting = true;
            _waitTimer = _waitSeconds;
        }

        /// <summary>
        /// 나갈 때 할 일은 없다. 다음 상태가 이동을 새로 정한다.
        /// </summary>
        public void Exit()
        {
        }

        /// <summary>
        /// 지금 향하는 순찰 지점으로 순찰 속도로 이동한다.
        /// </summary>
        private void MoveToCurrentPoint()
        {
            _movement.MoveTo(_route.CurrentPoint, _data.PatrolSpeed);
        }
    }
}
