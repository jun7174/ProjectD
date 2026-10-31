// 파일 경로: Assets/_Scripts/Network/PlayerFireSync.cs
using System.Collections.Generic;
using Photon.Pun;
using ProjectD.Combat;
using ProjectD.Data;
using UnityEngine;

namespace ProjectD.Network
{
    /// <summary>
    /// 플레이어의 발사를 다른 클라이언트에 전달하고, 받은 발사를 시각용 탄으로 재생한다
    /// 내 캐릭터: PlayerShooter가 한 번 쏠 때마다 방향 목록과 무기 ID를 RPC 하나로 다른 사람에게만 보낸다.
    /// 남의 캐릭터: 받은 무기 ID로 무기 데이터를 찾고, 이 캐릭터의 총구 위치에서 시각용 탄을 날린다.
    /// 시작점은 보내지 않는다. 남의 캐릭터는 약간 늦게 그려지므로, 받는 쪽 화면의 총구에서 쏘아야 탄이 캐릭터에서 나가 보인다.
    /// 플레이어 프리팹 루트에 붙인다.
    /// </summary>
    public class PlayerFireSync : MonoBehaviourPun
    {
        [Header("데이터")]
        [Tooltip("모든 무기 데이터 목록. 받은 무기 ID로 무기를 찾을 때 쓴다")]
        [SerializeField] private WeaponDatabase _weaponDatabase;

        [Header("참조 (비워 두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private PlayerShooter _shooter;
        [SerializeField] private ProjectileLauncher _launcher;

        /// <summary>
        /// 참조를 찾고 비어 있으면 경고한다.
        /// </summary>
        private void Awake()
        {
            if (_shooter == null)
            {
                _shooter = GetComponent<PlayerShooter>();
            }

            if (_launcher == null)
            {
                _launcher = GetComponent<ProjectileLauncher>();
            }

            if (_weaponDatabase == null)
            {
                Debug.LogWarning($"[{nameof(PlayerFireSync)}] WeaponDatabase가 비어 있습니다. 다른 플레이어의 탄이 보이지 않습니다.", this);
            }

            if (_shooter == null || _launcher == null)
            {
                Debug.LogWarning($"[{nameof(PlayerFireSync)}] PlayerShooter 또는 ProjectileLauncher를 찾지 못했습니다. 발사가 동기화되지 않습니다.", this);
            }
        }

        /// <summary>
        /// 발사 이벤트를 구독한다. 남의 캐릭터에서는 입력이 꺼져 있어 이벤트가 발생하지 않는다.
        /// </summary>
        private void OnEnable()
        {
            if (_shooter != null)
            {
                _shooter.OnShotFired += HandleShotFired;
            }
        }

        /// <summary>
        /// 발사 이벤트 구독을 해제한다.
        /// </summary>
        private void OnDisable()
        {
            if (_shooter != null)
            {
                _shooter.OnShotFired -= HandleShotFired;
            }
        }

        /// <summary>
        /// 내 캐릭터가 한 번 쏘면 방향 목록과 무기 ID를 다른 사람에게 보낸다.
        /// 방 밖(Sandbox)에서는 보내지 않는다. 지난 발사는 중도 참가자에게 필요 없으므로 서버에 보관하지 않는다.
        /// </summary>
        /// <param name="shotFireInfos">이번 발사로 나간 투사체들의 발사 정보</param>
        private void HandleShotFired(IReadOnlyList<FireInfo> shotFireInfos)
        {
            if (!PhotonNetwork.InRoom || !photonView.IsMine || shotFireInfos.Count == 0)
            {
                return;
            }

            int weaponId = shotFireInfos[0].Weapon.WeaponId;
            Vector3[] directions = new Vector3[shotFireInfos.Count];

            for (int i = 0; i < shotFireInfos.Count; i++)
            {
                directions[i] = shotFireInfos[i].Direction;
            }

            photonView.RPC(nameof(RpcPlayShot), RpcTarget.Others, weaponId, directions);
        }

        /// <summary>
        /// 다른 클라이언트가 보낸 발사를 받는다. 받은 값을 넘기기만 하고 실제 처리는 PlayRemoteShot에서 한다.
        /// </summary>
        /// <param name="weaponId">쏜 무기의 ID</param>
        /// <param name="directions">투사체마다의 발사 방향</param>
        [PunRPC]
        private void RpcPlayShot(int weaponId, Vector3[] directions)
        {
            PlayRemoteShot(weaponId, directions);
        }

        /// <summary>
        /// 받은 발사를 이 캐릭터의 총구 위치에서 시각용 탄으로 재생한다. 데미지 판정은 쏜 사람의 화면에서만 한다.
        /// </summary>
        /// <param name="weaponId">쏜 무기의 ID</param>
        /// <param name="directions">투사체마다의 발사 방향</param>
        private void PlayRemoteShot(int weaponId, Vector3[] directions)
        {
            if (_weaponDatabase == null || _shooter == null || _launcher == null || directions == null)
            {
                return;
            }

            if (!_weaponDatabase.TryGetWeapon(weaponId, out WeaponData weapon))
            {
                Debug.LogWarning($"[{nameof(PlayerFireSync)}] 무기 ID {weaponId}를 WeaponDatabase에서 찾지 못했습니다. 무기를 데이터베이스에 추가했는지 확인해 주세요.", this);
                return;
            }

            Vector3 origin = _shooter.MuzzlePosition;

            foreach (Vector3 direction in directions)
            {
                _launcher.LaunchVisualOnly(new FireInfo(origin, direction, weapon));
            }
        }
    }
}
