// 파일 경로: Assets/_Scripts/Network/PlayerHitReporter.cs
using Photon.Pun;
using ProjectD.AI;
using ProjectD.Combat;
using ProjectD.Data;
using UnityEngine;

namespace ProjectD.Network
{
    /// <summary>
    /// 네트워크 방 안에서 플레이어 탄의 명중을 처리한다
    /// 쏜 사람 화면: 판정용 탄이 네트워크 대상(NetworkHealth)에 맞으면 마스터에게 보고한다. 데미지는 직접 주지 않는다.
    /// 마스터 화면: 받은 보고를 검증하고, 통과하면 대상의 NetworkHealth로 데미지를 모두에게 적용한다.
    /// 방 안에서만 ProjectileLauncher의 명중 처리를 자신으로 바꾸므로, 방 밖(Sandbox)에서는 기존처럼 바로 데미지가 들어간다.
    /// 플레이어 프리팹 루트에 붙인다.
    /// </summary>
    public class PlayerHitReporter : MonoBehaviourPun, IHitHandler
    {
        [Header("데이터")]
        [Tooltip("모든 무기 데이터 목록. 보고받은 무기 ID로 데미지와 사거리를 찾는다")]
        [SerializeField] private WeaponDatabase _weaponDatabase;

        [Header("마스터 검증")]
        [Tooltip("사거리 검사 여유값 (m). 위치 동기화 지연 때문에 실제보다 조금 멀게 보일 수 있어 무기 최대 사거리에 더한다")]
        [Min(0f)]
        [SerializeField] private float _rangeTolerance = 2f;

