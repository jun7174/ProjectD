// 파일 경로: Assets/_Scripts/Network/EnemyDormancy.cs
using System.Collections;
using Photon.Pun;
using ProjectD.AI;
using ProjectD.Combat;
using ProjectD.Core.Define;
using UnityEngine;

namespace ProjectD.Network
{
    /// <summary>
    /// 플레이어와 먼 적을 휴면시켜 AI와 위치 전송을 멈춘다.
    /// 마스터가 일정 간격으로 가장 가까운 플레이어와의 거리를 보고,
    /// 깨우는 거리 안이면 깨우고, 재우는 거리 밖이면 재운다. 두 거리를 다르게 두어 경계에서 깜빡이지 않게 한다.
    /// 추적이나 공격 중에는 재우지 않고, 휴면 중에 맞으면 바로 깨운다.
    /// 휴면 중에는 PhotonView 전송을 끄고, 누가 들어오면 잠깐 켜서 현재 위치와 상태를 한 번 보낸다.
    /// AI라서 마스터에서만 동작한다. 적 프리팹 루트에 붙이고 EnemyNetworkSetup의 Authority Only 목록에 넣는다.
    /// </summary>
    public class EnemyDormancy : MonoBehaviourPunCallbacks
    {
        [Header("거리 (m)")]
        [Tooltip("가장 가까운 플레이어가 이 거리 안이면 깨운다")]
        [Min(0f)]
        [SerializeField] private float _wakeDistance = 25f;

        [Tooltip("가장 가까운 플레이어가 이 거리 밖이면 재운다. 깨우는 거리보다 커야 한다")]
        [Min(0f)]
        [SerializeField] private float _sleepDistance = 30f;

        [Header("시간 (초)")]
        [Tooltip("거리 검사 간격")]
        [Min(0.05f)]
        [SerializeField] private float _checkInterval = 0.5f;

        [Tooltip("누가 들어왔을 때 휴면 중인 적의 전송을 잠깐 켜 두는 시간. 정기 전송이 한 번 이상 나가도록 전송 간격보다 길게 둔다")]
        [Min(0.05f)]
        [SerializeField] private float _joinSyncSeconds = 0.3f;

