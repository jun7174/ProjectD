// 파일 경로: Assets/_Scripts/UI/LootWindow.cs
using System.Collections.Generic;
using ProjectD.Data;
using ProjectD.Inventory;
using ProjectD.Network;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectD.UI
{
    /// <summary>
    /// 열어 둔 상자(또는 적 시체)의 내용물 창. 인벤토리 창 옆에 뜨고, 상자를 열면 인벤토리 창도 함께 연다.
    /// 항목을 우클릭하거나 인벤토리 그리드로 끌어다 놓으면 꺼내기를 요청한다 (마스터 승인 후 인벤토리에 들어감).
    /// 다른 사람이 꺼내면 상자 내용 변경 이벤트를 받아 다시 그린다.
    /// Raid 씬 Canvas 아래에 두고, LocalPlayerUiBinder가 내 캐릭터와 연결한다.
    /// </summary>
    public class LootWindow : MonoBehaviour
    {
        [Header("화면")]
        [Tooltip("창 전체. 상자를 닫으면 꺼진다")]
        [SerializeField] private GameObject _windowRoot;

        [SerializeField] private TMP_Text _titleText;

        [Tooltip("항목을 늘어놓을 곳 (Grid Layout Group 권장)")]
        [SerializeField] private RectTransform _slotContainer;

        [SerializeField] private LootSlotView _slotPrefab;

        [Tooltip("드래그 중인 항목을 올려 둘 곳. 인벤토리 창과 같은 드래그 층")]
        [SerializeField] private RectTransform _dragLayer;

        [Header("데이터")]
        [SerializeField] private ItemDatabase _itemDatabase;
        [SerializeField] private ItemIconPalette _palette;

        [Header("다른 화면")]
        [SerializeField] private InventoryWindow _inventoryWindow;

        private readonly List<LootSlotView> _slots = new List<LootSlotView>();

        private PlayerLootSession _lootSession;
        private PlayerItemTransfer _transfer;
        private LootContainer _container;

        /// <summary>드래그 중인 항목을 올려 둘 곳</summary>
        public RectTransform DragLayer => _dragLayer;

        /// <summary>
        /// 창을 닫아 두고 참조를 확인한다.
        /// </summary>
        private void Awake()
        {
            if (_windowRoot == null || _slotContainer == null || _slotPrefab == null || _itemDatabase == null)
            {
                Debug.LogWarning($"[{nameof(LootWindow)}] 화면 또는 데이터 참조가 비어 있습니다. 상자 창이 동작하지 않습니다.", this);
                enabled = false;
                return;
            }

            _windowRoot.SetActive(false);
        }

        /// <summary>
        /// 구독을 해제한다.
        /// </summary>
        private void OnDestroy()
        {
            Unsubscribe();
            SetContainer(null);
        }

        /// <summary>
        /// 내 캐릭터의 루팅 세션과 아이템 요청 컴포넌트를 연결한다.
        /// </summary>
        /// <param name="lootSession">열어 둔 상자</param>
        /// <param name="transfer">꺼내기 요청</param>
        public void SetTarget(PlayerLootSession lootSession, PlayerItemTransfer transfer)
        {
            Unsubscribe();

            _lootSession = lootSession;
            _transfer = transfer;

            if (_lootSession != null)
            {
                _lootSession.OnContainerOpened += HandleContainerOpened;
                _lootSession.OnContainerClosed += HandleContainerClosed;
            }
        }

        /// <summary>
        /// 항목 하나를 꺼내 달라고 요청한다.
        /// </summary>
        /// <param name="entryIndex">항목 번호</param>
        public void TakeEntry(int entryIndex)
        {
            if (_transfer != null && _container != null)
            {
                _transfer.RequestTake(_container, entryIndex);
            }
        }

        /// <summary>
        /// 항목을 끌어다 놓은 위치가 인벤토리 그리드 위면 꺼내기를 요청한다.
        /// </summary>
        /// <param name="entryIndex">항목 번호</param>
        /// <param name="eventData">포인터 정보</param>
        public void HandleSlotDropped(int entryIndex, PointerEventData eventData)
        {
            if (_inventoryWindow != null && _inventoryWindow.IsOverGrid(eventData.position, eventData.pressEventCamera))
            {
                TakeEntry(entryIndex);
            }
        }

        /// <summary>
        /// 상자가 열리면 창과 인벤토리 창을 열고 내용을 그린다.
        /// </summary>
        /// <param name="container">연 상자</param>
        private void HandleContainerOpened(LootContainer container)
        {
            SetContainer(container);
            _windowRoot.SetActive(true);

            if (_titleText != null)
            {
                _titleText.text = container.DisplayName;
            }

            if (_inventoryWindow != null)
            {
                _inventoryWindow.Open();
            }

            Rebuild();
        }

        /// <summary>
        /// 상자가 닫히면 창을 닫는다.
        /// </summary>
        private void HandleContainerClosed()
        {
            SetContainer(null);
            _windowRoot.SetActive(false);
        }

        /// <summary>
        /// 그리는 상자를 바꾸고 내용 변경 이벤트를 다시 구독한다.
        /// </summary>
        /// <param name="container">새 상자. 없으면 null</param>
        private void SetContainer(LootContainer container)
        {
            if (_container != null)
            {
                _container.OnChanged -= Rebuild;
            }

            _container = container;

            if (_container != null)
            {
                _container.OnChanged += Rebuild;
            }
        }

        /// <summary>
        /// 남아 있는 항목을 모두 다시 그린다.
        /// </summary>
        private void Rebuild()
        {
            foreach (LootSlotView slot in _slots)
            {
                if (slot != null)
                {
                    Destroy(slot.gameObject);
                }
            }

            _slots.Clear();

            if (_container == null)
            {
                return;
            }

            for (int i = 0; i < _container.Entries.Count; i++)
            {
                if (!_container.TryGetEntry(i, out ItemStack entry) || !_itemDatabase.TryGetItem(entry.ItemId, out ItemData item))
                {
                    continue;
                }

                LootSlotView slot = Instantiate(_slotPrefab, _slotContainer);
                slot.Setup(i, item, entry.Count, _palette, this);
                _slots.Add(slot);
            }
        }

        /// <summary>
        /// 루팅 세션 이벤트 구독을 해제한다.
        /// </summary>
        private void Unsubscribe()
        {
            if (_lootSession != null)
            {
                _lootSession.OnContainerOpened -= HandleContainerOpened;
                _lootSession.OnContainerClosed -= HandleContainerClosed;
            }
        }
    }
}
