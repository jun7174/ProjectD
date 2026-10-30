// 파일 경로: Assets/_Scripts/Network/RoomListEntry.cs
namespace ProjectD.Network
{
    /// <summary>
    /// 로비 방 목록의 한 줄에 필요한 정보.
    /// UI가 Photon 타입(RoomInfo)을 몰라도 되도록 화면에 필요한 값만 옮겨 담는다.
    /// </summary>
    public readonly struct RoomListEntry
    {
        /// <summary>
        /// Photon 방 이름. 참가 요청에 사용하는 고유 문자열이다.
        /// </summary>
        public string RoomName { get; }

        /// <summary>
        /// 화면에 보여 줄 방 이름 (예: 형준의 방)
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// 현재 인원
        /// </summary>
        public int PlayerCount { get; }

        /// <summary>
        /// 최대 인원
        /// </summary>
        public int MaxPlayers { get; }

        /// <summary>
        /// 지금 참가할 수 있는지 여부. 방이 열려 있고 인원이 남아 있어야 true
        /// </summary>
        public bool CanJoin { get; }

        /// <summary>
        /// 방 목록 한 줄의 정보를 만든다.
        /// </summary>
        /// <param name="roomName">Photon 방 이름</param>
        /// <param name="displayName">화면에 보여 줄 방 이름</param>
        /// <param name="playerCount">현재 인원</param>
        /// <param name="maxPlayers">최대 인원</param>
        /// <param name="canJoin">참가 가능 여부</param>
        public RoomListEntry(string roomName, string displayName, int playerCount, int maxPlayers, bool canJoin)
        {
            RoomName = roomName;
            DisplayName = displayName;
            PlayerCount = playerCount;
            MaxPlayers = maxPlayers;
            CanJoin = canJoin;
        }
    }
}