        [Header("참조 (비워 두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private EnemyBrain _brain;
        [SerializeField] private EnemyPerception _perception;
        [SerializeField] private EnemyMovement _movement;
        [SerializeField] private EnemyStatus _status;
        [SerializeField] private Health _health;

        private ViewSynchronization _awakeSynchronization;
        private float _nextCheckTime;

        /// <summary>
        /// 휴면 중인지 여부
        /// </summary>
        public bool IsDormant { get; private set; }

        /// <summary>
        /// 참조를 찾고, 깨어 있을 때의 전송 방식을 기억한다.
        /// </summary>
        private void Awake()
        {
            if (_brain == null && !TryGetComponent(out _brain))
            {
                Debug.LogWarning($"[{nameof(EnemyDormancy)}] EnemyBrain을 찾지 못했습니다.", this);
            }

            if (_perception == null && !TryGetComponent(out _perception))
            {
                Debug.LogWarning($"[{nameof(EnemyDormancy)}] EnemyPerception을 찾지 못했습니다.", this);
            }

            if (_movement == null && !TryGetComponent(out _movement))
            {
                Debug.LogWarning($"[{nameof(EnemyDormancy)}] EnemyMovement를 찾지 못했습니다.", this);
            }

            if (_status == null && !TryGetComponent(out _status))
            {
                Debug.LogWarning($"[{nameof(EnemyDormancy)}] EnemyStatus를 찾지 못했습니다.", this);
            }

            if (_health == null && !TryGetComponent(out _health))
            {
                Debug.LogWarning($"[{nameof(EnemyDormancy)}] Health를 찾지 못했습니다. 휴면 중에 맞아도 깨어나지 않습니다.", this);
            }

            _awakeSynchronization = photonView.Synchronization;
        }

        /// <summary>
        /// 인스펙터 값이 바뀌면 재우는 거리가 깨우는 거리보다 작아지지 않게 맞춘다.
        /// </summary>
        private void OnValidate()
        {
            _sleepDistance = Mathf.Max(_sleepDistance, _wakeDistance);
        }

        /// <summary>
        /// 방 콜백을 등록하고 피격 이벤트를 구독한다.
        /// </summary>
        public override void OnEnable()
        {
            base.OnEnable();

            if (_health != null)
            {
                _health.OnDamaged += HandleDamaged;
            }
        }

        /// <summary>
        /// 방 콜백 등록과 구독을 해제한다.
        /// </summary>
        public override void OnDisable()
        {
            base.OnDisable();

            if (_health != null)
            {
                _health.OnDamaged -= HandleDamaged;
            }
        }

        /// <summary>
        /// 검사 간격마다 가장 가까운 플레이어와의 거리를 보고 깨우거나 재운다.
        /// </summary>
        private void Update()
        {
            if (Time.time < _nextCheckTime)
            {
                return;
            }

            _nextCheckTime = Time.time + _checkInterval;
            float nearestDistance = GetNearestTargetDistance();

            if (IsDormant)
            {
                if (nearestDistance <= _wakeDistance)
                {
                    Wake();
                }
            }
            else if (nearestDistance >= _sleepDistance && !IsEngaged())
            {
                Sleep();
            }
        }

        /// <summary>
        /// 누가 들어오면, 휴면 중인 적의 전송을 잠깐 켜서 현재 위치와 상태가 그 사람에게 가게 한다.
        /// </summary>
        /// <param name="newPlayer">들어온 플레이어</param>
        public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            if (IsDormant && PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(SendCurrentStateOnce());
            }
        }

        /// <summary>
        /// 전송을 잠깐 켰다가, 그동안 깨어나지 않았으면 다시 끈다.
        /// </summary>
        /// <returns>코루틴</returns>
        private IEnumerator SendCurrentStateOnce()
        {
            photonView.Synchronization = _awakeSynchronization;

            yield return new WaitForSeconds(_joinSyncSeconds);

            if (IsDormant)
            {
                photonView.Synchronization = ViewSynchronization.Off;
            }
        }

        /// <summary>
        /// 멈추고 AI와 위치 전송을 끈다.
        /// </summary>
        private void Sleep()
        {
            IsDormant = true;

            if (_movement != null)
            {
                _movement.Stop();
            }

            SetAiEnabled(false);
            photonView.Synchronization = ViewSynchronization.Off;
        }

        /// <summary>
        /// AI와 위치 전송을 다시 켠다. 멈춘 자리에서 하던 상태를 이어 간다.
        /// </summary>
        private void Wake()
        {
            IsDormant = false;
            SetAiEnabled(true);
            photonView.Synchronization = _awakeSynchronization;
        }

        /// <summary>
        /// 판단(EnemyBrain)과 감지(EnemyPerception)를 켜거나 끈다.
        /// </summary>
        /// <param name="isEnabled">켜려면 true</param>
        private void SetAiEnabled(bool isEnabled)
        {
            if (_brain != null)
            {
                _brain.enabled = isEnabled;
            }

            if (_perception != null)
            {
                _perception.enabled = isEnabled;
            }
        }

        /// <summary>
        /// 휴면 중에 맞으면 바로 깨운다. 멀리서 저격당해도 반응하게 하기 위함이다.
        /// </summary>
        /// <param name="amount">데미지 양</param>
        /// <param name="hitPoint">맞은 위치</param>
        private void HandleDamaged(float amount, Vector3 hitPoint)
        {
            if (IsDormant)
            {
                Wake();
            }
        }

        /// <summary>
        /// 추적이나 공격 중인지 확인한다.
        /// </summary>
        /// <returns>추적이나 공격 중이면 true</returns>
        private bool IsEngaged()
        {
            if (_status == null)
            {
                return false;
            }

            return _status.CurrentState == EnemyState.Chase || _status.CurrentState == EnemyState.Attack;
        }

        /// <summary>
        /// 가장 가까운 플레이어까지의 수평 거리를 구한다.
        /// </summary>
        /// <returns>가장 가까운 거리 (m). 플레이어가 없으면 무한대</returns>
        private float GetNearestTargetDistance()
        {
            float nearest = float.PositiveInfinity;

            foreach (EnemyTarget target in EnemyTarget.ActiveTargets)
            {
                Vector3 offset = target.transform.position - transform.position;
                offset.y = 0f;
                nearest = Mathf.Min(nearest, offset.magnitude);
            }

            return nearest;
        }
    }
}
