// 파일 경로: Assets/_Scripts/Core/Define/AnimatorStates.cs
using UnityEngine;

namespace ProjectD.Core.Define
{
    /// <summary>
    /// Animator 상태 이름을 해시로 미리 바꿔 모아 둔 상수 모음.
    /// 파라미터가 아니라 상태를 직접 재생해야 하는 예외 상황에서만 사용한다.
    /// 문자열은 Animator Controller의 상태 이름과 대소문자까지 정확히 같아야 한다.
    /// </summary>
    public static class AnimatorStates
    {
        /// <summary>
        /// 회피 상태. 연속 회피 시 클립을 처음부터 다시 재생하는 데 사용한다.
        /// </summary>
        public static readonly int Dodge = Animator.StringToHash("Dodge");
    }
}
