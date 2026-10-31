// 파일 경로: Assets/_Scripts/Combat/IHitHandler.cs
using UnityEngine;

namespace ProjectD.Combat
{
    /// <summary>
    /// 판정용 투사체가 무언가에 맞았을 때 할 일을 정한다
    /// ProjectileLauncher는 맞은 뒤 누가 무엇을 하는지 모르고 이 인터페이스만 호출한다.
    /// 구현은 두 가지다. 방 밖(Sandbox)에서는 바로 데미지를 주는 DirectDamageHitHandler,
    /// 네트워크 방 안에서는 마스터에게 명중을 보고하는 Network 모듈의 PlayerHitReporter.
    /// 이렇게 나누면 Combat 모듈은 네트워크를 몰라도 된다.
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
