// 파일 경로: Assets/_Scripts/UI/LocalPlayerCrosshairBinder.cs
using ProjectD.Combat;
using ProjectD.Network;
using ProjectD.Player;
using UnityEngine;

namespace ProjectD.UI
{
    /// <summary>
    /// 내 캐릭터가 생성되면 조준점(CrosshairView)이 그 캐릭터의 조준과 탄퍼짐을 읽도록 연결한다.
    /// 여러 명이 있는 Raid 씬에서 조준점이 남의 캐릭터를 잡지 않게 하기 위해 쓴다.
    /// 게임 로직이 UI를 모르도록, UI 쪽에서 생성 이벤트를 구독해 연결한다. Raid 씬에 둔다.
    /// </summary>
    public class LocalPlayerCrosshairBinder : MonoBehaviour
    {
        [Header("참조 (비워 두면 씬에서 찾는다)")]
        [SerializeField] private RaidPlayerSpawner _spawner;
        [SerializeField] private CrosshairView _crosshair;

        /// <summary>
        /// 참조를 찾고, 비어 있으면 경고 후 이 컴포넌트를 끈다.
        /// </summary>
        private void Awake()
        {
            if (_spawner == null)
            {
                _spawner = FindFirstObjectByType<RaidPlayerSpawner>();
            }

            if (_crosshair == null)
            {
                _crosshair = FindFirstObjectByType<CrosshairView>();
            }

            if (_spawner == null || _crosshair == null)
            {
                Debug.LogWarning($"[{nameof(LocalPlayerCrosshairBinder)}] RaidPlayerSpawner 또는 CrosshairView를 찾지 못했습니다. 조준점이 내 캐릭터와 연결되지 않습니다.", this);
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
        /// 내 캐릭터의 PlayerAimer와 PlayerWeaponSpread를 조준점에 연결한다.
        /// </summary>
        /// <param name="localPlayer">생성된 내 캐릭터</param>
        private void HandleLocalPlayerSpawned(GameObject localPlayer)
        {
            PlayerAimer aimer = localPlayer.GetComponent<PlayerAimer>();
            PlayerWeaponSpread weaponSpread = localPlayer.GetComponent<PlayerWeaponSpread>();

            if (aimer == null || weaponSpread == null)
            {
                Debug.LogWarning($"[{nameof(LocalPlayerCrosshairBinder)}] 내 캐릭터에서 PlayerAimer 또는 PlayerWeaponSpread를 찾지 못했습니다.", this);
            }

            _crosshair.SetTarget(aimer, weaponSpread);
        }
    }
}
