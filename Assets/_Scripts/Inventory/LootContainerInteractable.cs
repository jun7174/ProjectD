// 파일 경로: Assets/_Scripts/Inventory/LootContainerInteractable.cs
using ProjectD.Core;
using UnityEngine;

namespace ProjectD.Inventory
{
    /// <summary>
    /// 루팅 상자를 F를 길게 눌러 여는 상호작용. 열면 상호작용한 플레이어의 PlayerLootSession에 상자를 넘긴다.
    /// 상자를 여는 것 자체는 내 화면에서만 일어나고, 꺼내기만 마스터 승인을 받는다 (여러 명이 동시에 열 수 있음).
    /// 상자와 적 프리팹 루트에 LootContainer와 함께 붙인다. Interactable 레이어 콜라이더가 자신이나 자식에 있어야 한다.
    /// </summary>
    public class LootContainerInteractable : MonoBehaviour, IInteractable
    {
        [Tooltip("우선순위. 부활 2, 상자 1, 아이템 0")]
        [SerializeField] private int _priority = 1;

        [Tooltip("F를 누르고 있어야 하는 시간 (초)")]
        [Min(0f)]
        [SerializeField] private float _holdDuration = 1f;

        [Header("참조 (비워 두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private LootContainer _container;

        /// <summary>우선순위</summary>
        public int Priority => _priority;

        /// <summary>누르고 있어야 하는 시간</summary>
        public float HoldDuration => _holdDuration;

        /// <summary>거리 비교 위치</summary>
        public Vector3 Position => transform.position;

        /// <summary>안내 문구. 비었으면 비어 있다고 표시한다</summary>
        public string Prompt
        {
            get
            {
                if (_container == null)
                {
                    return string.Empty;
                }

                return _container.IsEmpty ? $"열기: {_container.DisplayName} (비어 있음)" : $"열기: {_container.DisplayName}";
            }
        }

        /// <summary>내용물을 받았고 켜져 있을 때만 열 수 있다</summary>
        public bool CanInteract => isActiveAndEnabled && _container != null && _container.HasContents;

        /// <summary>
        /// 참조를 찾고 비어 있으면 경고한다.
        /// </summary>
        private void Awake()
        {
            if (_container == null && !TryGetComponent(out _container))
            {
                Debug.LogWarning($"[{nameof(LootContainerInteractable)}] LootContainer를 찾지 못했습니다. 상자를 열 수 없습니다.", this);
            }
        }

        /// <summary>
        /// 상호작용한 플레이어의 루팅 창에 이 상자를 연다.
        /// </summary>
        /// <param name="interactor">상호작용한 플레이어 캐릭터</param>
        public void Interact(GameObject interactor)
        {
            if (interactor.TryGetComponent(out PlayerLootSession session))
            {
                session.Open(_container);
            }
        }
    }
}
