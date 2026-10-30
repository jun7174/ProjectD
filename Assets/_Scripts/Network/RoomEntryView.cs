// 파일 경로: Assets/_Scripts/UI/RoomEntryView.cs
using System;
using ProjectD.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectD.UI
{
    /// <summary>
    /// 방 목록의 한 줄. 방 이름, 인원, 참가 버튼을 보여 준다.
    /// RoomListView가 프리팹으로 생성하고 Setup으로 내용을 채운다.
    /// </summary>
    public class RoomEntryView : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private TMP_Text _roomNameText;
        [SerializeField] private TMP_Text _playerCountText;
        [SerializeField] private Button _joinButton;

        [Tooltip("참가 버튼 글자. 참가할 수 없는 방이면 문구를 바꾼다. 비워 두면 문구를 바꾸지 않는다")]
        [SerializeField] private TMP_Text _joinButtonText;

        private string _roomName;
        private Action<string> _onJoinClicked;

        /// <summary>
        /// 필수 참조를 확인하고 참가 버튼을 구독한다.
        /// </summary>
        private void Awake()
        {
            if (_roomNameText == null || _playerCountText == null || _joinButton == null)
            {
                Debug.LogWarning($"[{nameof(RoomEntryView)}] UI 참조(방 이름, 인원, 참가 버튼) 중 비어 있는 것이 있습니다. 항목 프리팹을 확인해 주세요.", this);
                return;
            }

            _joinButton.onClick.AddListener(HandleJoinClicked);
        }

        /// <summary>
        /// 참가 버튼 구독을 해제한다.
        /// </summary>
        private void OnDestroy()
        {
            if (_joinButton != null)
            {
                _joinButton.onClick.RemoveListener(HandleJoinClicked);
            }
        }

        /// <summary>
        /// 방 정보로 화면을 채운다.
        /// </summary>
        /// <param name="entry">표시할 방 정보</param>
        /// <param name="allowJoin">다른 요청을 기다리는 중이 아니어서 참가 버튼을 눌러도 되면 true</param>
        /// <param name="onJoinClicked">참가 버튼을 누르면 방 이름을 넘겨 호출할 함수</param>
        public void Setup(RoomListEntry entry, bool allowJoin, Action<string> onJoinClicked)
        {
            _roomName = entry.RoomName;
            _onJoinClicked = onJoinClicked;

            if (_roomNameText != null)
            {
                _roomNameText.text = entry.DisplayName;
            }

            if (_playerCountText != null)
            {
                _playerCountText.text = $"{entry.PlayerCount}/{entry.MaxPlayers}";
            }

            if (_joinButton != null)
            {
                _joinButton.interactable = entry.CanJoin && allowJoin;
            }

            if (_joinButtonText != null)
            {
                _joinButtonText.text = entry.CanJoin ? "참가" : "참가 불가";
            }
        }

        /// <summary>
        /// 참가 버튼을 누르면 이 방의 이름을 넘긴다.
        /// </summary>
        private void HandleJoinClicked()
        {
            _onJoinClicked?.Invoke(_roomName);
        }
    }
}
