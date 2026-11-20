// 파일 경로: Assets/_Scripts/Player/PlayerInputGate.cs
using System;
using System.Collections.Generic;
using ProjectD.Core.Define;
using UnityEngine;

namespace ProjectD.Player
{
    /// <summary>
    /// 인벤토리나 채팅처럼 게임 입력을 잠시 막아야 할 때 한곳에서 처리한다.
    /// 막는 이유마다 끌 입력 컴포넌트를 인스펙터 목록으로 받아, 이유가 하나라도 남아 있으면 끄고 모두 풀리면 다시 켠다.
    /// 목록 방식이라 Player 모듈이 Combat의 입력 컴포넌트 클래스를 몰라도 된다 (PlayerNetworkSetup과 같은 방식).
    /// 인벤토리를 연 동안의 조준 고정은 PlayerInputReader가 IsAimBlocked를 보고 처리한다.
    /// 플레이어 프리팹 루트에 붙이고 PlayerNetworkSetup의 Local Only 목록에 넣는다.
    /// </summary>
    public class PlayerInputGate : MonoBehaviour
    {
        [Tooltip("인벤토리를 연 동안 끌 컴포넌트 (WeaponInputReader)")]
        [SerializeField] private Behaviour[] _disabledWhileInventoryOpen;

        [Tooltip("채팅 입력 중 끌 컴포넌트 (PlayerInputReader, WeaponInputReader, InteractionInputReader)")]
        [SerializeField] private Behaviour[] _disabledWhileChatting;

        private readonly HashSet<InputBlockReason> _activeReasons = new HashSet<InputBlockReason>();

        /// <summary>
        /// 조준(마우스 방향 회전, 정밀 조준)을 막아야 하는지 여부. 막는 이유가 하나라도 있으면 true
        /// </summary>
        public bool IsAimBlocked => _activeReasons.Count > 0;

        /// <summary>
        /// 막는 이유가 생기거나 풀리면 발생한다.
        /// </summary>
        public event Action OnChanged;

        /// <summary>
        /// 이 이유로 입력을 막고 있는지 확인한다.
        /// </summary>
        /// <param name="reason">확인할 이유</param>
        /// <returns>막고 있으면 true</returns>
        public bool IsBlocked(InputBlockReason reason)
        {
            return _activeReasons.Contains(reason);
        }

        /// <summary>
        /// 이유 하나를 켜거나 끄고, 입력 컴포넌트를 다시 켜고 끈다.
        /// </summary>
        /// <param name="reason">막는 이유</param>
        /// <param name="isBlocked">막으려면 true, 풀려면 false</param>
        public void SetBlocked(InputBlockReason reason, bool isBlocked)
        {
            bool changed = isBlocked ? _activeReasons.Add(reason) : _activeReasons.Remove(reason);
            if (!changed)
            {
                return;
            }

            ApplyToList(_disabledWhileInventoryOpen);
            ApplyToList(_disabledWhileChatting);
            OnChanged?.Invoke();
        }

        /// <summary>
        /// 꺼질 때 모든 이유를 풀어 입력이 꺼진 채로 남지 않게 한다.
        /// </summary>
        private void OnDisable()
        {
            if (_activeReasons.Count == 0)
            {
                return;
            }

            _activeReasons.Clear();
            ApplyToList(_disabledWhileInventoryOpen);
            ApplyToList(_disabledWhileChatting);
            OnChanged?.Invoke();
        }

        /// <summary>
        /// 목록의 컴포넌트마다 지금 막혀야 하는지 판단해 켜고 끈다.
        /// </summary>
        /// <param name="behaviours">판단할 컴포넌트 목록</param>
        private void ApplyToList(Behaviour[] behaviours)
        {
            if (behaviours == null)
            {
                return;
            }

            foreach (Behaviour behaviour in behaviours)
            {
                if (behaviour != null)
                {
                    behaviour.enabled = !ShouldDisable(behaviour);
                }
            }
        }

        /// <summary>
        /// 켜져 있는 이유 중 하나라도 이 컴포넌트를 끄라고 하는지 확인한다.
        /// </summary>
        /// <param name="behaviour">확인할 컴포넌트</param>
        /// <returns>꺼야 하면 true</returns>
        private bool ShouldDisable(Behaviour behaviour)
        {
            bool byInventory = IsBlocked(InputBlockReason.Inventory) && Contains(_disabledWhileInventoryOpen, behaviour);
            bool byChat = IsBlocked(InputBlockReason.Chat) && Contains(_disabledWhileChatting, behaviour);
            return byInventory || byChat;
        }

        /// <summary>
        /// 목록에 컴포넌트가 있는지 확인한다.
        /// </summary>
        /// <param name="behaviours">목록</param>
        /// <param name="behaviour">찾을 컴포넌트</param>
        /// <returns>있으면 true</returns>
        private static bool Contains(Behaviour[] behaviours, Behaviour behaviour)
        {
            return behaviours != null && Array.IndexOf(behaviours, behaviour) >= 0;
        }
    }
}
