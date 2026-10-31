// 파일 경로: Assets/_Scripts/Core/Define/PlayerPrefsKeys.cs
namespace ProjectD.Core.Define
{
    /// <summary>
    /// PlayerPrefs 저장 키 모음. 키 문자열은 이 파일에만 둔다.
    /// 게임 진행 저장(창고, 출격 상태)은 추후에 로컬 JSON으로 따로 만들고, 여기에는 간단한 설정 값만 둔다.
    /// </summary>
    public static class PlayerPrefsKeys
    {
        /// <summary>
        /// 마지막으로 사용한 플레이어 이름 (string). 다음 실행 때 이름 입력칸에 미리 채운다.
        /// </summary>
        public const string PlayerName = "PlayerName";
    }
}
