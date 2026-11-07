// 파일 경로: Assets/_Scripts/Core/Define/Enum.cs
namespace ProjectD.Core.Define
{
    // 프로젝트에서 쓰는 열거형을 모두 이 파일에 모은다.
    // 새 열거형은 별도 파일을 만들지 않고 이 파일 아래에 추가한다.

    /// <summary>
    /// 탄약 종류. 무기는 자신이 사용하는 탄약 종류를 하나 가진다.
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

    /// <summary>
    /// 내 캐릭터를 가리는 물체(Occluder)를 안 보이게 하는 방식.
    /// </summary>
    public enum OcclusionMode
    {
        /// <summary>투명 머티리얼로 바꿔 너머가 비쳐 보이게 한다 (벽)</summary>
        Transparent,

        /// <summary>그림자만 남기고 안 보이게 한다 (천장, 실내 어둠 유지)</summary>
        ShadowsOnly
    }

    /// <summary>
    /// 적 상태 머신의 상태. 숫자를 byte로 바꿔 네트워크로 보내므로 순서를 바꾸지 않는다.
    /// </summary>
    public enum EnemyState
    {
        /// <summary>순찰 지점을 순서대로 걷는다</summary>
        Patrol = 0,

        /// <summary>소리 난 곳이나 잠깐 본 곳으로 가서 둘러본다</summary>
        Investigate = 1,

        /// <summary>대상을 쫓는다</summary>
        Chase = 2,

        /// <summary>공격 사거리 안에서 공격한다</summary>
        Attack = 3,

        /// <summary>대상을 놓쳐 순찰 경로로 돌아간다</summary>
        Return = 4,

        /// <summary>사망</summary>
        Dead = 5
    }

    /// <summary>
    /// 적의 공격 방식. 적 종류마다 하나를 정하고, EnemyBrain이 이 값으로 공격 상태를 고른다.
    /// </summary>
    public enum EnemyAttackType
    {
        /// <summary>예고 후 짧게 돌진해서 몸으로 친다 (돌격형)</summary>
        Charge,

        /// <summary>거리를 두고 예고 후 느린 탄을 여러 발 쏜다 (사수형)</summary>
        Burst
    }

    /// <summary>
    /// 적 공격 한 번의 진행 단계. 모든 공격은 예고 → 실행 → 빈틈 순서로 진행한다.
    /// </summary>
    public enum EnemyAttackPhase
    {
        /// <summary>예고 동작. 제자리에서 대상을 바라보며 색과 크기가 바뀐다</summary>
        Telegraph,

        /// <summary>실제 공격 (돌진, 점사)</summary>
        Strike,

        /// <summary>공격 후 빈틈 또는 다음 공격까지 쉬는 시간</summary>
        Recover
    }

    /// <summary>
    /// 투사체 풀의 종류. 발사 담당은 같은 종류의 풀에서 투사체를 꺼낸다.
    /// 플레이어 탄과 적 탄의 프리팹(색, 크기)을 나누기 위해 쓴다.
    /// </summary>
    public enum ProjectilePoolType
    {
        /// <summary>플레이어 탄</summary>
        Player,

        /// <summary>적 탄</summary>
        Enemy
    }
}
