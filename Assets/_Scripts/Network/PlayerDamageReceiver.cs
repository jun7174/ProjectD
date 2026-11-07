// 파일 경로: Assets/_Scripts/Network/PlayerDamageReceiver.cs
using System.Collections.Generic;
using Photon.Pun;
using ProjectD.Combat;
using ProjectD.Player;
using UnityEngine;

namespace ProjectD.Network
{
    /// <summary>
    /// 적 공격에 맞은 플레이어의 피격을 피격자 화면 기준으로 확인한 뒤 데미지를 적용한다.
    /// 1. 마스터: 적 공격이 닿으면 공격 번호를 만들어 데미지를 보관하고, 이 캐릭터의 주인에게 피격 신호를 보낸다.
    /// 2. 주인: 지금 회피 무적이거나 무적이 끝난 지 여유 시간 이내면 피한 것으로, 아니면 맞은 것으로 답한다.
    /// 3. 마스터: 답을 받으면 보관한 공격 번호를 지우고, 맞았다는 답이면 NetworkHealth로 데미지를 모두에게 적용한다.
    /// 피격자 화면에서 판단하므로, 위치 동기화 지연이 있어도 내 화면에서 피했으면 피한 것이 된다.
    /// 데미지 양은 마스터만 들고 있어 피격자가 바꿀 수 없고, 같은 공격 번호는 한 번만 처리된다.
    /// 플레이어 프리팹 루트에 붙이고, 모든 캐릭터에서 켜 둔다.
    /// </summary>
    public class PlayerDamageReceiver : MonoBehaviourPun
    {
        [Header("회피 판정")]
        [Tooltip("무적이 끝난 뒤에도 피한 것으로 봐 주는 시간 (초). 신호가 늦게 도착해 억울하게 맞는 것을 줄인다")]
        [Min(0f)]
        [SerializeField] private float _invincibleTolerance = 0.1f;

        [Tooltip("피해서 무시한 공격을 콘솔에 기록할지 여부. 회피 판정을 확인할 때 켠다")]
        [SerializeField] private bool _logDodgedHits = true;

