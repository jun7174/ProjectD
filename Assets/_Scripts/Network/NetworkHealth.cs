// 파일 경로: Assets/_Scripts/Network/NetworkHealth.cs
using Photon.Pun;
using ProjectD.Combat;
using UnityEngine;

namespace ProjectD.Network
{
    /// <summary>
    /// 네트워크 대상(더미 적,적)의 체력을 모든 클라이언트에서 같게 맞춘다
    /// 데미지는 마스터만 정하고, 정한 데미지를 RPC로 모두에게 보내 각자 Health.TakeDamage를 실행한다.
    /// RPC는 순서가 보장되므로 모든 화면의 체력이 같은 순서로 줄고, 피격 깜빡임, 데미지 숫자, 사망 이벤트도 모든 화면에서 발생한다.
    /// 중도 참가자에게는 마스터가 현재 체력을 한 번 보낸다. 이후 적과 상자에도 같은 방식을 쓴다.
    /// Health, PhotonView와 같은 오브젝트에 붙인다.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class NetworkHealth : MonoBehaviourPunCallbacks
    {
        private Health _health;

        /// <summary>
        /// 체력이 0인지 여부
        /// </summary>
        public bool IsDead => _health.IsDead;

        /// <summary>
        /// Health를 캐싱한다.
        /// </summary>
        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        /// <summary>
        /// 마스터가 검증을 마친 데미지를 모두에게 적용한다. 마스터가 아니면 무시한다.
        /// RpcTarget.All은 보내는 쪽(마스터)에서는 바로 실행되므로, 같은 프레임에 들어온 다음 보고는 이미 줄어든 체력으로 검사된다.
        /// 그래서 두 명이 동시에 쏴도 사망이 한 번만 처리된다.
        /// </summary>
        /// <param name="amount">데미지 양</param>
        /// <param name="hitPoint">맞은 위치 (월드 좌표). 데미지 숫자 위치로 쓴다</param>
        public void ApplyDamageAsMaster(float amount, Vector3 hitPoint)
        {
            if (!PhotonNetwork.IsMasterClient || IsDead)
            {
                return;
            }

            photonView.RPC(nameof(RpcApplyDamage), RpcTarget.All, amount, hitPoint);
        }

        /// <summary>
        /// 새 플레이어가 들어오면 마스터가 그 사람에게만 현재 체력을 보낸다.
        /// 받는 쪽은 Raid 씬을 불러온 뒤에 이 메시지를 처리하므로 대상 오브젝트가 이미 있다.
        /// </summary>
        /// <param name="newPlayer">들어온 플레이어</param>
        public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            photonView.RPC(nameof(RpcSyncHealth), newPlayer, _health.CurrentHealth);
        }

        /// <summary>
        /// 마스터가 정한 데미지를 받는다. 실제 처리는 ApplyDamage에서 한다.
        /// </summary>
        /// <param name="amount">데미지 양</param>
        /// <param name="hitPoint">맞은 위치</param>
        [PunRPC]
        private void RpcApplyDamage(float amount, Vector3 hitPoint)
        {
            ApplyDamage(amount, hitPoint);
        }

        /// <summary>
        /// 중도 참가 시 마스터가 보낸 현재 체력을 받는다. 실제 처리는 SyncHealth에서 한다.
        /// </summary>
        /// <param name="currentHealth">마스터의 현재 체력</param>
        [PunRPC]
        private void RpcSyncHealth(float currentHealth)
        {
            SyncHealth(currentHealth);
        }

        /// <summary>
        /// 데미지를 Health에 넣는다. 피격 연출과 사망 처리는 Health 이벤트로 각자 일어난다.
        /// </summary>
        /// <param name="amount">데미지 양</param>
        /// <param name="hitPoint">맞은 위치</param>
        private void ApplyDamage(float amount, Vector3 hitPoint)
        {
            _health.TakeDamage(amount, hitPoint);
        }

        /// <summary>
        /// 체력을 마스터의 값으로 맞춘다. 이미 죽어 있던 대상이면 사망 처리(숨김, 재등장 대기)가 시작된다.
        /// </summary>
        /// <param name="currentHealth">마스터의 현재 체력</param>
        private void SyncHealth(float currentHealth)
        {
            _health.SetCurrentHealth(currentHealth);
        }
    }
}
