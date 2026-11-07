// 파일 경로: Assets/_Scripts/AI/States/EnemyReturnState.cs
using ProjectD.Core.Define;
using ProjectD.Data;

namespace ProjectD.AI.States
{
    /// <summary>
    /// 복귀 상태. 대상을 놓쳤거나 조사를 마치면, 하던 순찰의 지금 향하던 지점으로 걸어가서 순찰을 이어 간다.
    /// 돌아가는 중에도 감지는 계속되므로, 다시 보거나 들으면 EnemyBrain이 추적이나 조사로 바꾼다.
    /// </summary>
    public class EnemyReturnState : IEnemyState
    {
        private readonly EnemyBrain _brain;
        private readonly EnemyMovement _movement;
        private readonly EnemyPatrolRoute _route;
        private readonly EnemyData _data;

        /// <summary>
        /// 복귀 상태를 만든다.
        /// </summary>
        /// <param name="brain">상태 전환을 요청할 대상</param>
        /// <param name="movement">이동 담당</param>
        /// <param name="route">돌아갈 순찰 지점</param>
        /// <param name="data">순찰 속도를 읽는다</param>
        public EnemyReturnState(EnemyBrain brain, EnemyMovement movement, EnemyPatrolRoute route, EnemyData data)
        {
            _brain = brain;
            _movement = movement;
            _route = route;
            _data = data;
        }

        /// <summary>
        /// 이 상태의 번호
        /// </summary>
        public EnemyState Id => EnemyState.Return;

        /// <summary>
        /// 순찰 지점으로 걷기 시작한다. 갈 수 없으면 멈춰서, 다음 Tick에서 도착한 것으로 보고 순찰로 넘어간다.
        /// Enter 안에서 바로 상태를 바꾸지 않는 것은 전환 도중에 또 전환이 겹치지 않게 하기 위해서다.
        /// </summary>
        public void Enter()
        {
            if (!_movement.MoveTo(_route.CurrentPoint, _data.PatrolSpeed))
            {
                _movement.Stop();
            }
        }

        /// <summary>
        /// 도착하면 순찰을 이어 간다.
        /// </summary>
        /// <param name="deltaTime">지난 프레임 이후 시간 (초)</param>
        public void Tick(float deltaTime)
        {
            if (_movement.HasArrived)
            {
                _brain.GoToPatrol();
            }
        }

        /// <summary>
        /// 나갈 때 할 일은 없다. 다음 상태가 이동을 새로 정한다.
        /// </summary>
        public void Exit()
        {
        }
    }
}
