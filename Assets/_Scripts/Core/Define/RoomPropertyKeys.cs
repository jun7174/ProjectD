// 파일 경로: Assets/_Scripts/Core/Define/RoomPropertyKeys.cs
namespace ProjectD.Core.Define
{
    /// <summary>
    /// Photon 방 속성(Custom Room Properties) 키 모음.
    /// 방 속성은 네트워크로 전송되므로 키를 짧게 둔다. 키 문자열은 이 파일에만 둔다.
    /// </summary>
    public static class RoomPropertyKeys
    {
        /// <summary>
        /// 방 목록에 보여 줄 방 이름 (string). 실제 방 이름은 중복을 피하려고 고유 문자열을 쓰기 때문에 따로 둔다.
        /// </summary>
        public const string DisplayName = "dn";

        /// <summary>
        /// 시작 지점 선택값 (int). 방을 만들 때 무작위로 정하고, 레이드 씬의 시작 지점 수로 나눈 나머지를 시작 지점 번호로 쓴다.
        /// </summary>
        public const string StartPointSeed = "sp";
    }
}
