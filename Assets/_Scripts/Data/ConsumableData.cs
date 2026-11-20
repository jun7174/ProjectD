// 파일 경로: Assets/_Scripts/Data/ConsumableData.cs
using ProjectD.Core.Define;
using UnityEngine;

namespace ProjectD.Data
{
    /// <summary>
    /// 소모품(체력 회복, 스태미나 회복) 아이템 데이터. 무엇을 얼마나 회복하는지와 사용 시간을 더한다.
    /// </summary>
    [CreateAssetMenu(fileName = "ConsumableData", menuName = "ProjectD/Consumable Data")]
    public class ConsumableData : ItemData
    {
        [Header("소모품")]
        [Tooltip("회복하는 대상")]
        [SerializeField] private ConsumableEffect _effect = ConsumableEffect.Health;

        [Tooltip("회복량. 스태미나 회복은 최대치까지 채우므로 쓰지 않는다")]
        [Min(0f)]
        [SerializeField] private float _amount = 30f;

        [Tooltip("사용에 걸리는 시간 (초)")]
        [Min(0f)]
        [SerializeField] private float _useSeconds = 1.5f;

        /// <summary>회복하는 대상</summary>
        public ConsumableEffect Effect => _effect;

        /// <summary>회복량</summary>
        public float Amount => _amount;

        /// <summary>사용 시간 (초)</summary>
        public float UseSeconds => _useSeconds;

        /// <summary>
        /// 공통 검사 후 분류를 소모품으로 고정한다.
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            SetCategory(ItemCategory.Consumable);
        }
    }
}