        [Header("참조 (비워 두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private Health _health;
        [SerializeField] private NetworkHealth _networkHealth;
        [SerializeField] private PlayerDodger _dodger;

        // 마스터가 답을 기다리는 공격. 공격 번호 → 데미지
        private readonly Dictionary<int, float> _pendingDamages = new Dictionary<int, float>();

        // 공격 번호와 함께 보관하는 맞은 위치. 데미지 숫자 위치로 쓴다
        private readonly Dictionary<int, Vector3> _pendingHitPoints = new Dictionary<int, Vector3>();

        private int _nextAttackId;

        /// <summary>
        /// 참조를 찾고 비어 있으면 경고한다.
        /// </summary>
        private void Awake()
        {
            if (_health == null && !TryGetComponent(out _health))
            {
                Debug.LogWarning($"[{nameof(PlayerDamageReceiver)}] Health를 찾지 못했습니다. 플레이어가 데미지를 받지 않습니다.", this);
            }

            if (_networkHealth == null && !TryGetComponent(out _networkHealth))
            {
                Debug.LogWarning($"[{nameof(PlayerDamageReceiver)}] NetworkHealth를 찾지 못했습니다. 방 안에서 플레이어가 데미지를 받지 않습니다.", this);
            }

            if (_dodger == null && !TryGetComponent(out _dodger))
            {
                Debug.LogWarning($"[{nameof(PlayerDamageReceiver)}] PlayerDodger를 찾지 못했습니다. 회피 무적이 적용되지 않습니다.", this);
            }
        }

        /// <summary>
        /// 마스터가 적 공격이 이 캐릭터에 닿았을 때 호출한다. 주인에게 피격 신호를 보낸다.
        /// 방 밖에서는 이 화면이 곧 주인이므로 바로 확인하고 데미지를 넣는다.
        /// </summary>
        /// <param name="damage">데미지 양</param>
        /// <param name="hitPoint">맞은 위치</param>
        public void SendHitSignal(float damage, Vector3 hitPoint)
        {
            if (_health == null || _health.IsDead)
            {
                return;
            }

            if (!PhotonNetwork.InRoom)
            {
                if (!IsProtectedByDodge())
                {
                    _health.TakeDamage(damage, hitPoint);
                }

                return;
            }

            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            _nextAttackId++;
            _pendingDamages[_nextAttackId] = damage;
            _pendingHitPoints[_nextAttackId] = hitPoint;

            photonView.RPC(nameof(RpcReceiveHitSignal), photonView.Owner, _nextAttackId);
        }

        /// <summary>
        /// 주인이 마스터의 피격 신호를 받는다. 실제 처리는 AnswerHitSignal에서 한다.
        /// </summary>
        /// <param name="attackId">공격 번호</param>
        [PunRPC]
        private void RpcReceiveHitSignal(int attackId)
        {
            AnswerHitSignal(attackId);
        }

        /// <summary>
        /// 마스터가 주인의 답을 받는다. 실제 처리는 ConfirmHit에서 한다.
        /// </summary>
        /// <param name="attackId">공격 번호</param>
        /// <param name="isHit">맞았으면 true, 피했으면 false</param>
        /// <param name="info">보낸 사람 등 메시지 정보 (PUN이 자동으로 채운다)</param>
        [PunRPC]
        private void RpcAnswerHitSignal(int attackId, bool isHit, PhotonMessageInfo info)
        {
            ConfirmHit(attackId, isHit, info.Sender.ActorNumber);
        }

        /// <summary>
        /// 내 화면 기준으로 피했는지 판단해 마스터에게 답한다.
        /// </summary>
        /// <param name="attackId">공격 번호</param>
        private void AnswerHitSignal(int attackId)
        {
            if (!photonView.IsMine)
            {
                return;
            }

            bool isHit = !IsProtectedByDodge();
            if (!isHit && _logDodgedHits)
            {
                Debug.Log($"[{nameof(PlayerDamageReceiver)}] 회피 무적으로 공격 {attackId}번을 피했습니다.", this);
            }

            photonView.RPC(nameof(RpcAnswerHitSignal), RpcTarget.MasterClient, attackId, isHit);
        }

        /// <summary>
        /// 주인의 답으로 피격을 확정한다. 주인이 보낸 답인지, 기다리던 공격 번호인지 확인한다.
        /// 공격 번호는 답을 받는 순간 지우므로, 같은 번호로 두 번 답해도 한 번만 처리된다.
        /// </summary>
        /// <param name="attackId">공격 번호</param>
        /// <param name="isHit">맞았으면 true</param>
        /// <param name="senderActorNumber">답을 보낸 플레이어 번호</param>
        private void ConfirmHit(int attackId, bool isHit, int senderActorNumber)
        {
            if (!PhotonNetwork.IsMasterClient || photonView.Owner == null || photonView.Owner.ActorNumber != senderActorNumber)
            {
                return;
            }

            if (!_pendingDamages.TryGetValue(attackId, out float damage))
            {
                return;
            }

            Vector3 hitPoint = _pendingHitPoints[attackId];
            _pendingDamages.Remove(attackId);
            _pendingHitPoints.Remove(attackId);

            if (isHit && _networkHealth != null)
            {
                _networkHealth.ApplyDamageAsMaster(damage, hitPoint);
            }
        }

        /// <summary>
        /// 지금 회피 무적이거나, 무적이 끝난 지 여유 시간이 지나지 않았는지 확인한다.
        /// </summary>
        /// <returns>피한 것으로 보면 true</returns>
        private bool IsProtectedByDodge()
        {
            if (_dodger == null)
            {
                return false;
            }

            return _dodger.IsInvincible || Time.time - _dodger.InvincibleEndTime <= _invincibleTolerance;
        }
    }
}
