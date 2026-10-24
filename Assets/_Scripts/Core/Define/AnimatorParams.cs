// 파일 경로: Assets/_Scripts/Core/Define/AnimatorParams.cs
using UnityEngine;

namespace ProjectD.Core.Define
{
    /// <summary>
    /// Animator 파라미터 이름을 해시로 미리 바꿔 모아 둔 상수 모음.
    /// 파라미터 이름 문자열은 이 파일에만 두고, 코드에서는 여기의 해시만 사용해서 오타를 막는다.
    /// 문자열은 Animator Controller의 파라미터 이름과 대소문자까지 정확히 같아야 한다.
    /// </summary>
    public static class AnimatorParams
    {
        /// <summary>
        /// 재생할 상태 번호 (int). 값은 PlayerAnimState를 따른다.
        /// </summary>
        public static readonly int State = Animator.StringToHash("State");

        /// <summary>
        /// 이동 클립 재생 방향 (float). 앞쪽 이동 1, 뒤쪽 이동 -1. 이동 상태의 Speed Multiplier에 연결한다.
        /// </summary>
        public static readonly int MoveDirection = Animator.StringToHash("MoveDirection");
    }
}
