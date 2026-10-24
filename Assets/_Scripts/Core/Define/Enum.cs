// 파일 경로: Assets/_Scripts/Core/Define/Enum.cs
namespace ProjectD.Core.Define
{
    // 프로젝트에서 쓰는 열거형을 모두 이 파일에 모은다.
    // 새 열거형은 별도 파일을 만들지 않고 이 파일 아래에 추가한다.

    /// <summary>
    /// 탄약 종류. 무기는 자신이 사용하는 탄약 종류를 하나 가진다.
    /// 인벤토리에서 탄약 아이템과 연결한다.
    /// </summary>
    public enum AmmoType
    {
        /// <summary>권총탄</summary>
        Pistol,

        /// <summary>소총탄</summary>
        Rifle,

        /// <summary>산탄</summary>
        Shotgun,

        /// <summary>고급 무기 전용탄</summary>
        Special
    }

    /// <summary>
    /// 플레이어 Animator의 상태 번호. Animator의 State 파라미터에 이 값을 넣는다.
    /// 숫자는 PlayerAnimator의 Any State 전환 조건(State Equals 값)과 반드시 같아야 한다.
    /// </summary>
    public enum PlayerAnimState
    {
        /// <summary>제자리 대기</summary>
        Idle = 0,

        /// <summary>걷기 (뒤로 이동 시 역재생)</summary>
        Walk = 1,

        /// <summary>달리기 (뒤로 이동 시 역재생)</summary>
        Run = 2,

        /// <summary>제자리 사격 자세</summary>
        IdleShoot = 3,

        /// <summary>걸으며 사격 자세 (뒤로 이동 시 역재생)</summary>
        WalkShoot = 4,

        /// <summary>회피</summary>
        Dodge = 5,

        /// <summary>재장전 (멈춰 있을 때만 재생, 재장전 동안 반복)</summary>
        Reload = 6
    }
}
