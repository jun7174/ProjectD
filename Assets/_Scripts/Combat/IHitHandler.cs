// 파일 경로: Assets/_Scripts/Combat/IHitHandler.cs
using UnityEngine;

namespace ProjectD.Combat
{
    /// <summary>
    /// 판정용 투사체가 무언가에 맞았을 때 할 일을 정한다
    /// </summary>
    public interface IHitHandler
    {
        /// <summary>
        /// 투사체가 맞았을 때 호출된다.
        /// </summary>
        /// <param name="fireInfo">맞은 투사체의 발사 정보</param>
        /// <param name="hit">명중 정보 (맞은 콜라이더, 위치)</param>
        void HandleHit(FireInfo fireInfo, RaycastHit hit);
    }
}
