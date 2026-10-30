// 파일 경로: Assets/_Scripts/UI/RoomListView.cs
using ProjectD.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectD.UI
{
    /// <summary>
    /// Lobby 씬 화면. 방 목록을 그리고 방 만들기, 참가, 혼자 플레이 버튼을 처리한다.
    /// 방 목록이 바뀌면 항목을 전부 지우고 다시 그린다. 최대 4인 게임이라 방 수가 적어 이 방식으로 충분하다.
    /// 요청을 보낸 뒤 응답을 기다리는 동안에는 버튼을 막아 같은 요청이 두 번 가지 않게 한다.
    /// </summary>
    public class RoomListView : MonoBehaviour
    {
        [Header("로직 참조 (비워 두면 씬에서 찾는다)")]
        [SerializeField] private RoomListService _roomListService;
        [SerializeField] private SoloPlayStarter _soloPlayStarter;

        [Header("목록")]
        [Tooltip("항목이 생성될 부모. Scroll View의 Content를 연결한다")]
        [SerializeField] private Transform _roomListContent;
        [Tooltip("방 목록 한 줄 프리팹")]
        [SerializeField] private RoomEntryView _roomEntryPrefab;
        [Tooltip("방이 하나도 없을 때 보여 줄 오브젝트. Content 밖에 두어야 한다 (Content의 자식은 다시 그릴 때 지워진다)")]
        [SerializeField] private GameObject _emptyListMessage;

        [Header("버튼과 문구")]
        [SerializeField] private Button _createRoomButton;
        [SerializeField] private Button _soloPlayButton;
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private TMP_Text _playerNameText;

        private bool _isWaitingResponse;

        /// <summary>
        /// 참조를 찾고, 필수 참조가 비어 있으면 경고 후 이 컴포넌트를 끈다.
        /// </summary>
        private void Awake()
        {
            if (_roomListService == null)
            {
                _roomListService = FindFirstObjectByType<RoomListService>();
            }

            if (_soloPlayStarter == null)
            {
                _soloPlayStarter = FindFirstObjectByType<SoloPlayStarter>();
            }

            if (!HasRequiredReferences())
            {
                enabled = false;
            }
        }

        /// <summary>
        /// 버튼과 로직 이벤트를 구독한다.
        /// </summary>
        private void OnEnable()
        {
            _createRoomButton.onClick.AddListener(HandleCreateRoomClicked);
            _soloPlayButton.onClick.AddListener(HandleSoloPlayClicked);
            _roomListService.OnLobbyJoined += HandleLobbyJoined;
            _roomListService.OnRoomListChanged += HandleRoomListChanged;
            _roomListService.OnRequestFailed += HandleRequestFailed;
            _soloPlayStarter.OnSoloPlayFailed += HandleRequestFailed;
        }

        /// <summary>
        /// 구독을 해제한다.
        /// </summary>
        private void OnDisable()
        {
            _createRoomButton.onClick.RemoveListener(HandleCreateRoomClicked);
            _soloPlayButton.onClick.RemoveListener(HandleSoloPlayClicked);
            _roomListService.OnLobbyJoined -= HandleLobbyJoined;
            _roomListService.OnRoomListChanged -= HandleRoomListChanged;
            _roomListService.OnRequestFailed -= HandleRequestFailed;
            _soloPlayStarter.OnSoloPlayFailed -= HandleRequestFailed;
        }

        /// <summary>
        /// 내 이름과 첫 화면 상태를 표시한다.
        /// </summary>
        private void Start()
        {
            if (_playerNameText != null)
            {
                _playerNameText.text = $"이름: {_roomListService.LocalPlayerName}";
            }

            SetStatus(_roomListService.IsInLobby ? string.Empty : "로비에 들어가는 중...");
            RefreshButtons();
            RedrawRoomList();
        }

        /// <summary>
        /// 로비에 들어가면 안내 문구를 보여 주고 버튼을 연다.
        /// </summary>
        private void HandleLobbyJoined()
        {
            SetStatus("방을 만들거나 목록에서 참가하세요.");
            RefreshButtons();
        }

        /// <summary>
        /// 방 목록이 바뀌면 다시 그린다.
        /// </summary>
        private void HandleRoomListChanged()
        {
            RedrawRoomList();
        }

        /// <summary>
        /// 요청이 실패하면 기다림을 풀고 실패 이유를 보여 준다. 혼자 플레이 실패도 같은 방식으로 처리한다.
        /// </summary>
        /// <param name="message">실패 이유</param>
        private void HandleRequestFailed(string message)
        {
            _isWaitingResponse = false;
            SetStatus(message);
            RefreshButtons();
            RedrawRoomList();
        }

        /// <summary>
        /// 방 만들기 버튼을 누르면 방을 만든다.
        /// </summary>
        private void HandleCreateRoomClicked()
        {
            BeginWaiting("방을 만드는 중...");
            _roomListService.CreateRoom();
        }

        /// <summary>
        /// 목록 항목의 참가 버튼을 누르면 그 방에 참가한다.
        /// </summary>
        /// <param name="roomName">참가할 Photon 방 이름</param>
        private void HandleJoinClicked(string roomName)
        {
            BeginWaiting("방에 참가하는 중...");
            _roomListService.JoinRoom(roomName);
        }

        /// <summary>
        /// 혼자 플레이 버튼을 누르면 혼자 플레이를 시작한다.
        /// </summary>
        private void HandleSoloPlayClicked()
        {
            BeginWaiting("혼자 플레이를 준비하는 중...");
            _soloPlayStarter.StartSoloPlay();
        }

        /// <summary>
        /// 응답을 기다리는 상태로 바꾸고 버튼을 막는다.
        /// 요청이 바로 실패하면 HandleRequestFailed가 곧바로 기다림을 풀기 때문에, 요청을 보내기 전에 호출한다.
        /// </summary>
        /// <param name="message">진행 문구</param>
        private void BeginWaiting(string message)
        {
            _isWaitingResponse = true;
            SetStatus(message);
            RefreshButtons();
            RedrawRoomList();
        }

        /// <summary>
        /// 로비 입장 여부와 응답 대기 여부에 따라 버튼을 누를 수 있는지 갱신한다.
        /// </summary>
        private void RefreshButtons()
        {
            _createRoomButton.interactable = _roomListService.IsInLobby && !_isWaitingResponse;
            _soloPlayButton.interactable = !_isWaitingResponse;
        }

        /// <summary>
        /// 기존 항목을 모두 지우고 현재 방 목록으로 다시 만든다.
        /// </summary>
        private void RedrawRoomList()
        {
            ClearRoomList();

            bool allowJoin = _roomListService.IsInLobby && !_isWaitingResponse;

            foreach (RoomListEntry entry in _roomListService.Rooms)
            {
                RoomEntryView entryView = Instantiate(_roomEntryPrefab, _roomListContent);
                entryView.Setup(entry, allowJoin, HandleJoinClicked);
            }

            if (_emptyListMessage != null)
            {
                _emptyListMessage.SetActive(_roomListService.Rooms.Count == 0);
            }
        }

        /// <summary>
        /// 목록의 항목을 모두 지운다. 지우는 중에 자식 순서가 바뀌지 않도록 뒤에서부터 지운다.
        /// </summary>
        private void ClearRoomList()
        {
            for (int i = _roomListContent.childCount - 1; i >= 0; i--)
            {
                Destroy(_roomListContent.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// 상태 문구를 바꾼다.
        /// </summary>
        /// <param name="message">보여 줄 문구</param>
        private void SetStatus(string message)
        {
            _statusText.text = message;
        }

        /// <summary>
        /// 필수 참조가 모두 연결되어 있는지 확인하고, 비어 있으면 경고를 출력한다.
        /// </summary>
        /// <returns>모두 연결되어 있으면 true</returns>
        private bool HasRequiredReferences()
        {
            bool isValid = true;

            if (_roomListService == null || _soloPlayStarter == null)
            {
                Debug.LogWarning($"[{nameof(RoomListView)}] RoomListService 또는 SoloPlayStarter를 찾지 못했습니다. 로비 화면이 동작하지 않습니다.", this);
                isValid = false;
            }

            if (_roomListContent == null || _roomEntryPrefab == null)
            {
                Debug.LogWarning($"[{nameof(RoomListView)}] 목록 Content 또는 항목 프리팹이 비어 있습니다. 인스펙터를 확인해 주세요.", this);
                isValid = false;
            }

            if (_createRoomButton == null || _soloPlayButton == null || _statusText == null)
            {
                Debug.LogWarning($"[{nameof(RoomListView)}] 버튼(방 만들기, 혼자 플레이) 또는 상태 문구가 비어 있습니다. 인스펙터를 확인해 주세요.", this);
                isValid = false;
            }

            return isValid;
        }
    }
}
