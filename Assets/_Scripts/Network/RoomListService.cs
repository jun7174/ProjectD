// 파일 경로: Assets/_Scripts/Network/RoomListService.cs
using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using ProjectD.Core.Define;
using UnityEngine;

namespace ProjectD.Network
{
    /// <summary>
    /// Lobby 씬에서 Photon 기본 로비에 들어가 방 목록을 보관하고, 방 만들기와 참가 요청을 보낸다.
    /// Photon은 방 목록 중 바뀐 방만 보내 주므로, 받은 내용을 딕셔너리에 반영해 전체 목록을 직접 유지한다.
    /// UI는 모르며, 로비 입장, 목록 변경, 요청 실패를 이벤트로 알린다.
    /// 방에 들어간 뒤 Raid 씬 이동은 RaidSceneLoader가 맡는다.
    /// </summary>
    public class RoomListService : MonoBehaviourPunCallbacks
    {
        [Header("방 설정")]
        [Tooltip("방 최대 인원")]
        [Range(1, 4)]
        [SerializeField] private int _maxPlayers = 4;

        [Header("참조 (비워 두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private SoloPlayStarter _soloPlayStarter;

        // 방 이름을 키로 최신 방 정보를 보관한다. Photon은 바뀐 방만 보내 주기 때문에 직접 모아 두어야 전체 목록이 된다
        private readonly Dictionary<string, RoomInfo> _cachedRooms = new Dictionary<string, RoomInfo>();
        private readonly List<RoomListEntry> _entries = new List<RoomListEntry>();
        private bool _isRequesting;
        private bool _isJoiningLobby;

        /// <summary>
        /// 로비에 들어갔을 때 발생한다. 이때부터 방 만들기와 참가를 할 수 있다.
        /// </summary>
        public event Action OnLobbyJoined;

        /// <summary>
        /// 방 목록이 바뀌었을 때 발생한다. 최신 목록은 Rooms로 읽는다.
        /// </summary>
        public event Action OnRoomListChanged;

        /// <summary>
        /// 방 만들기나 참가에 실패했을 때 발생한다. 인자는 화면에 보여 줄 실패 이유다.
        /// </summary>
        public event Action<string> OnRequestFailed;

        /// <summary>
        /// 현재 방 목록
        /// </summary>
        public IReadOnlyList<RoomListEntry> Rooms => _entries;

        /// <summary>
        /// 로비에 들어가 있는지 여부
        /// </summary>
        public bool IsInLobby => PhotonNetwork.InLobby;

        /// <summary>
        /// 내 플레이어 이름
        /// </summary>
        public string LocalPlayerName => PhotonNetwork.NickName;

        /// <summary>
        /// 참조를 찾고 비어 있으면 경고한다.
        /// </summary>
        private void Awake()
        {
            if (_soloPlayStarter == null)
            {
                _soloPlayStarter = GetComponent<SoloPlayStarter>();
            }

            if (_soloPlayStarter == null)
            {
                Debug.LogWarning($"[{nameof(RoomListService)}] SoloPlayStarter를 찾지 못했습니다. 혼자 플레이를 위해 끊은 접속을 끊김으로 잘못 처리할 수 있습니다.", this);
            }
        }

        /// <summary>
        /// 접속 상태를 보고 로비에 들어가거나, 접속되어 있지 않으면 Boot 씬으로 돌아간다.
        /// </summary>
        private void Start()
        {
            ClientState state = PhotonNetwork.NetworkClientState;
            if (state == ClientState.PeerCreated || state == ClientState.Disconnected || PhotonNetwork.OfflineMode)
            {
                Debug.LogWarning($"[{nameof(RoomListService)}] 서버에 접속되어 있지 않습니다. Boot 씬으로 돌아갑니다. Lobby 씬은 Boot 씬부터 실행해야 합니다.", this);
                PhotonNetwork.LoadLevel(SceneNames.Boot);
                return;
            }

            // Boot에서 바로 왔으면 이미 마스터 서버에 있으므로 지금 들어간다.
            // 레이드에서 막 나온 경우는 아직 서버를 옮기는 중이라 OnConnectedToMaster에서 들어간다
            if (PhotonNetwork.IsConnectedAndReady && !PhotonNetwork.InRoom)
            {
                JoinLobbyOnce();
            }
        }

        /// <summary>
        /// 새 방을 만든다. 실제 방 이름은 중복 없는 고유 문자열, 표시 이름은 "이름의 방"으로 방 속성에 넣는다.
        /// </summary>
        public void CreateRoom()
        {
            if (!CanSendRequest())
            {
                return;
            }

            _isRequesting = true;

            string roomName = Guid.NewGuid().ToString("N");
            string displayName = $"{PhotonNetwork.NickName}의 방";
            RoomOptions options = RaidRoomProperties.CreateRoomOptions(displayName, _maxPlayers);

            if (!PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default))
            {
                FailRequest("방 만들기 요청을 보내지 못했습니다.");
            }
        }

        /// <summary>
        /// 목록의 방에 참가한다. 레이드가 진행 중인 방이면 중도 참가가 된다.
        /// </summary>
        /// <param name="roomName">참가할 Photon 방 이름</param>
        public void JoinRoom(string roomName)
        {
            if (!CanSendRequest())
            {
                return;
            }

            if (!_cachedRooms.TryGetValue(roomName, out RoomInfo room) || !CanJoin(room))
            {
                OnRequestFailed?.Invoke("참가할 수 없는 방입니다.");
                return;
            }

            _isRequesting = true;

            if (!PhotonNetwork.JoinRoom(roomName))
            {
                FailRequest("방 참가 요청을 보내지 못했습니다.");
            }
        }

        /// <summary>
        /// 레이드에서 나와 마스터 서버로 돌아왔을 때 로비에 다시 들어간다. 혼자 플레이(오프라인 모드)는 무시한다.
        /// </summary>
        public override void OnConnectedToMaster()
        {
            if (PhotonNetwork.OfflineMode)
            {
                return;
            }

            JoinLobbyOnce();
        }

        /// <summary>
        /// 로비에 들어가면 보관한 목록을 비우고 입장을 알린다. 첫 목록은 이어서 OnRoomListUpdate로 들어온다.
        /// </summary>
        public override void OnJoinedLobby()
        {
            _isJoiningLobby = false;
            _cachedRooms.Clear();
            RebuildEntries();
            OnLobbyJoined?.Invoke();
            OnRoomListChanged?.Invoke();
        }

        /// <summary>
        /// 로비를 떠나면 보관한 목록을 비운다. 방에 들어갈 때도 로비를 떠난다.
        /// </summary>
        public override void OnLeftLobby()
        {
            _cachedRooms.Clear();
            RebuildEntries();
        }

        /// <summary>
        /// 바뀐 방 정보를 보관한 목록에 반영한다. 목록에서 빠진 방은 지우고, 나머지는 새 정보로 덮어쓴다.
        /// </summary>
        /// <param name="roomList">이번에 바뀐 방 목록</param>
        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            foreach (RoomInfo room in roomList)
            {
                if (room.RemovedFromList)
                {
                    _cachedRooms.Remove(room.Name);
                }
                else
                {
                    _cachedRooms[room.Name] = room;
                }
            }

            RebuildEntries();
            OnRoomListChanged?.Invoke();
        }

