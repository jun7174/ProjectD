// 파일 경로: Assets/_Scripts/Network/RaidSessionWatcher.cs
using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using ProjectD.Core.Define;
using UnityEngine;

namespace ProjectD.Network
{
    /// <summary>
    /// Raid 씬에서 방 상태를 지켜본다. 참가자 목록 변화를 알리고, 레이드 나가기와 방장 이탈 시 세션 종료를 처리한다.
    /// 방을 나가면 온라인은 Lobby 씬, 혼자 플레이는 Boot 씬으로 돌아간다.
    /// 2주차 임시 처리이며, 레이드 무효 처리와 출격 상태 저장은 6주차에 붙인다.
    /// </summary>
    /// <remarks>
    /// ProjectD.Player 네임스페이스와 이름이 겹치므로 Photon의 플레이어 클래스는 Photon.Realtime.Player로 전체 이름을 쓴다.
    /// </remarks>
    public class RaidSessionWatcher : MonoBehaviourPunCallbacks
    {
        private readonly List<string> _playerNames = new List<string>();
        private bool _isLeaving;

        /// <summary>
        /// 참가자가 들어오거나 나가서 목록이 바뀌었을 때 발생한다.
        /// </summary>
        public event Action OnPlayerListChanged;

        /// <summary>
        /// 방장이 나가 세션이 끝날 때 발생한다. 이 이벤트 직후 방을 나간다.
        /// </summary>
        public event Action OnMasterLeft;

        /// <summary>
        /// 현재 참가자 이름 목록
        /// </summary>
        public IReadOnlyList<string> PlayerNames => _playerNames;

        /// <summary>
        /// 현재 방장 이름
        /// </summary>
        public string MasterPlayerName { get; private set; } = string.Empty;

        /// <summary>
        /// 방 목록에 보이는 방 이름
        /// </summary>
        public string RoomDisplayName { get; private set; } = string.Empty;

        /// <summary>
        /// 방 최대 인원
        /// </summary>
        public int MaxPlayers { get; private set; }

        /// <summary>
        /// 혼자 플레이(오프라인 모드)인지 여부
        /// </summary>
        public bool IsOfflineMode => PhotonNetwork.OfflineMode;

        /// <summary>
        /// 방에 들어와 있는지 확인하고 방 정보와 참가자 목록을 읽는다.
        /// 방에 들어와 있지 않으면 Raid 씬을 바로 실행한 경우이므로 Boot 씬으로 돌아간다.
        /// </summary>
        private void Start()
        {
            if (!PhotonNetwork.InRoom)
            {
                Debug.LogWarning($"[{nameof(RaidSessionWatcher)}] 방에 들어가 있지 않습니다. Raid 씬은 Boot 씬부터 실행해야 합니다. Boot 씬으로 돌아갑니다.", this);
                PhotonNetwork.LoadLevel(SceneNames.Boot);
                return;
            }

            ReadRoomInfo();
            RefreshPlayerList();
        }

        /// <summary>
        /// 레이드에서 나간다. 방을 나가는 것이 끝나면 OnLeftRoom에서 씬을 옮긴다.
        /// </summary>
        public void LeaveRaid()
        {
            if (_isLeaving)
            {
                return;
            }

            _isLeaving = true;
            PhotonNetwork.LeaveRoom();
        }

        /// <summary>
        /// 방을 나가면 온라인은 Lobby, 혼자 플레이는 Boot 씬으로 돌아간다.
        /// 혼자 플레이 후에는 오프라인 모드를 끄고 다시 접속해야 하므로 Boot로 보낸다.
        /// </summary>
        public override void OnLeftRoom()
        {
            string nextScene = PhotonNetwork.OfflineMode ? SceneNames.Boot : SceneNames.Lobby;
            PhotonNetwork.LoadLevel(nextScene);
        }

        /// <summary>
        /// 새 참가자가 들어오면 참가자 목록을 갱신한다.
        /// </summary>
        /// <param name="newPlayer">들어온 플레이어</param>
        public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            RefreshPlayerList();
        }

        /// <summary>
        /// 참가자가 나가면 참가자 목록을 갱신한다.
        /// </summary>
        /// <param name="otherPlayer">나간 플레이어</param>
        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            RefreshPlayerList();
        }

        /// <summary>
        /// 방장이 나가 PUN이 방장을 다른 사람에게 넘기면 세션을 끝낸다. 마스터 이전은 지원하지 않는다 (GameDesign 9번).
        /// 새 방장이 된 클라이언트는 먼저 방을 닫아, 모두 나가는 사이에 다른 사람이 들어오지 못하게 한다.
        /// </summary>
        /// <param name="newMasterClient">새 방장</param>
        public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
        {
            if (newMasterClient.IsLocal && PhotonNetwork.CurrentRoom != null)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;
            }

            OnMasterLeft?.Invoke();
            LeaveRaid();
        }

        /// <summary>
        /// 서버 접속이 끊기면 Boot 씬으로 돌아간다. 끊김을 사망으로 처리하는 규칙은 5주차에 붙인다.
        /// </summary>
        /// <param name="cause">끊긴 원인</param>
        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogWarning($"[{nameof(RaidSessionWatcher)}] 서버 접속이 끊겼습니다. ({cause}) Boot 씬으로 돌아갑니다.", this);
            PhotonNetwork.LoadLevel(SceneNames.Boot);
        }

        /// <summary>
        /// 현재 방의 표시 이름과 최대 인원을 읽는다.
        /// </summary>
        private void ReadRoomInfo()
        {
            Room room = PhotonNetwork.CurrentRoom;
            RoomDisplayName = RaidRoomProperties.GetDisplayName(room);
            MaxPlayers = room.MaxPlayers;
        }

        /// <summary>
        /// 참가자 이름 목록과 방장 이름을 다시 읽고 변경을 알린다.
        /// </summary>
        private void RefreshPlayerList()
        {
            _playerNames.Clear();

            foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
            {
                _playerNames.Add(player.NickName);
            }

            MasterPlayerName = PhotonNetwork.MasterClient != null ? PhotonNetwork.MasterClient.NickName : string.Empty;
            OnPlayerListChanged?.Invoke();
        }
    }
}
