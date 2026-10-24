// 파일 경로: Assets/_Scripts/Core/Define/ShaderPropertyIds.cs
using UnityEngine;

namespace ProjectD.Core.Define
{
    /// <summary>
    /// 셰이더 속성 이름을 ID로 미리 바꿔 모아 둔 상수 모음.
    /// 속성 이름 문자열은 이 파일에만 두고, 코드에서는 여기의 ID만 사용해서 오타를 막는다.
    /// </summary>
    public static class ShaderPropertyIds
    {
        /// <summary>
        /// URP Lit 셰이더의 기본 색 (_BaseColor)
        /// </summary>
        public static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    }
}
