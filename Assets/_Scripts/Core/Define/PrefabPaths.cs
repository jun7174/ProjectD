// 파일 경로: Assets/_Scripts/Core/Define/PrefabPaths.cs
namespace ProjectD.Core.Define
{
    /// <summary>
    /// PhotonNetwork.Instantiate로 생성하는 프리팹 경로 모음.
    /// PUN 2는 기본적으로 Resources 폴더 기준 경로로 프리팹을 찾으므로, 값은 Resources 폴더 안의 경로(확장자 제외)와 같아야 한다.
    /// </summary>
    public static class PrefabPaths
    {
        /// <summary>
        /// 플레이어 캐릭터 프리팹 (Assets/Resources/Player.prefab)
        /// </summary>
        public const string Player = "Player";
    }
}
