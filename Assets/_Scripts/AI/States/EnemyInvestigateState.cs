// 파일 경로: Assets/_Scripts/AI/States/EnemyInvestigateState.cs
using ProjectD.Core.Define;
using ProjectD.Data;
using UnityEngine;

namespace ProjectD.AI.States
{
    /// <summary>
    /// 조사 상태.
    /// 소리 난 곳이나 잠깐 본 곳으로 걸어가서, 도착하면 좌우로 둘러본다. 둘러보는 동안 아무것도 없으면 순찰 경로로 돌아간다.
    /// 둘러보는 중에 플레이어를 보면 게이지가 차서 EnemyBrain이 추적으로 바꾼다.
    /// </summary>
    public class EnemyInvestigateState : IEnemyState
    {
        private readonly EnemyBrain _brain;
        private readonly EnemyMovement _movement;
        private readonly EnemyData _data;
        private readonly float _lookAroundSeconds;
        private readonly float _lookAroundAngle;
        private readonly float _lookAroundCycleSeconds;

        private Vector3 _destination;
        private bool _isLookingAround;
        private float _lookTimer;
        private float _baseYaw;

        /// <summary>
        /// 조사 상태를 만든다.
        /// </summary>
        /// <param name="brain">상태 전환을 요청할 대상</param>
        /// <param name="movement">이동 담당</param>
        /// <param name="data">이동 속도를 읽는다</param>
        /// <param name="lookAroundSeconds">도착 후 둘러보는 시간 (초)</param>
        /// <param name="lookAroundAngle">좌우로 도는 최대 각도 (도)</param>
        /// <param name="lookAroundCycleSeconds">좌우를 한 번 다 둘러보는 시간 (초)</param>
        public EnemyInvestigateState(EnemyBrain brain, EnemyMovement movement, EnemyData data,
            float lookAroundSeconds, float lookAroundAngle, float lookAroundCycleSeconds)
        {
            _brain = brain;
            _movement = movement;
            _data = data;
            _lookAroundSeconds = lookAroundSeconds;
            _lookAroundAngle = lookAroundAngle;
            _lookAroundCycleSeconds = lookAroundCycleSeconds;
        }

        /// <summary>
        /// 이 상태의 번호
        /// </summary>
        public EnemyState Id => EnemyState.Investigate;

        /// <summary>
        /// 조사할 위치를 정한다. 상태에 들어가기 전에 EnemyBrain이 호출한다.
        /// </summary>
        /// <param name="destination">조사할 위치</param>
        public void SetDestination(Vector3 destination)
        {
            _destination = destination;
        }

        /// <summary>
        /// 조사 위치로 걷기 시작한다. 갈 수 없는 위치면 그 자리에서 바로 둘러본다.
        /// </summary>
        public void Enter()
        {
            _isLookingAround = false;

            if (!_movement.MoveTo(_destination, _data.PatrolSpeed))
            {
                StartLookingAround();
            }
        }

        /// <summary>
        /// 도착할 때까지 걷고, 도착하면 둘러보다가 시간이 다 되면 복귀한다.
        /// </summary>
        /// <param name="deltaTime">지난 프레임 이후 시간 (초)</param>
        public void Tick(float deltaTime)
        {
            if (!_isLookingAround)
            {
                if (_movement.HasArrived)
                {
                    StartLookingAround();
                }

                return;
            }

            _lookTimer += deltaTime;

            // 사인 곡선으로 처음 방향 기준 왼쪽 끝과 오른쪽 끝을 부드럽게 오간다
            float cycleProgress = _lookTimer / _lookAroundCycleSeconds * Mathf.PI * 2f;
            float yaw = _baseYaw + Mathf.Sin(cycleProgress) * _lookAroundAngle;
            _movement.FaceDirection(Quaternion.Euler(0f, yaw, 0f) * Vector3.forward, deltaTime);

            if (_lookTimer >= _lookAroundSeconds)
            {
                _brain.GoToReturn();
            }
        }

        /// <summary>
        /// 나갈 때 할 일은 없다. 다음 상태가 이동을 새로 정한다.
        /// </summary>
        public void Exit()
        {
        }

        /// <summary>
        /// 멈춰서 둘러보기를 시작한다. 지금 바라보는 방향을 기준으로 좌우를 본다.
        /// </summary>
        private void StartLookingAround()
        {
            _movement.Stop();
            _isLookingAround = true;
            _lookTimer = 0f;
            _baseYaw = _brain.transform.eulerAngles.y;
        }
    }
}
