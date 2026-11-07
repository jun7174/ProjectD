// 파일 경로: Assets/_Scripts/Network/EnemyAttackSync.cs
using Photon.Pun;
using ProjectD.AI;
using ProjectD.Combat;
using ProjectD.Core.Define;
using ProjectD.Data;
using UnityEngine;

namespace ProjectD.Network
{
    /// <summary>
    /// 적의 공격 결과를 네트워크에 반영한다.
    /// 발사: 마스터는 판정용 탄을 날리고, 다른 사람에게는 방향만 RPC로 보내 각자 화면의 총구에서 시각용 탄을 날리게 한다.
    /// 명중: 돌진이 닿거나 판정용 탄이 맞으면, 맞은 플레이어의 PlayerDamageReceiver에 피격 신호를 맡긴다.
    /// 공격은 마스터의 AI만 하므로 발사와 명중 처리도 마스터에서만 일어난다.
    /// 적 프리팹 루트에 붙이고, 모든 클라이언트에서 켜 둔다 (다른 클라이언트는 시각용 탄 RPC를 받아야 한다).
    /// </summary>
    public class EnemyAttackSync : MonoBehaviourPun, IHitHandler
    {
        [Header("데이터")]
        [Tooltip("공격 데미지와 적 탄의 무기 데이터를 읽는다")]
        [SerializeField] private EnemyData _data;

        [Header("참조 (비워 두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private EnemyWeapon _weapon;
        [SerializeField] private ProjectileLauncher _launcher;

        /// <summary>
        /// 참조를 찾고, 판정용 탄이 맞았을 때의 처리를 이 컴포넌트로 바꾼다.
        /// </summary>
        private void Awake()
        {
            if (_weapon == null && !TryGetComponent(out _weapon))
            {
                Debug.LogWarning($"[{nameof(EnemyAttackSync)}] EnemyWeapon을 찾지 못했습니다. 적 공격이 전달되지 않습니다.", this);
            }

            bool needsLauncher = _data != null && _data.AttackType == EnemyAttackType.Burst;
            if (_launcher == null && !TryGetComponent(out _launcher) && needsLauncher)
            {
                Debug.LogWarning($"[{nameof(EnemyAttackSync)}] ProjectileLauncher를 찾지 못했습니다. 적 탄이 나가지 않습니다.", this);
            }

            if (_data == null)
            {
                Debug.LogWarning($"[{nameof(EnemyAttackSync)}] EnemyData가 비어 있습니다. 적 공격이 전달되지 않습니다.", this);
            }

            if (_launcher != null)
            {
                _launcher.SetHitHandler(this);
            }
        }

        /// <summary>
        /// 공격 이벤트를 구독한다.
        /// </summary>
        private void OnEnable()
        {
            if (_weapon != null)
            {
                _weapon.OnMeleeHit += HandleMeleeHit;
                _weapon.OnFired += HandleFired;
            }
        }

        /// <summary>
        /// 구독을 해제한다.
        /// </summary>
        private void OnDisable()
        {
            if (_weapon != null)
            {
                _weapon.OnMeleeHit -= HandleMeleeHit;
                _weapon.OnFired -= HandleFired;
            }
        }

        /// <summary>
        /// 판정용 탄이 맞았을 때 호출된다. 맞은 것이 플레이어면 피격 신호를 보낸다. 벽이면 아무것도 하지 않는다.
        /// </summary>
        /// <param name="fireInfo">맞은 탄의 발사 정보</param>
        /// <param name="hit">명중 정보</param>
        public void HandleHit(FireInfo fireInfo, RaycastHit hit)
        {
            PlayerDamageReceiver receiver = hit.collider.GetComponentInParent<PlayerDamageReceiver>();
            if (receiver != null && _data != null)
            {
                receiver.SendHitSignal(_data.AttackDamage, hit.point);
            }
        }

        /// <summary>
        /// 돌진이 플레이어에 닿으면 피격 신호를 보낸다.
        /// </summary>
        /// <param name="target">닿은 대상</param>
        private void HandleMeleeHit(EnemyTarget target)
        {
            if (_data != null && target.TryGetComponent(out PlayerDamageReceiver receiver))
            {
                receiver.SendHitSignal(_data.AttackDamage, target.AimPoint);
            }
        }

        /// <summary>
        /// 적이 한 발 쏘면 판정용 탄을 날리고, 다른 사람에게 방향을 보낸다.
        /// 지난 발사는 중도 참가자에게 필요 없으므로 서버에 보관하지 않는다.
        /// </summary>
        /// <param name="direction">발사 방향</param>
        private void HandleFired(Vector3 direction)
        {
            if (!CanLaunch())
            {
                return;
            }

            _launcher.Launch(new FireInfo(_weapon.MuzzlePosition, direction, _data.ProjectileWeapon));

            if (PhotonNetwork.InRoom)
            {
                photonView.RPC(nameof(RpcPlayShot), RpcTarget.Others, direction);
            }
        }

        /// <summary>
        /// 마스터가 보낸 적의 발사를 받는다. 실제 처리는 PlayRemoteShot에서 한다.
        /// </summary>
        /// <param name="direction">발사 방향</param>
        [PunRPC]
        private void RpcPlayShot(Vector3 direction)
        {
            PlayRemoteShot(direction);
        }

        /// <summary>
        /// 받은 발사를 이 화면의 총구 위치에서 시각용 탄으로 재생한다. 명중 판정은 마스터에서만 한다.
        /// </summary>
        /// <param name="direction">발사 방향</param>
        private void PlayRemoteShot(Vector3 direction)
        {
            if (!CanLaunch())
            {
                return;
            }

            _launcher.LaunchVisualOnly(new FireInfo(_weapon.MuzzlePosition, direction, _data.ProjectileWeapon));
        }

        /// <summary>
        /// 탄을 날리는 데 필요한 참조와 데이터가 있는지 확인한다.
        /// </summary>
        /// <returns>탄을 날릴 수 있으면 true</returns>
        private bool CanLaunch()
        {
            if (_weapon == null || _launcher == null || _data == null)
            {
                return false;
            }

            if (_data.ProjectileWeapon == null)
            {
                Debug.LogWarning($"[{nameof(EnemyAttackSync)}] {_data.name}의 Projectile Weapon이 비어 있습니다. 적 탄이 나가지 않습니다.", this);
                return false;
            }

            return true;
        }
    }
}
