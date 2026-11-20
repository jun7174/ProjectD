// 파일 경로: Assets/_Scripts/Inventory/PlayerLootSession.cs
using System;
using UnityEngine;

namespace ProjectD.Inventory
{
    /// <summary>
    /// 내 캐릭터가 지금 열어 둔 루팅 상자를 관리한다. 한 번에 하나만 열 수 있다.
    /// 상자에서 멀어지거나 상자가 사라지면 자동으로 닫는다.
    /// 화면(루팅 창)은 열림·닫힘 이벤트를 구독해 보여 준다.
    /// 플레이어 프리팹 루트에 붙이고 PlayerNetworkSetup의 Local Only 목록에 넣는다.
    /// </summary>
    public class PlayerLootSession : MonoBehaviour
    {
        [Tooltip("이 거리(m)보다 멀어지면 상자를 닫는다")]
        [Min(0.5f)]
        [SerializeField] private float _maxDistance = 3f;

        [Tooltip("거리 검사 간격 (초)")]
        [Min(0.05f)]
        [SerializeField] private float _checkInterval = 0.2f;

        private float _nextCheckTime;

        /// <summary>
        /// 열어 둔 상자. 없으면 null
        /// </summary>
        public LootContainer CurrentContainer { get; private set; }

        /// <summary>
        /// 상자를 열면 발생한다. 인자는 연 상자다.
        /// </summary>
        public event Action<LootContainer> OnContainerOpened;

        /// <summary>
        /// 상자를 닫으면 발생한다.
        /// </summary>
        public event Action OnContainerClosed;

        /// <summary>
        /// 상자를 연다. 다른 상자가 열려 있으면 먼저 닫는다.
        /// </summary>
        /// <param name="container">열 상자</param>
        public void Open(LootContainer container)
        {
            if (container == null || container == CurrentContainer)
            {
                return;
            }

            Close();
            CurrentContainer = container;
            OnContainerOpened?.Invoke(container);
        }

        /// <summary>
        /// 열어 둔 상자를 닫는다.
        /// </summary>
        public void Close()
        {
            if (CurrentContainer == null)
            {
                return;
            }

            CurrentContainer = null;
            OnContainerClosed?.Invoke();
        }

        /// <summary>
        /// 일정 간격으로 상자가 아직 있고 가까운지 확인한다.
        /// </summary>
        private void Update()
        {
            if (CurrentContainer == null || Time.time < _nextCheckTime)
            {
                return;
            }

            _nextCheckTime = Time.time + _checkInterval;

            bool isGone = !CurrentContainer.isActiveAndEnabled;
            bool isTooFar = Vector3.Distance(transform.position, CurrentContainer.transform.position) > _maxDistance;
            if (isGone || isTooFar)
            {
                Close();
            }
        }

        /// <summary>
        /// 꺼질 때 열어 둔 상자를 닫는다.
        /// </summary>
        private void OnDisable()
        {
            Close();
        }
    }
}
