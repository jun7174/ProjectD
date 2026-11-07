// 파일 경로: Assets/_Scripts/AI/EnemyMovement.cs
using UnityEngine;
using UnityEngine.AI;

namespace ProjectD.AI
{
    /// <summary>
    /// NavMeshAgent로 적을 움직이고 돌린다. 상태들은 이 컴포넌트를 통해서만 이동한다.
    /// 목적지가 조금씩 바뀔 때마다 경로를 다시 계산하지 않도록, 일정 거리 이상 바뀔 때만 다시 계산한다.
    /// 목적지가 NavMesh 밖이면 가까운 NavMesh 위치로 옮겨서 간다.
    /// AI라서 마스터에서만 동작한다. EnemyNetworkSetup의 Authority Only 목록에 넣는다.
    /// 적 프리팹 루트에 NavMeshAgent와 함께 붙인다.
    /// </summary>
    public class EnemyMovement : MonoBehaviour
    {
        [Header("이동")]
        [Tooltip("목적지가 이 거리 이상 바뀔 때만 경로를 다시 계산한다 (m)")]
        [Min(0f)]
        [SerializeField] private float _repathDistance = 0.5f;

        [Tooltip("목적지를 NavMesh 위로 옮길 때 찾는 반경 (m)")]
        [Min(0.1f)]
        [SerializeField] private float _navMeshSampleRadius = 2f;

        [Tooltip("도착으로 볼 여유 거리 (m). NavMeshAgent의 Stopping Distance에 더한다")]
        [Min(0f)]
        [SerializeField] private float _arriveTolerance = 0.2f;

        [Header("회전")]
        [Tooltip("제자리에서 몸을 돌리는 속도 (도/초). 이동 중 회전은 NavMeshAgent의 Angular Speed를 따른다")]
        [Min(1f)]
        [SerializeField] private float _turnSpeed = 360f;

        [Header("참조 (비워 두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private NavMeshAgent _agent;

        // 마지막으로 요청받은 목적지 (NavMesh 위로 옮기기 전 값). 경로를 다시 계산할지 비교할 때 쓴다
        private Vector3 _requestedDestination;
        private bool _hasDestination;

        /// <summary>
        /// 목적지에 도착했는지 여부. 목적지가 없거나 멈춰 있으면 도착한 것으로 본다
        /// </summary>
        public bool HasArrived
        {
            get
            {
                if (!CanUseAgent || !_hasDestination)
                {
                    return true;
                }

                if (_agent.pathPending)
                {
                    return false;
                }

                return _agent.remainingDistance <= _agent.stoppingDistance + _arriveTolerance;
            }
        }

        /// <summary>
        /// NavMeshAgent를 쓸 수 있는지 여부. 꺼져 있거나 NavMesh 위에 없으면 명령하면 오류가 나므로 확인한다
        /// </summary>
        private bool CanUseAgent => _agent != null && _agent.enabled && _agent.isOnNavMesh;

        /// <summary>
        /// 참조를 찾고 비어 있으면 경고한다.
        /// </summary>
        private void Awake()
        {
            if (_agent == null && !TryGetComponent(out _agent))
            {
                Debug.LogWarning($"[{nameof(EnemyMovement)}] NavMeshAgent를 찾지 못했습니다. 적이 움직이지 않습니다.", this);
            }
        }

        /// <summary>
        /// 목적지로 이동한다. 목적지가 지난번과 거의 같으면 경로를 다시 계산하지 않고 속도만 바꾼다.
        /// </summary>
        /// <param name="destination">가고 싶은 위치</param>
        /// <param name="speed">이동 속도 (m/s)</param>
        /// <returns>이동을 시작했거나 이미 그쪽으로 가고 있으면 true. 갈 수 있는 위치가 없으면 false</returns>
        public bool MoveTo(Vector3 destination, float speed)
        {
            if (!CanUseAgent)
            {
                return false;
            }

            _agent.speed = speed;
            _agent.isStopped = false;

            bool isSameDestination = _hasDestination
                && (destination - _requestedDestination).sqrMagnitude < _repathDistance * _repathDistance;
            if (isSameDestination)
            {
                return true;
            }

            if (!NavMesh.SamplePosition(destination, out NavMeshHit hit, _navMeshSampleRadius, NavMesh.AllAreas))
            {
                return false;
            }

            _requestedDestination = destination;
            _hasDestination = _agent.SetDestination(hit.position);
            return _hasDestination;
        }

        /// <summary>
        /// 그 자리에 멈춘다. 다음 MoveTo는 경로를 새로 계산한다.
        /// </summary>
        public void Stop()
        {
            _hasDestination = false;

            if (!CanUseAgent)
            {
                return;
            }

            _agent.isStopped = true;
            _agent.ResetPath();
            _agent.velocity = Vector3.zero;
        }

        /// <summary>
        /// 경로 계산 없이 이만큼 바로 움직인다. 돌진처럼 정해진 방향으로 밀고 나갈 때 쓴다.
        /// NavMesh 경계에서는 막히므로 벽을 뚫지 않는다.
        /// </summary>
        /// <param name="offset">이번 프레임에 움직일 양 (월드 기준)</param>
        /// <returns>실제로 움직인 수평 거리 (m)</returns>
        public float MoveDirectly(Vector3 offset)
        {
            if (!CanUseAgent)
            {
                return 0f;
            }

            Vector3 before = transform.position;
            _agent.Move(offset);

            Vector3 moved = transform.position - before;
            moved.y = 0f;
            return moved.magnitude;
        }

        /// <summary>
        /// 위치를 향해 제자리에서 몸을 돌린다. 한 번에 다 돌지 않고 회전 속도만큼만 돈다.
        /// </summary>
        /// <param name="point">바라볼 위치</param>
        /// <param name="deltaTime">지난 프레임 이후 시간 (초)</param>
        public void FaceTowards(Vector3 point, float deltaTime)
        {
            FaceDirection(point - transform.position, deltaTime);
        }

        /// <summary>
        /// 방향을 향해 제자리에서 몸을 돌린다. 위아래 성분은 무시한다.
        /// </summary>
        /// <param name="direction">바라볼 방향</param>
        /// <param name="deltaTime">지난 프레임 이후 시간 (초)</param>
        public void FaceDirection(Vector3 direction, float deltaTime)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _turnSpeed * deltaTime);
        }
    }
}
