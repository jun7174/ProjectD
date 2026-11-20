// 파일 경로: Assets/_Scripts/Data/ItemData.cs
using ProjectD.Core.Define;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProjectD.Data
{
    /// <summary>
    /// 모든 아이템의 공통 데이터: ID, 이름, 분류, 희귀도, 그리드 크기, 최대 스택, 아이콘, 월드 모양, 판매가.
    /// 동작이 다른 아이템(무기, 탄약, 소모품)만 이 클래스를 상속해 필드를 더하고,
    /// 잡동사니, 교환 재화, 열쇠처럼 공통 값만 있는 아이템은 이 에셋을 그대로 만든다.
    /// ID 대역: 무기 1000, 탄약 3000, 방어구 4000, 가방 5000, 소모품 6000, 잡동사니 7000, 교환 재화 8000, 열쇠 9000.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemData", menuName = "ProjectD/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("기본 정보")]
        [Tooltip("아이템 고유 ID. 네트워크 전송과 저장에 쓰므로 모든 아이템이 서로 달라야 한다")]
        [FormerlySerializedAs("_weaponId")]
        [SerializeField] private int _itemId;

        [Tooltip("화면에 표시할 이름")]
        [FormerlySerializedAs("_weaponName")]
        [SerializeField] private string _displayName = "새 아이템";

        [Tooltip("아이템 분류. 무기, 탄약, 소모품 데이터는 저장할 때 알맞은 값으로 자동으로 맞춰진다")]
        [SerializeField] private ItemCategory _category = ItemCategory.Junk;

        [Tooltip("희귀도")]
        [SerializeField] private ItemRarity _rarity = ItemRarity.Common;

        [Header("인벤토리")]
        [Tooltip("그리드에서 차지하는 칸 수 (가로, 세로). 회전하면 가로와 세로가 바뀐다")]
        [SerializeField] private Vector2Int _gridSize = Vector2Int.one;

        [Tooltip("한 칸에 겹쳐 둘 수 있는 최대 개수. 1이면 겹치지 않는다")]
        [Min(1)]
        [SerializeField] private int _maxStack = 1;

        [Header("표시")]
        [Tooltip("인벤토리 아이콘. 비워 두면 분류별 색 사각형으로 표시한다")]
        [SerializeField] private Sprite _icon;

        [Tooltip("땅에 떨어졌을 때 보일 모양. 비워 두면 기본 상자 모양을 쓴다")]
        [SerializeField] private GameObject _worldPrefab;

        [Header("경제")]
        [Tooltip("상점 판매가")]
        [Min(0)]
        [SerializeField] private int _sellPrice;

        /// <summary>아이템 고유 ID</summary>
        public int ItemId => _itemId;

        /// <summary>화면에 표시할 이름</summary>
        public string DisplayName => _displayName;

        /// <summary>분류</summary>
        public ItemCategory Category => _category;

        /// <summary>희귀도</summary>
        public ItemRarity Rarity => _rarity;

        /// <summary>그리드에서 차지하는 칸 수 (회전 전)</summary>
        public Vector2Int GridSize => _gridSize;

        /// <summary>한 칸에 겹쳐 둘 수 있는 최대 개수</summary>
        public int MaxStack => _maxStack;

        /// <summary>인벤토리 아이콘. 없으면 null</summary>
        public Sprite Icon => _icon;

        /// <summary>땅에 떨어졌을 때 보일 모양. 없으면 null</summary>
        public GameObject WorldPrefab => _worldPrefab;

        /// <summary>상점 판매가</summary>
        public int SellPrice => _sellPrice;

        /// <summary>
        /// 인스펙터 값이 바뀌면 그리드 크기와 최대 스택이 1보다 작아지지 않게 맞춘다.
        /// 상속한 데이터는 이 함수를 덮어써 분류를 고정한다.
        /// </summary>
        protected virtual void OnValidate()
        {
            _gridSize = new Vector2Int(Mathf.Max(1, _gridSize.x), Mathf.Max(1, _gridSize.y));
            _maxStack = Mathf.Max(1, _maxStack);
        }

        /// <summary>
        /// 분류를 정한다. 무기·탄약·소모품 데이터가 OnValidate에서 자기 분류로 고정할 때 쓴다.
        /// </summary>
        /// <param name="category">고정할 분류</param>
        protected void SetCategory(ItemCategory category)
        {
            _category = category;
        }
    }
}
