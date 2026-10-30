// 파일 경로: Assets/_Scripts/Network/RaidRoomProperties.cs
using Photon.Realtime;
using ProjectD.Core.Define;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace ProjectD.Network
{
    /// <summary>
    /// 레이드 방의 생성 옵션을 만들고 방 속성을 읽는다.
    /// 방 만들기(RoomListService)와 혼자 플레이(SoloPlayStarter)가 같은 규칙으로 방을 만들도록 한곳에 모은다.
    /// </summary>
    public static class RaidRoomProperties
    {
        // 시작 지점 선택값의 범위. 시작 지점 수(2~3곳)로 나눈 나머지를 쓰므로 충분히 크기만 하면 된다
        private const int StartPointSeedRange = 1000;

        /// <summary>
        /// 레이드 방 생성 옵션을 만든다.
        /// 방 목록에 공개하고 중도 참가를 허용하며, 표시 이름과 시작 지점 선택값을 방 속성에 넣는다.
        /// </summary>
        /// <param name="displayName">방 목록에 보여 줄 방 이름</param>
        /// <param name="maxPlayers">최대 인원</param>
        /// <returns>CreateRoom에 넘길 방 옵션</returns>
        public static RoomOptions CreateRoomOptions(string displayName, int maxPlayers)
        {
            Hashtable properties = new Hashtable
            {
                { RoomPropertyKeys.DisplayName, displayName },
                { RoomPropertyKeys.StartPointSeed, Random.Range(0, StartPointSeedRange) }
            };

            return new RoomOptions
            {
                // PUN 2 버전에 따라 MaxPlayers가 byte 또는 int라서, 두 경우 모두 컴파일되도록 byte로 넘긴다
                MaxPlayers = (byte)maxPlayers,
                IsVisible = true,
                // 레이드 중에도 방을 열어 두어 중도 참가를 허용한다 (GameDesign 9번)
                IsOpen = true,
                // 5주차 재참가 제한에서 나간 플레이어를 UserId로 구분하기 위해 공개한다
                PublishUserId = true,
                CustomRoomProperties = properties,
                // 로비의 방 목록에서도 표시 이름을 읽을 수 있도록 로비에 공개할 속성을 지정한다
                CustomRoomPropertiesForLobby = new[] { RoomPropertyKeys.DisplayName }
            };
        }

        /// <summary>
        /// 방의 표시 이름을 읽는다. 속성이 없으면 Photon 방 이름을 그대로 쓴다.
        /// 로비 목록의 RoomInfo와 방 안의 Room 모두에 사용할 수 있다.
        /// </summary>
        /// <param name="room">읽을 방 정보</param>
        /// <returns>화면에 보여 줄 방 이름</returns>
        public static string GetDisplayName(RoomInfo room)
        {
            if (room.CustomProperties.TryGetValue(RoomPropertyKeys.DisplayName, out object value) && value is string displayName)
            {
                return displayName;
            }

            return room.Name;
        }

        /// <summary>
        /// 방의 시작 지점 선택값을 읽는다. 속성이 없으면 0을 돌려 첫 번째 시작 지점을 쓰게 한다.
        /// </summary>
        /// <param name="room">읽을 방 정보</param>
        /// <returns>시작 지점 선택값 (0 이상)</returns>
        public static int GetStartPointSeed(RoomInfo room)
        {
            if (room.CustomProperties.TryGetValue(RoomPropertyKeys.StartPointSeed, out object value) && value is int seed && seed >= 0)
            {
                return seed;
            }

            return 0;
        }
    }
}
