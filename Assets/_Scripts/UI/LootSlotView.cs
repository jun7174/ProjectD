// 파일 경로: Assets/_Scripts/UI/LootSlotView.cs
using ProjectD.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectD.UI
{
    /// <summary>
    /// 상자 창의 항목 하나. 아이콘(또는 분류 색), 이름, 개수를 보여 준다.
    /// 우클릭이나 인벤토리 그리드로 끌어다 놓으면 LootWindow에 꺼내기를 넘긴다.
    /// </summary>
    public class LootSlotView : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _countText;

        [Tooltip("드래그하는 동안 마우스 입력을 통과시키는 데 쓴다")]
        [SerializeField] private CanvasGroup _canvasGroup;

        private LootWindow _owner;
        private Transform _originalParent;
        private int _originalSiblingIndex;
        private Vector3 _dragOffsetWorld;

        /// <summary>상자 안 항목 번호</summary>
        public int EntryIndex { get; private set; }

        /// <summary>
        /// 항목 내용을 그린다.
        /// </summary>
        /// <param name="entryIndex">상자 안 항목 번호</param>
        /// <param name="item">아이템 데이터</param>
        /// <param name="count">개수</param>
        /// <param name="palette">아이콘이 없을 때 쓸 색 모음</param>
        /// <param name="owner">꺼내기를 넘길 상자 창</param>
        public void Setup(int entryIndex, ItemData item, int count, ItemIconPalette palette, LootWindow owner)
        {
            EntryIndex = entryIndex;
            _owner = owner;

            bool hasIcon = item.Icon != null;
            _background.sprite = item.Icon;
            _background.color = hasIcon || palette == null ? Color.white : palette.GetColor(item);

            _nameText.text = item.DisplayName;
            _countText.text = item.MaxStack > 1 ? count.ToString() : string.Empty;
        }

        /// <summary>
        /// 우클릭하면 꺼내기를 요청한다.
        /// </summary>
        /// <param name="eventData">포인터 정보</param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right && _owner != null)
            {
                _owner.TakeEntry(EntryIndex);
            }
        }

        /// <summary>
        /// 드래그를 시작한다. 가장 위에 그려지도록 드래그 층으로 옮긴다.
        /// </summary>
        /// <param name="eventData">포인터 정보</param>
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_owner == null || _owner.DragLayer == null)
            {
                eventData.pointerDrag = null;
                return;
            }

            _originalParent = transform.parent;
            _originalSiblingIndex = transform.GetSiblingIndex();

            RectTransformUtility.ScreenPointToWorldPointInRectangle(_owner.DragLayer, eventData.position, eventData.pressEventCamera, out Vector3 pointerWorld);
            _dragOffsetWorld = transform.position - pointerWorld;
            transform.SetParent(_owner.DragLayer, true);

            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = false;
            }
        }

        /// <summary>
        /// 드래그 중 마우스를 따라간다.
        /// </summary>
        /// <param name="eventData">포인터 정보</param>
        public void OnDrag(PointerEventData eventData)
        {
            if (_owner != null && RectTransformUtility.ScreenPointToWorldPointInRectangle(_owner.DragLayer, eventData.position, eventData.pressEventCamera, out Vector3 pointerWorld))
            {
                transform.position = pointerWorld + _dragOffsetWorld;
            }
        }

        /// <summary>
        /// 원래 자리로 돌아오고, 인벤토리 그리드 위에 놓았으면 꺼내기를 요청한다.
        /// </summary>
        /// <param name="eventData">포인터 정보</param>
        public void OnEndDrag(PointerEventData eventData)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = true;
            }

            transform.SetParent(_originalParent, false);
            transform.SetSiblingIndex(_originalSiblingIndex);

            if (_owner != null)
            {
                _owner.HandleSlotDropped(EntryIndex, eventData);
            }
        }
    }
}
