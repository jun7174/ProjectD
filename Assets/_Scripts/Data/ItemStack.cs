// 파일 경로: Assets/_Scripts/Data/ItemStack.cs
namespace ProjectD.Data
{
    /// <summary>
    /// 아이템 묶음 하나: 어떤 아이템(ID)이 몇 개인지.
    /// 네트워크 전송, 상자 내용물, 마스터의 아이템 목록 사본은 모두 이 값으로 다룬다.
    /// ScriptableObject는 보낼 수 없으므로 아이템은 ID로 가리키고, 필요할 때 ItemDatabase에서 찾는다.
    /// </summary>
    public readonly struct ItemStack
    {
        /// <summary>
        /// 아이템 묶음을 만든다.
        /// </summary>
        /// <param name="itemId">아이템 ID</param>
        /// <param name="count">개수</param>
        public ItemStack(int itemId, int count)
        {
            ItemId = itemId;
            Count = count;
        }

        /// <summary>
        /// 아이템 ID
        /// </summary>
        public int ItemId { get; }

        /// <summary>
        /// 개수
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// 비어 있는지 여부 (꺼내져서 개수가 0인 상자 칸 등)
        /// </summary>
        public bool IsEmpty => Count <= 0;

        /// <summary>
        /// 같은 아이템에 개수만 바꾼 묶음을 만든다.
        /// </summary>
        /// <param name="count">새 개수</param>
        /// <returns>개수가 바뀐 새 묶음</returns>
        public ItemStack WithCount(int count)
        {
            return new ItemStack(ItemId, count);
        }
    }
}
