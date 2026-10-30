// 파일 경로: Assets/_Scripts/Network/LocalPlayerCameraBinder.cs
using ProjectD.Player;
using Unity.Cinemachine;
using UnityEngine;

namespace ProjectD.Network
{
    /// <summary>
    /// 내 캐릭터가 생성되면 플레이어 카메라(Cinemachine Camera)가 그 캐릭터의 CameraTarget을 따라가게 연결한다.
    /// 플레이어가 프리팹으로 나중에 생성되기 때문에, 씬의 카메라와 캐릭터를 생성 시점에 이어 준다.
    /// Raid 씬에 둔다.
    /// </summary>
    public class LocalPlayerCameraBinder : MonoBehaviour
    {
        [Header("참조 (비워 두면 씬에서 찾는다)")]
        [SerializeField] private RaidPlayerSpawner _spawner;
        [SerializeField] private CinemachineCamera _playerCamera;

        /// <summary>
        /// 참조를 찾고, 비어 있으면 경고 후 이 컴포넌트를 끈다.
        /// </summary>
        private void Awake()
        {
            if (_spawner == null)
            {
                _spawner = FindFirstObjectByType<RaidPlayerSpawner>();
            }

            if (_playerCamera == null)
            {
                _playerCamera = FindFirstObjectByType<CinemachineCamera>();
            }

            if (_spawner == null || _playerCamera == null)
            {
                Debug.LogWarning($"[{nameof(LocalPlayerCameraBinder)}] RaidPlayerSpawner 또는 CinemachineCamera를 찾지 못했습니다. 카메라가 내 캐릭터를 따라가지 않습니다.", this);
                enabled = false;
            }
        }

        /// <summary>
        /// 내 캐릭터 생성 이벤트를 구독한다.
        /// </summary>
        private void OnEnable()
        {
            _spawner.OnLocalPlayerSpawned += HandleLocalPlayerSpawned;
        }

        /// <summary>
        /// 구독을 해제한다.
        /// </summary>
        private void OnDisable()
        {
            _spawner.OnLocalPlayerSpawned -= HandleLocalPlayerSpawned;
        }

        /// <summary>
        /// 내 캐릭터의 CameraTarget을 카메라 추적 대상으로 정하고, 이전 위치에서 끌려오지 않도록 바로 이동시킨다.
        /// CameraTarget은 PlayerCameraTargetOffset이 붙은 자식 오브젝트이며, 없으면 캐릭터 루트를 쓴다.
        /// </summary>
        /// <param name="localPlayer">생성된 내 캐릭터</param>
        private void HandleLocalPlayerSpawned(GameObject localPlayer)
        {
            PlayerCameraTargetOffset cameraTargetOffset = localPlayer.GetComponentInChildren<PlayerCameraTargetOffset>();

            if (cameraTargetOffset == null)
            {
                Debug.LogWarning($"[{nameof(LocalPlayerCameraBinder)}] 내 캐릭터에서 CameraTarget(PlayerCameraTargetOffset)을 찾지 못했습니다. 캐릭터 루트를 따라갑니다.", this);
            }

            Transform cameraTarget = cameraTargetOffset != null ? cameraTargetOffset.transform : localPlayer.transform;
            _playerCamera.Follow = cameraTarget;

            // 이전 프레임 상태를 무효로 해서, 다음 프레임에 감쇠 없이 바로 새 대상 위치로 이동하게 한다
            _playerCamera.PreviousStateIsValid = false;
        }
    }
}
