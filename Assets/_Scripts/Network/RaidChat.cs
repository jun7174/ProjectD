// 파일 경로: Assets/_Scripts/Network/RaidChat.cs
using System;
using Photon.Pun;
using UnityEngine;

namespace ProjectD.Network
{
    /// <summary>
    /// 레이드 방 안 채팅. 메시지를 RPC로 방 전원에게 보내고, 받은 메시지와 입장·퇴장 알림을 이벤트로 알린다.
    /// 보낸 사람 이름은 받는 쪽에서 메시지 정보의 보낸 사람 닉네임으로 붙여, 이름을 바꿔 보낼 수 없게 한다.
    /// 지난 메시지는 서버에 보관하지 않으므로 중도 참가자는 들어오기 전 메시지를 받지 않는다.
    /// 혼자 플레이(오프라인)에서는 쓰지 않는다.
    /// Raid 씬의 PhotonView가 있는 오브젝트에 붙인다.
    /// </summary>
    public class RaidChat : MonoBehaviourPunCallbacks
    {
        [Tooltip("메시지 최대 글자 수")]
        [Min(1)]
        [SerializeField] private int _maxLength = 60;

        /// <summary>메시지 최대 글자 수</summary>
        public int MaxLength => _maxLength;

        /// <summary>채팅을 쓸 수 있는지 여부. 온라인 방 안에서만 true</summary>
        public bool IsAvailable => PhotonNetwork.InRoom && !PhotonNetwork.OfflineMode;

        /// <summary>
        /// 메시지를 받으면 발생한다. 인자는 보낸 사람 이름과 메시지다.
        /// </summary>
        public event Action<string, string> OnMessageReceived;

        /// <summary>
        /// 입장·퇴장 같은 알림이 생기면 발생한다. 인자는 알림 문구다.
        /// </summary>
        public event Action<string> OnSystemMessage;

        /// <summary>
        /// 메시지를 방 전원에게 보낸다. 앞뒤 공백을 지우고, 비었으면 보내지 않고, 길면 자른다.
        /// </summary>
        /// <param name="message">보낼 메시지</param>
        public void Send(string message)
        {
            if (!IsAvailable)
            {
                return;
            }

            string trimmed = Clean(message);
            if (trimmed.Length == 0)
            {
                return;
            }

            photonView.RPC(nameof(RpcReceiveMessage), RpcTarget.All, trimmed);
        }

        /// <summary>
        /// 누가 들어오면 알림을 띄운다. 각자 자기 화면에서 만들므로 전송하지 않는다.
        /// </summary>
        /// <param name="newPlayer">들어온 플레이어</param>
        public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            OnSystemMessage?.Invoke($"{newPlayer.NickName} 님이 들어왔습니다");
        }

        /// <summary>
        /// 누가 나가면 알림을 띄운다.
        /// </summary>
        /// <param name="otherPlayer">나간 플레이어</param>
        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            OnSystemMessage?.Invoke($"{otherPlayer.NickName} 님이 나갔습니다");
        }

        /// <summary>
        /// 메시지를 받는다. 실제 처리는 Receive에서 한다.
        /// </summary>
        /// <param name="message">메시지</param>
        /// <param name="info">보낸 사람 등 메시지 정보 (PUN이 자동으로 채운다)</param>
        [PunRPC]
        private void RpcReceiveMessage(string message, PhotonMessageInfo info)
        {
            string senderName = info.Sender != null ? info.Sender.NickName : "알 수 없음";
            Receive(senderName, message);
        }

        /// <summary>
        /// 받은 메시지를 한 번 더 정리해 알린다. 보낸 쪽 코드가 달라도 긴 메시지가 화면을 덮지 않게 한다.
        /// </summary>
        /// <param name="senderName">보낸 사람 이름</param>
        /// <param name="message">메시지</param>
        private void Receive(string senderName, string message)
        {
            string trimmed = Clean(message);
            if (trimmed.Length > 0)
            {
                OnMessageReceived?.Invoke(senderName, trimmed);
            }
        }

        /// <summary>
        /// 앞뒤 공백을 지우고 최대 글자 수로 자른다.
        /// </summary>
        /// <param name="message">원래 메시지</param>
        /// <returns>정리한 메시지. 비었으면 빈 문자열</returns>
        private string Clean(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return string.Empty;
            }

            string trimmed = message.Trim();
            return trimmed.Length > _maxLength ? trimmed.Substring(0, _maxLength) : trimmed;
        }
    }
}
