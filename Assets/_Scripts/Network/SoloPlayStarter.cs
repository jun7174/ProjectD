// 파일 경로: Assets/_Scripts/Network/SoloPlayStarter.cs
using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace ProjectD.Network
{
    /// <summary>
    /// 혼자 플레이를 시작한다. 서버 접속을 끊고 PUN 2 오프라인 모드로 바꾼 뒤, 오프라인 방을 만든다.
    /// 오프라인 방은 다른 플레이어의 방 목록에 보이지 않는다.
    /// 방에 들어간 뒤 Raid 씬 이동은 RaidSceneLoader가 맡으므로, 온라인과 같은 흐름으로 레이드에 들어간다.
    /// Boot 씬과 Lobby 씬에 하나씩 둔다.
    /// </summary>
    public class SoloPlayStarter : MonoBehaviourPunCallbacks
    {
        // 오프라인 방은 목록에 보이지 않으므로 이름이 겹쳐도 상관없다
        private const string OfflineRoomName = "OfflineRaid";

        // 혼자 플레이 방의 최대 인원. 규칙상 항상 1명이라 인스펙터에 노출하지 않는다
        private const int SoloMaxPlayers = 1;

        /// <summary>
        /// 혼자 플레이를 준비하는 중인지 여부.
        /// 같은 씬의 BootConnector, RoomListService가 이 값으로 일부러 끊은 접속을 접속 실패로 처리하지 않게 한다.
        /// </summary>
        public bool IsStarting { get; private set; }

        /// <summary>
        /// 혼자 플레이 시작에 실패했을 때 발생한다. 인자는 화면에 보여 줄 실패 이유다.
        /// </summary>
        public event Action<string> OnSoloPlayFailed;

        /// <summary>
        /// 혼자 플레이를 시작한다. 서버에 접속되어 있으면 먼저 끊고, 끊긴 뒤 오프라인 모드로 바꾼다.
        /// </summary>
        public void StartSoloPlay()
        {
            if (IsStarting)
            {
                return;
            }

            IsStarting = true;

            // 오프라인 모드는 서버 접속이 끊긴 상태에서만 켤 수 있다
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Disconnect();
                return;
            }

            EnterOfflineMode();
        }

        /// <summary>
        /// 혼자 플레이를 위해 끊은 접속이 끊기면 오프라인 모드로 바꾼다.
        /// </summary>
        /// <param name="cause">끊긴 원인</param>
        public override void OnDisconnected(DisconnectCause cause)
        {
            if (!IsStarting)
            {
                return;
            }

            EnterOfflineMode();
        }

        /// <summary>
        /// 오프라인 모드에서 마스터 서버 접속 콜백이 오면 오프라인 방을 만든다.
        /// </summary>
        public override void OnConnectedToMaster()
        {
            if (!IsStarting || !PhotonNetwork.OfflineMode)
            {
                return;
            }

            RoomOptions options = RaidRoomProperties.CreateRoomOptions(PhotonNetwork.NickName, SoloMaxPlayers);
            PhotonNetwork.CreateRoom(OfflineRoomName, options);
        }

        /// <summary>
        /// 오프라인 방에 들어가면 준비를 마친다. 씬 이동은 RaidSceneLoader가 처리한다.
        /// </summary>
        public override void OnJoinedRoom()
        {
            IsStarting = false;
        }

        /// <summary>
        /// 오프라인 방 만들기에 실패하면 오프라인 모드를 끄고 실패를 알린다.
        /// </summary>
        /// <param name="returnCode">Photon 오류 코드</param>
        /// <param name="message">Photon 오류 메시지</param>
        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            if (!IsStarting)
            {
                return;
            }

            IsStarting = false;
            PhotonNetwork.OfflineMode = false;
            Debug.LogWarning($"[{nameof(SoloPlayStarter)}] 오프라인 방을 만들지 못했습니다. ({returnCode}) {message}", this);
            OnSoloPlayFailed?.Invoke("혼자 플레이를 시작하지 못했습니다.");
        }

        /// <summary>
        /// 오프라인 모드를 켠다. 오프라인 모드를 켜면 서버 없이 바로 OnConnectedToMaster가 호출된다.
        /// </summary>
        private void EnterOfflineMode()
        {
            PhotonNetwork.OfflineMode = true;
        }
    }
}
