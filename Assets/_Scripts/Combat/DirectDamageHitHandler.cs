// 파일 경로: Assets/_Scripts/Combat/DirectDamageHitHandler.cs
using UnityEngine;

namespace ProjectD.Combat
{
    /// <summary>
    /// 맞은 대상에게 바로 데미지를 주는 명중 처리. 판정 권한을 따질 필요가 없는 방 밖(Sandbox)에서 쓴다.
    /// ProjectileLauncher의 기본 명중 처리이며, 네트워크 방 안에서는 PlayerHitReporter로 교체된다.
    /// 컴포넌트가 아닌 일반 클래스라 오브젝트에 붙이지 않는다.
    /// </summary>
    public class DirectDamageHitHandler : IHitHandler
    {
        /// <summary>
        /// 맞은 대상이 데미지를 받을 수 있으면 무기 데미지를 전달한다.
        /// </summary>
        /// <param name="fireInfo">맞은 투사체의 발사 정보</param>
        /// <param name="hit">명중 정보</param>
        public void HandleHit(FireInfo fireInfo, RaycastHit hit)
        {
            // 콜라이더가 자식에 붙어 있는 경우도 있어 부모 방향으로 찾는다
            IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable == null || fireInfo.Weapon == null)
            {
                return;
            }

            damageable.TakeDamage(fireInfo.Weapon.Damage, hit.point);
        }
    }
}
