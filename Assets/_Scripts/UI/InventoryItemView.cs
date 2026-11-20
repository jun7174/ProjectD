// 파일 경로: Assets/_Scripts/UI/InventoryItemView.cs
using ProjectD.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectD.UI
{
    /// <summary>
    /// 인벤토리 그리드에 놓인 아이템 하나의 그림. 크기만큼의 사각형에 아이콘(또는 분류 색), 이름, 개수를 보여 준다.
    /// 드래그 입력을 받으면 판단은 하지 않고 InventoryWindow에 넘긴다.
    /// 프리팹 루트의 RectTransform은 앵커와 피벗을 왼쪽 위(0, 1)로 둔다.
    /// </summary>
    public class InventoryItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _countText;

        [Tooltip("드래그하는 동안 아래 칸을 찾을 수 있게 마우스 입력을 통과시키는 데 쓴다")]
        [SerializeField] private CanvasGroup _canvasGroup;

        [Tooltip("승인을 기다리는 동안 흐리게 보일 투명도")]
        [Range(0f, 1f)]
        [SerializeField] private float _reservedAlpha = 0.5f;

        private InventoryWindow _owner;

        /// <summary>그리는 아이템</summary>
        public InventoryEntry Entry { get; private set; }

        /// <summary>이 그림의 RectTransform</summary>
        public RectTransform RectTransform => (RectTransform)transform;

        /// <summary>
        /// 아이템과 주인 창을 정하고 내용을 그린다.
        /// </summary>
        /// <param name="entry">그릴 아이템</param>
        /// <param name="owner">드래그를 넘길 인벤토리 창</param>
        /// <param name="palette">아이콘이 없을 때 쓸 색 모음</param>
        public void Setup(InventoryEntry entry, InventoryWindow owner, ItemIconPalette palette)
        {
            Entry = entry;
            _owner = owner;

            bool hasIcon = entry.Item.Icon != null;
            _background.sprite = entry.Item.Icon;
            _background.color = hasIcon || palette == null ? Color.white : palette.GetColor(entry.Item);

            _nameText.text = entry.Item.DisplayName;
            _countText.text = entry.Item.MaxStack > 1 ? entry.Count.ToString() : string.Empty;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = entry.IsReserved ? _reservedAlpha : 1f;
            }
        }

        /// <summary>
        /// 드래그를 시작한다. 창이 거절하면(승인 대기 중 등) 드래그를 취소한다.
        /// </summary>
        /// <param name="eventData">포인터 정보</param>
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_owner == null || !_owner.BeginItemDrag(this, eventData))
            {
                eventData.pointerDrag = null;
                return;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = false;
            }
        }

        /// <summary>
        /// 드래그 중 위치를 창에 넘긴다.
        /// </summary>
        /// <param name="eventData">포인터 정보</param>
        public void OnDrag(PointerEventData eventData)
        {
            if (_owner != null)
            {
                _owner.DragItem(this, eventData);
            }
        }

        /// <summary>
        /// 드래그를 끝내고 놓은 위치를 창에 넘긴다.
        /// </summary>
        /// <param name="eventData">포인터 정보</param>
        public void OnEndDrag(PointerEventData eventData)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = true;
            }

            if (_owner != null)
            {
                _owner.EndItemDrag(this, eventData);
            }
        }
    }
}
