// 파일 경로: Assets/_Scripts/Network/RaidPlayerSpawner.cs
using System;
using Photon.Pun;
using ProjectD.Core.Define;
using UnityEngine;

namespace ProjectD.Network
{
    /// <summary>
    /// Raid 씬에 들어오면 내 캐릭터를 시작 지점에 네트워크 생성한다. 중도 참가자도 같은 방식으로 생성한다.
    /// 시작 지점은 방 속성의 시작 지점 선택값으로 고르고, 시작 지점 안의 자리는 ActorNumber로 고른다
    /// 각 클라이언트는 자기 캐릭터만 생성하고, 남의 캐릭터는 PUN이 자동으로 생성해 준다.
    /// 내 캐릭터가 생기면 이벤트로 알려 카메라, 조준점이 연결되게 한다.
    /// </summary>
    public class RaidPlayerSpawner : MonoBehaviour
    {
        [Header("시작 지점")]
        [Tooltip("시작 지점 후보 2~3곳. 각 시작 지점의 자식 오브젝트들이 플레이어가 서는 자리다 (자리 4개 권장)")]
        [SerializeField] private Transform[] _startPoints;

        private bool _hasSpawned;

        /// <summary>
        /// 내 캐릭터가 생성되었을 때 발생한다. 인자는 생성된 내 캐릭터다.
        /// </summary>
        public event Action<GameObject> OnLocalPlayerSpawned;

        /// <summary>
        /// 생성된 내 캐릭터. 아직 생성 전이면 null
        /// </summary>
        public GameObject LocalPlayer { get; private set; }

        /// <summary>
        /// 시작 지점이 비어 있으면 경고한다.
        /// </summary>
        private void Awake()
        {
            if (_startPoints == null || _startPoints.Length == 0)
            {
                Debug.LogWarning($"[{nameof(RaidPlayerSpawner)}] 시작 지점이 비어 있습니다. 이 오브젝트 위치에 생성합니다.", this);
            }
        }

        /// <summary>
        /// 방에 들어와 있으면 내 캐릭터를 생성한다. 방 밖이면 RaidSessionWatcher가 Boot 씬으로 돌려보낸다.
        /// </summary>
        private void Start()
        {
            if (!PhotonNetwork.InRoom)
            {
                return;
            }

            SpawnLocalPlayer();
        }

        /// <summary>
        /// 내 캐릭터를 자리 위치에 네트워크 생성하고 알린다. 한 번만 생성한다.
        /// </summary>
        private void SpawnLocalPlayer()
        {
            if (_hasSpawned)
            {
                return;
            }

            Transform spawnSlot = ChooseSpawnSlot();
            LocalPlayer = PhotonNetwork.Instantiate(PrefabPaths.Player, spawnSlot.position, spawnSlot.rotation);
            _hasSpawned = true;

            if (LocalPlayer == null)
            {
                Debug.LogWarning($"[{nameof(RaidPlayerSpawner)}] 플레이어를 생성하지 못했습니다. Assets/Resources/{PrefabPaths.Player}.prefab이 있는지 확인해 주세요.", this);
                return;
            }

            OnLocalPlayerSpawned?.Invoke(LocalPlayer);
        }

        /// <summary>
        /// 내 캐릭터가 설 자리를 고른다.
        /// 시작 지점은 방 속성의 선택값을 시작 지점 수로 나눈 나머지로 골라 모두 같은 지점에 모이게 하고,
        /// 자리는 ActorNumber로 골라 서로 겹치지 않게 한다.
        /// </summary>
        /// <returns>생성할 자리</returns>
        private Transform ChooseSpawnSlot()
        {
            if (_startPoints == null || _startPoints.Length == 0)
            {
                return transform;
            }

            int seed = RaidRoomProperties.GetStartPointSeed(PhotonNetwork.CurrentRoom);
            Transform startPoint = _startPoints[seed % _startPoints.Length];

            if (startPoint.childCount == 0)
            {
                return startPoint;
            }

            // ActorNumber는 1부터 시작하므로 1을 빼서 첫 번째 플레이어가 첫 번째 자리에 서게 한다.
            // 나갔다 들어온 사람은 번호가 커져 다른 사람과 자리가 겹칠 수 있지만, 팀원끼리 충돌이 없어 문제되지 않는다
            int slotIndex = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % startPoint.childCount;
            return startPoint.GetChild(slotIndex);
        }
    }
}
