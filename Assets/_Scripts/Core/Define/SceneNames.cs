// 파일 경로: Assets/_Scripts/Core/Define/SceneNames.cs
namespace ProjectD.Core.Define
{
    /// <summary>
    /// 씬 이름 상수 모음.
    /// 씬 이름 문자열은 이 파일에만 두고, 코드에서는 여기의 상수만 사용해서 오타를 막는다.
    /// 문자열은 Build Profiles에 등록한 씬 파일 이름과 대소문자까지 정확히 같아야 한다.
    /// </summary>
    public static class SceneNames
    {
        /// <summary>
        /// 이름 입력과 Photon 접속을 하는 첫 씬
        /// </summary>
        public const string Boot = "Boot";

        /// <summary>
        /// 방 목록을 보고 방을 만들거나 참가하는 씬
        /// </summary>
        public const string Lobby = "Lobby";

        /// <summary>
        /// 실제 게임을 진행하는 레이드 씬
        /// </summary>
        public const string Raid = "Raid";
    }
}