        [Header("참조 (비워 두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private ProjectileLauncher _launcher;

        [Tooltip("디버그 보고에 쓸 현재 무기를 읽는다")]
        [SerializeField] private PlayerWeaponHolder _weaponHolder;

        [Tooltip("이 캐릭터를 적의 추적 대상으로 나타내는 컴포넌트. 맞은 적이 공격자를 추적할 때 쓴다")]
        [SerializeField] private EnemyTarget _selfTarget;

        /// <summary>
        /// 참조를 찾고, 방 안이면 ProjectileLauncher의 명중 처리를 이 컴포넌트로 바꾼다.
        /// </summary>
        private void Awake()
        {
            if (_launcher == null)
            {
                _launcher = GetComponent<ProjectileLauncher>();
            }

            if (_weaponHolder == null)
            {
                _weaponHolder = GetComponent<PlayerWeaponHolder>();
            }

            if (_selfTarget == null && !TryGetComponent(out _selfTarget))
            {
                Debug.LogWarning($"[{nameof(PlayerHitReporter)}] EnemyTarget을 찾지 못했습니다. 맞은 적이 공격자를 추적하지 않습니다.", this);
            }

            if (_weaponDatabase == null || _launcher == null)
            {
                Debug.LogWarning($"[{nameof(PlayerHitReporter)}] WeaponDatabase 또는 ProjectileLauncher가 비어 있습니다. 네트워크 명중 처리가 동작하지 않습니다.", this);
                return;
            }

            if (PhotonNetwork.InRoom)
            {
                _launcher.SetHitHandler(this);
            }
        }

        /// <summary>
        /// 내 판정용 탄이 맞았을 때 호출된다. 맞은 것이 네트워크 대상이면 마스터에게 보고한다.
        /// 벽처럼 체력이 없는 대상은 보고하지 않는다.
        /// </summary>
        /// <param name="fireInfo">맞은 투사체의 발사 정보</param>
        /// <param name="hit">명중 정보</param>
        public void HandleHit(FireInfo fireInfo, RaycastHit hit)
        {
            if (!photonView.IsMine || fireInfo.Weapon == null)
            {
                return;
            }

            NetworkHealth target = hit.collider.GetComponentInParent<NetworkHealth>();
            if (target == null)
            {
                return;
            }

            SendHitReport(target, fireInfo.Weapon.WeaponId, hit.point);
        }

        /// <summary>
        /// 마스터에게 명중을 보고한다. 이 캐릭터의 PhotonView로 보내므로, 마스터 쪽에서도 같은 캐릭터 오브젝트가 받는다.
        /// </summary>
        /// <param name="target">맞은 대상</param>
        /// <param name="weaponId">쏜 무기의 ID</param>
        /// <param name="hitPoint">맞은 위치</param>
        private void SendHitReport(NetworkHealth target, int weaponId, Vector3 hitPoint)
        {
            photonView.RPC(nameof(RpcReportHit), RpcTarget.MasterClient, target.photonView.ViewID, weaponId, hitPoint);
        }

        /// <summary>
        /// 마스터가 명중 보고를 받는다. 실제 처리는 ValidateAndApplyHit에서 한다.
        /// </summary>
        /// <param name="targetViewId">맞은 대상의 PhotonView ID</param>
        /// <param name="weaponId">쏜 무기의 ID</param>
        /// <param name="hitPoint">맞은 위치</param>
        /// <param name="info">보낸 사람 등 메시지 정보 (PUN이 자동으로 채운다)</param>
        [PunRPC]
        private void RpcReportHit(int targetViewId, int weaponId, Vector3 hitPoint, PhotonMessageInfo info)
        {
            ValidateAndApplyHit(targetViewId, weaponId, hitPoint, info.Sender.ActorNumber);
        }

        /// <summary>
        /// 명중 보고를 검증하고, 통과하면 데미지를 적용한다. 하나라도 맞지 않으면 무시한다.
        /// 1. 이 캐릭터의 주인이 보낸 보고인지  2. 대상이 있고 살아 있는지  3. 무기 ID가 있는지
        /// 4. 쏜 사람과 대상의 거리가 무기 최대 사거리 + 여유값 이내인지
        /// 협동 PvE라 치트 방지는 이 정도로만 한다 (GameDesign 9-1).
        /// </summary>
        /// <param name="targetViewId">맞은 대상의 PhotonView ID</param>
        /// <param name="weaponId">쏜 무기의 ID</param>
        /// <param name="hitPoint">맞은 위치</param>
        /// <param name="senderActorNumber">보고를 보낸 플레이어 번호</param>
        private void ValidateAndApplyHit(int targetViewId, int weaponId, Vector3 hitPoint, int senderActorNumber)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            if (photonView.Owner == null || photonView.Owner.ActorNumber != senderActorNumber)
            {
                RejectHit("다른 플레이어의 캐릭터 이름으로 보낸 보고");
                return;
            }

            PhotonView targetView = PhotonView.Find(targetViewId);
            NetworkHealth target = targetView != null ? targetView.GetComponent<NetworkHealth>() : null;
            if (target == null || target.IsDead)
            {
                RejectHit("대상이 없거나 이미 죽음");
                return;
            }

            if (_weaponDatabase == null || !_weaponDatabase.TryGetWeapon(weaponId, out WeaponData weapon))
            {
                RejectHit($"알 수 없는 무기 ID {weaponId}");
                return;
            }

            float distance = Vector3.Distance(transform.position, target.transform.position);
            float allowedDistance = weapon.MaxRange + _rangeTolerance;
            if (distance > allowedDistance)
            {
                RejectHit($"사거리 밖 (거리 {distance:F1}m, 허용 {allowedDistance:F1}m)");
                return;
            }

            target.ApplyDamageAsMaster(weapon.Damage, hitPoint);
            NotifyAttacked(target);
        }

        /// <summary>
        /// 맞은 대상이 적이면 공격자(이 캐릭터)를 알려 바로 추적하게 한다
        /// 적 AI는 마스터에서만 돌고 이 함수도 마스터의 검증 흐름에서만 호출되므로, 별도 전송이 필요 없다.
        /// </summary>
        /// <param name="target">맞은 대상</param>
        private void NotifyAttacked(NetworkHealth target)
        {
            if (_selfTarget != null && target.TryGetComponent(out EnemyPerception perception))
            {
                perception.ReactToAttack(_selfTarget);
            }
        }

        /// <summary>
        /// 검증에 실패한 보고를 기록한다. 거절된 보고는 데미지를 주지 않는다.
        /// </summary>
        /// <param name="reason">거절 이유</param>
        private void RejectHit(string reason)
        {
            Debug.Log($"[{nameof(PlayerHitReporter)}] 명중 보고 거절: {reason} ({gameObject.name})", this);
        }

        /// <summary>
        /// 디버그용. 실제로 맞히지 않아도 씬의 첫 번째 네트워크 대상에게 명중을 보고한다.
        /// 대상에서 무기 사거리보다 멀리 떨어진 곳에서 실행하면 마스터가 거절하는지 확인할 수 있다.
        /// 플레이 중 인스펙터에서 이 컴포넌트 제목줄을 우클릭해 실행한다.
        /// </summary>
        [ContextMenu("디버그: 첫 번째 대상에게 명중 보고 보내기")]
        private void DebugSendHitReport()
        {
            if (!PhotonNetwork.InRoom || !photonView.IsMine)
            {
                Debug.LogWarning($"[{nameof(PlayerHitReporter)}] 방 안의 내 캐릭터에서만 실행할 수 있습니다.", this);
                return;
            }

            NetworkHealth target = FindFirstObjectByType<NetworkHealth>();
            WeaponData weapon = _weaponHolder != null ? _weaponHolder.CurrentWeapon : null;
            if (target == null || weapon == null)
            {
                Debug.LogWarning($"[{nameof(PlayerHitReporter)}] 네트워크 대상 또는 현재 무기를 찾지 못했습니다.", this);
                return;
            }

            SendHitReport(target, weapon.WeaponId, target.transform.position);
            Debug.Log($"[{nameof(PlayerHitReporter)}] 디버그 명중 보고를 보냈습니다. 대상까지 거리 {Vector3.Distance(transform.position, target.transform.position):F1}m, 무기 사거리 {weapon.MaxRange:F1}m", this);
        }
    }
}
