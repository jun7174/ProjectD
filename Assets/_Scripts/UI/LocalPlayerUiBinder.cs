// 파일 경로: Assets/_Scripts/UI/LocalPlayerUiBinder.cs
using ProjectD.Inventory;
using ProjectD.Network;
using ProjectD.Player;
using UnityEngine;

namespace ProjectD.UI
{
    /// <summary>
    /// 내 캐릭터가 생기면 인벤토리 창, 상자 창, 상호작용 안내, 채팅 화면을 그 캐릭터의 컴포넌트와 연결한다.
    /// Raid 씬에서는 RaidPlayerSpawner의 생성 이벤트로, Sandbox처럼 캐릭터가 씬에 미리 있으면 씬에서 찾아 연결한다.
    /// 화면들이 캐릭터를 직접 찾지 않게 연결을 이 컴포넌트 한곳에 모은다.
    /// Raid 씬(Sandbox 포함)에 하나 둔다.
    /// </summary>
    public class LocalPlayerUiBinder : MonoBehaviour
    {
        [Header("캐릭터 찾기")]
        [Tooltip("Raid 씬의 스폰 담당. 비워 두면 씬에서 찾는다")]
        [SerializeField] private RaidPlayerSpawner _spawner;

        [Tooltip("캐릭터가 씬에 미리 있는 경우(Sandbox) 시작할 때 씬에서 찾아 연결한다")]
        [SerializeField] private bool _findPlayerInScene;

        [Header("연결할 화면 (없는 화면은 비워 둔다)")]
        [SerializeField] private InventoryWindow _inventoryWindow;
        [SerializeField] private LootWindow _lootWindow;
        [SerializeField] private InteractionPromptView _interactionPrompt;
        [SerializeField] private ChatView _chatView;

        /// <summary>
        /// 스폰 담당을 찾는다.
        /// </summary>
        private void Awake()
        {
            if (_spawner == null)
            {
                _spawner = FindFirstObjectByType<RaidPlayerSpawner>();
            }

            if (_spawner == null && !_findPlayerInScene)
            {
                Debug.LogWarning($"[{nameof(LocalPlayerUiBinder)}] RaidPlayerSpawner를 찾지 못했습니다. Sandbox라면 Find Player In Scene을 켜 주세요.", this);
            }
        }

        /// <summary>
        /// 생성 이벤트를 구독하고, 이미 생성된 캐릭터가 있으면 바로 연결한다.
        /// </summary>
        private void OnEnable()
        {
            if (_spawner == null)
            {
                return;
            }

            _spawner.OnLocalPlayerSpawned += Bind;

            if (_spawner.LocalPlayer != null)
            {
                Bind(_spawner.LocalPlayer);
            }
        }

        /// <summary>
        /// 구독을 해제한다.
        /// </summary>
        private void OnDisable()
        {
            if (_spawner != null)
            {
                _spawner.OnLocalPlayerSpawned -= Bind;
            }
        }

        /// <summary>
        /// Sandbox처럼 캐릭터가 씬에 미리 있으면 찾아서 연결한다.
        /// </summary>
        private void Start()
        {
            if (!_findPlayerInScene)
            {
                return;
            }

            PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();
            if (inventory != null)
            {
                Bind(inventory.gameObject);
            }
            else
            {
                Debug.LogWarning($"[{nameof(LocalPlayerUiBinder)}] 씬에서 PlayerInventory가 있는 캐릭터를 찾지 못했습니다.", this);
            }
        }

        /// <summary>
        /// 캐릭터의 컴포넌트를 찾아 각 화면에 넘긴다.
        /// </summary>
        /// <param name="localPlayer">내 캐릭터</param>
        private void Bind(GameObject localPlayer)
        {
            localPlayer.TryGetComponent(out PlayerInventory inventory);
            localPlayer.TryGetComponent(out PlayerInputGate inputGate);
            localPlayer.TryGetComponent(out PlayerItemTransfer transfer);
            localPlayer.TryGetComponent(out PlayerLootSession lootSession);
            localPlayer.TryGetComponent(out PlayerInteractor interactor);

            if (inventory == null || inputGate == null || lootSession == null || interactor == null)
            {
                Debug.LogWarning($"[{nameof(LocalPlayerUiBinder)}] 내 캐릭터에서 PlayerInventory, PlayerInputGate, PlayerLootSession, PlayerInteractor 중 일부를 찾지 못했습니다.", localPlayer);
            }

            if (_inventoryWindow != null)
            {
                _inventoryWindow.SetTarget(inventory, inputGate, transfer, lootSession);
            }

            if (_lootWindow != null)
            {
                _lootWindow.SetTarget(lootSession, transfer);
            }

            if (_interactionPrompt != null)
            {
                _interactionPrompt.SetTarget(interactor);
            }

            if (_chatView != null)
            {
                _chatView.SetTarget(inputGate);
            }
        }
    }
}