        /// <summary>
        /// 방에 들어가면 요청을 마친다. 씬 이동은 RaidSceneLoader가 처리한다.
        /// </summary>
        public override void OnJoinedRoom()
        {
            _isRequesting = false;
        }

        /// <summary>
        /// 방 만들기 실패를 알린다.
        /// </summary>
        /// <param name="returnCode">Photon 오류 코드</param>
        /// <param name="message">Photon 오류 메시지</param>
        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            if (!_isRequesting)
            {
                return;
            }

            Debug.LogWarning($"[{nameof(RoomListService)}] 방 만들기 실패 ({returnCode}) {message}", this);
            FailRequest("방을 만들지 못했습니다.");
        }

        /// <summary>
        /// 방 참가 실패를 이유와 함께 알린다.
        /// </summary>
        /// <param name="returnCode">Photon 오류 코드</param>
        /// <param name="message">Photon 오류 메시지</param>
        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            if (!_isRequesting)
            {
                return;
            }

            Debug.LogWarning($"[{nameof(RoomListService)}] 방 참가 실패 ({returnCode}) {message}", this);
            FailRequest(BuildJoinFailedMessage(returnCode));
        }

        /// <summary>
        /// 접속이 끊기면 Boot 씬으로 돌아간다. 혼자 플레이를 위해 일부러 끊은 경우는 무시한다.
        /// </summary>
        /// <param name="cause">끊긴 원인</param>
        public override void OnDisconnected(DisconnectCause cause)
        {
            if (PhotonNetwork.OfflineMode || (_soloPlayStarter != null && _soloPlayStarter.IsStarting))
            {
                return;
            }

            Debug.LogWarning($"[{nameof(RoomListService)}] 서버 접속이 끊겼습니다. ({cause}) Boot 씬으로 돌아갑니다.", this);
            _cachedRooms.Clear();
            PhotonNetwork.LoadLevel(SceneNames.Boot);
        }

        /// <summary>
        /// 로비 입장 요청을 한 번만 보낸다. Start와 OnConnectedToMaster가 모두 요청할 수 있어 중복을 막는다.
        /// </summary>
        private void JoinLobbyOnce()
        {
            if (_isJoiningLobby || PhotonNetwork.InLobby)
            {
                return;
            }

            _isJoiningLobby = true;

            if (!PhotonNetwork.JoinLobby())
            {
                _isJoiningLobby = false;
                Debug.LogWarning($"[{nameof(RoomListService)}] 로비 입장 요청을 보내지 못했습니다.", this);
            }
        }

        /// <summary>
        /// 지금 방 만들기나 참가 요청을 보낼 수 있는지 확인한다. 로비에 들어가 있지 않으면 실패를 알린다.
        /// </summary>
        /// <returns>요청을 보낼 수 있으면 true</returns>
        private bool CanSendRequest()
        {
            if (_isRequesting)
            {
                return false;
            }

            if (!PhotonNetwork.InLobby)
            {
                OnRequestFailed?.Invoke("아직 로비에 들어가지 못했습니다. 잠시 후 다시 시도해 주세요.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 요청 상태를 풀고 실패를 알린다.
        /// </summary>
        /// <param name="message">화면에 보여 줄 실패 이유</param>
        private void FailRequest(string message)
        {
            _isRequesting = false;
            OnRequestFailed?.Invoke(message);
        }

        /// <summary>
        /// 보관한 방 정보로 화면용 목록을 다시 만든다.
        /// </summary>
        private void RebuildEntries()
        {
            _entries.Clear();

            foreach (RoomInfo room in _cachedRooms.Values)
            {
                RoomListEntry entry = new RoomListEntry(
                    room.Name,
                    RaidRoomProperties.GetDisplayName(room),
                    room.PlayerCount,
                    room.MaxPlayers,
                    CanJoin(room));

                _entries.Add(entry);
            }
        }

        /// <summary>
        /// 방에 참가할 수 있는지 확인한다. 방이 열려 있고 인원이 남아 있어야 한다.
        /// </summary>
        /// <param name="room">확인할 방</param>
        /// <returns>참가할 수 있으면 true</returns>
        private static bool CanJoin(RoomInfo room)
        {
            return room.IsOpen && room.PlayerCount < room.MaxPlayers;
        }

        /// <summary>
        /// 방 참가 실패 코드를 화면에 보여 줄 문장으로 바꾼다.
        /// </summary>
        /// <param name="returnCode">Photon 오류 코드</param>
        /// <returns>실패 이유 문장</returns>
        private static string BuildJoinFailedMessage(short returnCode)
        {
            switch (returnCode)
            {
                case ErrorCode.GameFull:
                    return "방이 가득 찼습니다.";
                case ErrorCode.GameClosed:
                    return "지금은 참가할 수 없는 방입니다.";
                case ErrorCode.GameDoesNotExist:
                    return "방이 사라졌습니다.";
                default:
                    return $"방에 참가하지 못했습니다. (코드 {returnCode})";
            }
        }
    }
}
