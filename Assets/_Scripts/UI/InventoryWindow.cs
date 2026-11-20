// 파일 경로: Assets/_Scripts/UI/InventoryWindow.cs
using System.Collections.Generic;
using ProjectD.Core.Define;
using ProjectD.Inventory;
using ProjectD.Network;
using ProjectD.Player;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectD.UI
{
    /// <summary>
    /// 내 인벤토리 창. Tab으로 열고 닫고, 그리드 칸과 아이템을 그리고, 드래그로 옮기기·회전·합치기·버리기를 처리한다.
    /// 그리드 계산은 하지 않고 PlayerInventory에 요청만 한다. 결과는 인벤토리 변경 이벤트를 받아 다시 그린다.
    /// 열린 동안에는 조준점을 숨겨 시스템 커서를 보이고, 사격과 조준을 막는다 (PlayerInputGate).
    /// 드래그 중 R로 회전하고, 창과 상자 창 밖에 놓으면 버린다.
    /// Raid 씬(Sandbox 포함) Canvas 아래에 두고, LocalPlayerUiBinder가 내 캐릭터와 연결한다.
    /// </summary>
    public class InventoryWindow : MonoBehaviour
    {
        [Header("화면")]
        [Tooltip("창 전체. 닫으면 꺼진다")]
        [SerializeField] private GameObject _windowRoot;

        [Tooltip("그리드 영역. 앵커와 피벗을 왼쪽 위(0, 1)로 둔다")]
        [SerializeField] private RectTransform _gridArea;

        [Tooltip("드래그 중인 아이템을 올려 둘 곳. Canvas 맨 아래 자식(가장 위에 그려짐)으로 둔다")]
        [SerializeField] private RectTransform _dragLayer;

        [Tooltip("빈 칸 배경 프리팹. 앵커와 피벗을 왼쪽 위로 둔다")]
        [SerializeField] private Image _cellPrefab;

        [Tooltip("아이템 그림 프리팹")]
        [SerializeField] private InventoryItemView _itemViewPrefab;

        [Tooltip("드래그 중 놓일 자리를 보여 줄 사각형 (그리드 영역의 자식, 앵커와 피벗 왼쪽 위)")]
        [SerializeField] private Image _placementPreview;

        [Tooltip("실패 문구를 띄울 글자 (선택)")]
        [SerializeField] private TMP_Text _messageText;

        [Tooltip("아이콘이 없을 때 쓸 분류별 색")]
        [SerializeField] private ItemIconPalette _palette;

        [Tooltip("이 영역 위에 놓으면 버리지 않는다 (인벤토리 창 배경, 상자 창)")]
        [SerializeField] private RectTransform[] _noDropAreas;

        [Tooltip("열린 동안 숨길 조준점 오브젝트. 조준점이 꺼지면 시스템 커서가 보인다")]
        [SerializeField] private GameObject _crosshairObject;

        [Header("모양")]
        [Tooltip("한 칸의 크기 (px)")]
        [Min(16f)]
        [SerializeField] private float _cellSize = 64f;

        [SerializeField] private Color _canPlaceColor = new Color(0.3f, 1f, 0.3f, 0.35f);
        [SerializeField] private Color _cannotPlaceColor = new Color(1f, 0.3f, 0.3f, 0.35f);

        [Tooltip("실패 문구를 보여 주는 시간 (초)")]
        [Min(0.1f)]
        [SerializeField] private float _messageSeconds = 2f;

        private readonly List<InventoryItemView> _itemViews = new List<InventoryItemView>();
        private readonly List<Image> _cells = new List<Image>();

        private PlayerControls _controls;
        private PlayerInventory _inventory;
        private PlayerInputGate _inputGate;
        private PlayerItemTransfer _transfer;
        private PlayerLootSession _lootSession;

        private InventoryItemView _draggedView;
        private bool _dragRotated;
        private Vector2Int _grabOffset;
        private Vector3 _dragOffsetWorld;
        private bool _rebuildPending;
        private float _messageHideTime;

        /// <summary>창이 열려 있는지 여부</summary>
        public bool IsOpen => _windowRoot != null && _windowRoot.activeSelf;

        /// <summary>
        /// 입력 클래스를 만들고 창을 닫아 둔다.
        /// </summary>
        private void Awake()
        {
            _controls = new PlayerControls();

            if (_windowRoot == null || _gridArea == null || _itemViewPrefab == null || _cellPrefab == null || _dragLayer == null)
            {
                Debug.LogWarning($"[{nameof(InventoryWindow)}] 화면 참조가 비어 있습니다. 인벤토리 창이 동작하지 않습니다.", this);
                enabled = false;
                return;
            }

            _windowRoot.SetActive(false);
            SetPreviewVisible(false);
            SetMessage(string.Empty);
        }

        /// <summary>
        /// 입력을 켠다.
        /// </summary>
        private void OnEnable()
        {
            _controls.Player.Enable();
        }

        /// <summary>
        /// 입력을 끈다.
        /// </summary>
        private void OnDisable()
        {
            _controls.Player.Disable();
        }

        /// <summary>
        /// 입력 클래스를 정리하고 구독을 해제한다.
        /// </summary>
        private void OnDestroy()
        {
            _controls.Dispose();
            Unsubscribe();
        }

        /// <summary>
        /// 내 캐릭터의 인벤토리와 관련 컴포넌트를 연결한다.
        /// </summary>
        /// <param name="inventory">인벤토리</param>
        /// <param name="inputGate">입력 막기</param>
        /// <param name="transfer">버리기 요청</param>
        /// <param name="lootSession">열어 둔 상자</param>
        public void SetTarget(PlayerInventory inventory, PlayerInputGate inputGate, PlayerItemTransfer transfer, PlayerLootSession lootSession)
        {
            Unsubscribe();

            _inventory = inventory;
            _inputGate = inputGate;
            _transfer = transfer;
            _lootSession = lootSession;

            if (_inventory != null)
            {
                _inventory.OnChanged += Rebuild;
            }

            if (_transfer != null)
            {
                _transfer.OnTransferFailed += SetMessage;
            }

            BuildCells();
            Rebuild();
        }

        /// <summary>
        /// 창을 연다.
        /// </summary>
        public void Open()
        {
            if (IsOpen || _inventory == null)
            {
                return;
            }

            _windowRoot.SetActive(true);
            SetCrosshairVisible(false);

            if (_inputGate != null)
            {
                _inputGate.SetBlocked(InputBlockReason.Inventory, true);
            }

            Rebuild();
        }

        /// <summary>
        /// 창을 닫는다. 열어 둔 상자도 닫는다.
        /// </summary>
        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            CancelDrag();
            _windowRoot.SetActive(false);
            SetCrosshairVisible(true);

            if (_inputGate != null)
            {
                _inputGate.SetBlocked(InputBlockReason.Inventory, false);
            }

            if (_lootSession != null)
            {
                _lootSession.Close();
            }
        }

        /// <summary>
        /// 화면 좌표가 그리드 위인지 확인한다. 상자 창에서 끌어온 아이템을 받을 때 쓴다.
        /// </summary>
        /// <param name="screenPoint">화면 좌표</param>
        /// <param name="eventCamera">UI 이벤트 카메라 (Overlay 캔버스는 null)</param>
        /// <returns>그리드 위면 true</returns>
        public bool IsOverGrid(Vector2 screenPoint, Camera eventCamera)
        {
            return IsOpen && TryGetCell(screenPoint, eventCamera, out _);
        }

        /// <summary>
        /// 아이템 드래그를 시작한다. 승인을 기다리는 아이템은 옮길 수 없다.
        /// </summary>
        /// <param name="view">드래그할 아이템 그림</param>
        /// <param name="eventData">포인터 정보</param>
        /// <returns>시작했으면 true</returns>
        public bool BeginItemDrag(InventoryItemView view, PointerEventData eventData)
        {
            if (view.Entry == null || view.Entry.IsReserved)
            {
                return false;
            }

            _draggedView = view;
            _dragRotated = view.Entry.IsRotated;

            _grabOffset = Vector2Int.zero;
            if (TryGetCell(eventData.position, eventData.pressEventCamera, out Vector2Int cell))
            {
                Vector2Int size = view.Entry.Size;
                _grabOffset = new Vector2Int(
                    Mathf.Clamp(cell.x - view.Entry.Position.x, 0, size.x - 1),
                    Mathf.Clamp(cell.y - view.Entry.Position.y, 0, size.y - 1));
            }

            RectTransformUtility.ScreenPointToWorldPointInRectangle(_dragLayer, eventData.position, eventData.pressEventCamera, out Vector3 pointerWorld);
            _dragOffsetWorld = view.RectTransform.position - pointerWorld;
            view.RectTransform.SetParent(_dragLayer, true);
            return true;
        }

        /// <summary>
        /// 드래그 중 아이템이 마우스를 따라오게 하고, 놓일 자리를 미리 보여 준다.
        /// </summary>
        /// <param name="view">드래그 중인 아이템 그림</param>
        /// <param name="eventData">포인터 정보</param>
        public void DragItem(InventoryItemView view, PointerEventData eventData)
        {
            if (view != _draggedView)
            {
                return;
            }

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(_dragLayer, eventData.position, eventData.pressEventCamera, out Vector3 pointerWorld))
            {
                view.RectTransform.position = pointerWorld + _dragOffsetWorld;
            }

            UpdatePreview(eventData.position, eventData.pressEventCamera);
        }

        /// <summary>
        /// 놓은 위치에 따라 옮기기, 합치기, 버리기를 요청하고 다시 그린다.
        /// </summary>
        /// <param name="view">드래그한 아이템 그림</param>
        /// <param name="eventData">포인터 정보</param>
        public void EndItemDrag(InventoryItemView view, PointerEventData eventData)
        {
            if (view != _draggedView)
            {
                return;
            }

            InventoryEntry entry = view.Entry;
            _draggedView = null;
            SetPreviewVisible(false);

            if (TryGetCell(eventData.position, eventData.pressEventCamera, out Vector2Int pointerCell))
            {
                InventoryEntry other = _inventory.Grid.GetEntryAt(pointerCell);
                if (CanMergeInto(entry, other))
                {
                    _inventory.TryMerge(entry, other);
                }
                else
                {
                    _inventory.TryMove(entry, pointerCell - _grabOffset, _dragRotated);
                }
            }
            else if (!IsOverNoDropArea(eventData.position, eventData.pressEventCamera) && _transfer != null)
            {
                _transfer.RequestDrop(entry);
            }

            _rebuildPending = false;
            Rebuild();
        }

        /// <summary>
        /// Tab으로 열고 닫고, Esc로 닫고, 드래그 중 R로 회전한다. 채팅 입력 중에는 키를 무시한다.
        /// </summary>
        private void Update()
        {
            UpdateMessage();

            if (_inventory == null)
            {
                return;
            }

            bool isChatting = _inputGate != null && _inputGate.IsBlocked(InputBlockReason.Chat);
            if (isChatting)
            {
                return;
            }

            if (_controls.Player.Inventory.WasPressedThisFrame())
            {
                if (IsOpen)
                {
                    Close();
                }
                else
                {
                    Open();
                }
            }
            else if (IsOpen && _controls.Player.Pause.WasPressedThisFrame())
            {
                Close();
            }

            if (_draggedView != null && _controls.Player.RotateItem.WasPressedThisFrame())
            {
                RotateDraggedItem();
            }
        }

        /// <summary>
        /// 드래그 중인 아이템을 90도 돌린다. 정사각형이면 그대로 둔다.
        /// 돌린 뒤에는 마우스가 아이템 왼쪽 위 칸의 가운데를 잡은 것으로 맞춘다.
        /// </summary>
        private void RotateDraggedItem()
        {
            Vector2Int baseSize = _draggedView.Entry.Item.GridSize;
            if (baseSize.x == baseSize.y)
            {
                return;
            }

            _dragRotated = !_dragRotated;
            _draggedView.RectTransform.sizeDelta = ToPixels(InventoryEntry.GetSize(_draggedView.Entry.Item, _dragRotated));

            _grabOffset = Vector2Int.zero;
            float halfCellWorld = _cellSize * 0.5f * _gridArea.lossyScale.x;
            Vector3 oldTopLeft = _draggedView.RectTransform.position;
            Vector3 pointerWorld = oldTopLeft - _dragOffsetWorld;
            _dragOffsetWorld = new Vector3(-halfCellWorld, halfCellWorld, 0f);
            _draggedView.RectTransform.position = pointerWorld + _dragOffsetWorld;
        }

        /// <summary>
        /// 그리드 크기에 맞춰 빈 칸 배경을 만든다.
        /// </summary>
        private void BuildCells()
        {
            foreach (Image cell in _cells)
            {
                Destroy(cell.gameObject);
            }

            _cells.Clear();

            if (_inventory == null)
            {
                return;
            }

            InventoryGrid grid = _inventory.Grid;
            _gridArea.sizeDelta = ToPixels(new Vector2Int(grid.Width, grid.Height));

            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    Image cell = Instantiate(_cellPrefab, _gridArea);
                    PlaceRect(cell.rectTransform, new Vector2Int(x, y), Vector2Int.one);
                    cell.transform.SetAsFirstSibling();
                    _cells.Add(cell);
                }
            }
        }

        /// <summary>
        /// 아이템 그림을 모두 다시 만든다. 드래그 중이면 끝난 뒤로 미룬다 (드래그 중인 그림이 사라지지 않게).
        /// </summary>
        private void Rebuild()
        {
            if (_draggedView != null)
            {
                _rebuildPending = true;
                return;
            }

            foreach (InventoryItemView view in _itemViews)
            {
                if (view != null)
                {
                    Destroy(view.gameObject);
                }
            }

            _itemViews.Clear();

            if (_inventory == null || !IsOpen)
            {
                return;
            }

            foreach (InventoryEntry entry in _inventory.Grid.Entries)
            {
                InventoryItemView view = Instantiate(_itemViewPrefab, _gridArea);
                PlaceRect(view.RectTransform, entry.Position, entry.Size);
                view.Setup(entry, this, _palette);
                _itemViews.Add(view);
            }
        }

        /// <summary>
        /// 드래그를 취소하고 원래 자리로 다시 그린다.
        /// </summary>
        private void CancelDrag()
        {
            if (_draggedView == null)
            {
                return;
            }

            _draggedView = null;
            SetPreviewVisible(false);
            _rebuildPending = false;
            Rebuild();
        }

        /// <summary>
        /// 마우스 아래에 놓일 자리를 초록(가능)이나 빨강(불가)으로 보여 준다. 그리드 밖이면 숨긴다.
        /// </summary>
        /// <param name="screenPoint">마우스 화면 좌표</param>
        /// <param name="eventCamera">UI 이벤트 카메라</param>
        private void UpdatePreview(Vector2 screenPoint, Camera eventCamera)
        {
            if (_placementPreview == null)
            {
                return;
            }

            if (!TryGetCell(screenPoint, eventCamera, out Vector2Int pointerCell))
            {
                SetPreviewVisible(false);
                return;
            }

            InventoryEntry entry = _draggedView.Entry;
            InventoryEntry other = _inventory.Grid.GetEntryAt(pointerCell);
            Vector2Int size = InventoryEntry.GetSize(entry.Item, _dragRotated);

            if (CanMergeInto(entry, other))
            {
                PlaceRect(_placementPreview.rectTransform, other.Position, other.Size);
                _placementPreview.color = _canPlaceColor;
            }
            else
            {
                Vector2Int target = pointerCell - _grabOffset;
                PlaceRect(_placementPreview.rectTransform, target, size);
                _placementPreview.color = _inventory.Grid.CanPlace(target, size, entry) ? _canPlaceColor : _cannotPlaceColor;
            }

            SetPreviewVisible(true);
            _placementPreview.transform.SetAsLastSibling();
        }

        /// <summary>
        /// 화면 좌표가 가리키는 그리드 칸을 구한다.
        /// </summary>
        /// <param name="screenPoint">화면 좌표</param>
        /// <param name="eventCamera">UI 이벤트 카메라</param>
        /// <param name="cell">칸 좌표</param>
        /// <returns>그리드 안이면 true</returns>
        private bool TryGetCell(Vector2 screenPoint, Camera eventCamera, out Vector2Int cell)
        {
            cell = Vector2Int.zero;
            if (_inventory == null || !RectTransformUtility.ScreenPointToLocalPointInRectangle(_gridArea, screenPoint, eventCamera, out Vector2 local))
            {
                return false;
            }

            // 그리드 영역의 피벗이 왼쪽 위라서, 아래로 갈수록 local.y가 음수가 된다
            cell = new Vector2Int(Mathf.FloorToInt(local.x / _cellSize), Mathf.FloorToInt(-local.y / _cellSize));
            return cell.x >= 0 && cell.y >= 0 && cell.x < _inventory.Grid.Width && cell.y < _inventory.Grid.Height;
        }

        /// <summary>
        /// 화면 좌표가 버리지 않는 영역 위인지 확인한다.
        /// </summary>
        /// <param name="screenPoint">화면 좌표</param>
        /// <param name="eventCamera">UI 이벤트 카메라</param>
        /// <returns>버리지 않는 영역 위면 true</returns>
        private bool IsOverNoDropArea(Vector2 screenPoint, Camera eventCamera)
        {
            if (_noDropAreas == null)
            {
                return false;
            }

            foreach (RectTransform area in _noDropAreas)
            {
                if (area != null && area.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(area, screenPoint, eventCamera))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 드래그한 묶음을 대상 묶음에 합칠 수 있는지 확인한다.
        /// </summary>
        /// <param name="source">드래그한 묶음</param>
        /// <param name="target">놓은 칸의 묶음</param>
        /// <returns>같은 아이템이고 대상에 자리가 있으면 true</returns>
        private static bool CanMergeInto(InventoryEntry source, InventoryEntry target)
        {
            return target != null && target != source && target.Item == source.Item && target.SpaceLeft > 0 && !target.IsReserved;
        }

        /// <summary>
        /// 칸 좌표와 크기로 사각형의 위치와 크기를 정한다. 사각형은 앵커와 피벗이 왼쪽 위여야 한다.
        /// </summary>
        /// <param name="rect">정할 사각형</param>
        /// <param name="cell">왼쪽 위 칸</param>
        /// <param name="size">차지 칸 수</param>
        private void PlaceRect(RectTransform rect, Vector2Int cell, Vector2Int size)
        {
            rect.anchoredPosition = new Vector2(cell.x * _cellSize, -cell.y * _cellSize);
            rect.sizeDelta = ToPixels(size);
        }

        /// <summary>
        /// 칸 수를 픽셀 크기로 바꾼다.
        /// </summary>
        /// <param name="size">칸 수</param>
        /// <returns>픽셀 크기</returns>
        private Vector2 ToPixels(Vector2Int size)
        {
            return new Vector2(size.x * _cellSize, size.y * _cellSize);
        }

        /// <summary>
        /// 놓일 자리 사각형을 보이거나 숨긴다.
        /// </summary>
        /// <param name="isVisible">보이려면 true</param>
        private void SetPreviewVisible(bool isVisible)
        {
            if (_placementPreview != null)
            {
                _placementPreview.gameObject.SetActive(isVisible);
            }
        }

        /// <summary>
        /// 조준점을 보이거나 숨긴다. 조준점이 꺼지면 시스템 커서가 보인다.
        /// </summary>
        /// <param name="isVisible">보이려면 true</param>
        private void SetCrosshairVisible(bool isVisible)
        {
            if (_crosshairObject != null)
            {
                _crosshairObject.SetActive(isVisible);
            }
        }

        /// <summary>
        /// 실패 문구를 잠시 보여 준다.
        /// </summary>
        /// <param name="message">문구. 빈 문자열이면 숨긴다</param>
        private void SetMessage(string message)
        {
            if (_messageText == null)
            {
                return;
            }

            _messageText.text = message;
            _messageText.gameObject.SetActive(!string.IsNullOrEmpty(message));
            _messageHideTime = Time.time + _messageSeconds;
        }

        /// <summary>
        /// 문구를 보여 주는 시간이 지나면 숨기고, 미뤄 둔 다시 그리기를 처리한다.
        /// </summary>
        private void UpdateMessage()
        {
            if (_messageText != null && _messageText.gameObject.activeSelf && Time.time >= _messageHideTime)
            {
                _messageText.gameObject.SetActive(false);
            }

            if (_rebuildPending && _draggedView == null)
            {
                _rebuildPending = false;
                Rebuild();
            }
        }

        /// <summary>
        /// 연결했던 컴포넌트의 이벤트 구독을 해제한다.
        /// </summary>
        private void Unsubscribe()
        {
            if (_inventory != null)
            {
                _inventory.OnChanged -= Rebuild;
            }

            if (_transfer != null)
            {
                _transfer.OnTransferFailed -= SetMessage;
            }
        }
    }
}
