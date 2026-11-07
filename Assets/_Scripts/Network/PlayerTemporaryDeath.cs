// 파일 경로: Assets/_Scripts/Network/PlayerTemporaryDeath.cs
using System;
using System.Collections;
using Photon.Pun;
using ProjectD.AI;
using ProjectD.Combat;
using UnityEngine;

namespace ProjectD.Network
{
    /// <summary>
    /// 플레이어 체력이 0이 되었을 때의 임시 처리. 다운과 사망이 들어오기 전까지 테스트를 끊지 않고 이어 가기 위한 것이다.
    /// 체력이 0이 되면 적의 대상에서 빠지고, 마스터가 정해진 시간 뒤 모두에게 회복을 알린다.
    /// 회복도 체력처럼 마스터가 정해야 모든 화면의 체력이 같게 유지된다.
    /// 플레이어 프리팹 루트에 붙이고, 모든 캐릭터에서 켜 둔다.
    /// </summary>
    public class PlayerTemporaryDeath : MonoBehaviourPun
    {
        [Tooltip("체력이 0이 된 뒤 회복까지 걸리는 시간 (초)")]
        [Min(0f)]
        [SerializeField] private float _reviveDelay = 3f;

        [Header("참조 (비워 두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private Health _health;
        [SerializeField] private EnemyTarget _enemyTarget;

        private Coroutine _reviveRoutine;

        /// <summary>
        /// 체력이 0이 되었을 때 발생한다.
        /// </summary>
        public event Action OnDown;

        /// <summary>
        /// 회복했을 때 발생한다.
        /// </summary>
        public event Action OnRevived;

        /// <summary>
        /// 참조를 찾고 비어 있으면 경고한다.
        /// </summary>
        private void Awake()
        {
            if (_health == null && !TryGetComponent(out _health))
            {
                Debug.LogWarning($"[{nameof(PlayerTemporaryDeath)}] Health를 찾지 못했습니다.", this);
            }

            if (_enemyTarget == null && !TryGetComponent(out _enemyTarget))
            {
                Debug.LogWarning($"[{nameof(PlayerTemporaryDeath)}] EnemyTarget을 찾지 못했습니다. 쓰러져도 적이 계속 노립니다.", this);
            }
        }

        /// <summary>
        /// 사망 이벤트를 구독한다.
        /// </summary>
        private void OnEnable()
        {
            if (_health != null)
            {
                _health.OnDied += HandleDied;
            }
        }

        /// <summary>
        /// 구독을 해제하고 대기 중인 회복을 멈춘다.
        /// </summary>
        private void OnDisable()
        {
            if (_health != null)
            {
                _health.OnDied -= HandleDied;
            }

            if (_reviveRoutine != null)
            {
                StopCoroutine(_reviveRoutine);
                _reviveRoutine = null;
            }
        }

        /// <summary>
        /// 체력이 0이 되면 적의 대상에서 빠진다. 마스터(방 밖이면 이 화면)는 회복을 예약한다.
        /// </summary>
        private void HandleDied()
        {
            if (_enemyTarget != null)
            {
                _enemyTarget.SetTargetable(false);
            }

            OnDown?.Invoke();

            bool decidesRevive = !PhotonNetwork.InRoom || PhotonNetwork.IsMasterClient;
            if (decidesRevive && _reviveRoutine == null)
            {
                _reviveRoutine = StartCoroutine(ReviveAfterDelay());
            }
        }

        /// <summary>
        /// 정해진 시간을 기다린 뒤 회복시킨다. 방 안이면 모두에게 알린다.
        /// </summary>
        /// <returns>코루틴</returns>
        private IEnumerator ReviveAfterDelay()
        {
            yield return new WaitForSeconds(_reviveDelay);

            _reviveRoutine = null;

            if (PhotonNetwork.InRoom)
            {
                photonView.RPC(nameof(RpcRevive), RpcTarget.All);
            }
            else
            {
                Revive();
            }
        }

        /// <summary>
        /// 마스터가 보낸 회복을 받는다. 실제 처리는 Revive에서 한다.
        /// </summary>
        [PunRPC]
        private void RpcRevive()
        {
            Revive();
        }

        /// <summary>
        /// 체력을 가득 채우고 다시 적의 대상이 된다.
        /// </summary>
        private void Revive()
        {
            if (_health != null)
            {
                _health.ResetHealth();
            }

            if (_enemyTarget != null)
            {
                _enemyTarget.SetTargetable(true);
            }

            OnRevived?.Invoke();
        }
    }
}
